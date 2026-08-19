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
        public string AccountCode { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string AccountName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string AccountType { get; set; } = "Asset";

        public decimal Balance { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
