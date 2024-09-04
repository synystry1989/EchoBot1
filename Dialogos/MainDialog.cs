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
using System;
using System.Collections.Generic;
using EchoBot1.Bots;
using EchoBot1.Servicos;

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
        private readonly IStorageHelper _storageHelper;
        private readonly IConfiguration _configuration;

        public MainDialog(
            PersonalDataDialog personalDataDialog,
            LearningModeDialog learningModeDialog,
            KnowledgeBase knowledgeBase,
            ILogger<MainDialog> logger,
            IConfiguration configuration,
            IStorageHelper storageHelper)
            : base(nameof(MainDialog))
        {
            _knowledgeBase = knowledgeBase;
            _logger = logger;
            _openAiEndpoint = configuration["OpenAI:APIEndpoint"];
            _openAiApiKey = configuration["OpenAI:ApiKey"];
            _model = configuration["OpenAI:Model"];
            _maxTokens = int.Parse(configuration["OpenAI:MaxTokens"]);
            _configuration = configuration;
            _storageHelper = storageHelper;

            if (!double.TryParse(configuration["OpenAI:Temperature"], out _temperature))
            {
                _logger.LogError("Invalid temperature format in configuration.");
                _temperature = 0.5;
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
            var userId = stepContext.Context.Activity.From.Id;

            // Verifica se o usuário existe no armazenamento
            bool userExists = await _storageHelper.UserExistsAsync(userId);

            if (!userExists)
            {
                // Inicia o PersonalDataDialog se o usuário for novo
                return await stepContext.BeginDialogAsync(nameof(PersonalDataDialog), null, cancellationToken);
            }

            // Tenta buscar o nome do usuário a partir do PersonalDataEntity
            string userName = await GetUserNameAsync(userId) ?? "usuário";

            // Mensagem personalizada com o nome do usuário
            var messageText = stepContext.Options?.ToString() ?? $"Em que posso ser útil, {userName}?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        // Método auxiliar para obter o nome do usuário do PersonalDataEntity
        private async Task<string> GetUserNameAsync(string userId)
        {
            try
            {
                var personalData = await _storageHelper.GetEntityAsync<PersonalDataEntity>(_configuration["StorageAcc:UserProfileTable"], userId, userId);
                return personalData?.Name;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao buscar o nome do usuário: {ex.Message}");
                return null;
            }
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userMessage = (string)stepContext.Result;
            var chatContext = stepContext.Options as ChatContext ?? new ChatContext
            {
                Model = _model,
                Messages = new List<Message>()
            };

            if (userMessage.ToLower().Contains("learn"))
            {
                return await stepContext.BeginDialogAsync(nameof(LearningModeDialog), null, cancellationToken);
            }
            else if (userMessage.ToLower().Contains("personal data"))
            {
                return await stepContext.BeginDialogAsync(nameof(PersonalDataDialog), null, cancellationToken);
            }
            else
            {
                var matchingKeys = _knowledgeBase.SearchKeys(userMessage);
                string response;

                if (matchingKeys.Count > 0)
                {
                    response = _knowledgeBase.GetResponse(matchingKeys[0]);
                }
                else
                {
                    response = await GetOpenAiResponseAsync(userMessage, cancellationToken) ?? "Desculpe, não entendi sua pergunta.";
                }

                // Registra a resposta do assistente
                chatContext.Messages.Add(new Message { Role = "assistant", Content = response });
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(response), cancellationToken);

                // Salva o contexto de chat atualizado
                await SaveChatContextAsync(stepContext, chatContext);

                return await stepContext.NextAsync(chatContext, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var chatContext = stepContext.Result as ChatContext;
            var promptMessage = "O que mais posso fazer por você?";

            // Registra a mensagem do assistente
            chatContext.Messages.Add(new Message { Role = "assistant", Content = promptMessage });

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(promptMessage), cancellationToken);

            // Salva o contexto de chat atualizado
            await SaveChatContextAsync(stepContext, chatContext);

            return await stepContext.ReplaceDialogAsync(InitialDialogId, chatContext, cancellationToken);
        }

        private async Task SaveChatContextAsync(WaterfallStepContext stepContext, ChatContext chatContext)
        {
            var userId = stepContext.Context.Activity.From.Id;
            var conversationId = stepContext.Context.Activity.Conversation.Id;
            await _storageHelper.SaveChatContextToStorageAsync(_configuration["StorageAcc:GPTContextTable"], userId, conversationId, chatContext);
        }

        private async Task<string> GetOpenAiResponseAsync(string userMessage, CancellationToken cancellationToken)
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri(_openAiEndpoint)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAiApiKey}");

            var requestBody = new
            {
                model = _model,
                messages = new[] { new { role = "user", content = userMessage } },
                max_tokens = _maxTokens,
                temperature = _temperature,
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
