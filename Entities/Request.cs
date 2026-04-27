using System.Text.Json.Serialization;

namespace WorkOrderApp.Entities
{
    public class Request : BaseEntity
    {
        public required string Title { get; set; }
        public string? Description { get; set; }

        public required string LocationId { get; set; }

        // Auto-generated sequential number via DB sequence — never set manually.
        public int Number { get; set; }

        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        public required string RequestedById { get; set; }
        public Priority Priority { get; set; } = Priority.Medium;

        public string? DeclineReason { get; set; }
        public string? Picture { get; set; }
        public string? ContactInfo { get; set; }

        [JsonIgnore] public Location Location { get; set; } = null!;
        [JsonIgnore] public User RequestedBy { get; set; } = null!;
        [JsonIgnore] public ICollection<Attachment> Attachments { get; set; } = [];
        [JsonIgnore] public ICollection<RequestLog> Logs { get; set; } = [];
        [JsonIgnore] public WorkOrder? WorkOrder { get; set; }
    }
}
