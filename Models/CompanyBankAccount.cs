using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("CompanyBankAccounts")]
    public class CompanyBankAccount
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string BankName { get; set; } = string.Empty; // e.g. "HDFC Bank", "State Bank of India"

        [Required]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        public string IFSCCode { get; set; } = string.Empty;

        public string? BranchName { get; set; } = string.Empty;
        public string AccountType { get; set; } = "Current"; // Current, Savings, Overdraft

        [Column(TypeName = "decimal(18,2)")]
        public decimal OpeningBalance { get; set; } = 0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentBalance { get; set; } = 0m;

        public bool IsPrimaryAccount { get; set; } = false; // Used for default payroll/invoicing
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
