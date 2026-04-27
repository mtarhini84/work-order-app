using System.Text.Json.Serialization;

namespace WorkOrderApp.Entities
{
    // Explicit join entity for the User ↔ Location many-to-many.
    // Does NOT inherit BaseEntity — no Id/Active/timestamps needed on a join row.
    public class UserLocation
    {
        public required string UserId { get; set; }
        public required string LocationId { get; set; }

        [JsonIgnore]
        public User User { get; set; } = null!;

        [JsonIgnore]
        public Location Location { get; set; } = null!;
    }
}
