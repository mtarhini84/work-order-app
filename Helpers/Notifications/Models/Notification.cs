namespace WorkOrderApp.Helpers.Notifications
{
    public class Notification
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string UserId { get; set; }
        public bool Read { get; set; }
    }
}
