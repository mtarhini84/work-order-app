namespace WorkOrderApp.Helpers.Notifications
{
    public class DeviceToken
    {
        public string UserId { get; set; }
        public string Token { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Active { get; set; }
    }
}
