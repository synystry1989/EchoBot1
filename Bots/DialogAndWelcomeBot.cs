using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EchoBot1.Servicos;

namespace EchoBot1.Bots
{
    public class DialogAndWelcomeBot<T> : ActivityHandler where T : Dialog
    {
        private readonly BotState _conversationState;
        private readonly BotState _userState;
        private readonly Dialog _dialog;
        private readonly IStorageHelper _storageHelper;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public DialogAndWelcomeBot(ConversationState conversationState, UserState userState, T dialog, IStorageHelper storageHelper, ILogger<DialogBot<T>> logger, IConfiguration configuration)
        {
            _conversationState = conversationState;
            _userState = userState;
            _dialog = dialog;
            _storageHelper = storageHelper;
            _logger = logger;
            _configuration = configuration;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occurred during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    // Check if the user exists in the UserProfiles table
                    bool userExists = await _storageHelper.UserExistsAsync(member.Id, _configuration["StorageAcc:UserProfileTable"]);

                    if (userExists)
                    {
                        // User already exists, send a "welcome back" message
                        var welcomeBackMessage = $"Bem-vindo de volta, {member.Name}! Em que posso ajudar hoje?";
                        await turnContext.SendActivityAsync(MessageFactory.Text(welcomeBackMessage), cancellationToken);
                    }
                    else
                    {
                        // User is new, start the personal data dialog to collect user information
                        await _dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
                    }
                }
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Running dialog with Message Activity.");

            // Run the MainDialog with the new message Activity
            await _dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
        }

        private Attachment CreateAdaptiveCardAttachment()
        {
            // Implement your adaptive card loading logic here
            var cardResourcePath = GetType().Assembly.GetManifestResourceNames().First(name => name.EndsWith("welcomeCard.json"));

            using (var stream = GetType().Assembly.GetManifestResourceStream(cardResourcePath))
            {
                using (var reader = new StreamReader(stream))
                {
                    var adaptiveCard = reader.ReadToEnd();
                    return new Attachment()
                    {
                        ContentType = "application/vnd.microsoft.card.adaptive",
                        Content = Newtonsoft.Json.JsonConvert.DeserializeObject(adaptiveCard, new Newtonsoft.Json.JsonSerializerSettings { MaxDepth = null }),
                    };
                }
            }
        }
    }
}
