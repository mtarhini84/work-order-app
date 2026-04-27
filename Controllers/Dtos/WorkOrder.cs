using WorkOrderApp.Entities;

namespace WorkOrderApp.Controllers
{
    // ── WorkOrder DTOs ────────────────────────────────────────────────────────

    public class CreateWorkOrderDto
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public required string CustomerId { get; set; }
        public required string LocationId { get; set; }
        public string? RequestId { get; set; }
        public string? AssignedToId { get; set; }
        public string? AssignedById { get; set; }
        public Priority Priority { get; set; } = Priority.Medium;
        public int? EstimatedTime { get; set; }
        public DateTimeOffset? DueDate { get; set; }
        public DateTimeOffset? StartDate { get; set; }
    }

    public class UpdateWorkOrderDto
    {
        public required string Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? AssignedToId { get; set; }
        public string? AssignedById { get; set; }
        public WorkOrderStatus? Status { get; set; }
        public Priority? Priority { get; set; }
        public int? EstimatedTime { get; set; }
        public DateTimeOffset? DueDate { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public string? Notes { get; set; }
    }

    public class AssignWorkOrderDto
    {
        public required string Id { get; set; }
        public required string AssignedToId { get; set; }
    }

    public class WorkOrderDetails : BaseDetails
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public UserDetails? Customer { get; set; }
        public string LocationId { get; set; } = string.Empty;
        public LocationDetails? Location { get; set; }
        public string? RequestId { get; set; }
        public int Number { get; set; }
        public string? AssignedToId { get; set; }
        public UserDetails? AssignedTo { get; set; }
        public string? AssignedById { get; set; }
        public UserDetails? AssignedBy { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public int? EstimatedTime { get; set; }
        public DateTimeOffset? DueDate { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }

    // ── WorkOrderLog DTOs ─────────────────────────────────────────────────────

    public class WorkOrderLogDetails : BaseDetails
    {
        public string WorkOrderId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? OldStatus { get; set; }
        public string? NewStatus { get; set; }
        public string? Notes { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
