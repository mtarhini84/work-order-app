using System.Text.Json.Serialization;

namespace WorkOrderApp.Entities
{
    public class City : BaseEntity
    {
        public required string Name { get; set; }

        public required string CountryId { get; set; }

        [JsonIgnore]
        public Country Country { get; set; } = null!;
    }
}
