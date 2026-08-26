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

    [Table("erp_SalaryStructureMasters")]
    public class SalaryStructureMaster
    {
        [Key]
        public int StructureId { get; set; }

        [Required]
        [StringLength(150)]
        public string StructureName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string Department { get; set; } = "All Departments";

        [StringLength(100)]
        public string Designation { get; set; } = "All Designations";

        [Column(TypeName = "decimal(18, 2)")]
        public decimal BasicPercent { get; set; } = 50.00m;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal HRAPercent { get; set; } = 20.00m;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal LTAPercent { get; set; } = 5.00m;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ConveyanceAllowance { get; set; } = 1600.00m;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal MedicalAllowance { get; set; } = 1250.00m;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal OtherAllowance { get; set; } = 0.00m;

        public bool AutoCalculateSpecialAllowance { get; set; } = true;

        public bool IsPFEnabled { get; set; } = true;
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PFEmployeeRate { get; set; } = 12.00m;
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PFEmployerRate { get; set; } = 12.00m;
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PFWageCeiling { get; set; } = 15000.00m;

        public bool IsESIEnabled { get; set; } = true;
        [Column(TypeName = "decimal(18, 2)")]
        public decimal ESIEmployeeRate { get; set; } = 0.75m;
        [Column(TypeName = "decimal(18, 2)")]
        public decimal ESIEmployerRate { get; set; } = 3.25m;
        [Column(TypeName = "decimal(18, 2)")]
        public decimal ESIWageCeiling { get; set; } = 21000.00m;

        public bool IsPTEnabled { get; set; } = true;
        public bool IsTDSEnabled { get; set; } = true;

        public DateTime EffectiveFrom { get; set; } = DateTime.Today;
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    [Table("erp_EmployeeSalaryAssignments")]
    public class EmployeeSalaryAssignment
    {
        [Key]
        public int AssignmentId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        public int? StructureId { get; set; }

        [ForeignKey("StructureId")]
        public SalaryStructureMaster? Structure { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal AnnualCTC { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal MonthlyBasic { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal MonthlyHRA { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal MonthlyLTA { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal MonthlySpecialAllowance { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal MonthlyConveyance { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal MonthlyMedical { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal MonthlyOtherAllowance { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal MonthlyGrossSalary { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal MonthlyPFEmployee { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal MonthlyESIEmployee { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal MonthlyPT { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal MonthlyTDS { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal MonthlyNetSalary { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal MonthlyPFEmployer { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal MonthlyESIEmployer { get; set; }

        public DateTime EffectiveFrom { get; set; } = DateTime.Today;
        public DateTime? EffectiveTo { get; set; }

        public bool IsCurrent { get; set; } = true;

        [StringLength(255)]
        public string? Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}