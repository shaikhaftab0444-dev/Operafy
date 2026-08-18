using System.Collections.Generic;

namespace ERP_System.Models
{
    public class InventoryViewModel
    {
        public List<Product> Products { get; set; } = new List<Product>();
        public List<StockAdjustment> RecentAdjustments { get; set; } = new List<StockAdjustment>();
        public List<Branch> BranchesList { get; set; } = new List<Branch>();
        public List<string> CategoriesList { get; set; } = new List<string>();

        // KPI metrics
        public decimal TotalStockValue { get; set; }
        public int TotalSKUs { get; set; }
        public int TotalItemsInStock { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }

        // Filter states
        public string SearchTerm { get; set; } = string.Empty;
        public string SelectedCategory { get; set; } = string.Empty;
        public string SelectedStatus { get; set; } = string.Empty;
        public int? SelectedBranchId { get; set; }
    }
}
