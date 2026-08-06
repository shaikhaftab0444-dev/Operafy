using System.Collections.Generic;

namespace ERP_System.Models
{
    public class ManagerDashboardViewModel
    {
        public int TotalTeamMembers { get; set; }
        public int TotalProducts { get; set; }
        public int LowStockCount { get; set; }
        public int PendingApprovalsCount { get; set; }

        public List<Transaction> PendingTransactions { get; set; } = new List<Transaction>();
        public List<Product> StockAlerts { get; set; } = new List<Product>();
        public List<ActivityLog> RecentOperationsLog { get; set; } = new List<ActivityLog>();
    }
}
