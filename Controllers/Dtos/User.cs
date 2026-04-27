using System.ComponentModel.DataAnnotations;

namespace WorkOrderApp.Controllers
{
    public class CreateUserDto
    {
        public required string Name { get; set; }
        [EmailAddress]
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Role { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Picture { get; set; }
    }

    public class UpdateUserDto
    {
        public required string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Picture { get; set; }
        public string? Settings { get; set; }
    }

    public class UserDetails : BaseDetails
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Picture { get; set; }
    }
}
