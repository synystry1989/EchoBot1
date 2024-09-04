//using EchoBot1.Modelos;
//using EchoBot1.Servicos;
//using Microsoft.Bot.Builder;
//using Microsoft.Bot.Schema;
//using Newtonsoft.Json.Linq;
//using Newtonsoft.Json;
//using System.Collections.Generic;
//using System.Net.Http;
//using System.Text;
//using System.Threading.Tasks;
//using System.Threading;
//using System;
//using Microsoft.Extensions.Configuration;
//namespace EchoBot1
//{


//    public class EchoBot1 : ActivityHandler
//    {
//        private readonly IConfiguration _configuration;
//        private readonly KnowledgeBase _knowledgeBase;
//        private readonly IStorageHelper _storageHelper;
//        private const string VimapontoAI = "**VimapontoAI**";

//        public EchoBot1(KnowledgeBase knowledgeBase, IStorageHelper storageHelper, IConfiguration configuration)
//        {
//            _configuration = configuration;
//            _storageHelper = storageHelper;
//            _knowledgeBase = knowledgeBase;
//            _knowledgeBase.LoadResponses("C:\\Users\\synys\\Desktop\\1\\");
//        }

//        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
//        {
//            var userMessage = turnContext.Activity.Text.Trim();
//            var userId = turnContext.Activity.From.Id;
//            var conversationId = turnContext.Activity.Conversation.Id;
//            ChatContext chatContext = null;

//            // Step 1: Check if the user exists in the storage and aggregate messages from all conversations
//            bool userExists = await _storageHelper.UserExistsAsync(userId);

//            if (userExists)
//            {
//                // Initialize a new chat context to aggregate all messages
//                chatContext = new ChatContext()
//                {
//                    Model = _configuration["OpenAI:Model"],
//                    Messages = new List<Message>()
//                };

//                // Load all previous conversations
//                var existingConversationIds = await _storageHelper.GetConversationIdsByUserIdAsync(userId);
//                foreach (var existingConversationId in existingConversationIds)
//                {
//                    var chatContextEntity = await _storageHelper.GetEntityAsync<GptResponseEntity>(_configuration["StorageAcc:GPTContextTable"], userId, existingConversationId);
//                    if (chatContextEntity != null)
//                    {
//                        // Append each message from the previous context to the current chat context
//                        var previousMessages = JsonConvert.DeserializeObject<List<Message>>(chatContextEntity.UserContext);
//                        chatContext.Messages.AddRange(previousMessages);
//                    }
//                }
//            }

//            // Step 2: Initialize a new chat context if no previous context exists
//            if (chatContext == null)
//            {
//                chatContext = new ChatContext()
//                {
//                    Model = _configuration["OpenAI:Model"],
//                    Messages = new List<Message>()
//                };
//            }

//            // Step 3: Process user messages based on chat context
//            switch (userMessage.ToLower())
//            {
//                case "apagar":
//                    await _storageHelper.DeleteEntityAsync(_configuration["StorageAcc:GPTContextTable"], userId, conversationId);
//                    await turnContext.SendActivityAsync(MessageFactory.Text("Conversa Apagada."), cancellationToken);
//                    return;

//                case "reiniciar":
//                    chatContext.Messages.Clear();
//                    chatContext.Messages.Add(new Message() { Role = "user", Content = "Hi" });
//                    break;

//                case "modo aprendizagem":
//                    var botResponse = "Por favor insira o nome do grupo que deseja adicionar à base de conhecimento, seguido da nova base de conhecimento. Formato: Entidade: <entidade> grupo: <grupo> = <chave> = <resposta>";
//                    await turnContext.SendActivityAsync(MessageFactory.Text(botResponse), cancellationToken);
//                    return;

//                default:
//                    // Add the user message to the chat context
//                    chatContext.Messages.Add(new Message() { Role = "user", Content = userMessage });
//                    break;
//            }

//            // Step 4: Handle knowledge base responses or generate OpenAI responses
//            var matchingKeys = _knowledgeBase.SearchKeys(userMessage);
//            if (userMessage.StartsWith("Entidade:", StringComparison.OrdinalIgnoreCase) && userMessage.Contains("grupo:") && userMessage.Contains("="))
//            {
//                try
//                {
//                    var entityStart = userMessage.IndexOf("Entidade:") + "Entidade:".Length;
//                    var groupStart = userMessage.IndexOf("grupo:") + "grupo:".Length;
//                    var keyStart = userMessage.IndexOf("=", groupStart) + 1;
//                    var responseStart = userMessage.IndexOf("=", keyStart) + 1;

