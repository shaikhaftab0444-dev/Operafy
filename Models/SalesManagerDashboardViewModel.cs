using System;
using System.Collections.Generic;
using ERP_System.Models;

namespace ERP_System.Models
{
    public class RepLeaderboardItem
    {
        public string RepId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int DealsClosed { get; set; }
        public bool IsActive { get; set; }
    }

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
        public decimal GoalAmount { get; set; } = 15000000m;

        public Dictionary<string, (int Count, decimal Value)> PipelineStages { get; set; } = new();
        public List<SalesQuotation> PendingApprovals { get; set; } = new();
        public List<RepLeaderboardItem> Leaderboard { get; set; } = new();
        public List<SalesInvoice> RecentInvoices { get; set; } = new();
        public List<User> RepUsers { get; set; } = new();

        public string DateRange { get; set; } = "Q3 2026";
        public string? Territory { get; set; }

        public List<Transaction> RecentTeamTransactions { get; set; } = new List<Transaction>();
        public List<User> TopExecutives { get; set; } = new List<User>();
        public List<ActivityLog> RecentActivities { get; set; } = new List<ActivityLog>();
    }
}
