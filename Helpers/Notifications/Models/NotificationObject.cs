namespace WorkOrderApp.Helpers.Notifications
{
	public class NotificationObject
	{
		public string Id { get; set; }
		public string? CustomerId { get; set; }
		public string Title { get; set; }
		public string Body { get; set; }
		public string? Image { get; set; }
		public DateTime CreatedOn { get; set; }

		public NotificationObject()
		{

		}

		public NotificationObject(NotificationObjectDto data)
		{
			Id = Guid.NewGuid().ToString();
			CustomerId = data.CustomerId;
			Title = data.Title;
			Body = data.Body;
			CreatedOn = DateTime.UtcNow;
		}
	}

	public class DelayedNotificationObject
	{
		public string Id { get; set; }
		public string? CustomerId { get; set; }
		public string Title { get; set; }
		public string Body { get; set; }
		public string OnlineOrderId { get; set; }
		public string? Image { get; set; }
		public int Delay {  get; set; }
		public DateTime CreatedOn { get; set; }

		public DelayedNotificationObject()
		{

		}

		public DelayedNotificationObject(NotificationObjectDto data)
		{
			Id = Guid.NewGuid().ToString();
			CustomerId = data.CustomerId;
			Title = data.Title;
			Body = data.Body;
			CreatedOn = DateTime.UtcNow;
		}
	}
}
