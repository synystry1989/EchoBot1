namespace bot.Dialogs
{
    using bot.Dialogs.Bot.Dialogs;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;
    using SendGrid;
    using SendGrid.Helpers.Mail;
    using System.Threading;
    using System.Threading.Tasks;

    public class EmailDialog : ComponentDialog
    {
        private readonly string _sendGridApiKey;
        private readonly string _fromEmail;

        public EmailDialog(string sendGridApiKey, string fromEmail) : base(nameof(EmailDialog))
        {
            _sendGridApiKey = sendGridApiKey;
            _fromEmail = fromEmail;

            var waterfallSteps = new WaterfallStep[]
            {
            AskForRecipientEmailStepAsync,
            AskForEmailSubjectStepAsync,
            AskForEmailBodyStepAsync,
            SendEmailStepAsync
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AskForRecipientEmailStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Para qual email você deseja enviar?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> AskForEmailSubjectStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["recipientEmail"] = (string)stepContext.Result;
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Qual é o assunto do email?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> AskForEmailBodyStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["emailSubject"] = (string)stepContext.Result;
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Qual é o conteúdo do email?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> SendEmailStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var recipientEmail = (string)stepContext.Values["recipientEmail"];
            var emailSubject = (string)stepContext.Values["emailSubject"];
            var emailBody = (string)stepContext.Result;

            var client = new SendGridClient(_sendGridApiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(_fromEmail, "Seu Nome"),
                Subject = emailSubject,
                PlainTextContent = emailBody,
                HtmlContent = emailBody
            };
            msg.AddTo(new EmailAddress(recipientEmail));

            var response = await client.SendEmailAsync(msg);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Email enviado para {recipientEmail}."), cancellationToken);

            return await stepContext.ReplaceDialogAsync(nameof(MainMenuDialog), null, cancellationToken);

        }
    }

}
