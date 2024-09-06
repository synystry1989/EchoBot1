using EchoBot1.Modelos;
using EchoBot1.Servicos;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EchoBot1.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly KnowledgeBase _knowledgeBase;
       
        private readonly IStorageHelper _storageHelper;
        private readonly IConfiguration _configuration;

        public MainDialog(PersonalDataDialog personalDataDialog, LearningModeDialog learningModeDialog, KnowledgeBase knowledgeBase, ILogger<MainDialog> logger, IConfiguration configuration, IStorageHelper storageHelper)
            : base(nameof(MainDialog))
        {
            _storageHelper = storageHelper;
            _configuration = configuration;
            _knowledgeBase = knowledgeBase;



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

            List<string> users = await _storageHelper.GetPaginatedUserIdsAsync();
            
            PersonalDataEntity userProfile = new PersonalDataEntity();
            
            foreach (var user in users)
            {
                if (user == stepContext.Context.Activity.From.Id)
                {
                    userProfile.Id = stepContext.Context.Activity.From.Id;
                    // Tenta buscar o nome do usuário a partir do PersonalDataEntity
                    userProfile.Name = await _storageHelper.GetUserNameAsync(userProfile.Id) ?? "defaultName";
                }
            }

            if (userProfile.Name == "defaultName")
            {
                // Inicia o PersonalDataDialog se o usuário for novo
                return await stepContext.BeginDialogAsync(nameof(PersonalDataDialog), null, cancellationToken);
            }
            else
            {

                // Mensagem personalizada com o nome do usuário
                var messageText = stepContext.Options?.ToString() ?? $"Em que posso ser útil, {userProfile.Name}?";
                var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }
        }

        // Método auxiliar para obter o nome do usuário do PersonalDataEntity


        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userId = stepContext.Context.Activity.From.Id;
            var conversationId = stepContext.Context.Activity.Conversation.Id;
            var userMessage = (string)stepContext.Result;
            var chatContext = stepContext.Options as ChatContext ?? new ChatContext
            {
                Model = _configuration["OpenAI:Model"],
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
                await _storageHelper.SaveChatContextToStorageAsync(_configuration["StorageAcc:GPTContextTable"], userId, conversationId, chatContext);

                return await stepContext.NextAsync(chatContext, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var conversationId = stepContext.Context.Activity.Conversation.Id;
            var chatContext = stepContext.Result as ChatContext;
            var promptMessage = "O que mais posso fazer por você?";
            var userId = stepContext.Context.Activity.From.Id;
            // Registra a mensagem do assistente
            chatContext.Messages.Add(new Message { Role = "assistant", Content = promptMessage });

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(promptMessage), cancellationToken);

            // Salva o contexto de chat atualizado
            await _storageHelper.SaveChatContextToStorageAsync(_configuration["StorageAcc:GPTContextTable"], userId, conversationId, chatContext);

            return await stepContext.ReplaceDialogAsync(InitialDialogId, chatContext, cancellationToken);
        }

        private async Task<string> GetOpenAiResponseAsync(string userMessage, CancellationToken cancellationToken)
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri(_configuration["OpenAI:APIEndpoint"])
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Authorization", _configuration["OpenAI:ApiKey"]);

            var requestBody = new
            {
                model = _configuration["OpenAI:Model"],
                messages = new[] { new { role = "user", content = userMessage } },
                max_tokens = _configuration["OpenAI:MaxTokens"],
                temperature = _configuration["OpenAI:Temperature"],
                
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
