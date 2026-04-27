using System.Text.Json.Serialization;

namespace WorkOrderApp.Entities
{
    public class RequestLog : BaseEntity
    {
        public required string RequestId { get; set; }
        public required string UserId { get; set; }
        public required string Action { get; set; }
        public string? OldStatus { get; set; }
        public string? NewStatus { get; set; }
        public string? Notes { get; set; }

        [JsonIgnore] public Request Request { get; set; } = null!;
        [JsonIgnore] public User User { get; set; } = null!;
    }
}
