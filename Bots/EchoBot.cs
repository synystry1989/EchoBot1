using EchoBot1.Modelos;
using EchoBot1.Servicos;

namespace EchoBot1.Bots
{
    public class EchoBot : ActivityHandler
    {
        private readonly IConfiguration _configuration;
        private readonly KnowledgeBase _knowledgeBase;
        private readonly IStorageHelper _storageHelper;
        private const string VimapontoAI = "**VimapontoAI**";

        public EchoBot(KnowledgeBase knowledgeBase, IStorageHelper storageHelper, IConfiguration configuration)
        {
            _configuration = configuration;
            _storageHelper = storageHelper;
            _knowledgeBase = knowledgeBase;
            _knowledgeBase.LoadResponses("C:\\Users\\synys\\Desktop\\1\\");
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var userMessage = turnContext.Activity.Text.Trim();
            ChatContext chatContext = null;
            string botResponse = string.Empty;

            switch (userMessage.ToLower())
            {
                case "apagar":
                    await _storageHelper.DeleteEntityAsync(_configuration["StorageAcc:GPTContextTable"], turnContext.Activity.From.Id, turnContext.Activity.Conversation.Id);
                    await turnContext.SendActivityAsync(MessageFactory.Text("Conversa Apagada."), cancellationToken);
                    return;

                case "reiniciar":
                    chatContext = new ChatContext()
                    {
                        Model = _configuration["OpenAI:Model"],
                        Messages = new List<Message>()
                        {
                            new Message() { Role = "user", Content = "Hi" }
                        }
                    };
                    break;

                case "modo aprendizagem":
                    botResponse = "Por favor insira o nome do grupo que deseja adicionar à base de conhecimento, seguido da nova base de conhecimento. Formato: Entidade: <entidade> grupo: <grupo> = <chave> = <resposta>";
                    await turnContext.SendActivityAsync(MessageFactory.Text(botResponse), cancellationToken);
                    return;

                default:
                    var chatContextEntity = await _storageHelper.GetEntityAsync<GptResponseEntity>(_configuration["StorageAcc:GPTContextTable"], turnContext.Activity.From.Id, turnContext.Activity.Conversation.Id);

                    if (chatContextEntity == null)
                    {
                        chatContext = new ChatContext()
                        {
                            Model = _configuration["OpenAI:Model"],
                            Messages = new List<Message>()
                            {
                                new Message() { Role = "user", Content = userMessage }
                            }
                        };
                    }
                    else
                    {
                        chatContext = new ChatContext
                        {
                            Messages = JsonConvert.DeserializeObject<List<Message>>(chatContextEntity.UserContext),
                            Model = _configuration["OpenAI:Model"]
                        };
                        chatContext.Messages.Add(new Message() { Role = "user", Content = userMessage });
                    }
                    break;
            }
            var matchingKeys = _knowledgeBase.SearchKeys(userMessage);



            if (userMessage.StartsWith("Entidade:", StringComparison.OrdinalIgnoreCase) && userMessage.Contains("grupo:") && userMessage.Contains("="))
            {
                try
                {
                    // Improved input parsing using index-based extraction
                    var entityStart = userMessage.IndexOf("Entidade:") + "Entidade:".Length;
                    var groupStart = userMessage.IndexOf("grupo:") + "grupo:".Length;
                    var keyStart = userMessage.IndexOf("=", groupStart) + 1;
                    var responseStart = userMessage.IndexOf("=", keyStart) + 1;

                    if (entityStart > 0 && groupStart > 0 && keyStart > 0 && responseStart > 0)
                    {
                        var entidade = userMessage.Substring(entityStart, groupStart - "grupo:".Length - entityStart).Trim();
                        var grupo = userMessage.Substring(groupStart, keyStart - 1 - groupStart).Trim();
                        var key = userMessage.Substring(keyStart, responseStart - 1 - keyStart).Trim();
                        var response = userMessage.Substring(responseStart).Trim();

                        _knowledgeBase.AddOrUpdateResponse("C:\\Users\\synys\\Desktop\\1\\", entidade, grupo, key, response);

                        await turnContext.SendActivityAsync(MessageFactory.Text("Base de conhecimento atualizada com sucesso!"), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text("Erro no formato: A entrada não corresponde ao formato esperado."), cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    // Handle unexpected errors during parsing
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Ocorreu um erro ao processar a entrada: {ex.Message}"), cancellationToken);
                }
            }
            else if (matchingKeys.Count > 0)
            {
                string response = null;




                response = _knowledgeBase.GetResponse(matchingKeys[0]);



                await turnContext.SendActivityAsync(MessageFactory.Text(response), cancellationToken);
                chatContext.Messages.Add(new Message() { Role = "assistant", Content = response });
            }
            else
            {
                // If no response found in knowledge base, call OpenAI API
                var openAiResponse = await GetOpenAiResponseAsync(chatContext, cancellationToken);

                if (!string.IsNullOrEmpty(openAiResponse))
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(openAiResponse), cancellationToken);
                    chatContext.Messages.Add(new Message() { Role = "assistant", Content = openAiResponse });
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Desculpe, não entendi sua pergunta."), cancellationToken);
                }
            }


            // Save the chat context to Azure Table Storage
            await _storageHelper.InsertEntityAsync(_configuration["StorageAcc:GPTContextTable"], new GptResponseEntity()
            {
                PartitionKey = turnContext.Activity.From.Id,
                RowKey = turnContext.Activity.Conversation.Id,
                UserContext = JsonConvert.SerializeObject(chatContext.Messages)
            });
        }


        private async Task<string> GetOpenAiResponseAsync(ChatContext chatContext, CancellationToken cancellationToken)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(_configuration["OpenAI:APIEndpoint"])
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_configuration["OpenAI:ApiKey"]}");

            var requestBody = new
            {
                model = chatContext.Model,
                messages = chatContext.Messages,
                max_tokens = 1000,
                temperature = 0.5,
            };

            var requestContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(client.BaseAddress, requestContent);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var jsonObject = JObject.Parse(jsonResponse);

                // Extract the actual text response from the OpenAI response JSON
                var messageContent = jsonObject["choices"]?[0]?["message"]?["content"]?.ToString();
                return messageContent;
            }
            return null;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var heroCard = new HeroCard
            {
                Title = "  Bem vindo ao seu assistente VMPontoAI",
                Subtitle = "Ajuda Personalizada:",
                Text = "Envio Emails | Realizar encomendas | Ver faturas | Falar c/ Suporte",
                Images = new List<CardImage> { new CardImage("C:\\Users\\synys\\Pictures\\Screenshots\\VMP.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Os nossos Produtos", value: "https://docs.microsoft.com/bot-framework") },
            };
            var attachments = new List<Attachment>();

            // Reply to the activity we received with an activity.
            var reply = MessageFactory.Attachment(attachments);

            reply.Attachments.Add(heroCard.ToAttachment());


            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(reply, cancellationToken);
                    // await turnContext.SendActivityAsync(MessageFactory.Text($"{member.Name},{welcomeText}", welcomeText), cancellationToken);
                }
            }
        }
    }
}
