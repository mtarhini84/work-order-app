namespace WorkOrderApp.Helpers.Notifications
{
    public class NotificationObjectDto
    {
        public string? CustomerId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public bool? ContentAvailable { get; set; } = false;
        public string? ChannelName = string.Empty;
    }
}
