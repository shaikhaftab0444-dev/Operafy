using System.Collections.Generic;
using ERP_System.Models;

namespace ERP_System.Models
{
    public class SalesDashboardViewModel
    {
        public string CurrentUserFullName { get; set; } = string.Empty;
        public string CurrentUserRole { get; set; } = string.Empty;
        public string CurrentCompany { get; set; } = string.Empty;

        // Sales Metrics
        public decimal TotalSalesRevenue { get; set; }
        public int TotalInvoicesCount { get; set; }
        public int PaidInvoicesCount { get; set; }
        public int PendingReceivablesCount { get; set; }
        public decimal TotalPendingAmount { get; set; }

        // Today's Sales
        public decimal TodaySales { get; set; }
        public decimal TodaySalesPending { get; set; }
        public decimal TodaySalesPaid { get; set; }

        // Customers & Products Stats
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }

        // Lists for Grid
        public List<Transaction> RecentSales { get; set; } = new List<Transaction>();
        public List<Product> TopProducts { get; set; } = new List<Product>();
    }
}
