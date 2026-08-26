using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_Payslips")]
    public class Payslip
    {
        [Key]
        public int PayslipId { get; set; }

        public int? PayrollRunId { get; set; }

        [ForeignKey("PayrollRunId")]
        public PayrollRun? PayrollRun { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        [StringLength(50)]
        public string PayPeriod { get; set; } = string.Empty;

        [StringLength(50)]
        public string PayslipNumber { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal BasicSalary { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal HRA { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TransportAllowance { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal MedicalAllowance { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal LTA { get; set; } = 0.00m;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal SpecialAllowance { get; set; } = 0.00m;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal OtherAllowance { get; set; } = 0.00m;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal BonusIncentives { get; set; } = 0.00m;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal OvertimePay { get; set; } = 0.00m;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal GrossSalary { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ProvidentFund { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ESI { get; set; } = 0.00m;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ProfessionalTax { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TDS { get; set; } = 0.00m;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal LOPDeduction { get; set; } = 0.00m;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalDeductions { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal NetSalary { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal EmployerPF { get; set; } = 0.00m;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal EmployerESI { get; set; } = 0.00m;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalCTC { get; set; } = 0.00m;

        public int TotalWorkingDays { get; set; } = 30;
        public int PresentDays { get; set; } = 30;
        public int AbsentDays { get; set; } = 0;
        public int PaidLeaveDays { get; set; } = 0;
        public int UnpaidLeaveDays { get; set; } = 0;
        public int PaidDays { get; set; } = 30;
        public int OvertimeHours { get; set; } = 0;

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Paid"; // "Draft", "Approved", "Paid"
    }
}
