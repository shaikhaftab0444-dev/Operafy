using System.Collections.Generic;

namespace ERP_System.Models
{
    public class BranchSummaryReportViewModel
    {
        public List<BranchPerformanceSummary> BranchSummaries { get; set; } = new List<BranchPerformanceSummary>();
        
        public int TotalBranches { get; set; }
        public int TotalHeadcount { get; set; }
        public decimal ConsolidatedRevenue { get; set; }
        public string TopPerformingBranch { get; set; } = string.Empty;

        // Dynamic Chart Lists
        public List<string> BranchNames { get; set; } = new List<string>();
        public List<decimal> MonthlyRevenues { get; set; } = new List<decimal>();
        public List<decimal> MonthlyExpenses { get; set; } = new List<decimal>();
        public List<int> StaffCounts { get; set; } = new List<int>();
    }

    public class BranchPerformanceSummary
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int StaffCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal Expenses { get; set; }
        public decimal StockValuation { get; set; }
        public int TransactionCount { get; set; }
        public string Status { get; set; } = "Active";
    }
}
