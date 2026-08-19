using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_Customers")]
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Required]
        [StringLength(200)]
        public string CustomerName { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Email { get; set; }

        [StringLength(40)]
        public string? PhoneNumber { get; set; }

        public DateTime? JoinedDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
