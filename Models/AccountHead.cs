using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_AccountHeads")]
    public class AccountHead
    {
        [Key]
        public int AccountHeadId { get; set; }

        [Required]
        [StringLength(50)]
        public string HeadCode { get; set; } = string.Empty; // e.g. "AC-1001"

        [Required]
        [StringLength(150)]
        public string HeadName { get; set; } = string.Empty; // e.g. "Cash Account"

        [Required]
        [StringLength(50)]
        public string AccountType { get; set; } = string.Empty; // Asset, Liability, Equity, Revenue, Expense

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}