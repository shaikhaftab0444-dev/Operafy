using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("StatutoryReturnFilings")]
    public class StatutoryReturnFiling
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string ComplianceReturn { get; set; } = string.Empty; // "Provident Fund ECR Filing", "ESI Monthly Return", "TDS 24Q Quarterly Return"

        public string Frequency { get; set; } = "Monthly"; // Monthly, Quarterly, Annual

        [Required, MaxLength(50)]
        public string FilingPeriod { get; set; } = string.Empty; // "August 2026", "Q2 2026"

        public DateTime DueDate { get; set; }
        public DateTime FilingDate { get; set; } = DateTime.Today;

        [Required, MaxLength(100)]
        public string ReceiptOrChallanNumber { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ChallanAmountPaid { get; set; }

        // Linked to Company Bank Account added by Super Admin
        public int? CompanyBankAccountId { get; set; }
        [ForeignKey("CompanyBankAccountId")]
        public virtual CompanyBankAccount? CompanyBankAccount { get; set; }

        public string Status { get; set; } = "Filed"; // Filed, Paid, Verified

        public string? LoggedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    [Table("StatutoryRuleConfigs")]
    public class StatutoryRuleConfig
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string RuleKey { get; set; } = string.Empty; // "PF", "ESI", "TDS"

        [Column(TypeName = "decimal(18, 2)")]
        public decimal EmployerRate { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal EmployeeRate { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal WageCeiling { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal StandardDeduction { get; set; }

        public string ActiveRegime { get; set; } = "New Tax Regime";
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    [Table("BankTransactions")]
    public class BankTransaction
    {
        [Key]
        public int Id { get; set; }

        public int CompanyBankAccountId { get; set; }
        [ForeignKey("CompanyBankAccountId")]
        public virtual CompanyBankAccount? CompanyBankAccount { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.Today;

        [Required, MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal DebitAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal CreditAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal BalanceAfter { get; set; }

        [MaxLength(100)]
        public string Category { get; set; } = "General";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
