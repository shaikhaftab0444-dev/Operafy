using System;
using System.Collections.Generic;

namespace ERP_System.Models
{
    public class SalesDashboardViewModel
    {
        public string MtdRevenue { get; set; } = "₹ 0.00";
        public string PipelineValue { get; set; } = "₹ 0.00";
        public int OpenLeadsCount { get; set; }
        public double WinRatePercentage { get; set; }

        // Subordinate Sales Team Pulse
        public int TotalExecutivesCount { get; set; }
        public int ExecutivesOnDutyCount { get; set; }
        public int PendingApprovalsCount { get; set; }

        public List<Lead> RecentLeads { get; set; } = new();
        public List<SalesOrder> RecentOrders { get; set; } = new();
        public List<SalesTargetItemVM> TeamTargetProgress { get; set; } = new();

        // Compatibility fields for legacy views & controllers
        public string CurrentUserFullName { get; set; } = string.Empty;
        public string CurrentUserRole { get; set; } = string.Empty;
        public string CurrentCompany { get; set; } = string.Empty;
        public decimal TotalSalesRevenue { get; set; }
        public int TotalInvoicesCount { get; set; }
        public int PaidInvoicesCount { get; set; }
        public int PendingReceivablesCount { get; set; }
        public decimal TotalPendingAmount { get; set; }
        public decimal TodaySales { get; set; }
        public decimal TodaySalesPending { get; set; }
        public decimal TodaySalesPaid { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
        public List<Transaction> RecentSales { get; set; } = new();
        public List<Product> TopProducts { get; set; } = new();
    }

    public class SalesTargetItemVM
    {
        public string ExecutiveName { get; set; } = string.Empty;
        public decimal Target { get; set; }
        public decimal Achieved { get; set; }
        public double PercentComplete => Target > 0 ? (double)(Achieved / Target) * 100 : 0;
    }
}
