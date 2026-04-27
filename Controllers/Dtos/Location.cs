namespace WorkOrderApp.Controllers
{
    public class CreateLocationDto
    {
        public required string Name { get; set; }
        public required string Address { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateLocationDto
    {
        public required string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class LocationDetails : BaseDetails
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class AssignUserToLocationDto
    {
        public required string UserId { get; set; }
        public required string LocationId { get; set; }
    }
}
