using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("Customers")]
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        [StringLength(450)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of Birth is required")]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; } = DateTime.UtcNow.AddYears(-20);

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = "Default@123";

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Joined Date")]
        public DateTime? JoinedDate { get; set; } = DateTime.UtcNow;

        [NotMapped]
        [Display(Name = "Customer Name")]
        public string CustomerName => $"{FirstName} {LastName}".Trim();
    }
}