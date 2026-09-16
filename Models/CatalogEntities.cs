using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_ProductCategories")]
    public class ProductCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty; // e.g. "Electronics"

        [StringLength(50)]
        public string? HsnCode { get; set; } // e.g. "8471"

        [Column(TypeName = "decimal(18,2)")]
        public decimal DefaultGstRate { get; set; } = 18.0m;

        public bool IsActive { get; set; } = true;

        public virtual ICollection<ProductSubCategory> SubCategories { get; set; } = new List<ProductSubCategory>();
        public virtual ICollection<CatalogItem> Items { get; set; } = new List<CatalogItem>();
    }

    [Table("erp_ProductSubCategories")]
    public class ProductSubCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual ProductCategory? Category { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty; // e.g. "Laptops", "Cameras"

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<CatalogItem> Items { get; set; } = new List<CatalogItem>();
    }

    [Table("erp_UnitsOfMeasure")]
    public class UnitOfMeasure
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty; // e.g. "PCS", "KG", "BOX"

        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        public bool IsBaseUnit { get; set; } = true;

        public bool AllowDecimals { get; set; } = false; // False for PCS, True for KG

        [StringLength(20)]
        public string? OfficialGstUomCode { get; set; } // e.g. "NOS", "KGS"

        public bool IsActive { get; set; } = true;
    }

    [Table("erp_UomConversions")]
    public class UomConversion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FromUomId { get; set; }

        [ForeignKey("FromUomId")]
        public virtual UnitOfMeasure? FromUom { get; set; } // e.g. BOX

        [Required]
        public int ToUomId { get; set; }

        [ForeignKey("ToUomId")]
        public virtual UnitOfMeasure? ToUom { get; set; } // e.g. PCS

        [Column(TypeName = "decimal(18,4)")]
        public decimal ConversionFactor { get; set; } // 1 BOX = 10 PCS -> Factor = 10
    }

    [Table("erp_CatalogItems")]
    public class CatalogItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string SKU { get; set; } = string.Empty; // e.g. "SKU-EL-0921"

        [Required]
        [StringLength(100)]
        public string Barcode { get; set; } = string.Empty; // e.g. "890123456789"

        [Required]
        [StringLength(250)]
        public string ItemName { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual ProductCategory? Category { get; set; }

        public int? SubCategoryId { get; set; }

        [ForeignKey("SubCategoryId")]
        public virtual ProductSubCategory? SubCategory { get; set; }

        [Required]
        public int UomId { get; set; }

        [ForeignKey("UomId")]
        public virtual UnitOfMeasure? Uom { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchasePrice { get; set; } // Cost Price

        [Column(TypeName = "decimal(18,2)")]
        public decimal SellingPrice { get; set; } // Sale/MRP

        public int CurrentStock { get; set; }

        public int MinimumReorderLevel { get; set; }

        [StringLength(150)]
        public string BranchLocation { get; set; } = "Head Office";

        [StringLength(100)]
        public string? BinLocation { get; set; } // e.g. "Rack A-2"

        public int UnitsSold { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public string StockStatus => CurrentStock <= 0 ? "Out of Stock" : (CurrentStock <= MinimumReorderLevel ? "Low Stock" : "In Stock");
    }

    // ViewModels and DTOs for the Catalog Pages
    public class ItemsPageViewModel
    {
        public List<CatalogItem> Items { get; set; } = new List<CatalogItem>();
        public List<ProductCategory> Categories { get; set; } = new List<ProductCategory>();
        public List<ProductSubCategory> SubCategories { get; set; } = new List<ProductSubCategory>();
        public List<UnitOfMeasure> UnitsOfMeasure { get; set; } = new List<UnitOfMeasure>();
        public string? SearchTerm { get; set; }
        public int? SelectedCategoryId { get; set; }
        public string? SelectedStatus { get; set; }
        public int TotalItemsCount { get; set; }
        public int InStockCount { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
    }

    public class CategoriesPageViewModel
    {
        public List<ProductCategory> Categories { get; set; } = new List<ProductCategory>();
        public int TotalCategoriesCount { get; set; }
        public int TotalSubCategoriesCount { get; set; }
        public int TotalLinkedProductsCount { get; set; }
    }

    public class UomPageViewModel
    {
        public List<UnitOfMeasure> UnitsOfMeasure { get; set; } = new List<UnitOfMeasure>();
        public List<UomConversion> Conversions { get; set; } = new List<UomConversion>();
        public int TotalUomCount { get; set; }
        public int BaseUnitsCount { get; set; }
        public int ConversionUnitsCount { get; set; }
    }

    public class RestockTriggerDto
    {
        public int? CatalogItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int ReorderLevel { get; set; }
        public int SuggestedQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string UrgencyLevel { get; set; } = "High Priority";
        public string? Justification { get; set; }
    }

    public class StockAdjustmentDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string AdjustmentType { get; set; } = "Add"; // "Add" or "Deduct"
        public int AdjustmentQty { get; set; }
        public string Reason { get; set; } = string.Empty; // Damaged, Loss, Audit Reconciliation, etc.
        public string? Remarks { get; set; }
    }
}
