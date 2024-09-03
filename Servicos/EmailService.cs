using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

namespace EchoBot1.Servicos
{
    public class EmailService
    {
        private readonly ISendGridClient _sendGridClient;
        private readonly string _fromEmail;

        public EmailService(ISendGridClient sendGridClient, IConfiguration configuration)
        {
            _sendGridClient = sendGridClient ?? throw new ArgumentNullException(nameof(sendGridClient));
            _fromEmail = configuration["SendGrid:FromEmail"] ?? throw new ArgumentNullException(nameof(configuration), "From email configuration is missing.");
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("Recipient email cannot be null or empty.", nameof(toEmail));

            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException("Subject cannot be null or empty.", nameof(subject));

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be null or empty.", nameof(message));

            var msg = new SendGridMessage()
            {
                From = new EmailAddress(_fromEmail),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };

            msg.AddTo(new EmailAddress(toEmail));

            try
            {
                var response = await _sendGridClient.SendEmailAsync(msg);
                if (response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    throw new Exception($"Failed to send email. Status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it according to your needs
                Console.WriteLine($"Error sending email: {ex.Message}");
                throw; // rethrow the exception after logging it
            }
        }
    }
}
