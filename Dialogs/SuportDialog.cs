namespace bot.Dialogs
{

    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Choices;
    using Microsoft.Bot.Schema;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class SupportDialog : ComponentDialog
    {
        public SupportDialog() : base(nameof(SupportDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
            AskForSupportTopicStepAsync,
            OfferLiveSupportStepAsync,
            ConfirmSupportRequestStepAsync
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AskForSupportTopicStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Qual é o assunto do suporte?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> OfferLiveSupportStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["supportTopic"] = (string)stepContext.Result;

            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Você deseja suporte ao vivo?"),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Sim", "Não" })
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmSupportRequestStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var choice = ((FoundChoice)stepContext.Result).Value;
            var supportTopic = (string)stepContext.Values["supportTopic"];

            if (choice.ToLower() == "sim")
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Um de nossos assistentes entrará em contato em breve para suporte ao vivo."), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Você solicitou suporte para: {supportTopic}. Um de nossos assistentes entrará em contato em breve."), cancellationToken);
            }

            return await stepContext.ReplaceDialogAsync(nameof(MainMenuDialog), null, cancellationToken);

        }
    }

}


