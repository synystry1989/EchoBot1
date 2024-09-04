using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using EchoBot1.Modelos;
using EchoBot1.Dialogos;
using System;

namespace EchoBot1.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly KnowledgeBase _knowledgeBase;
        private readonly ILogger _logger;
        private readonly string _openAiEndpoint;
        private readonly string _openAiApiKey;
        private readonly string _model;
        private readonly int _maxTokens;
        private readonly double _temperature;

        public MainDialog(PersonalDataDialog personalDataDialog, LearningModeDialog learningModeDialog, KnowledgeBase knowledgeBase, ILogger<MainDialog> logger, IConfiguration configuration)
            : base(nameof(MainDialog))
        {
            
            _knowledgeBase = knowledgeBase;
            _logger = logger;
            _openAiEndpoint = configuration["OpenAI:APIEndpoint"];
            _openAiApiKey = configuration["OpenAI:ApiKey"];
            _model = configuration["OpenAI:Model"];
            _maxTokens = int.Parse(configuration["OpenAI:MaxTokens"]);
            string temperatureStr = configuration["OpenAI:Temperature"];
            if (!double.TryParse(temperatureStr, out _temperature))
            {
                _logger.LogError("Invalid temperature format in configuration: {0}", temperatureStr);
                _temperature = 0.0; // or some default value
            }

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(personalDataDialog);
            AddDialog(learningModeDialog);
    

            var waterfallSteps = new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var messageText = stepContext.Options?.ToString() ?? "What can I help you with today?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userMessage = (string)stepContext.Result;

            if (userMessage.ToLower().Contains("learn"))
            {
                return await stepContext.BeginDialogAsync(nameof(LearningModeDialog), null, cancellationToken);
            
           
            }
            else
            {
                var matchingKeys = _knowledgeBase.SearchKeys(userMessage);

                if (matchingKeys.Count > 0)
                {
                    string response = _knowledgeBase.GetResponse(matchingKeys[0]);
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(response), cancellationToken);
                    return await stepContext.NextAsync(null, cancellationToken);
                }
                else
                {
                    var openAiResponse = await GetOpenAiResponseAsync(userMessage, cancellationToken);
                    if (!string.IsNullOrEmpty(openAiResponse))
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text(openAiResponse), cancellationToken);
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text("Desculpe, não entendi sua pergunta."), cancellationToken);
                    }

                    return await stepContext.NextAsync(null, cancellationToken);
                }
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptMessage = "What else can I do for you?";
            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
        }

        private async Task<string> GetOpenAiResponseAsync(string userMessage, CancellationToken cancellationToken)
        {
            using var client = new HttpClient { BaseAddress = new Uri(_openAiEndpoint) };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAiApiKey}");

            var requestBody = new
            {
                model = _model,
                messages = new[] { new { role = "user", content = userMessage } },
                max_tokens = _maxTokens,
                temperature = _temperature
            };

            var requestContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(client.BaseAddress, requestContent);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var jsonObject = JObject.Parse(jsonResponse);
                var messageContent = jsonObject["choices"]?[0]?["message"]?["content"]?.ToString();
                return messageContent;
            }

            return null;
        }
    }
}
