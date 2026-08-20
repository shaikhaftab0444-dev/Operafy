using System.Collections.Generic;

namespace ERP_System.Models
{
    public class AuditorDashboardViewModel
    {
        // Core Summary Counts
        public int TotalActivityLogsCount { get; set; }
        public int TotalTransactionsCount { get; set; }
        public int StockAdjustmentsCount { get; set; }
        public int TotalRolePermissionsCount { get; set; }
        public int TotalUsersCount { get; set; }
        public int TotalRolesCount { get; set; }

        // Master Telemetry Counts
        public int TotalCompaniesCount { get; set; }
        public int TotalBranchesCount { get; set; }
        public int TotalSuppliersCount { get; set; }
        public int TotalCustomersCount { get; set; }

        // Financial Metrics
        public decimal TotalRevenue { get; set; }
        public decimal TotalPurchases { get; set; }
        public decimal NetMargin { get; set; }
        public int PendingTransactionsCount { get; set; }
        public int HighValuePendingTxnCount { get; set; }

        // Audit Telemetry & Risk Indicators
        public double ComplianceScore { get; set; } = 98.4;
        public int ActivityLogsTodayCount { get; set; }
        public int StockDecreaseCount { get; set; }
        public int StockIncreaseCount { get; set; }

        // Event Category Breakdown
        public int SecurityLogsCount { get; set; }
        public int FinancialLogsCount { get; set; }
        public int InventoryLogsCount { get; set; }
        public int SystemLogsCount { get; set; }

        // Filter / Search parameters
        public string? SearchQuery { get; set; }
        public string? SelectedCategory { get; set; }

        // Data Collections
        public List<ActivityLog> RecentActivityLogs { get; set; } = new List<ActivityLog>();
        public List<Transaction> RecentTransactions { get; set; } = new List<Transaction>();
        public List<StockAdjustment> RecentStockAdjustments { get; set; } = new List<StockAdjustment>();
        public List<RolePermission> SystemRolePermissions { get; set; } = new List<RolePermission>();
    }
}
