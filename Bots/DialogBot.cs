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
    // This IBot implementation can run any type of Dialog. The use of type parameterization is to allows multiple different bots
    // to be run at different endpoints within the same project. This can be achieved by defining distinct Controller types
    // each with dependency on distinct IBot types, this way ASP Dependency Injection can glue everything together without ambiguity.
    // The ConversationState is used by the Dialog system. The UserState isn't, however, it might have been used in a Dialog implementation,
    // and the requirement is that all BotState objects are saved at the end of a turn.
    public class DialogBot<T> : ActivityHandler
        where T : Dialog
    {

        private readonly ConversationState _conversationState;
        private readonly Dialog _dialog;
        private readonly IStorageHelper _storageHelper;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IStatePropertyAccessor<DialogState> _dialogStateAccessor;
        private readonly IStatePropertyAccessor<ChatContext> _chatContextAccessor;

        public DialogBot(ConversationState conversationState, T dialog, IStorageHelper storageHelper, ILogger logger, IConfiguration configuration)
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
               _storageHelper.AddMessageToChatContext(chatContext, "user", turnContext.Activity.Text);

                // Execute the main dialog
                await _dialog.RunAsync(turnContext, _dialogStateAccessor, cancellationToken);

                // Save bot's response to ChatContext
                var botResponse = turnContext.Activity.AsMessageActivity()?.Text;
                if (!string.IsNullOrEmpty(botResponse))
                {
                   _storageHelper.AddMessageToChatContext(chatContext, "assistant", botResponse);
                }

                // Save chat context to persistent storage
                await _storageHelper.SaveChatContextToStorageAsync(_configuration["StorageAcc:GPTContextTable"], userId, conversationId, chatContext);
            }

            // Save any state changes that might have occurred during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }



        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {   
          
            _logger.LogInformation("Running dialog with Message Activity.");

            // Run the Dialog with the new message Activity.
            await _dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
        }
    }
}
