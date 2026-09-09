using System;
using System.Collections.Generic;

namespace ERP_System.Models
{
    public class AuditorDashboardViewModel
    {
        // Core Auditor Command Center Metrics & Lists
        public int TotalAuditsConducted { get; set; }
        public int PendingDiscrepanciesCount { get; set; }
        public int HighRiskFlagsCount { get; set; }
        public string TotalDiscrepancyAmount { get; set; } = "₹ 0.00";

        public List<AuditFlaggedItem> FlaggedItems { get; set; } = new();
        public List<SystemAuditTrail> RecentAuditLogs { get; set; } = new();

        // Backward compatibility properties for legacy Auditor dashboard views
        public int TotalActivityLogsCount { get; set; }
        public int TotalTransactionsCount { get; set; }
        public int StockAdjustmentsCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalPurchases { get; set; }
        public decimal NetMargin { get; set; }
        public int TotalRolePermissionsCount { get; set; }
        public List<ActivityLog> RecentActivityLogs { get; set; } = new();
        public List<Transaction> RecentTransactions { get; set; } = new();
        public List<StockAdjustment> RecentStockAdjustments { get; set; } = new();
        public List<RolePermission> SystemRolePermissions { get; set; } = new();
    }

    public class UpdateAuditStatusInput
    {
        public int FlagId { get; set; }
        public string NewStatus { get; set; } = string.Empty;
        public string? AuditNote { get; set; }
    }

    public class CreateAuditFlagInput
    {
        public string Module { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal DiscrepancyAmount { get; set; }
        public string Severity { get; set; } = "Medium";
    }
}
