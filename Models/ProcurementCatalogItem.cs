using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_ProcurementCatalogItems")]
    public class ProcurementCatalogItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string ItemName { get; set; } = string.Empty;

        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [StringLength(50)]
        public string UnitOfMeasure { get; set; } = "Unit"; // Box, Pack, Piece, Unit

        public bool IsActive { get; set; } = true;
    }

    [Table("erp_PurchaseRequisitions")]
    public class PurchaseRequisition
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }

        public int CatalogItemId { get; set; }

        [ForeignKey("CatalogItemId")]
        public virtual ProcurementCatalogItem? CatalogItem { get; set; }

        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedTotalCost { get; set; }

        [StringLength(100)]
        public string UrgencyLevel { get; set; } = "Medium Priority";

        [StringLength(1000)]
        public string? BusinessJustification { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
