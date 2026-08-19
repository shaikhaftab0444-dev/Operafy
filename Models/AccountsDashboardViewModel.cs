using System.Collections.Generic;
using ERP_System.Models;

namespace ERP_System.Models
{
    public class AccountsDashboardViewModel
    {
        public string CurrentUserFullName { get; set; } = string.Empty;
        public string CurrentUserRole { get; set; } = string.Empty;
        public string CurrentCompany { get; set; } = string.Empty;

        // Financial Metrics
        public decimal TotalSalesRevenue { get; set; }
        public decimal TotalPurchaseExpenses { get; set; }
        public decimal TotalOtherExpenses { get; set; }
        public decimal CashBankBalance { get; set; }
        public decimal TotalReceivable { get; set; }
        public decimal TotalPayable { get; set; }

        public List<Transaction> RecentTransactions { get; set; } = new List<Transaction>();
        public List<AccountHead> ChartOfAccounts { get; set; } = new List<AccountHead>();
    }
}
