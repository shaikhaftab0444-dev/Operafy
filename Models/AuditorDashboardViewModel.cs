using System.Collections.Generic;
using ERP_System.Models;

namespace ERP_System.Models
{
    public class AuditorDashboardViewModel
    {
        public string CurrentUserFullName { get; set; } = string.Empty;
        public string CurrentUserRole { get; set; } = string.Empty;
        public string CurrentCompany { get; set; } = string.Empty;

        // Auditing Metrics
        public int RegisteredCompaniesCount { get; set; }
        public int RegisteredUsersCount { get; set; }
        public int TotalRolesCount { get; set; }
        public int AccountHeadsCount { get; set; }
        public int TotalTransactionsCount { get; set; }

        public List<ActivityLog> RecentActivities { get; set; } = new List<ActivityLog>();
        public List<Transaction> AuditTransactions { get; set; } = new List<Transaction>();
    }
}
