using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_FinancialYears")]
    public class FinancialYear
    {
        [Key]
        public int FinancialYearId { get; set; }

        [Required]
        [StringLength(50)]
        public string YearName { get; set; } = string.Empty; // e.g. "FY 2026-27"

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        public bool IsCurrent { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;
    }
}