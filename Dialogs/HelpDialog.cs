using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System.Collections.Generic;
using Microsoft.Bot.Builder;

namespace EchoBot1.Dialogs
{
    public class HelpDialog : ComponentDialog
    {
        public HelpDialog() : base(nameof(HelpDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                DisplayHelpTopicsStepAsync,
                HandleHelpSelectionStepAsync,
                EndHelpDialogStepAsync
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> DisplayHelpTopicsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var helpMessage = "Aqui estão alguns tópicos de ajuda disponíveis:";
            var options = new List<string> { "Modos disponíveis", "Funcionalidades", "Outros tópicos" };

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(helpMessage), cancellationToken);

            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("Selecione um tópico para saber mais:"),
                Choices = ChoiceFactory.ToChoices(options)
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> HandleHelpSelectionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var choice = ((FoundChoice)stepContext.Result).Value;

            string helpDetailMessage = choice switch
            {
                "Modos disponíveis" => "Modos disponíveis: Empresarial, Aprendizagem, etc.",
                "Funcionalidades" => "Funcionalidades: Envio de Emails, Realizar Encomendas, etc.",
                "Outros tópicos" => "Outros tópicos: Você pode explorar mais funcionalidades pedindo suporte.",
                _ => "Opção inválida. Tente novamente."
            };

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(helpDetailMessage), cancellationToken);
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> EndHelpDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Encerra o diálogo de ajuda e retorna ao diálogo anterior
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
