using System.Text.Json.Serialization;

namespace WorkOrderApp.Entities
{
    public class Cost : BaseEntity
    {
        public required string WorkOrderId { get; set; }
        public required string Name { get; set; }

        /// <summary>Executor who recorded this cost.</summary>
        public required string UserId { get; set; }

        public decimal Amount { get; set; }
        public string? Description { get; set; }

        /// <summary>Free-text category: labor, transport, equipment, etc.</summary>
        public required string Category { get; set; }

        public string? Picture { get; set; }

        [JsonIgnore] public WorkOrder WorkOrder { get; set; } = null!;
        [JsonIgnore] public User User { get; set; } = null!;
    }
}
