using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("StockAdjustments")]
    public class StockAdjustment
    {
        [Key]
        public int StockAdjustmentId { get; set; }

        public int ProductId { get; set; }

        [Required]
        [StringLength(100)]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string AdjustmentType { get; set; } = "Restock"; // Restock, Spoilage/Damage, Audit Correction, Return

        public int PreviousQty { get; set; }

        public int QuantityChange { get; set; } // Positive for add, negative for remove

        public int NewQty { get; set; }

        [StringLength(255)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(100)]
        public string PerformedBy { get; set; } = "System Admin";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
    }
}
