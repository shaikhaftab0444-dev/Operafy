using System.Collections.Generic;

namespace ERP_System.Models
{
    public class ExpenseViewModel
    {
        public List<Transaction> Expenses { get; set; } = new List<Transaction>();
        public List<AccountHead> AccountHeadsList { get; set; } = new List<AccountHead>();
        public List<string> CategoriesList { get; set; } = new List<string>();

        // KPI metrics
        public decimal TotalExpenseSpend { get; set; }
        public int TotalVouchersCount { get; set; }
        public int PaidVouchersCount { get; set; }
        public int PendingVouchersCount { get; set; }
        public decimal TotalPendingAmount { get; set; }

        // Filter states
        public string SearchTerm { get; set; } = string.Empty;
        public string SelectedStatus { get; set; } = string.Empty;
        public string SelectedCategory { get; set; } = string.Empty;
    }
}
