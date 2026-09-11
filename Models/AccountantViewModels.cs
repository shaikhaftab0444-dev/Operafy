using System;
using System.Collections.Generic;

namespace ERP_System.Models
{
    public class AccountantDashboardViewModel
    {
        public decimal TodayInflow { get; set; }
        public decimal TodayOutflow { get; set; }
        public int PendingVouchersCount { get; set; }
        public int UnreconciledItemsCount { get; set; }

        // Direct Tasks assigned to this Accountant by Finance Manager
        public List<HierarchicalTask> AssignedTasks { get; set; } = new();

        // Operational Lists
        public List<JournalVoucher> RecentVouchers { get; set; } = new();
        public List<BankReconciliationItem> BankAccounts { get; set; } = new();
    }

    public class CreateVoucherInput
    {
        public string VoucherType { get; set; } = "Payment";
        public string DebitAccount { get; set; } = string.Empty;
        public string CreditAccount { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Narration { get; set; } = string.Empty;
    }
}
