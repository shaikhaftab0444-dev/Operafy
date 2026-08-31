using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERP_System.Data;
using ERP_System.Models;

namespace ERP_System.Controllers
{
    public class FinanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        private static List<PayableBillViewModel> _bills = new List<PayableBillViewModel>
        {
            new PayableBillViewModel { BillId = 1, BillNo = "VND-2026-9081", VendorName = "TechCorp Solutions", PurchaseOrderNo = "PO-2026-0041", GrnNo = "GRN-2026-041", BillDate = DateTime.Today.AddDays(-10), DueDate = DateTime.Today.AddDays(5), Amount = 125000.00m, Status = "Pending Approval", PoMatch = true, GrnMatch = true, PriceMatch = true },
            new PayableBillViewModel { BillId = 2, BillNo = "VND-2026-9082", VendorName = "Logix Logistics", PurchaseOrderNo = "PO-2026-0042", GrnNo = "GRN-2026-042", BillDate = DateTime.Today.AddDays(-5), DueDate = DateTime.Today.AddDays(15), Amount = 48000.00m, Status = "Approved", PoMatch = true, GrnMatch = true, PriceMatch = true },
            new PayableBillViewModel { BillId = 3, BillNo = "VND-2026-9083", VendorName = "Global Office Supplies", PurchaseOrderNo = "PO-2026-0043", GrnNo = "GRN-2026-043", BillDate = DateTime.Today.AddDays(-12), DueDate = DateTime.Today.AddDays(-2), Amount = 64000.00m, Status = "Pending Approval", PoMatch = true, GrnMatch = false, PriceMatch = true },
            new PayableBillViewModel { BillId = 4, BillNo = "VND-2026-9084", VendorName = "Spark Power Grid", PurchaseOrderNo = "PO-2026-0044", GrnNo = "GRN-2026-044", BillDate = DateTime.Today.AddDays(-8), DueDate = DateTime.Today.AddDays(1), Amount = 185000.00m, Status = "Pending Approval", PoMatch = true, GrnMatch = true, PriceMatch = true },
            new PayableBillViewModel { BillId = 5, BillNo = "VND-2026-9085", VendorName = "Precision Mfg Inc", PurchaseOrderNo = "PO-2026-0045", GrnNo = "GRN-2026-045", BillDate = DateTime.Today.AddDays(-3), DueDate = DateTime.Today.AddDays(12), Amount = 35000.00m, Status = "Pending Approval", PoMatch = true, GrnMatch = true, PriceMatch = true }
        };

        private static List<JournalEntryViewModel> _journalEntries = new List<JournalEntryViewModel>
        {
            new JournalEntryViewModel { JournalId = 1, JournalNo = "JV-2026-0001", PostingDate = DateTime.Today.AddDays(-4), Reference = "REF-99821", Description = "Accrued utilities expense for office building", AccountCode = "AC-5004", AccountName = "Utilities Expense", Debit = 25000.00m, Credit = 0.00m, Status = "Posted" },
            new JournalEntryViewModel { JournalId = 2, JournalNo = "JV-2026-0001", PostingDate = DateTime.Today.AddDays(-4), Reference = "REF-99821", Description = "Accrued utilities expense for office building", AccountCode = "AC-2002", AccountName = "Accrued Liabilities", Debit = 0.00m, Credit = 25000.00m, Status = "Posted" },
            new JournalEntryViewModel { JournalId = 3, JournalNo = "JV-2026-0002", PostingDate = DateTime.Today.AddDays(-2), Reference = "REF-99822", Description = "Inter-company transfer to secondary reserve", AccountCode = "AC-1002", AccountName = "Reserve Bank Account", Debit = 150000.00m, Credit = 0.00m, Status = "Posted" },
            new JournalEntryViewModel { JournalId = 4, JournalNo = "JV-2026-0002", PostingDate = DateTime.Today.AddDays(-2), Reference = "REF-99822", Description = "Inter-company transfer to secondary reserve", AccountCode = "AC-1001", AccountName = "Main Cash Account", Debit = 0.00m, Credit = 150000.00m, Status = "Posted" }
        };

        public FinanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Calculate Receivables from PaymentReceipts
            decimal totalReceivable = await _context.PaymentReceipts
                .Where(pr => pr.Status != "Paid")
                .SumAsync(pr => pr.PendingBalance);

            // Calculate Payables from static list
            decimal totalPayable = _bills
                .Where(b => b.Status != "Paid" && b.Status != "Rejected")
                .Sum(b => b.Amount);

