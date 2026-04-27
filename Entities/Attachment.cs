using System.Text.Json.Serialization;

namespace WorkOrderApp.Entities
{
    // Exactly one of RequestId or WorkOrderId must be set.
    public class Attachment : BaseEntity
    {
        public required string Url { get; set; }
        public required string FileName { get; set; }
        public string? ContentType { get; set; }

        public string? RequestId { get; set; }
        public string? WorkOrderId { get; set; }

        [JsonIgnore] public Request? Request { get; set; }
        [JsonIgnore] public WorkOrder? WorkOrder { get; set; }
    }
}
