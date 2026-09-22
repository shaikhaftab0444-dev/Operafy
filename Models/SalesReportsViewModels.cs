using System;
using System.Collections.Generic;

namespace ERP_System.Models
{
    // ==========================================
    // 1. Sales Summary Report ViewModels
    // ==========================================
    public class MonthlySalesSummaryItem
    {
        public string MonthYear { get; set; } = string.Empty; // e.g. "Apr 2026"
        public int Year { get; set; }
        public int Month { get; set; }
        public int TotalInvoices { get; set; }
        public decimal GrossSales { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal NetRevenue { get; set; }
        public int HeightPercentage { get; set; } = 40; // Scaled CSS bar height (0-100)
        public string BarColorClass { get; set; } = "bg-primary";
        public decimal GrowthRate { get; set; }
    }

    public class SalesSummaryReportViewModel
    {
        public string Period { get; set; } = "6M"; // "6M", "1Y", "ALL"
        public decimal TotalRevenue { get; set; }
        public int TotalInvoicesCount { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal NetRevenue { get; set; }
        public decimal TotalTaxes { get; set; }
        public List<MonthlySalesSummaryItem> MonthlyLedger { get; set; } = new List<MonthlySalesSummaryItem>();
    }

    // ==========================================
    // 2. Sales By Customer Report ViewModels
    // ==========================================
    public class CustomerSalesReportViewModel
    {
        public int CustomerId { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int InvoicesCount { get; set; }
        public decimal GrossRevenue { get; set; }
        public decimal ContributionPercentage { get; set; }
        public decimal AverageOrderValue { get; set; }
        public string Status { get; set; } = "Active";
    }

    public class CustomerReportsListViewModel
    {
        public List<CustomerSalesReportViewModel> Customers { get; set; } = new List<CustomerSalesReportViewModel>();
        public decimal TotalRevenue { get; set; }
        public int TotalInvoices { get; set; }
        public int TotalActiveCustomers { get; set; }
        public string TopCustomerName { get; set; } = string.Empty;
        public decimal TopCustomerRevenue { get; set; }
    }

    public class CustomerInvoiceDrilldownItem
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public string LinkedOrderNumber { get; set; } = string.Empty;
        public string InvoiceDate { get; set; } = string.Empty;
        public string DueDate { get; set; } = string.Empty;
        public decimal TaxableValue { get; set; }
        public decimal GstAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Paid";
    }

    // ==========================================
    // 3. Sales By Product Report ViewModels
    // ==========================================
    public class ProductSalesReportViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int UnitsSold { get; set; }
        public decimal TotalSales { get; set; }
        public decimal ContributionPercentage { get; set; }
        public decimal UnitSellingPrice { get; set; }
        public string StockStatus { get; set; } = "In Stock";
        public int FreeStock { get; set; }
    }

    public class ProductReportsListViewModel
    {
        public List<ProductSalesReportViewModel> Products { get; set; } = new List<ProductSalesReportViewModel>();
        public decimal TotalProductRevenue { get; set; }
        public int TotalUnitsSold { get; set; }
        public string TopSellingProduct { get; set; } = string.Empty;
        public string DominantCategory { get; set; } = string.Empty;
    }

    // ==========================================
    // 4. Sales By Salesperson Report ViewModels
    // ==========================================
    public class SalespersonReportViewModel
    {
        public string SalespersonName { get; set; } = string.Empty;
        public string ExecCode { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public decimal TargetMonth { get; set; }
        public decimal SalesAchieved { get; set; }
        public decimal AchievementPercentage { get; set; }
        public string PacingStatus { get; set; } = "On Track"; // "Ahead", "On Track", "Lagging Target"
        public string MobileNumber { get; set; } = string.Empty;
    }

    public class SalespersonReportsListViewModel
    {
        public List<SalespersonReportViewModel> Salespeople { get; set; } = new List<SalespersonReportViewModel>();
        public decimal TotalTeamTarget { get; set; }
        public decimal TotalTeamAchieved { get; set; }
        public decimal TeamAchievementPercentage { get; set; }
        public string TopPerformerName { get; set; } = string.Empty;
        public decimal TopPerformerRevenue { get; set; }
    }

    // ==========================================
    // 5. Sales Target Report ViewModels
    // ==========================================
    public class TargetReportViewModel
    {
        public string Quarter { get; set; } = "Q3 2026";
        public decimal TargetAmount { get; set; }
        public decimal AchievedAmount { get; set; }
        public decimal Variance { get; set; } // Achieved - Target
        public string Status { get; set; } = "Under Target"; // "Exceeded", "Under Target"
        public decimal AchievementPercentage { get; set; }
    }

    public class CategoryTargetBreakdownItem
    {
        public string Category { get; set; } = string.Empty;
        public decimal TargetAmount { get; set; }
        public decimal AchievedAmount { get; set; }
        public decimal AchievementPercentage { get; set; }
    }

    public class TargetReportsListViewModel
    {
        public string SelectedQuarter { get; set; } = "All";
        public List<TargetReportViewModel> Quarters { get; set; } = new List<TargetReportViewModel>();
        public List<CategoryTargetBreakdownItem> CategoryBreakdowns { get; set; } = new List<CategoryTargetBreakdownItem>();
        public decimal TotalTarget { get; set; }
        public decimal TotalAchieved { get; set; }
        public decimal TotalVariance { get; set; }
        public decimal OverallAchievementPercentage { get; set; }
    }

    // ==========================================
    // 6. Outstanding Receivables Aging Report ViewModels
    // ==========================================
    public class CustomerAgingViewModel
    {
        public int CustomerId { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public decimal Days0_30 { get; set; }
        public decimal Days31_60 { get; set; }
        public decimal Days61_90 { get; set; }
        public decimal Days90Plus { get; set; }
        public decimal TotalDue { get; set; }
        public string RiskLevel { get; set; } = "Normal"; // "Normal", "Watch", "High Risk"
    }

    public class ReceivablesAgingReportViewModel
    {
        public List<CustomerAgingViewModel> Customers { get; set; } = new List<CustomerAgingViewModel>();
        public decimal TotalOutstanding { get; set; }
        public decimal Total0_30 { get; set; }
        public decimal Total31_60 { get; set; }
        public decimal Total61_90 { get; set; }
        public decimal Total90Plus { get; set; }
        public int OverdueAccountsCount { get; set; }
    }
}
