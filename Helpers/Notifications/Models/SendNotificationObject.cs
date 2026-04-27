namespace WorkOrderApp.Helpers.Notifications
{
	public class SendNotificationObject
	{
		public string Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Message { get; set; } = string.Empty;
		public IDictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
		public bool ContentAvailable { get; set; } = false;
		public string? ChannelId { get; set; } = string.Empty;
		public string? PushSound { get; set; } = string.Empty;
	}
}
