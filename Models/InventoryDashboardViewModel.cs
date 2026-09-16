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

        public string SelectedBranch { get; set; } = "All Branches";
        public string SelectedDateRange { get; set; } = "All Time";
        public List<string> Branches { get; set; } = new List<string>();

        public List<Product> TopProducts { get; set; } = new List<Product>();
        public List<Product> LowStockProducts { get; set; } = new List<Product>();
        public List<CatalogItem> LowStockCatalogItems { get; set; } = new List<CatalogItem>();
        public List<CatalogItem> AllCatalogItems { get; set; } = new List<CatalogItem>();
    }

    public class TeamAttendanceRadarItemViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string ShiftName { get; set; } = "General Shift";
        public string ShiftTiming { get; set; } = "09:00 AM - 06:00 PM";
        public string? ClockInTime { get; set; }
        public string? ClockOutTime { get; set; }
        public string WorkHours { get; set; } = "-";
        public string PunchSource { get; set; } = "Biometric WH-Gate";
        public string Status { get; set; } = "Absent"; // Present, Late, Absent, On Leave
        public string WarehouseLocation { get; set; } = "Main Warehouse";
    }
}
