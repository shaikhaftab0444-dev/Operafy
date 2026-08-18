using System;
using System.Collections.Generic;

namespace ERP_System.Models
{
    public class BankAccountSummary
    {
        public string AccountName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty; // Checking, Savings, Petty Cash, Reserve
        public string BankName { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string Currency { get; set; } = "INR";
        public string Status { get; set; } = "Active";
        public string IconClass { get; set; } = "fa-building-columns";
        public string BadgeClass { get; set; } = "bg-primary-subtle text-primary";
    }

    public class AccountsViewModel
    {
        // KPI Metrics
        public decimal TotalAssets { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetCashFlow { get; set; }
        public int TotalAccountHeads { get; set; }
        public int ActiveAccountHeadsCount { get; set; }
        public int PendingVouchersCount { get; set; }
        public decimal PendingVouchersAmount { get; set; }

        // Data Collections
        public List<AccountHead> AccountHeads { get; set; } = new List<AccountHead>();
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
        public List<BankAccountSummary> BankAccounts { get; set; } = new List<BankAccountSummary>();
        public List<FinancialYear> FinancialYears { get; set; } = new List<FinancialYear>();

        // Account Type breakdown stats
        public int AssetHeadsCount { get; set; }
        public int LiabilityHeadsCount { get; set; }
        public int EquityHeadsCount { get; set; }
        public int RevenueHeadsCount { get; set; }
        public int ExpenseHeadsCount { get; set; }

        // Filters & UI state
        public string SearchTerm { get; set; } = string.Empty;
        public string SelectedAccountType { get; set; } = string.Empty;
        public string SelectedVoucherType { get; set; } = string.Empty;
        public string ActiveTab { get; set; } = "overview";
    }
}
