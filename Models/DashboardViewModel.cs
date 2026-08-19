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

        public int InStockQty { get; set; }
        public int LowStockQty { get; set; }
        public int OutOfStockQty { get; set; }

        public List<Transaction> RecentTransactions { get; set; } = new List<Transaction>();
        public List<Product> TopProducts { get; set; } = new List<Product>();
        public List<ActivityLog> RecentActivities { get; set; } = new List<ActivityLog>();

        public string CurrentUserFullName { get; set; } = string.Empty;
        public string CurrentUserRole { get; set; } = string.Empty;
        public string CurrentCompany { get; set; } = string.Empty;

        // Custom properties for screenshot KPI cards
        public string ActiveMonthName { get; set; } = "February 2023";
        public string CurrencySymbol { get; set; } = "PKR";
        public decimal CurrentMonthRevenue { get; set; } = 0;
        public decimal CurrentMonthExpenses { get; set; } = 0;
        public decimal CurrentMonthRecovery { get; set; } = 0;
        public decimal NetProfitLoss { get; set; } = 432706;
        public decimal OwnerCapital { get; set; } = 180000;
        public decimal CashBankBalance { get; set; } = 639131;
        public decimal TotalReceivable { get; set; } = 130250;
        public decimal TotalPayable { get; set; } = 26425;

        // Daily sale/purchase metrics
        public decimal TodaySales { get; set; } = 0;
        public decimal TodaySalesPending { get; set; } = 0;
        public decimal TodaySalesPaid { get; set; } = 0;
        public decimal TodayPurchases { get; set; } = 0;
        public decimal TodayPurchasesPending { get; set; } = 0;
        public decimal TodayPurchasesPaid { get; set; } = 0;
    }
}
