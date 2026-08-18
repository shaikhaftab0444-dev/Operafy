using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using ERP_System.Models;

namespace ERP_System.Models
{
    public class ProfileViewModel
    {
        public User User { get; set; } = new User();
        public Company? Company { get; set; }
        public Branch? Branch { get; set; }
        public List<ActivityLog> RecentActivities { get; set; } = new List<ActivityLog>();

        // Form Binding Fields
        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(150)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email Address is required.")]
        [EmailAddress]
        [StringLength(150)]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        [Display(Name = "Mobile Number")]
        public string? MobileNumber { get; set; }

        public string? ProfilePhoto { get; set; }

        [Display(Name = "Upload New Profile Picture")]
        public IFormFile? ProfileImage { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Current Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm Password is required.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        [Display(Name = "Confirm New Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
