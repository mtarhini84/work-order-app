using Newtonsoft.Json;
using WorkOrderApp.Helpers.Queues;
using WorkOrderApp.Services.Email;

namespace WorkOrderApp.BackgroundServices.Handlers
{
    public class EmailHandler : IQueueHandler
    {
        private readonly EmailService _emailService;

        public EmailHandler(EmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task ExecuteMessage(QueueMessage message)
        {
            var payload = JsonConvert.DeserializeObject<EmailTemplateModel>(message.Content);
            if (payload is null) return;

            await _emailService.SendEmailAsync(payload);
        }
    }
}
