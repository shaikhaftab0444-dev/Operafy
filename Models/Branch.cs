using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_Branches")]
    public class Branch
    {
        [Key]
        public int BranchId { get; set; }

        public int CompanyId { get; set; }

        [Required]
        [StringLength(100)]
        public string BranchName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string BranchCode { get; set; } = string.Empty;

        [StringLength(250)]
        public string AddressLine1 { get; set; } = string.Empty;

        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [StringLength(100)]
        public string State { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        [ForeignKey("CompanyId")]
        public Company? Company { get; set; }
    }
}
