using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("PayrollComponents")]
    public class PayrollComponent
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string ComponentName { get; set; } = string.Empty;

        [Required, MaxLength(15)]
        public string Code { get; set; } = string.Empty; // HRA, CONV, PF, ESI, PT

        [Required]
        public string Type { get; set; } = "Allowance"; // "Allowance" or "Deduction"

        public string Taxability { get; set; } = "Fully Taxable"; // "Fully Taxable", "Partially Exempt", "Tax Exempt", "Fully Deductible"

        public string CalculationBasis { get; set; } = "Fixed Amount"; // "Fixed Amount", "Percentage of Basic", "Percentage of CTC", "Slab Based"

        [Column(TypeName = "decimal(18, 2)")]
        public decimal DefaultValueOrRate { get; set; } = 0.00m;

        public string? MaxCapLimit { get; set; } = "No Limit";

        public string PayFrequency { get; set; } = "Monthly"; // "Monthly", "Yearly"

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
