using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ERP_System.Models
{
    public class FinanceDashboardViewModel
    {
        public decimal TotalLiquidCash { get; set; }
        public decimal AccountsReceivableTotal { get; set; }
        public decimal AccountsPayableTotal { get; set; }
        public decimal NetProfitMargin { get; set; }
        public decimal WorkingCapital { get; set; }

        public List<PayableBillViewModel> PendingPaymentAuthorizations { get; set; } = new List<PayableBillViewModel>();
        public List<CashFlowMonth> CashFlowTrend { get; set; } = new List<CashFlowMonth>();
        public List<DepartmentBudgetViewModel> DepartmentBudgets { get; set; } = new List<DepartmentBudgetViewModel>();
        public List<JournalEntryViewModel> RecentTransactions { get; set; } = new List<JournalEntryViewModel>();
    }

    public class CashFlowMonth
    {
        public string Month { get; set; } = string.Empty;
        public decimal Inflow { get; set; }
        public decimal Outflow { get; set; }
    }

    public class DepartmentBudgetViewModel
    {
        public string DepartmentName { get; set; } = string.Empty;
        public decimal Allocated { get; set; }
        public decimal Utilized { get; set; }
        public decimal BurnRatePercentage => Allocated > 0 ? (Utilized / Allocated) * 100 : 0;
    }

    public class ReceivableInvoiceViewModel
    {
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal BalanceDue { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Paid, Overdue
        public int AgingDays => (DateTime.Today - InvoiceDate).Days;
        public string AgingBucket
        {
            get
            {
                if (Status == "Paid") return "Current";
                int days = (DateTime.Today - DueDate).Days;
                if (days <= 0) return "Current";
                if (days <= 30) return "1-30 Days";
                if (days <= 60) return "31-60 Days";
                return "Overdue 90+ Days";
            }
        }
    }

    public class PayableBillViewModel
    {
        public int BillId { get; set; }
        public string BillNo { get; set; } = string.Empty;
        public string VendorName { get; set; } = string.Empty;
        public string PurchaseOrderNo { get; set; } = string.Empty;
        public string GrnNo { get; set; } = string.Empty;
        public DateTime BillDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending Match"; // Pending Approval, Approved, Paid, Rejected
        
        // 3-way match indicators
        public bool PoMatch { get; set; }
        public bool GrnMatch { get; set; }
        public bool PriceMatch { get; set; }
        public bool IsThreeWayMatched => PoMatch && GrnMatch && PriceMatch;
    }

    public class JournalEntryViewModel
    {
        public int JournalId { get; set; }
        public string JournalNo { get; set; } = string.Empty;
        public DateTime PostingDate { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string Status { get; set; } = "Posted"; // Draft, Posted, Cancelled
    }

    public class JournalEntryInputModel
    {
        [Required]
        public DateTime PostingDate { get; set; } = DateTime.Today;

        [Required]
        [StringLength(100)]
        public string Reference { get; set; } = string.Empty;

        [StringLength(250)]
        public string Description { get; set; } = string.Empty;

        public List<JournalEntryLineInput> Lines { get; set; } = new List<JournalEntryLineInput>();
    }

    public class JournalEntryLineInput
    {
        [Required]
        public int AccountHeadId { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }

    public class TaxSummaryViewModel
    {
        public string Period { get; set; } = string.Empty;
        public decimal TaxableSales { get; set; }
        public decimal CGSTCollected { get; set; }
        public decimal SGSTCollected { get; set; }
        public decimal IGSTCollected { get; set; }
        public decimal TotalGSTCollected => CGSTCollected + SGSTCollected + IGSTCollected;

        public decimal TaxablePurchases { get; set; }
        public decimal CGSTPaid { get; set; }
        public decimal SGSTPaid { get; set; }
        public decimal IGSTPaid { get; set; }
        public decimal TotalGSTPaid => CGSTPaid + SGSTPaid + IGSTPaid;

        public decimal NetGstPayable => TotalGSTCollected - TotalGSTPaid;

        // TDS items
        public decimal TdsTaxableBase { get; set; }
        public decimal TdsDeducted { get; set; }

        public List<TaxFilingStatusItem> FilingStatusList { get; set; } = new List<TaxFilingStatusItem>();
    }

    public class TaxFilingStatusItem
    {
        public string FormName { get; set; } = string.Empty; // GSTR-1, GSTR-3B, TDS 26Q
        public string Period { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public DateTime? FilingDate { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Filed, Overdue
        public string PaymentChallanNo { get; set; } = string.Empty;
    }

    public class FinancialStatementViewModel
    {
        public string StatementName { get; set; } = string.Empty; // Balance Sheet, P&L, Trial Balance
        public string Period { get; set; } = string.Empty;
        public List<StatementSection> Sections { get; set; } = new List<StatementSection>();
        public decimal NetIncomeOrEquityTotal { get; set; }
    }

    public class StatementSection
    {
        public string SectionName { get; set; } = string.Empty; // Current Assets, Revenue, etc.
        public List<StatementLineItem> Items { get; set; } = new List<StatementLineItem>();
        public decimal SectionTotal { get; set; }
    }

    public class StatementLineItem
    {
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Debit { get; set; } // For Trial Balance
        public decimal Credit { get; set; } // For Trial Balance
    }
}