            var model = new FinanceDashboardViewModel
            {
                TotalLiquidCash = 42500000.00m, // ₹ 4.25 Cr
                AccountsReceivableTotal = totalReceivable > 0 ? totalReceivable : 8240000.00m, // Fallback if DB empty
                AccountsPayableTotal = totalPayable,
                NetProfitMargin = 24.8m,
                WorkingCapital = 38500000.00m,
                PendingPaymentAuthorizations = _bills.Where(b => b.Status == "Pending Approval" && b.Amount > 50000.00m).ToList(),
                CashFlowTrend = new List<CashFlowMonth>
                {
                    new CashFlowMonth { Month = "Mar", Inflow = 4500000.00m, Outflow = 3200000.00m },
                    new CashFlowMonth { Month = "Apr", Inflow = 5200000.00m, Outflow = 3800000.00m },
                    new CashFlowMonth { Month = "May", Inflow = 4800000.00m, Outflow = 4100000.00m },
                    new CashFlowMonth { Month = "Jun", Inflow = 6100000.00m, Outflow = 4400000.00m },
                    new CashFlowMonth { Month = "Jul", Inflow = 5900000.00m, Outflow = 4900000.00m },
                    new CashFlowMonth { Month = "Aug", Inflow = 6700000.00m, Outflow = 5100000.00m }
                },
                DepartmentBudgets = new List<DepartmentBudgetViewModel>
                {
                    new DepartmentBudgetViewModel { DepartmentName = "Sales & Marketing", Allocated = 1200000.00m, Utilized = 840000.00m },
                    new DepartmentBudgetViewModel { DepartmentName = "Information Technology", Allocated = 2500000.00m, Utilized = 1950000.00m },
                    new DepartmentBudgetViewModel { DepartmentName = "Human Resources", Allocated = 500000.00m, Utilized = 280000.00m },
                    new DepartmentBudgetViewModel { DepartmentName = "Operations & Supply", Allocated = 3000000.00m, Utilized = 2100000.00m }
                },
                RecentTransactions = _journalEntries.Take(6).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Receivables(string agingFilter = "All", string search = "")
        {
            var query = _context.PaymentReceipts.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(p => p.InvoiceNo.ToLower().Contains(search) || p.CustomerName.ToLower().Contains(search));
            }

            var receipts = await query.ToListAsync();

            // If empty, let's generate mock items inside the DB / in-memory if needed
            if (!receipts.Any())
            {
                receipts = new List<PaymentReceipt>
                {
                    new PaymentReceipt { PaymentReceiptId = 1, InvoiceNo = "INV-2026-1001", CustomerName = "Acme Corp", InvoiceDate = DateTime.Today.AddDays(-15), DueDate = DateTime.Today.AddDays(15), PendingBalance = 450000.00m, Status = "Pending" },
                    new PaymentReceipt { PaymentReceiptId = 2, InvoiceNo = "INV-2026-1002", CustomerName = "Krypton Tech", InvoiceDate = DateTime.Today.AddDays(-45), DueDate = DateTime.Today.AddDays(-15), PendingBalance = 320000.00m, Status = "Overdue" },
                    new PaymentReceipt { PaymentReceiptId = 3, InvoiceNo = "INV-2026-1003", CustomerName = "Apex Logistics", InvoiceDate = DateTime.Today.AddDays(-5), DueDate = DateTime.Today.AddDays(25), PendingBalance = 680000.00m, Status = "Pending" },
                    new PaymentReceipt { PaymentReceiptId = 4, InvoiceNo = "INV-2026-1004", CustomerName = "Zenith Retail", InvoiceDate = DateTime.Today.AddDays(-95), DueDate = DateTime.Today.AddDays(-65), PendingBalance = 150000.00m, Status = "Overdue" },
                    new PaymentReceipt { PaymentReceiptId = 5, InvoiceNo = "INV-2026-1005", CustomerName = "Nimbus Software", InvoiceDate = DateTime.Today.AddDays(-20), DueDate = DateTime.Today.AddDays(10), PendingBalance = 240000.00m, Status = "Pending" }
                };
            }

            var viewModels = receipts.Select(p => new ReceivableInvoiceViewModel
            {
                InvoiceId = p.PaymentReceiptId,
                InvoiceNo = p.InvoiceNo,
                CustomerName = p.CustomerName,
                InvoiceDate = p.InvoiceDate,
                DueDate = p.DueDate,
                TotalAmount = p.PendingBalance,
                BalanceDue = p.PendingBalance,
                Status = p.Status
            }).ToList();

            if (agingFilter != "All")
            {
                viewModels = viewModels.Where(v => v.AgingBucket == agingFilter).ToList();
            }

            ViewBag.AgingFilter = agingFilter;
            ViewBag.SearchTerm = search;
            return View(viewModels);
        }

