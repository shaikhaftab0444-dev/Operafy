using System;
using System.Collections.Generic;

namespace ERP_System.Models
{
    public class LoginAuditViewModel
    {
        public List<AdminLoginAudit> Logs { get; set; } = new List<AdminLoginAudit>();

        // Filters
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SelectedStatus { get; set; }
        public string? SelectedRole { get; set; }
        public string? SearchQuery { get; set; }

        // Metrics
        public int TotalLoginsToday { get; set; }
        public int ActiveLiveSessions { get; set; }
        public int FailedAttempts { get; set; }
        public int LockedAccounts { get; set; }

        // Lists for filters
        public List<string> RolesList { get; set; } = new List<string>();
    }
}