//                    if (entityStart > 0 && groupStart > 0 && keyStart > 0 && responseStart > 0)
//                    {
//                        var entidade = userMessage.Substring(entityStart, groupStart - "grupo:".Length - entityStart).Trim();
//                        var grupo = userMessage.Substring(groupStart, keyStart - 1 - groupStart).Trim();
//                        var key = userMessage.Substring(keyStart, responseStart - 1 - keyStart).Trim();
//                        var response = userMessage.Substring(responseStart).Trim();

//                        _knowledgeBase.AddOrUpdateResponse("C:\\Users\\synys\\Desktop\\1\\", entidade, grupo, key, response);
//                        await turnContext.SendActivityAsync(MessageFactory.Text("Base de conhecimento atualizada com sucesso!"), cancellationToken);
//                    }
//                    else
//                    {
//                        await turnContext.SendActivityAsync(MessageFactory.Text("Erro no formato: A entrada não corresponde ao formato esperado."), cancellationToken);
//                    }
//                }
//                catch (Exception ex)
//                {
//                    await turnContext.SendActivityAsync(MessageFactory.Text($"Ocorreu um erro ao processar a entrada: {ex.Message}"), cancellationToken);
//                }
//            }
//            else if (matchingKeys.Count > 0)
//            {
//                string response = _knowledgeBase.GetResponse(matchingKeys[0]);
//                await turnContext.SendActivityAsync(MessageFactory.Text(response), cancellationToken);
//                chatContext.Messages.Add(new Message() { Role = "assistant", Content = response });
//            }
//            else
//            {
//                var openAiResponse = await GetOpenAiResponseAsync(chatContext, cancellationToken);
//                if (!string.IsNullOrEmpty(openAiResponse))
//                {
//                    await turnContext.SendActivityAsync(MessageFactory.Text(openAiResponse), cancellationToken);
//                    chatContext.Messages.Add(new Message() { Role = "assistant", Content = openAiResponse });
//                }
//                else
//                {
//                    await turnContext.SendActivityAsync(MessageFactory.Text("Desculpe, não entendi sua pergunta."), cancellationToken);
//                }
//            }

//            // Step 5: Save updated chat context back to storage
//            await _storageHelper.InsertEntityAsync(_configuration["StorageAcc:GPTContextTable"], new GptResponseEntity()
//            {
//                PartitionKey = userId,
//                RowKey = conversationId,
//                ConversationId = conversationId,
//                UserId = userId,
//                UserContext = JsonConvert.SerializeObject(chatContext.Messages)
//            });
//        }

//        private async Task<string> GetOpenAiResponseAsync(ChatContext chatContext, CancellationToken cancellationToken)
//        {
//            using var client = new HttpClient
//            {
//                BaseAddress = new Uri(_configuration["OpenAI:APIEndpoint"])
//            };
//            client.DefaultRequestHeaders.Accept.Clear();
//            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_configuration["OpenAI:ApiKey"]}");

//            var requestBody = new
//            {
//                model = chatContext.Model,
//                messages = chatContext.Messages,
//                max_tokens = 1000,
//                temperature = 0.5,
//            };

//            var requestContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
//            var response = await client.PostAsync(client.BaseAddress, requestContent);

//            if (response.IsSuccessStatusCode)
//            {
//                var jsonResponse = await response.Content.ReadAsStringAsync();
//                var jsonObject = JObject.Parse(jsonResponse);
//                var messageContent = jsonObject["choices"]?[0]?["message"]?["content"]?.ToString();
//                return messageContent;
//            }
//            return null;
//        }

//        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
//        {
//            foreach (var member in membersAdded)
//            {
//                if (member.Id != turnContext.Activity.Recipient.Id)
//                {
//                    // Check if the user already exists in storage
//                    bool userExists = await _storageHelper.UserExistsAsync(member.Id);

//                    if (userExists)
//                    {
//                        // User already exists, send a "welcome back" message
//                        var welcomeBackMessage = $"Bem-vindo de volta, {member.Name}! Em que posso ajudar hoje?";
//                        await turnContext.SendActivityAsync(MessageFactory.Text(welcomeBackMessage), cancellationToken);
//                    }
//                    else
//                    {
//                        // User is new, send a welcome card
//                        var heroCard = new HeroCard
//                        {
//                            Title = "Bem-vindo ao seu assistente VMPontoAI",
//                            Subtitle = "Ajuda Personalizada:",
//                            Text = "Envio Emails | Realizar encomendas | Ver faturas | Falar c/ Suporte",
//                            Images = new List<CardImage> { new CardImage("C:\\Users\\synys\\Pictures\\Screenshots\\VMP.png") },
//                            Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Os nossos Produtos", value: "https://docs.microsoft.com/bot-framework") },
//                        };

//                        var reply = MessageFactory.Attachment(heroCard.ToAttachment());
//                        await turnContext.SendActivityAsync(reply, cancellationToken);
//                    }
//                }
//            }
//        }
//    }
//}