        [HttpGet]
        public IActionResult Payables(string statusFilter = "All", string search = "")
        {
            var bills = _bills.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                bills = bills.Where(b => b.BillNo.ToLower().Contains(search) || b.VendorName.ToLower().Contains(search));
            }

            if (statusFilter != "All")
            {
                bills = bills.Where(b => b.Status == statusFilter);
            }

            ViewBag.StatusFilter = statusFilter;
            ViewBag.SearchTerm = search;
            return View(bills.ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Ledger(string search = "")
        {
            var accountsQuery = _context.AccountHeads.Where(a => a.IsActive).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                accountsQuery = accountsQuery.Where(a => a.HeadName.ToLower().Contains(search) || a.HeadCode.ToLower().Contains(search));
            }

            var accounts = await accountsQuery.OrderBy(a => a.HeadCode).ToListAsync();

            if (!accounts.Any())
            {
                // Return default account heads if DB table is empty
                accounts = new List<AccountHead>
                {
                    new AccountHead { AccountHeadId = 1, HeadCode = "AC-1001", HeadName = "Main Cash Account", AccountType = "Asset", Description = "Primary cash holding" },
                    new AccountHead { AccountHeadId = 2, HeadCode = "AC-1002", HeadName = "Reserve Bank Account", AccountType = "Asset", Description = "High-interest bank deposit" },
                    new AccountHead { AccountHeadId = 3, HeadCode = "AC-2001", HeadName = "Accounts Payable Control", AccountType = "Liability", Description = "AP liability ledger" },
                    new AccountHead { AccountHeadId = 4, HeadCode = "AC-2002", HeadName = "Accrued Liabilities", AccountType = "Liability", Description = "Unbilled accrued expense" },
                    new AccountHead { AccountHeadId = 5, HeadCode = "AC-3001", HeadName = "Share Capital", AccountType = "Equity", Description = "Equity capital ledger" },
                    new AccountHead { AccountHeadId = 6, HeadCode = "AC-4001", HeadName = "Sales Revenue", AccountType = "Revenue", Description = "Core business sales" },
                    new AccountHead { AccountHeadId = 7, HeadCode = "AC-5001", HeadName = "Cost of Goods Sold (COGS)", AccountType = "Expense", Description = "Direct material cost" },
                    new AccountHead { AccountHeadId = 8, HeadCode = "AC-5004", HeadName = "Utilities Expense", AccountType = "Expense", Description = "Office utilities expense" }
                };
            }

            ViewBag.AccountHeads = accounts;
            ViewBag.SearchTerm = search;
            return View(_journalEntries);
        }

        [HttpGet]
        public IActionResult Budgets()
        {
            var budgets = new List<DepartmentBudgetViewModel>
            {
                new DepartmentBudgetViewModel { DepartmentName = "Sales & Marketing", Allocated = 1200000.00m, Utilized = 840000.00m },
                new DepartmentBudgetViewModel { DepartmentName = "Information Technology", Allocated = 2500000.00m, Utilized = 1950000.00m },
                new DepartmentBudgetViewModel { DepartmentName = "Human Resources", Allocated = 500000.00m, Utilized = 280000.00m },
                new DepartmentBudgetViewModel { DepartmentName = "Operations & Supply", Allocated = 3000000.00m, Utilized = 2100000.00m },
                new DepartmentBudgetViewModel { DepartmentName = "Administration", Allocated = 800000.00m, Utilized = 450000.00m }
            };

            return View(budgets);
        }

