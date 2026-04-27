using WorkOrderApp.AppDbContext;
using WorkOrderApp.Helpers.AST;
using WorkOrderApp.Helpers.Notifications;
using WorkOrderApp.Helpers.Queues;

namespace WorkOrderApp.BackgroundServices
{
    public class DelayedNotificationsListener : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public DelayedNotificationsListener(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var nextRunTime = DateTime.UtcNow.AddMinutes(1);
                nextRunTime = new DateTime(nextRunTime.Year, nextRunTime.Month, nextRunTime.Day, nextRunTime.Hour, nextRunTime.Minute, 1);
                var delay = nextRunTime - DateTime.UtcNow;

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var azureService = scope.ServiceProvider.GetRequiredService<IAzureTableService>();
                    var queueService = scope.ServiceProvider.GetRequiredService<IQueueService>();

                    var queueManager = new QueueManager(queueService);
                    var notificationService = new NotificationService(azureService, queueManager);

                    var notifications = await notificationService.GetDelayedNotificationsListing();

                    foreach (var notification in notifications)
                    {
                        if (notification.CreatedOn.AddMinutes(notification.Delay) <= DateTime.UtcNow)
                        {
                            await notificationService.EnqueueNotification(
                                new SendNotificationObject
                                {
                                    Id = notification.CustomerId,
                                    Title = notification.Title,
                                    Message = notification.Body,
                                    ContentAvailable = false
                                });
                            await notificationService.DeleteDelayedNotification(notification);
                        }
                    }
                }
                catch (Exception ex)
                {
                }

                await Task.Delay(delay, stoppingToken);
            }
        }
    }


}
