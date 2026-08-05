using System.Collections.Generic;

namespace ERP_System.Models
{
    public class DashboardViewModel
    {
        public decimal TotalSales { get; set; } = 1245000;
        public decimal TotalPurchase { get; set; } = 875000;
        public int TotalCustomers { get; set; } = 1245;
        public int TotalEmployees { get; set; } = 48;
        public int TotalProducts { get; set; } = 320;
        public decimal StockValue { get; set; } = 1865000;

        public List<Transaction> RecentTransactions { get; set; } = new List<Transaction>();
        public List<Product> TopProducts { get; set; } = new List<Product>();
        public List<ActivityLog> RecentActivities { get; set; } = new List<ActivityLog>();

        public string CurrentUserFullName { get; set; } = string.Empty;
        public string CurrentUserRole { get; set; } = string.Empty;
        public string CurrentCompany { get; set; } = string.Empty;
    }
}
