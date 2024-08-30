namespace bot.Dialogs
{
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using System.Threading;
    using System.Threading.Tasks;

    public class PersonalDataDialog : ComponentDialog
    {
        public PersonalDataDialog() : base(nameof(PersonalDataDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
            AskForPersonalDataStepAsync,
            ProcessPersonalDataStepAsync,
            FinalStepAsync
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
        }

        private async Task<DialogTurnResult> AskForPersonalDataStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Quais dados pessoais você gostaria de alterar?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> ProcessPersonalDataStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var personalData = (string)stepContext.Result;
            // Lógica para processar a alteração dos dados pessoais aqui
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Os dados pessoais foram atualizados para: {personalData}."), cancellationToken);
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Seus dados pessoais foram atualizados com sucesso."), cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }

}
