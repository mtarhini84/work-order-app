using System.Text.Json.Serialization;

namespace WorkOrderApp.Entities
{
    public class Location : BaseEntity
    {
        public required string Name { get; set; }
        public required string Address { get; set; }
        public string? Description { get; set; }

        [JsonIgnore]
        public ICollection<UserLocation> UserLocations { get; set; } = [];
    }
}
