namespace EchoBot1.Servicos
{
    using Microsoft.Extensions.Configuration;
    using SendGrid;
    using SendGrid.Helpers.Mail;
    using System.Threading.Tasks;

    public class EmailService
    {
        private readonly ISendGridClient _sendGridClient;
        private readonly string _fromEmail;

        public EmailService(ISendGridClient sendGridClient, IConfiguration configuration)
        {
            _sendGridClient = sendGridClient;
            _fromEmail = configuration["SendGrid:FromEmail"];
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(_fromEmail),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };

            msg.AddTo(new EmailAddress(toEmail));
            await _sendGridClient.SendEmailAsync(msg);
        }
    }
}
