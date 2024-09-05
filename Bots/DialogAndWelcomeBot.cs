// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.22.0

using EchoBot1.Modelos;
using EchoBot1.Servicos;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EchoBot1.Bots
{
    public class DialogAndWelcomeBot<T> : DialogBot<T>
        where T : Dialog
    {
        private readonly IConfiguration _configuration;
        private readonly IStorageHelper _storageHelper;
        public DialogAndWelcomeBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger, IConfiguration configuration, IStorageHelper storageHelper)
            : base(conversationState, dialog, storageHelper, logger, configuration)
        {
            _configuration = configuration;
            _storageHelper = storageHelper;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {

            var conversationId = turnContext.Activity.Conversation.Id;

            var userId = turnContext.Activity.From.Id;

            ChatContext chatContext = null;

            // Step 1: Check if the user exists in the storage and aggregate messages from all conversations
            bool userExists = await _storageHelper.UserExistsAsync(userId);

            if (userExists)
            {
                // Initialize a new chat context to aggregate all messages
                chatContext = new ChatContext()
                {
                    Model = _configuration["OpenAI:Model"],
                    Messages = new List<Message>()
                };

                // Load all previous conversations
                var existingConversationIds = await _storageHelper.GetPaginatedConversationIdsByUserIdAsync(userId);
                foreach (var existingConversationId in existingConversationIds)
                {
                    var chatContextEntity = await _storageHelper.GetEntityAsync<GptResponseEntity>(_configuration["StorageAcc:GPTContextTable"], userId, existingConversationId);
                    if (chatContextEntity != null)
                    {
                        // Append each message from the previous context to the current chat context
                        var previousMessages = JsonConvert.DeserializeObject<List<Message>>(chatContextEntity.UserContext);
                        chatContext.Messages.AddRange(previousMessages);
                    }
                }
                var welcomeBackMessage = $"Bem-vindo de volta, ! Em que posso ajudar hoje?";
                await turnContext.SendActivityAsync(MessageFactory.Text(welcomeBackMessage), cancellationToken);
            }

            // Step 2: Initialize a new chat context if no previous context exists
            if (chatContext == null)
            {
                chatContext = new ChatContext()
                {
                    Model = _configuration["OpenAI:Model"],
                    Messages = new List<Message>()
                };
            }
            foreach (var member in membersAdded)
            {


                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    // Check if the user already exists in storage
                    userExists = await _storageHelper.UserExistsAsync(turnContext.Activity.Recipient.Id);

                    if (userExists)
                    {
                        // User already exists, send a "welcome back" message


                    }
                    else
                    {
                        // User is new, send a welcome card
                        var heroCard = new HeroCard
                        {
                            Title = "Bem-vindo ao seu assistente VMPontoAI",
                            Subtitle = "Ajuda Personalizada:",
                            Text = "Envio Emails | Realizar encomendas | Ver faturas | Falar c/ Suporte",
                            Images = new List<CardImage> { new CardImage("C:\\Users\\synys\\Pictures\\Screenshots\\VMP.png") },
                            Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Os nossos Produtos", value: "https://docs.microsoft.com/bot-framework") },
                        };

                        var reply = MessageFactory.Attachment(heroCard.ToAttachment());

                        await turnContext.SendActivityAsync(reply, cancellationToken);

                    }
                }
            }
        }
    }
}

