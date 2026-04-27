namespace WorkOrderApp.Controllers
{
    public class CreatePartDto
    {
        public required string WorkOrderId { get; set; }
        public required string Name { get; set; }
        public required string UserId { get; set; }
        public decimal UnitCost { get; set; }
        public int Count { get; set; } = 1;
        public string? Description { get; set; }
        public string? QRCode { get; set; }
        public string? Picture { get; set; }
    }

    public class UpdatePartDto
    {
        public required string Id { get; set; }
        public string? Name { get; set; }
        public decimal? UnitCost { get; set; }
        public int? Count { get; set; }
        public string? Description { get; set; }
        public string? QRCode { get; set; }
        public string? Picture { get; set; }
    }

    public class PartDetails : BaseDetails
    {
        public string WorkOrderId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public decimal UnitCost { get; set; }
        public int Count { get; set; }
        public string? Description { get; set; }
        public string? QRCode { get; set; }
        public string? Picture { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
