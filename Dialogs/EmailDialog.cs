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
                AskForEmailAddressStepAsync,
                AskForEmailSubjectStepAsync,
                AskForEmailContentStepAsync,
                SendEmailStepAsync,
                FinalStepAsync
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AskForEmailAddressStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptMessage = "Para quem você gostaria de enviar o email?";
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(promptMessage) }, cancellationToken);
        }

        private async Task<DialogTurnResult> AskForEmailSubjectStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["emailAddress"] = (string)stepContext.Result;
            var promptMessage = "Qual é o assunto do email?";
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(promptMessage) }, cancellationToken);
        }

        private async Task<DialogTurnResult> AskForEmailContentStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["emailSubject"] = (string)stepContext.Result;
            var promptMessage = "Qual é o conteúdo do email?";
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(promptMessage) }, cancellationToken);
        }

        private async Task<DialogTurnResult> SendEmailStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["emailContent"] = (string)stepContext.Result;

            var emailAddress = (string)stepContext.Values["emailAddress"];
            var emailSubject = (string)stepContext.Values["emailSubject"];
            var emailContent = (string)stepContext.Values["emailContent"];

            if (!IsValidEmail(emailAddress))
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Endereço de email inválido. Tente novamente."), cancellationToken);
                return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
            }

            try
            {
                await _emailService.SendEmailAsync(emailAddress, emailSubject, emailContent);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Email enviado para {emailAddress}."), cancellationToken);
            }
            catch (Exception ex)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Erro ao enviar o email: {ex.Message}"), cancellationToken);
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private bool IsValidEmail(string email)
        {
            // Simples validação de email
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
