using Newtonsoft.Json;
using WorkOrderApp.Helpers.Notifications;
using WorkOrderApp.Helpers.Queues;

namespace WorkOrderApp.BackgroundServices.Handlers
{
	public class NotificationHandler : IQueueHandler
	{
		private readonly NotificationService _notificationService;

		public NotificationHandler(NotificationService notificationService)
		{
			_notificationService = notificationService;
		}

		public async Task ExecuteMessage(QueueMessage message)
		{
			if (message != null && !string.IsNullOrEmpty(message.Content))
			{
				var payload = JsonConvert.DeserializeObject<SendNotificationObject>(message.Content);

				if (string.IsNullOrEmpty(payload.Title))
				{
					throw new Exception("Title is Empty");
				}

				if (string.IsNullOrEmpty(payload.Message))
				{
					throw new Exception("Message is Empty");
				}

				bool result;

				if (string.IsNullOrEmpty(payload.Id) || payload.Id.Equals("all"))
				{
					result = await _notificationService.SendToAllAsync(payload);
				}
				else
				{
					result = await _notificationService.SendToUserAsync(payload);
				}

				if (!result)
				{
					throw new Exception("Error sending notification");
				}
			}
		}
	}
}
