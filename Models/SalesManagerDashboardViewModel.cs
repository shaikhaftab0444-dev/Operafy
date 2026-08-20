using System.Collections.Generic;
using ERP_System.Models;

namespace ERP_System.Models
{
    public class SalesManagerDashboardViewModel
    {
        public string CurrentUserFullName { get; set; } = string.Empty;
        public string CurrentUserRole { get; set; } = string.Empty;
        public string CurrentCompany { get; set; } = string.Empty;

        // Manager / Team Metrics
        public decimal TotalTeamSalesRevenue { get; set; }
        public int ActiveSalesRepsCount { get; set; }
        public decimal TeamTargetAchievementPercent { get; set; }
        public decimal AverageSalesPerRep { get; set; }

        public List<Transaction> RecentTeamTransactions { get; set; } = new List<Transaction>();
        public List<User> TopExecutives { get; set; } = new List<User>();
        public List<ActivityLog> RecentActivities { get; set; } = new List<ActivityLog>();
    }
}
