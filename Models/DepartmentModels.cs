using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_Departments")]
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required]
        [StringLength(50)]
        public string DepartmentCode { get; set; } = string.Empty; // e.g. HR-DEPT, IT-DEPT

        [Required]
        [StringLength(100)]
        public string DepartmentName { get; set; } = string.Empty; // e.g. Human Resources

        public int? HODId { get; set; } // Head of Department

        [ForeignKey("HODId")]
        public User? HOD { get; set; }

        [Required]
        public int BranchId { get; set; }

        [ForeignKey("BranchId")]
        public Branch? Branch { get; set; }

        public int? ParentDepartmentId { get; set; } // Self-referencing relationship

        [ForeignKey("ParentDepartmentId")]
        public Department? ParentDepartment { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class DepartmentViewModel
    {
        public List<Department> Departments { get; set; } = new List<Department>();

        public List<Branch> Branches { get; set; } = new List<Branch>();

        public List<User> StaffList { get; set; } = new List<User>();

        public int SelectedBranchId { get; set; } = 0;

        public string SearchTerm { get; set; } = string.Empty;

        [Required]
        [StringLength(50, ErrorMessage = "Department Code cannot exceed 50 characters.")]
        public string DepartmentCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "Department Name cannot exceed 100 characters.")]
        public string DepartmentName { get; set; } = string.Empty;

        public int? HODId { get; set; }

        [Required]
        public int BranchId { get; set; }

        public int? ParentDepartmentId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
