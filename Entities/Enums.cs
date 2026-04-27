using System.ComponentModel.DataAnnotations;

namespace WorkOrderApp.Entities
{
    public class BaseEnum : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }

    // ── Example BaseEnum subtypes ─────────────────────────────────────────────
    // Each gets its own table via TPC. Add navigation properties to domain
    // entities in your project once those entities exist.

    public class Currency : BaseEnum
    {
        public string Code { get; set; } = string.Empty;
        public decimal ExchangeRate { get; set; }
        public bool IsDefault { get; set; }
    }

    public class FeeType : BaseEnum
    {
        [Range(0, 100)]
        public decimal PenaltyPercentage { get; set; }
    }

    public class IdentificationType : BaseEnum { }

    public class PaymentType : BaseEnum { }

    public class LeaseType : BaseEnum { }

    public class Appliance : BaseEnum { }

    public class PropertyType : BaseEnum
    {
        public bool IsCommercial { get; set; }
    }

    public class Role : BaseEnum { }
}
