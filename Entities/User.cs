using System.Text.Json.Serialization;

namespace WorkOrderApp.Entities
{
    public class User : BaseEntity
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
        public required string PasswordHash { get; set; }

        public string? PhoneNumber { get; set; }
        public string? Picture { get; set; }

        /// <summary>Stored as jsonb in PostgreSQL. Free-form per-user settings.</summary>
        public string? Settings { get; set; }

        [JsonIgnore]
        public ICollection<UserLocation> UserLocations { get; set; } = [];
    }
}
