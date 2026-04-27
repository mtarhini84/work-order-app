using WorkOrderApp.Entities;

namespace WorkOrderApp.Controllers
{
    // ── Request DTOs ──────────────────────────────────────────────────────────

    public class CreateRequestDto
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public required string LocationId { get; set; }
        public Priority Priority { get; set; } = Priority.Medium;
        public string? Picture { get; set; }
        public string? ContactInfo { get; set; }
    }

    public class UpdateRequestDto
    {
        public required string Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public Priority? Priority { get; set; }
        public string? Picture { get; set; }
        public string? ContactInfo { get; set; }
    }

    public class ApproveRequestDto
    {
        public required string Id { get; set; }
        public string? Notes { get; set; }

        // Optional work order initialisation on approval
        public string? AssignedToId { get; set; }
        public DateTimeOffset? DueDate { get; set; }
        public int? EstimatedTime { get; set; }
    }

    public class DeclineRequestDto
    {
        public required string Id { get; set; }
        public required string DeclineReason { get; set; }
        public string? Notes { get; set; }
    }

    public class RequestDetails : BaseDetails
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string LocationId { get; set; } = string.Empty;
        public LocationDetails? Location { get; set; }
        public int Number { get; set; }
        public string Status { get; set; } = string.Empty;
        public string RequestedById { get; set; } = string.Empty;
        public UserDetails? RequestedBy { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string? DeclineReason { get; set; }
        public string? Picture { get; set; }
        public string? ContactInfo { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public List<AttachmentDetails> Attachments { get; set; } = [];
    }

    // ── RequestLog DTOs ───────────────────────────────────────────────────────

    public class RequestLogDetails : BaseDetails
    {
        public string RequestId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? OldStatus { get; set; }
        public string? NewStatus { get; set; }
        public string? Notes { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
