namespace WorkOrderApp.Controllers
{
    public class CreateAttachmentDto
    {
        public required string Url { get; set; }
        public required string FileName { get; set; }
        public string? ContentType { get; set; }
        public string? RequestId { get; set; }
        public string? WorkOrderId { get; set; }
    }

    public class AttachmentDetails : BaseDetails
    {
        public string Url { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string? ContentType { get; set; }
        public string? RequestId { get; set; }
        public string? WorkOrderId { get; set; }
    }
}