        [HttpGet]
        public IActionResult Taxation()
        {
            var summary = new TaxSummaryViewModel
            {
                Period = "August 2026",
                TaxableSales = 18500000.00m,
                CGSTCollected = 1665000.00m,
                SGSTCollected = 1665000.00m,
                IGSTCollected = 0.00m,
                TaxablePurchases = 9200000.00m,
                CGSTPaid = 828000.00m,
                SGSTPaid = 828000.00m,
                IGSTPaid = 0.00m,
                TdsTaxableBase = 2400000.00m,
                TdsDeducted = 240000.00m,
                FilingStatusList = new List<TaxFilingStatusItem>
                {
                    new TaxFilingStatusItem { FormName = "GSTR-1", Period = "August 2026", DueDate = DateTime.Today.AddDays(11), FilingDate = null, Status = "Pending" },
                    new TaxFilingStatusItem { FormName = "GSTR-3B", Period = "August 2026", DueDate = DateTime.Today.AddDays(20), FilingDate = null, Status = "Pending" },
                    new TaxFilingStatusItem { FormName = "TDS 26Q", Period = "Q2 2026", DueDate = DateTime.Today.AddDays(-30), FilingDate = DateTime.Today.AddDays(-31), Status = "Filed", PaymentChallanNo = "CHL-2026-90812" }
                }
            };

            return View(summary);
        }

        [HttpGet]
        public async Task<IActionResult> Reports(string period = "August 2026")
        {
            var pAndL = new FinancialStatementViewModel
            {
                StatementName = "Profit & Loss Statement",
                Period = period,
                Sections = new List<StatementSection>
                {
                    new StatementSection
                    {
                        SectionName = "Revenue",
                        Items = new List<StatementLineItem>
                        {
                            new StatementLineItem { AccountCode = "AC-4001", AccountName = "Sales Revenue", Amount = 18500000.00m },
                            new StatementLineItem { AccountCode = "AC-4002", AccountName = "Consulting Revenue", Amount = 1450000.00m }
                        },
                        SectionTotal = 19950000.00m
                    },
                    new StatementSection
                    {
                        SectionName = "Cost of Goods Sold (COGS)",
                        Items = new List<StatementLineItem>
                        {
                            new StatementLineItem { AccountCode = "AC-5001", AccountName = "Direct Material Cost", Amount = 9200000.00m },
                            new StatementLineItem { AccountCode = "AC-5002", AccountName = "Freight Inward", Amount = 350000.00m }
                        },
                        SectionTotal = 9550000.00m
                    },
                    new StatementSection
                    {
                        SectionName = "Operating Expenses",
                        Items = new List<StatementLineItem>
                        {
                            new StatementLineItem { AccountCode = "AC-5003", AccountName = "Salaries & Wages", Amount = 4200000.00m },
                            new StatementLineItem { AccountCode = "AC-5004", AccountName = "Utilities Expense", Amount = 120000.00m },
                            new StatementLineItem { AccountCode = "AC-5005", AccountName = "Office Rent", Amount = 450000.00m }
                        },
                        SectionTotal = 4770000.00m
                    }
                },
                NetIncomeOrEquityTotal = 5630000.00m
            };

            var balanceSheet = new FinancialStatementViewModel
            {
                StatementName = "Balance Sheet",
                Period = period,
                Sections = new List<StatementSection>
                {
                    new StatementSection
                    {
                        SectionName = "Assets",
                        Items = new List<StatementLineItem>
                        {
                            new StatementLineItem { AccountCode = "AC-1001", AccountName = "Main Cash Account", Amount = 42500000.00m },
                            new StatementLineItem { AccountCode = "AC-1003", AccountName = "Accounts Receivable", Amount = 8240000.00m },
                            new StatementLineItem { AccountCode = "AC-1005", AccountName = "Inventory Asset", Amount = 15400000.00m }
                        },
                        SectionTotal = 66140000.00m
                    },
                    new StatementSection
                    {
                        SectionName = "Liabilities",
                        Items = new List<StatementLineItem>
                        {
                            new StatementLineItem { AccountCode = "AC-2001", AccountName = "Accounts Payable Control", Amount = 4615000.00m },
                            new StatementLineItem { AccountCode = "AC-2002", AccountName = "Accrued Liabilities", Amount = 25000.00m }
                        },
                        SectionTotal = 4640000.00m
                    },
                    new StatementSection
                    {
                        SectionName = "Equity",
                        Items = new List<StatementLineItem>
                        {
                            new StatementLineItem { AccountCode = "AC-3001", AccountName = "Share Capital", Amount = 50000000.00m },
                            new StatementLineItem { AccountCode = "AC-3002", AccountName = "Retained Earnings", Amount = 11500000.00m }
                        },
                        SectionTotal = 61500000.00m
                    }
                },
                NetIncomeOrEquityTotal = 66140000.00m
            };

            var trialBalance = new FinancialStatementViewModel
            {
                StatementName = "Trial Balance",
                Period = period,
                Sections = new List<StatementSection>
                {
                    new StatementSection
                    {
                        SectionName = "Trial Balance Accounts",
                        Items = new List<StatementLineItem>
                        {
                            new StatementLineItem { AccountCode = "AC-1001", AccountName = "Main Cash Account", Debit = 42500000.00m, Credit = 0.00m },
                            new StatementLineItem { AccountCode = "AC-1003", AccountName = "Accounts Receivable", Debit = 8240000.00m, Credit = 0.00m },
                            new StatementLineItem { AccountCode = "AC-1005", AccountName = "Inventory Asset", Debit = 15400000.00m, Credit = 0.00m },
                            new StatementLineItem { AccountCode = "AC-2001", AccountName = "Accounts Payable Control", Debit = 0.00m, Credit = 4615000.00m },
                            new StatementLineItem { AccountCode = "AC-2002", AccountName = "Accrued Liabilities", Debit = 0.00m, Credit = 25000.00m },
                            new StatementLineItem { AccountCode = "AC-3001", AccountName = "Share Capital", Debit = 0.00m, Credit = 50000000.00m },
                            new StatementLineItem { AccountCode = "AC-3002", AccountName = "Retained Earnings", Debit = 0.00m, Credit = 11500000.00m },
                            new StatementLineItem { AccountCode = "AC-4001", AccountName = "Sales Revenue", Debit = 0.00m, Credit = 18500000.00m },
                            new StatementLineItem { AccountCode = "AC-4002", AccountName = "Consulting Revenue", Debit = 0.00m, Credit = 1450000.00m },
                            new StatementLineItem { AccountCode = "AC-5001", AccountName = "Direct Material Cost", Debit = 9200000.00m, Credit = 0.00m },
                            new StatementLineItem { AccountCode = "AC-5002", AccountName = "Freight Inward", Debit = 350000.00m, Credit = 0.00m },
                            new StatementLineItem { AccountCode = "AC-5003", AccountName = "Salaries & Wages", Debit = 4200000.00m, Credit = 0.00m },
                            new StatementLineItem { AccountCode = "AC-5004", AccountName = "Utilities Expense", Debit = 120000.00m, Credit = 0.00m },
                            new StatementLineItem { AccountCode = "AC-5005", AccountName = "Office Rent", Debit = 450000.00m, Credit = 0.00m }
                        },
                        SectionTotal = 80460000.00m
                    }
                },
                NetIncomeOrEquityTotal = 80460000.00m
            };

            ViewBag.PL = pAndL;
            ViewBag.BS = balanceSheet;
            ViewBag.TB = trialBalance;
            ViewBag.Period = period;

            return View();
        }

