using System;
using System.Collections.Generic;

namespace ERP_System.Models
{
    public class UserActivityReportViewModel
    {
        public List<AuditLogEntry> Logs { get; set; } = new List<AuditLogEntry>();
        
        public int TotalEvents { get; set; }
        public int ModificationsToday { get; set; }
        public int CriticalAlerts { get; set; }
        public int ActiveUsersCount { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SelectedUser { get; set; }
        public string? SelectedModule { get; set; }
        public string? SelectedSeverity { get; set; }
        public string? SearchQuery { get; set; }

        public List<string> UserNames { get; set; } = new List<string>();
    }
}
