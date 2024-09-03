using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;
using EchoBot1.Servicos;
using System;
using Microsoft.Bot.Builder;

namespace EchoBot1.Dialogs
{
    public class EmailDialog : ComponentDialog
    {
        private readonly EmailService _emailService;

        public EmailDialog(EmailService emailService) : base(nameof(EmailDialog))
        {
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));

            var waterfallSteps = new WaterfallStep[]
            {
                AskForEmailDetailsStepAsync,
                SendEmailStepAsync,
                FinalStepAsync
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AskForEmailDetailsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptMessage = "Para quem você gostaria de enviar o email?";
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(promptMessage) }, cancellationToken);
        }

        private async Task<DialogTurnResult> SendEmailStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Result is string emailAddress && !string.IsNullOrWhiteSpace(emailAddress))
            {
                // Lógica para enviar email aqui
                await _emailService.SendEmailAsync(emailAddress, "Assunto do Email", "Conteúdo do email.");
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Email enviado para {emailAddress}."), cancellationToken);
                return await stepContext.NextAsync(null, cancellationToken);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Endereço de email inválido. Tente novamente."), cancellationToken);
            return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