        [HttpPost]
        public IActionResult ApprovePaymentRelease(int billId)
        {
            var bill = _bills.FirstOrDefault(b => b.BillId == billId);
            if (bill != null)
            {
                bill.Status = "Approved";
                return Json(new { success = true, message = $"Bill {bill.BillNo} authorized for release." });
            }
            return Json(new { success = false, message = "Bill not found." });
        }

        [HttpPost]
        public async Task<IActionResult> PostJournalEntry([FromBody] JournalEntryInputModel model)
        {
            if (model == null || model.Lines == null || !model.Lines.Any())
            {
                return Json(new { success = false, message = "No lines provided." });
            }

            decimal totalDebit = model.Lines.Sum(l => l.Debit);
            decimal totalCredit = model.Lines.Sum(l => l.Credit);

            if (totalDebit != totalCredit)
            {
                return Json(new { success = false, message = $"Debits (₹{totalDebit}) and Credits (₹{totalCredit}) must balance." });
            }

            string nextNo = $"JV-2026-{(1000 + _journalEntries.Select(j => j.JournalId).DefaultIfEmpty(0).Max()):0000}";

            foreach (var line in model.Lines)
            {
                var account = await _context.AccountHeads.FindAsync(line.AccountHeadId);
                string accCode = account?.HeadCode ?? $"AC-{line.AccountHeadId}";
                string accName = account?.HeadName ?? "Unknown Account";

                _journalEntries.Insert(0, new JournalEntryViewModel
                {
                    JournalId = _journalEntries.Select(j => j.JournalId).DefaultIfEmpty(0).Max() + 1,
                    JournalNo = nextNo,
                    PostingDate = model.PostingDate,
                    Reference = model.Reference,
                    Description = model.Description,
                    AccountCode = accCode,
                    AccountName = accName,
                    Debit = line.Debit,
                    Credit = line.Credit,
                    Status = "Posted"
                });
            }

            return Json(new { success = true, message = $"Journal entry {nextNo} posted successfully." });
        }
    }
}
