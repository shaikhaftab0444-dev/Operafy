using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_Suppliers")]
    public class Supplier
    {
        [Key]
        public int SupplierId { get; set; }

        [Required]
        public int CompanyId { get; set; } = 1;

        [Required]
        public int BranchId { get; set; } = 3;

        [Required]
        [StringLength(20)]
        public string SupplierCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string SupplierName { get; set; } = string.Empty;

        [StringLength(150)]
        public string? ContactPerson { get; set; }

        [StringLength(100)]
        public string? Designation { get; set; }

        [StringLength(300)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(40)]
        public string? Phone { get; set; }

        [Required]
        [StringLength(40)]
        public string Mobile { get; set; } = string.Empty;

        [StringLength(40)]
        public string? AlternateMobile { get; set; }

        [StringLength(30)]
        public string? GSTIN { get; set; }

        [StringLength(20)]
        public string? PANNumber { get; set; }

        [StringLength(300)]
        public string? Website { get; set; }

        [Required]
        [StringLength(500)]
        public string AddressLine1 { get; set; } = string.Empty;

        [StringLength(500)]
        public string? AddressLine2 { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string State { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [StringLength(200)]
        public string? BankName { get; set; }

        [StringLength(100)]
        public string? AccountNumber { get; set; }

        [StringLength(30)]
        public string? IFSCCode { get; set; }

        [StringLength(100)]
        public string? UPIId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CreditLimit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OpeningBalance { get; set; }

        [StringLength(100)]
        public string? PaymentTerms { get; set; }

        [StringLength(500)]
        public string? WebsiteRemarks { get; set; }

        [StringLength(500)]
        public string? LogoPath { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;
    }
}