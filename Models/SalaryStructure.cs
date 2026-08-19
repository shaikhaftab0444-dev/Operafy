using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_SalaryStructures")]
    public class SalaryStructure
    {
        [Key]
        public int SalaryStructureId { get; set; }

        public int UserId { get; set; }

        public decimal BasicSalary { get; set; }

        public decimal HRA { get; set; }

        public decimal MedicalAllowance { get; set; }

        public decimal TransportAllowance { get; set; }

        public decimal ProvidentFund { get; set; }

        public decimal ProfessionalTax { get; set; }

        public decimal NetSalary { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
