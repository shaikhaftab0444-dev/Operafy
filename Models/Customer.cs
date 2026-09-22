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

        public string CustomerCode { get; set; } = string.Empty; // e.g., "CUST#0009"

        private string? _customerName;
        public string CustomerName
        {
            get => !string.IsNullOrWhiteSpace(_customerName) 
                ? _customerName 
                : $"{FirstName} {LastName}".Trim();
            set
            {
                _customerName = value;
                if (!string.IsNullOrWhiteSpace(value) && string.IsNullOrWhiteSpace(FirstName))
                {
                    var parts = value.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                    FirstName = parts.Length > 0 ? parts[0] : value;
                    LastName = parts.Length > 1 ? parts[1] : string.Empty;
                }
            }
        }

        public string? CompanyName { get; set; }
        public string? TaxIdOrGSTIN { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        [StringLength(450)]
        public string Email { get; set; } = string.Empty;

        [NotMapped]
        public string Phone
        {
            get => PhoneNumber;
            set => PhoneNumber = value ?? string.Empty;
        }

        [StringLength(50)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal CreditLimit { get; set; } = 500000m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal OutstandingBalance { get; set; } = 0m;

        public string? AssignedRepId { get; set; }
        public int? AssignedRepUserUserId { get; set; }

        [ForeignKey("AssignedRepUserUserId")]
        public virtual User? AssignedRepUser { get; set; }

        [NotMapped]
        public virtual ApplicationUser? AssignedRep
        {
            get
            {
                if (AssignedRepUser == null) return null;
                if (AssignedRepUser is ApplicationUser appUser) return appUser;
                return new ApplicationUser
                {
                    UserId = AssignedRepUser.UserId,
                    FullName = AssignedRepUser.FullName,
                    Email = AssignedRepUser.Email,
                    UserName = AssignedRepUser.UserName,
                    Role = AssignedRepUser.Role
                };
            }
            set => AssignedRepUser = value;
        }

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Joined Date")]
        public DateTime? JoinedDate
        {
            get => CreatedAt;
            set => CreatedAt = value ?? DateTime.UtcNow;
        }

        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; } = DateTime.UtcNow.AddYears(-20);

        public string Password { get; set; } = "Default@123";
    }
}