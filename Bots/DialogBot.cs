﻿// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.22.0

using EchoBot1.Modelos;
using EchoBot1.Servicos;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
        protected readonly Dialog Dialog;
        protected readonly BotState ConversationState;
        protected readonly BotState UserState;
        protected readonly ILogger Logger;
        private readonly IConfiguration _configuration;
    
        private readonly IStorageHelper _storageHelper;
      

        public DialogBot( IStorageHelper storageHelper, IConfiguration configuration,ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
        {
            _configuration = configuration;
            _storageHelper = storageHelper;         
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
            Logger = logger;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {

            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
   
            var userId = turnContext.Activity.From.Id;
            var conversationId = turnContext.Activity.Conversation.Id;
            ChatContext chatContext = new ChatContext();

          await _storageHelper.InitializeChatContextAsync( userId);
           // Step 5: Save updated chat context back to storage
            await _storageHelper.InsertEntityAsync(_configuration["StorageAcc:GPTContextTable"], new GptResponseEntity()
            {
                PartitionKey = userId,
                RowKey = conversationId,
                ConversationId = conversationId,
                UserId = userId,
                UserContext = JsonConvert.SerializeObject(chatContext.Messages)
            });


            Logger.LogInformation("Running dialog with Message Activity.");

            // Run the Dialog with the new message Activity.
            await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
        }
    }
}
