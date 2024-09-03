using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;
using EchoBot1.Servicos;
using Microsoft.Bot.Builder;
using System;
using System.Collections.Generic;

namespace EchoBot1.Dialogs
{
    public class WelcomeBot : ComponentDialog
    {
        public WelcomeBot(UserProfileService userProfileService, PersonalDataDialog personalDataDialog, HelpDialog helpDialog) : base(nameof(WelcomeBot))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                DisplayWelcomeMessageStepAsync,
                CheckForHelpStepAsync,
                CallPersonalDataDialogStepAsync,
                EndWelcomeDialogStepAsync
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(personalDataDialog ?? throw new ArgumentNullException(nameof(personalDataDialog)));
            AddDialog(helpDialog ?? throw new ArgumentNullException(nameof(helpDialog)));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> DisplayWelcomeMessageStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var heroCard = new HeroCard
            {
                Title = "Bem-vindo ao seu assistente VMPontoAI",
                Subtitle = "Ajuda Personalizada:",
                Text = "Envio Emails | Realizar encomendas | Ver faturas | Falar c/ Suporte",
                Images = new List<CardImage> { new CardImage("C:\\Users\\synys\\Pictures\\Screenshots\\VMP.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Os nossos Produtos", value: "https://docs.microsoft.com/bot-framework") },
            };
            var attachments = new List<Attachment>();
            var reply = MessageFactory.Attachment(attachments);
            reply.Attachments.Add(heroCard.ToAttachment());

            await stepContext.Context.SendActivityAsync(reply, cancellationToken);

            var promptMessage = "Se precisar de ajuda, digite 'help'. Agora, vamos continuar com a coleta de seus dados pessoais.";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(promptMessage), cancellationToken);

            // Passa para o próximo passo
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> CheckForHelpStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Verifica se o usuário pediu ajuda digitando "help"
            var userInput = stepContext.Context.Activity.Text?.ToLower();

            if (userInput == "help")
            {
                // Inicia o diálogo de ajuda
                return await stepContext.BeginDialogAsync(nameof(HelpDialog), null, cancellationToken);
            }

            // Continua para o próximo passo se "help" não for digitado
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> CallPersonalDataDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Inicia o diálogo para coleta de dados pessoais
            return await stepContext.BeginDialogAsync(nameof(PersonalDataDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> EndWelcomeDialogStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // O WelcomeBot termina e passa o controle de volta para o MainDialog
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
