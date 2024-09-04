using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;
using EchoBot1.Modelos;
using Microsoft.Extensions.Configuration;
using System;

namespace EchoBot1.Dialogs
{
    public class LearningModeDialog : ComponentDialog
    {
        private readonly KnowledgeBase _knowledgeBase;
        private readonly string _knowledgeBasePath;

        public LearningModeDialog(KnowledgeBase knowledgeBase, IConfiguration configuration)
            : base(nameof(LearningModeDialog))
        {
            _knowledgeBase = knowledgeBase;
            _knowledgeBasePath = configuration["KnowledgeBasePath:path"];

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                LearningStepAsync,
                FinalStepAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> LearningStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptMessage = "Please provide the data you want to add to the knowledge base in the format: Entity: <entity> Group: <group> = <key> = <response>";
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(promptMessage) }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userInput = (string)stepContext.Result;

            // Parse user input to extract entity, group, key, and response
            var parts = userInput.Split(new[] { "Entity:", "Group:", "=", "=" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 4)
            {
                var entity = parts[0].Trim();
                var group = parts[1].Trim();
                var key = parts[2].Trim();
                var response = parts[3].Trim();

                // Update the knowledge base
                _knowledgeBase.AddOrUpdateResponse(_knowledgeBasePath, entity, group, key, response);

                await stepContext.Context.SendActivityAsync(MessageFactory.Text("The knowledge base has been updated."), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Invalid input format. Please try again."), cancellationToken);
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
