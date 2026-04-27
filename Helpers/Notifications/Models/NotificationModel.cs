namespace WorkOrderApp.Helpers.Notifications
{
    public class NotificationModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public bool Read { get; set; }
        public DateTime DateTime { get; set; }

        public static NotificationModel FromNotificationObject(NotificationObject obj)
        {
            return
                new NotificationModel()
                {
                    Id = obj.Id,
                    Title = obj.Title,
                    Description = obj.Body,
                    Image = obj.Image ?? "",
                    Read = false,
                    DateTime = obj.CreatedOn
                };
        }
    }
}
