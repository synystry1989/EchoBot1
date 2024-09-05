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
        protected readonly IConfiguration _configuration;
        protected readonly Dialog Dialog;
        protected readonly BotState ConversationState;
        protected readonly BotState UserState;
        protected readonly ILogger Logger;
        protected readonly IStorageHelper _storageHelper;

        public DialogBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger, IStorageHelper storageHelper)
        {
            _storageHelper = storageHelper;
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
            Logger = logger;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occurred during the turn.

            // Step 5: Save updated chat context back to storage
           
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
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
                var existingConversationIds = await _storageHelper.GetConversationIdsByUserIdAsync(userId);
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
           

            Logger.LogInformation("Running dialog with Message Activity.");

            // Run the Dialog with the new message Activity.
            await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
        }
    }
}
