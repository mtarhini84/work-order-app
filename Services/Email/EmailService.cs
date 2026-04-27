using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using WorkOrderApp.Settings;

namespace WorkOrderApp.Services.Email
{
    public class EmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendEmailAsync(EmailTemplateModel model)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(new MailboxAddress(model.ToName, model.ToEmail));
            message.Subject = model.Subject;

            message.Body = new BodyBuilder { HtmlBody = model.HtmlBody }.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.SmtpServer, _settings.Port, SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(_settings.SenderEmail, _settings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
