using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("SalarySlips")]
    public class SalarySlip
    {
        [Key]
        public int Id { get; set; }

        public int PayrollRunId { get; set; }
        [ForeignKey("PayrollRunId")]
        public virtual PayrollRun? PayrollRun { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthName { get; set; } = string.Empty;

        public int WorkingDays { get; set; } = 30;
        public int PaidDays { get; set; } = 30;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal BasicSalary { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Hra { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Allowances { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal GrossSalary { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PfDeduction { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PtDeduction { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TdsDeduction { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalDeductions { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal NetSalary { get; set; }

        public string Status { get; set; } = "Draft"; // "Draft", "Paid"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    [Table("InternalMessages")]
    public class InternalMessage
    {
        [Key]
        public int Id { get; set; }

        public int SenderUserId { get; set; }

        [ForeignKey("SenderUserId")]
        public virtual User? Sender { get; set; }

        public int RecipientUserId { get; set; }

        [ForeignKey("RecipientUserId")]
        public virtual User? Recipient { get; set; }

        [Required, MaxLength(250)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Folder { get; set; } = "Inbox"; // Inbox, Sent, Trash

        [MaxLength(100)]
        public string Category { get; set; } = "Payroll & Audit";

        public bool IsRead { get; set; } = false;

        [MaxLength(500)]
        public string? AttachmentUrl { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }

    [Table("SystemNotifications")]
    public class SystemNotification
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Category { get; set; } = "HR"; // System, Sales, Inventory, HR, Finance

        [MaxLength(500)]
        public string TargetUrl { get; set; } = "/HRPayroll/DownloadPayslipPdf";

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
