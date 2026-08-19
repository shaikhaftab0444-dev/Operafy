using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_Payslips")]
    public class Payslip
    {
        [Key]
        public int PayslipId { get; set; }

        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string MonthYear { get; set; } = string.Empty;

        public decimal BasicSalary { get; set; }

        public decimal Allowances { get; set; }

        public decimal Deductions { get; set; }

        public decimal NetPay { get; set; }

        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string Status { get; set; } = "Paid";

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
