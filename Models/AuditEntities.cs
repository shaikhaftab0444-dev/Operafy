using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    // Alias / wrapper for ApplicationUser pointing to the system's User model
    public class ApplicationUser : User
    {
    }

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
        public string Module { get; set; } = string.Empty; // "Finance", "Procurement", "Inventory", "HR"

        [Required]
        [StringLength(100)]
        public string ReferenceNumber { get; set; } = string.Empty; // e.g. "INV-2026-081", "PO-9021"

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscrepancyAmount { get; set; }

        [Required]
        [StringLength(50)]
        public string Severity { get; set; } = "Medium"; // "High", "Medium", "Low"

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending Review"; // "Pending Review", "Cleared", "Escalated"

        [StringLength(100)]
        public string? FlaggedByUserId { get; set; }

        public DateTime FlaggedOn { get; set; } = DateTime.UtcNow;
    }
}
