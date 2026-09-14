using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_ESSPunches")]
    public class ESSPunch
    {
        [Key]
        public int PunchId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public DateTime? CheckInTime { get; set; }

        public DateTime? CheckOutTime { get; set; }

        [Required]
        [StringLength(50)]
        public string PunchSource { get; set; } = "Web Clock";
    }

    [Table("erp_ESSLeaveApplications")]
    public class ESSLeaveApplication
    {
        [Key]
        public int LeaveApplicationId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string LeaveType { get; set; } = "Casual Leave"; // Casual Leave, Sick Leave, Earned Leave

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public int TotalDays { get; set; }

        [Required]
        [StringLength(255)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        [StringLength(150)]
        public string? EmployeeName { get; set; }

        [StringLength(50)]
        public string? ManagerStatus { get; set; } = "Pending";

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(255)]
        public string? ManagerRemarks { get; set; }

        [StringLength(150)]
        public string? ReviewedBy { get; set; }

        public DateTime? ReviewedAt { get; set; }
    }

    [Table("erp_ESSTasks")]
    public class ESSTask
    {
        [Key]
        public int TaskId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(150)]
        public string TaskTitle { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, In Progress, Completed

        public int? DepartmentTaskId { get; set; }
    }

    [Table("erp_ESSExpenseClaims")]
    public class ESSExpenseClaim
    {
        [Key]
        [Column("ExpenseClaimId")]
        public int ExpenseClaimId { get; set; }

        [NotMapped]
        public int Id { get => ExpenseClaimId; set => ExpenseClaimId = value; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [Required]
        [StringLength(100)]
        [Column("ExpenseType")]
        public string ExpenseType { get; set; } = "Travel & Conveyance"; // Travel & Conveyance, Client Entertainment, Office Supplies

        [NotMapped]
        public string ExpenseCategory { get => ExpenseType; set => ExpenseType = value; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [Column("ClaimDate")]
        public DateTime ClaimDate { get; set; } = DateTime.Today;

        [NotMapped]
        public DateTime ExpenseDate { get => ClaimDate; set => ClaimDate = value; }

        [StringLength(1000)]
        public string? Description { get; set; } = string.Empty;

        [StringLength(500)]
        [Column("ReceiptFileName")]
        public string? ReceiptFileName { get; set; }

        [NotMapped]
        public string? ReceiptFilePath { get => ReceiptFileName; set => ReceiptFileName = value; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Manager Approved, Finance Reimbursed, Rejected

        [StringLength(150)]
        public string? EmployeeName { get; set; }

        [StringLength(50)]
        public string? ManagerStatus { get; set; } = "Pending";

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(255)]
        public string? ManagerRemarks { get; set; }

        [StringLength(150)]
        public string? ReviewedBy { get; set; }

        public DateTime? ReviewedAt { get; set; }
    }

    [Table("erp_ESSSupportTickets")]
    public class ESSSupportTicket
    {
        [Key]
        [Column("TicketId")]
        public int TicketId { get; set; }

        [NotMapped]
        public int Id { get => TicketId; set => TicketId = value; }

        [StringLength(50)]
        public string? TicketNumber { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [Required]
        [StringLength(100)]
        [Column("Department")]
        public string Department { get; set; } = "Information Technology"; // Information Technology, Human Resources, Facility

        [NotMapped]
        public string DepartmentTarget { get => Department; set => Department = value; }

        [StringLength(50)]
        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical

        [Required]
        [StringLength(250)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Open"; // Open, In Progress, Resolved, Closed

        [StringLength(2000)]
        public string? ResolutionRemarks { get; set; }

        [NotMapped]
        public string? ResolutionNotes { get => ResolutionRemarks; set => ResolutionRemarks = value; }

        [StringLength(150)]
        public string? ResolvedBy { get; set; }

        public DateTime? ResolvedAt { get; set; }
    }
}
