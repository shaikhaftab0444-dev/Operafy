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

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}