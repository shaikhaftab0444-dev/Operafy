using System.Collections.Generic;
using ERP_System.Models;

namespace ERP_System.Models
{
    public class InventoryDashboardViewModel
    {
        public string CurrentUserFullName { get; set; } = string.Empty;
        public string CurrentUserRole { get; set; } = string.Empty;
        public string CurrentCompany { get; set; } = string.Empty;

        // Inventory Metrics
        public int TotalProductsCount { get; set; }
        public long TotalStockQuantity { get; set; }
        public long TotalStockValue { get; set; }
        public int LowStockItemsCount { get; set; }
        public int OutOfStockItemsCount { get; set; }
        public decimal PendingPurchaseOrdersAmount { get; set; }

        public List<Product> TopProducts { get; set; } = new List<Product>();
        public List<Product> LowStockProducts { get; set; } = new List<Product>();
    }
}
