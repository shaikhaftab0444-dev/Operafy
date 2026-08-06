using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_Users")]
    public class User
    {
        [Key]
        public int UserId { get; set; }

        public int CompanyId { get; set; } = 1; // Seeded company ID (AIT Technologies)

        public int BranchId { get; set; } = 3; // Seeded branch ID (Head Office)

        [Required]
        [StringLength(20)]
        public string UserCode { get; set; } = "USR001";

        [Required]
        [StringLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(200)]
        public string? PasswordSalt { get; set; }

        [StringLength(20)]
        public string? MobileNumber { get; set; }

        [StringLength(500)]
        public string? ProfilePhoto { get; set; }

        public DateTime? LastLogin { get; set; }

        public DateTime? LastPasswordChanged { get; set; }

        public int FailedLoginAttempts { get; set; } = 0;

        public bool IsLocked { get; set; } = false;

        public bool IsEmailVerified { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        public bool IsActive { get; set; } = true;

        public int? RoleId { get; set; }

        [ForeignKey("RoleId")]
        public Role? Role { get; set; }
    }
}
