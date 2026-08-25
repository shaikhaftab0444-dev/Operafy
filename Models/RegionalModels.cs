using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_RegionalConfigurations")]
    public class RegionalConfiguration
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Country { get; set; } = "India";

        [Required]
        [StringLength(10)]
        public string CurrencyCode { get; set; } = "INR";

        [Required]
        [StringLength(10)]
        public string CurrencySymbol { get; set; } = "₹";

        [Required]
        [StringLength(50)]
        public string NumberSystem { get; set; } = "Lakhs/Crores";

        [Required]
        [StringLength(50)]
        public string DateFormat { get; set; } = "DD/MM/YYYY";

        [Required]
        [StringLength(100)]
        public string Timezone { get; set; } = "India Standard Time";

        [Required]
        [StringLength(100)]
        public string TaxSystem { get; set; } = "GST";

        [Required]
        [StringLength(50)]
        public string FinancialYearCycle { get; set; } = "April 1 - March 31";

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
