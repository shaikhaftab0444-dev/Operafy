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
        public List<SystemMutationLog> RecentMutationLogs { get; set; } = new();

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

    public class UpdateFlagStatusInput
    {
        public int FlagId { get; set; }
        public string Decision { get; set; } = string.Empty; // "Cleared" or "Escalated"
        public string? ResolutionNotes { get; set; }
    }

    public class CreateAuditFlagInput
    {
        public string Module { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal DiscrepancyAmount { get; set; }
        public string Severity { get; set; } = "Medium";
    }

    public class AuditorGeneralLedgerViewModel
    {
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal Variance { get; set; }
        public bool IsEquilibrium => Variance == 0;
        public string SelectedQuarter { get; set; } = "All";
        public string SearchQuery { get; set; } = string.Empty;
        public List<JournalVoucher> Vouchers { get; set; } = new();
    }

    public class FinancialVoucherAuditItemViewModel
    {
        public int Id { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal DiscrepancyAmount { get; set; }
        public string Severity { get; set; } = "Medium";
        public string Status { get; set; } = "Pending Review";
        public DateTime FlaggedOn { get; set; }
        public string? ResolutionNotes { get; set; }
        public string? AuditedByUserName { get; set; }
        public DateTime? ResolvedAt { get; set; }

        // 3-Way Match Details
        public decimal PoExpectedAmount { get; set; }
        public decimal GrnReceivedAmount { get; set; }
        public decimal VendorClaimedAmount { get; set; }
        public string PoNumber { get; set; } = string.Empty;
        public string GrnNumber { get; set; } = string.Empty;
        public string VendorInvoiceNumber { get; set; } = string.Empty;
        public string VendorName { get; set; } = string.Empty;
        public bool IsThreeWayMatch => PoExpectedAmount == GrnReceivedAmount && GrnReceivedAmount == VendorClaimedAmount;
    }
}
