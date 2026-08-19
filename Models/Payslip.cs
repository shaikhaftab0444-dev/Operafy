using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_Payslips")]
    public class Payslip
    {
        [Key]
        public int PayslipId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        [StringLength(50)]
        public string PayPeriod { get; set; } = string.Empty; // e.g. "August 2026"

        [Column(TypeName = "decimal(18, 2)")]
        public decimal BasicSalary { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal HRA { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TransportAllowance { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal MedicalAllowance { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ProvidentFund { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ProfessionalTax { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal NetSalary { get; set; }

        public int PaidDays { get; set; } = 30;

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Paid"; // Paid, Pending
    }
}
