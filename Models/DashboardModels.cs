using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("Transactions")]
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Required]
        [StringLength(50)]
        public string TransactionNo { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // Sales Invoice, Purchase Order, Expense Entry

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(100)]
        public string PartyName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Paid, Pending, Cancelled
    }

    [Table("Products")]
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        [StringLength(100)]
        public string ProductName { get; set; } = string.Empty;

        public int SoldQty { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Revenue { get; set; }

        public int StockQty { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "In Stock"; // In Stock, Low Stock, Out of Stock

        [StringLength(100)]
        public string Category { get; set; } = "General";

        public int BranchId { get; set; } = 3;

        [ForeignKey("BranchId")]
        public Branch? Branch { get; set; }
    }

    [Table("ActivityLogs")]
    public class ActivityLog
    {
        [Key]
        public int ActivityLogId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string IconClass { get; set; } = "fa-info-circle";

        [StringLength(50)]
        public string ColorClass { get; set; } = "text-primary";
    }
}
