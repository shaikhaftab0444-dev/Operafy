using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_TaxSlabs")]
    public class TaxSlab
    {
        [Key]
        public int TaxSlabId { get; set; }

        [Required]
        [StringLength(50)]
        public string TaxCode { get; set; } = string.Empty; // e.g., GST-18, VAT-15

        [Required]
        [StringLength(250)]
        public string Description { get; set; } = string.Empty; // e.g., Goods & Services Tax Standard

        [Column(TypeName = "decimal(18, 2)")]
        public decimal CombinedRate { get; set; } // e.g., 18.00 %

        [Column(TypeName = "decimal(18, 2)")]
        public decimal CGST { get; set; } // Central GST %

        [Column(TypeName = "decimal(18, 2)")]
        public decimal SGST { get; set; } // State GST %

        [Column(TypeName = "decimal(18, 2)")]
        public decimal IGST { get; set; } // Integrated GST %

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = "GST"; // GST, VAT, Cess, Zero-Rated

        [Required]
        [StringLength(50)]
        public string Regime { get; set; } = "GST India"; // GST India, VAT, Sales Tax

        public bool IsRcmActive { get; set; } = false; // Reverse Charge Mechanism

        public DateTime EffectiveDate { get; set; } = DateTime.Today;

        public bool IsActive { get; set; } = true;
    }

    public class TaxViewModel
    {
        public List<TaxSlab> TaxSlabs { get; set; } = new List<TaxSlab>();

        public string SelectedRegime { get; set; } = "All";

        [Required]
        [StringLength(50, ErrorMessage = "Tax Code cannot exceed 50 characters.")]
        public string TaxCode { get; set; } = string.Empty;

        [Required]
        [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.00, 100.00, ErrorMessage = "Combined Rate must be between 0% and 100%.")]
        public decimal CombinedRate { get; set; }

        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }

        [Required]
        public string Category { get; set; } = "GST";

        [Required]
        public string Regime { get; set; } = "GST India";

        public bool IsRcmActive { get; set; } = false;

        [Required]
        public DateTime EffectiveDate { get; set; } = DateTime.Today;

        public bool IsActive { get; set; } = true;
    }
}
