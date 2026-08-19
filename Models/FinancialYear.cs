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
        [StringLength(100)]
        public string YearName { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
