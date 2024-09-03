using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;

namespace EchoBot1.Dialogs
{
    public class HelpDialog : ComponentDialog
    {
        public HelpDialog()
            : base(nameof(HelpDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ShowHelpAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> ShowHelpAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var helpMessage = "Você pode entrar em modo de aprendizagem digitando 'modo aprendizagem'.";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(helpMessage), cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
