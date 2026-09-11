using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_JournalVouchers")]
    public class JournalVoucher
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string VoucherNumber { get; set; } = string.Empty; // e.g. "JV-2026-0045"

        public DateTime VoucherDate { get; set; } = DateTime.Today;

        [Required]
        [StringLength(50)]
        public string VoucherType { get; set; } = "Payment"; // "Payment", "Receipt", "Journal", "Contra"

        [Required]
        [StringLength(200)]
        public string DebitAccount { get; set; } = string.Empty; // e.g. "Vendor: TechInfra Ltd" or "Rent Expense"

        [Required]
        [StringLength(200)]
        public string CreditAccount { get; set; } = string.Empty; // e.g. "HDFC Bank A/c"

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [StringLength(500)]
        public string? Narration { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Draft"; // "Draft", "Submitted", "Posted"

        [StringLength(100)]
        public string? CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    [Table("erp_BankReconciliations")]
    public class BankReconciliationItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string BankAccountName { get; set; } = string.Empty; // e.g. "HDFC Current A/c - 50200"

        [Column(TypeName = "decimal(18,2)")]
        public decimal StatementBalance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BookBalance { get; set; }

        public int UnreconciledEntriesCount { get; set; }

        public DateTime LastSyncDate { get; set; } = DateTime.Today;
    }
}
