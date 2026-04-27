namespace WorkOrderApp.Settings
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int Port { get; set; } = 465;
        public string SenderName { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
