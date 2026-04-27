using System.Text.Json.Serialization;

namespace WorkOrderApp.Entities
{
    public class WorkOrder : BaseEntity
    {
        public required string Title { get; set; }
        public string? Description { get; set; }

        public required string CustomerId { get; set; }
        public required string LocationId { get; set; }

        // Nullable: work orders can be created independently of a request.
        public string? RequestId { get; set; }

        // Auto-generated sequential number via DB sequence — never set manually.
        public int Number { get; set; }

        // Nullable until explicitly assigned.
        public string? AssignedToId { get; set; }
        public string? AssignedById { get; set; }

        public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Open;
        public Priority Priority { get; set; } = Priority.Medium;

        /// <summary>Estimated completion time in minutes.</summary>
        public int? EstimatedTime { get; set; }

        public DateTimeOffset? DueDate { get; set; }
        public DateTimeOffset? StartDate { get; set; }

        [JsonIgnore] public User Customer { get; set; } = null!;
        [JsonIgnore] public Location Location { get; set; } = null!;
        [JsonIgnore] public Request? Request { get; set; }
        [JsonIgnore] public User? AssignedTo { get; set; }
        [JsonIgnore] public User? AssignedBy { get; set; }
        [JsonIgnore] public ICollection<Cost> Costs { get; set; } = [];
        [JsonIgnore] public ICollection<Part> Parts { get; set; } = [];
        [JsonIgnore] public ICollection<Attachment> Attachments { get; set; } = [];
        [JsonIgnore] public ICollection<WorkOrderLog> Logs { get; set; } = [];
    }
}
