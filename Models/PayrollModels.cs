using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_AllowanceDeductionMasters")]
    public class AllowanceDeductionMaster
    {
        [Key]
        public int ComponentId { get; set; }

        [Required]
        [StringLength(150)]
        public string ComponentName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string ComponentCode { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string ComponentType { get; set; } = "Allowance"; // "Allowance" or "Deduction"

        [Required]
        [StringLength(50)]
        public string Taxability { get; set; } = "Fully Taxable"; // "Fully Taxable", "Partially Exempt", "Tax Exempt", "Fully Deductible"

        [Required]
        [StringLength(50)]
        public string CalculationBasis { get; set; } = "Fixed Amount"; // "Fixed Amount", "Percentage of Basic", "Percentage of CTC", "Slab Based"

        [Column(TypeName = "decimal(18, 2)")]
        public decimal DefaultValueOrRate { get; set; } = 0.00m;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal MinLimit { get; set; } = 0.00m;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal MaxLimit { get; set; } = 0.00m;

        [Required]
        [StringLength(20)]
        public string PayFrequency { get; set; } = "Monthly"; // "Monthly", "Yearly"

        public bool IsActive { get; set; } = true;
        public DateTime EffectiveDate { get; set; } = DateTime.Today;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    [Table("erp_StatutoryConfigurations")]
    public class StatutoryConfiguration
    {
        [Key]
        public int StatutoryId { get; set; }

        [Required]
        [StringLength(50)]
        public string RuleType { get; set; } = string.Empty; // "PF", "ESI", "PT", "TDS"

        [Column(TypeName = "decimal(18, 2)")]
        public decimal EmployeeRate { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal EmployerRate { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal WageCeilingLimit { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal StandardDeductionAnnual { get; set; } = 75000.00m;

        [StringLength(50)]
        public string DefaultTaxRegime { get; set; } = "New Tax Regime";

        [StringLength(2000)]
        public string? ConfigurationDetailsJson { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime EffectiveDate { get; set; } = DateTime.Today;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    [Table("erp_StatutoryFilingLogs")]
    public class StatutoryFilingLog
    {
        [Key]
        public int FilingId { get; set; }

        [Required]
        [StringLength(100)]
        public string ComplianceAct { get; set; } = string.Empty; // e.g., "PF ECR", "ESI Monthly Return", "TDS 24Q"

        [Required]
        [StringLength(50)]
        public string Frequency { get; set; } = "Monthly";

        [Required]
        [StringLength(100)]
        public string Period { get; set; } = string.Empty; // e.g., "July 2026"

        public DateTime DueDate { get; set; }
        public DateTime FilingDate { get; set; } = DateTime.Today;

        [Required]
        [StringLength(100)]
        public string ReceiptNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Filed";
    }

    [Table("erp_BonusIncentives")]
    public class BonusIncentive
    {
        [Key]
        public int BonusId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        [StringLength(100)]
        public string Type { get; set; } = "Performance Bonus"; // "Performance Bonus", "Festival Bonus", "Annual Bonus", "Sales Incentive", "Project Incentive", "Custom Incentive"

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }

        [Required]
        [StringLength(50)]
        public string PerformancePeriod { get; set; } = string.Empty; // e.g., "August 2026" or "Q2 2026"

        [Required]
        [StringLength(50)]
        public string PayoutMonth { get; set; } = string.Empty; // e.g., "August 2026"

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Draft"; // "Draft", "Submitted", "Approved", "Included in Payroll", "Paid"

        public int? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public int? PayrollRunId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    [Table("erp_PayrollRuns")]
    public class PayrollRun
    {
        [Key]
        public int PayrollRunId { get; set; }

        [Required]
        [StringLength(50)]
        public string PayPeriod { get; set; } = string.Empty; // e.g., "August 2026"

        public int Month { get; set; }
        public int Year { get; set; }

        [StringLength(100)]
        public string Department { get; set; } = "All Departments";

        public int TotalEmployees { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalGrossSalary { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalDeductions { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalNetSalary { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalEmployerPF { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalEmployerESI { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalCTC { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Draft"; // "Draft", "Calculated", "Under Review", "Approved", "Locked", "Paid", "Cancelled"

        public int? ProcessedByUserId { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

        public int? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public int? PaidByUserId { get; set; }
        public DateTime? PaidAt { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
