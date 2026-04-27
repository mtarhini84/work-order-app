namespace WorkOrderApp.Services.Email
{
    public class EmailTemplateModel
    {
        public string ToName { get; set; } = string.Empty;
        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string HtmlBody { get; set; } = string.Empty;
    }
}
