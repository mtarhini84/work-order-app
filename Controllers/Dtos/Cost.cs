namespace WorkOrderApp.Controllers
{
    public class CreateCostDto
    {
        public required string WorkOrderId { get; set; }
        public required string Name { get; set; }
        public required string UserId { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public required string Category { get; set; }
        public string? Picture { get; set; }
    }

    public class UpdateCostDto
    {
        public required string Id { get; set; }
        public string? Name { get; set; }
        public decimal? Amount { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? Picture { get; set; }
    }

    public class CostDetails : BaseDetails
    {
        public string WorkOrderId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? Picture { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
