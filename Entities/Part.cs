using System.Text.Json.Serialization;

namespace WorkOrderApp.Entities
{
    public class Part : BaseEntity
    {
        public required string WorkOrderId { get; set; }
        public required string Name { get; set; }

        /// <summary>Executor who recorded this part usage.</summary>
        public required string UserId { get; set; }

        public decimal UnitCost { get; set; }
        public int Count { get; set; } = 1;
        public string? Description { get; set; }
        public string? QRCode { get; set; }
        public string? Picture { get; set; }

        [JsonIgnore] public WorkOrder WorkOrder { get; set; } = null!;
        [JsonIgnore] public User User { get; set; } = null!;
    }
}
