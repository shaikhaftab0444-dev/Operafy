using System.Collections.Generic;

namespace ERP_System.Models
{
    public class AuditorDashboardViewModel
    {
        public int TotalActivityLogsCount { get; set; }
        public int TotalTransactionsCount { get; set; }
        public int StockAdjustmentsCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalPurchases { get; set; }
        public decimal NetMargin { get; set; }
        public int TotalRolePermissionsCount { get; set; }
        public List<ActivityLog> RecentActivityLogs { get; set; } = new List<ActivityLog>();
        public List<Transaction> RecentTransactions { get; set; } = new List<Transaction>();
        public List<StockAdjustment> RecentStockAdjustments { get; set; } = new List<StockAdjustment>();
        public List<RolePermission> SystemRolePermissions { get; set; } = new List<RolePermission>();
    }
}
