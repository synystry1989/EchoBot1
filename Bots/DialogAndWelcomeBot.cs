using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EchoBot1.Servicos;
using EchoBot1.Modelos;
using Newtonsoft.Json;
using Azure.Data.Tables;
using EchoBot1.Dialogs;

namespace EchoBot1.Bots
{
    public class DialogAndWelcomeBot<T> : ActivityHandler where T : Dialog
    {
        private readonly ConversationState _conversationState;
        private readonly Dialog _dialog;
        private readonly IStorageHelper _storageHelper;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IStatePropertyAccessor<DialogState> _dialogStateAccessor;
        private readonly IStatePropertyAccessor<ChatContext> _chatContextAccessor;

        public DialogAndWelcomeBot(
            ConversationState conversationState,
            T dialog,
            IStorageHelper storageHelper,
            ILogger<DialogAndWelcomeBot<T>> logger,
            IConfiguration configuration)
        {
            _conversationState = conversationState;
            _dialog = dialog;
            _storageHelper = storageHelper;
            _logger = logger;
            _configuration = configuration;

            _dialogStateAccessor = _conversationState.CreateProperty<DialogState>("DialogState");
            _chatContextAccessor = _conversationState.CreateProperty<ChatContext>("ChatContext");
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            // Load state properties
            var dialogState = await _dialogStateAccessor.GetAsync(turnContext, () => new DialogState(), cancellationToken);
            var chatContext = await _chatContextAccessor.GetAsync(turnContext, () => new ChatContext(), cancellationToken);
            var userId = turnContext.Activity.From.Id;
            var conversationId = turnContext.Activity.Conversation.Id;

            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                // Save user's message to ChatContext
                AddMessageToChatContext(chatContext, "user", turnContext.Activity.Text);

                // Execute the main dialog
                await _dialog.RunAsync(turnContext, _dialogStateAccessor, cancellationToken);

                // Save bot's response to ChatContext
                var botResponse = turnContext.Activity.AsMessageActivity()?.Text;
                if (!string.IsNullOrEmpty(botResponse))
                {
                    AddMessageToChatContext(chatContext, "assistant", botResponse);
                }

                // Save chat context to persistent storage
                await _storageHelper.SaveChatContextToStorageAsync(_configuration["StorageAcc:GPTContextTable"], userId, conversationId, chatContext);
            }

            // Save any state changes that might have occurred during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var userId = member.Id;
                    var chatContext = await InitializeChatContextAsync(userId);
                    bool userExists = await _storageHelper.UserExistsAsync(userId);

                    if (userExists)
                    {
                        // Welcome back message for existing user
                        var welcomeBackMessage = $"Bem-vindo de volta, {member.Name}! Em que posso ajudar hoje?";
                        AddMessageToChatContext(chatContext, "assistant", welcomeBackMessage);
                        await turnContext.SendActivityAsync(MessageFactory.Text(welcomeBackMessage), cancellationToken);
                    }
                    else
                    {
                        // New user: show hero card and start WelcomeDialog
                        await ShowHeroCardAsync(turnContext, cancellationToken);

                        var personalDataMessage = "Vamos começar a coletar alguns dados pessoais.";
                        AddMessageToChatContext(chatContext, "assistant", personalDataMessage);
                        await turnContext.SendActivityAsync(MessageFactory.Text(personalDataMessage), cancellationToken);

                        // Start the WelcomeDialog for new users
                        var dialogSet = new DialogSet(_dialogStateAccessor);
                        dialogSet.Add(_dialog);
                        var dialogContext = await dialogSet.CreateContextAsync(turnContext, cancellationToken);
                        await dialogContext.BeginDialogAsync(nameof(PersonalDataDialog), null, cancellationToken);
                    }

                    // Save the updated chat context after welcome message or card
                    await _storageHelper.SaveChatContextToStorageAsync(_configuration["StorageAcc:GPTContextTable"], userId, turnContext.Activity.Conversation.Id, chatContext);
                }
            }
        }

        private async Task ShowHeroCardAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
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

        private async Task<ChatContext> InitializeChatContextAsync(string userId)
        {
            bool userExists = await _storageHelper.UserExistsAsync(userId);
            ChatContext chatContext = new ChatContext
            {
                Messages = new List<Message>()
            };

            if (userExists)
            {
                var conversationIds = await GetPaginatedConversationIdsByUserIdAsync(userId);

                foreach (var conversationId in conversationIds)
                {
                    var chatContextEntity = await _storageHelper.GetEntityAsync<GptResponseEntity>(_configuration["StorageAcc:GPTContextTable"], userId, conversationId);
                    if (chatContextEntity != null)
                    {
                        var previousMessages = JsonConvert.DeserializeObject<List<Message>>(chatContextEntity.UserContext);
                        chatContext.Messages.AddRange(previousMessages);
                    }
                }
            }

            return chatContext;
        }

        private void AddMessageToChatContext(ChatContext chatContext, string role, string content)
        {
            chatContext.Messages.Add(new Message { Role = role, Content = content });
        }

        private async Task<List<string>> GetPaginatedConversationIdsByUserIdAsync(string userId)
        {
            var conversationIds = new List<string>();
            var tableClient = await _storageHelper.GetTableClient("UserContext");

            var query = tableClient.QueryAsync<GptResponseEntity>(filter: $"PartitionKey eq '{userId}'");

            await foreach (var entity in query)
            {
                conversationIds.Add(entity.RowKey);
            }

            return conversationIds;
        }
    }
}
