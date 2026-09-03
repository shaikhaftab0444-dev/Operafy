using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace ERP_System.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }

        [Required]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? MobileNumber { get; set; }

        public int? RoleId { get; set; }

        public string? RoleName { get; set; }

        public int? DepartmentId { get; set; }

        public string? DepartmentName { get; set; }

        public string? ReportingManagerId { get; set; }

        public string? ReportingManagerName { get; set; }

        public string? BranchName { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class CreateEmployeeInputModel
    {
        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email Address is required.")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Password { get; set; }

        [StringLength(20)]
        public string? MobileNumber { get; set; }

        [Required(ErrorMessage = "Role / Designation is required.")]
        public int SelectedRoleId { get; set; }

        [Required(ErrorMessage = "Department is required.")]
        public int? DepartmentId { get; set; }

        public string? DepartmentName { get; set; }

        public string? ReportingManagerId { get; set; }

        public string? ReportingManagerName { get; set; }

        public string? BranchName { get; set; }

        public IFormFile? ProfilePhotoFile { get; set; }
    }
}
