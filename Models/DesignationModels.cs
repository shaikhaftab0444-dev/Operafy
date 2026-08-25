using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_Designations")]
    public class Designation
    {
        [Key]
        public int DesignationId { get; set; }

        [Required(ErrorMessage = "Designation Code is required")]
        [StringLength(10, ErrorMessage = "Code cannot exceed 10 characters")]
        public string DesignationCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Job Title is required")]
        [StringLength(100, ErrorMessage = "Job Title cannot exceed 100 characters")]
        public string JobTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }

        [Required(ErrorMessage = "Hierarchy Level / Grade is required")]
        [StringLength(50)]
        public string HierarchyLevel { get; set; } = string.Empty;

        [Required(ErrorMessage = "Minimum CTC is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MinCTC { get; set; }

        [Required(ErrorMessage = "Maximum CTC is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MaxCTC { get; set; }

        [StringLength(500)]
        public string? JobDescription { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class DesignationViewModel
    {
        public Designation Designation { get; set; } = new Designation();
        public List<Designation> Designations { get; set; } = new List<Designation>();
        public List<Department> Departments { get; set; } = new List<Department>();

        public int SelectedDepartmentId { get; set; }
        public string SearchQuery { get; set; } = string.Empty;
    }
}
