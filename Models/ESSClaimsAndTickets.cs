using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    public class ApplicationUser : User
    {
        [NotMapped]
        public virtual ApplicationUser? ReportingManager { get; set; }
    }

    [Table("erp_ESSExpenseClaims")]
    public class ExpenseClaim
    {
        [Key]
        [Column("ExpenseClaimId")]
        public int Id { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [Column("ExpenseType")]
        [StringLength(100)]
        public string ExpenseCategory { get; set; } = "Travel & Conveyance"; // "Travel & Conveyance", "Client Entertainment", "Office Supplies"

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column("ClaimDate")]
        public DateTime ExpenseDate { get; set; } = DateTime.Today;

        [StringLength(1000)]
        public string? Description { get; set; } = string.Empty;

        [Column("ReceiptFileName")]
        [StringLength(500)]
        public string? ReceiptFilePath { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // "Pending", "Manager Approved", "Finance Reimbursed", "Rejected"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(150)]
        public string? EmployeeName { get; set; }

        [StringLength(50)]
        public string? ManagerStatus { get; set; } = "Pending";

        [StringLength(255)]
        public string? ManagerRemarks { get; set; }

        [StringLength(150)]
        public string? ReviewedBy { get; set; }

        public DateTime? ReviewedAt { get; set; }
    }

    [Table("erp_ESSSupportTickets")]
    public class SupportTicket
    {
        [Key]
        [Column("TicketId")]
        public int Id { get; set; }

        [StringLength(50)]
        public string? TicketNumber { get; set; } = string.Empty; // e.g. "TCK-2026-0042"

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [Column("Department")]
        [StringLength(100)]
        public string DepartmentTarget { get; set; } = "Information Technology"; // "Information Technology", "Human Resources", "Facility"

        [StringLength(50)]
        public string Priority { get; set; } = "Medium"; // "Low", "Medium", "High", "Critical"

        [Required]
        [StringLength(250)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(50)]
        public string Status { get; set; } = "Open"; // "Open", "In Progress", "Resolved", "Closed"

        [StringLength(2000)]
        public string? ResolutionRemarks { get; set; }

        [NotMapped]
        public string? ResolutionNotes { get => ResolutionRemarks; set => ResolutionRemarks = value; }

        [StringLength(150)]
        public string? ResolvedBy { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
