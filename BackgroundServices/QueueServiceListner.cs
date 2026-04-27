using WorkOrderApp.AppDbContext;
using WorkOrderApp.BackgroundServices.Handlers;
using WorkOrderApp.Helpers.AST;
using WorkOrderApp.Helpers.Notifications;
using WorkOrderApp.Helpers.Queues;
using WorkOrderApp.Services.Email;
using WorkOrderApp.Services.OTP;

namespace WorkOrderApp.BackgroundServices
{
    public class QueueServiceListner : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private IDictionary<string, IQueueHandler> _handlers = new Dictionary<string, IQueueHandler>();

        public QueueServiceListner(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var queueManager       = scope.ServiceProvider.GetRequiredService<QueueManager>();
            var tableService       = scope.ServiceProvider.GetRequiredService<IAzureTableService>();
            var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();
            var otpService         = scope.ServiceProvider.GetRequiredService<OTPService>();
            var emailService       = scope.ServiceProvider.GetRequiredService<EmailService>();

            _handlers = new Dictionary<string, IQueueHandler>
            {
                { "notifications",     new NotificationHandler(notificationService) },
                { "otp-notifications", new OTPHandler(otpService)                  },
                { "emails",            new EmailHandler(emailService)               },
            };

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var tasks = _handlers.Select(h => ProcessQueueAsync(h.Key, h.Value, stoppingToken));
                await Task.WhenAll(tasks);
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }

        private async Task ProcessQueueAsync(
            string queueName,
            IQueueHandler handler,
            CancellationToken cancellationToken)
        {
            var messageBody = string.Empty;
            try
            {
                using var scope        = _scopeFactory.CreateScope();
                var queueManager       = scope.ServiceProvider.GetRequiredService<QueueManager>();
                var tableService       = scope.ServiceProvider.GetRequiredService<IAzureTableService>();

                var message = await queueManager.DequeueMessageAsync(queueName, 1);
                if (message is null || string.IsNullOrEmpty(message.Content)) return;

                messageBody = message.Content;
                await handler.ExecuteMessage(message);
            }
            catch (Exception ex)
            {
                using var scope  = _scopeFactory.CreateScope();
                var queueManager = scope.ServiceProvider.GetRequiredService<QueueManager>();
                var tableService = scope.ServiceProvider.GetRequiredService<IAzureTableService>();

                await queueManager.EnqueueMessageAsync($"{queueName}-error", messageBody);
                await tableService.CreateEntity(
                    "FailedEventsLogs", queueName, Guid.NewGuid().ToString(),
                    new Dictionary<string, object> { { "Error", ex.Message }, { "Body", messageBody } });
            }
        }
    }
}
