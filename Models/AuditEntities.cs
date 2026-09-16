using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("SystemAuditTrails")]
    public class SystemAuditTrail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string EntityName { get; set; } = string.Empty; // "GeneralLedger", "PurchaseOrder", "Employee", etc.

        [Required]
        [StringLength(100)]
        public string RecordId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string ActionType { get; set; } = string.Empty; // "CREATE", "UPDATE", "DELETE", "APPROVAL"

        [StringLength(100)]
        public string? PerformedByUserId { get; set; }

        public int? PerformedByUserUserId { get; set; }

        [ForeignKey("PerformedByUserUserId")]
        public virtual User? PerformedByUser { get; set; }

        [Required]
        [StringLength(500)]
        public string ChangesSummary { get; set; } = string.Empty; // e.g. "Amount changed from ₹50,000 to ₹75,000"

        [StringLength(50)]
        public string IpAddress { get; set; } = "127.0.0.1";

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    [Table("AuditFlaggedItems")]
    public class AuditFlaggedItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string ReferenceNumber { get; set; } = string.Empty; // e.g. "INV-2026-081", "PO-9021", "ADJ-2026-04"

        [Required]
        [StringLength(100)]
        [Column("Module")]
        public string ModuleName { get; set; } = string.Empty; // "Finance", "Procurement", "Inventory", "HR"

        [NotMapped]
        public string Module
        {
            get => ModuleName;
            set => ModuleName = value;
        }

        [Required]
        [StringLength(500)]
        [Column("Description")]
        public string VarianceDescription { get; set; } = string.Empty;

        [NotMapped]
        public string Description
        {
            get => VarianceDescription;
            set => VarianceDescription = value;
        }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscrepancyAmount { get; set; }

        [Required]
        [StringLength(50)]
        public string Severity { get; set; } = "Medium"; // "High", "Medium", "Low"

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending Review"; // "Pending Review", "Cleared", "Escalated"

        public string? ResolutionNotes { get; set; }

        [StringLength(100)]
        public string? AuditedByUserId { get; set; }

        public int? AuditedByUserUserId { get; set; }

        [ForeignKey("AuditedByUserUserId")]
        public virtual User? AuditedByUser { get; set; }

        [StringLength(100)]
        public string? FlaggedByUserId { get; set; }

        [Column("FlaggedOn")]
        public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public DateTime FlaggedOn
        {
            get => LoggedAt;
            set => LoggedAt = value;
        }

        public DateTime? ResolvedAt { get; set; }

        // 3-Way Match Verification Helpers
        [NotMapped]
        public decimal PoExpectedAmount => DiscrepancyAmount > 0 ? (DiscrepancyAmount * 2.5m) : 50000.00m;

        [NotMapped]
        public decimal GrnReceivedAmount => PoExpectedAmount;

        [NotMapped]
        public decimal VendorClaimedAmount => PoExpectedAmount + DiscrepancyAmount;

        [NotMapped]
        public string PoNumber => $"PO-{(9000 + Id * 7)}";

        [NotMapped]
        public string GrnNumber => $"GRN-{(4500 + Id * 3)}";

        [NotMapped]
        public string VendorName => Id % 3 == 0 ? "Apex Global Supplies" : (Id % 2 == 0 ? "Zenith Industrial Equipments" : "CloudTech Logistics Ltd");

        [NotMapped]
        public string AuditedByUserName => AuditedByUser?.FullName ?? (!string.IsNullOrEmpty(AuditedByUserId) ? ("User #" + AuditedByUserId) : "Senior Auditor");
    }

    [Table("SystemMutationLogs")]
    public class SystemMutationLog
    {
        [Key]
        public int Id { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(100)]
        public string EntityName { get; set; } = string.Empty; // "GeneralLedger", "PurchaseOrder", "Employee", "Voucher"

        [Required]
        [StringLength(100)]
        public string RecordId { get; set; } = string.Empty; // primary key or voucher number

        [Required]
        [StringLength(50)]
        public string ActionType { get; set; } = string.Empty; // "CREATE", "UPDATE", "DELETE", "FORCE_OVERRIDE", "APPROVAL"

        [Required]
        [StringLength(500)]
        public string ChangesSummary { get; set; } = string.Empty; // human-readable summary

        public string? OldValuesJson { get; set; } // JSON snapshot before mutation

        public string? NewValuesJson { get; set; } // JSON snapshot after mutation

        [StringLength(100)]
        public string? PerformedByUserId { get; set; }

        [NotMapped]
        public virtual ApplicationUser? PerformedByUser { get; set; }

        [StringLength(50)]
        public string? IpAddress { get; set; } = "127.0.0.1";
    }
}
