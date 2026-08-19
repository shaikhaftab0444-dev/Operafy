using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Data;
using ERP_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,Manager,Accountant,Finance Manager,Auditor")]
    public class AccountsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Accounts
        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? accountType, string? voucherType, string? tab)
        {
            // Seed default Chart of Accounts if empty
            if (!await _context.AccountHeads.AnyAsync())
            {
                await SeedDefaultAccountHeadsAsync();
            }

            // Fetch Account Heads
            var headQuery = _context.AccountHeads.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                headQuery = headQuery.Where(a => a.HeadCode.Contains(search) || a.HeadName.Contains(search) || (a.Description != null && a.Description.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(accountType))
            {
                headQuery = headQuery.Where(a => a.AccountType == accountType);
            }

            var accountHeads = await headQuery.OrderBy(a => a.AccountType).ThenBy(a => a.HeadCode).ToListAsync();
            var allHeads = await _context.AccountHeads.ToListAsync();

            // Fetch Transactions / Vouchers
            var txQuery = _context.Transactions.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                txQuery = txQuery.Where(t => t.TransactionNo.Contains(search) || t.PartyName.Contains(search) || t.Type.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(voucherType))
            {
                txQuery = txQuery.Where(t => t.Type == voucherType);
            }

            var transactions = await txQuery.OrderByDescending(t => t.Date).ToListAsync();
            var allTransactions = await _context.Transactions.ToListAsync();

            // Fetch Financial Years
            var financialYears = await _context.FinancialYears.OrderByDescending(f => f.StartDate).ToListAsync();
            if (!financialYears.Any())
            {
                financialYears = GetDefaultFinancialYears();
            }

            // Calculate Metrics
            decimal totalSalesRevenue = allTransactions.Where(t => t.Type == "Sales Invoice" || t.Type == "Receipt Voucher").Sum(t => t.Amount);
            decimal totalExpenseSpend = allTransactions.Where(t => t.Type == "Expense Entry" || t.Type == "Payment Voucher").Sum(t => t.Amount);

            // Default baseline calculations if DB transactions are few
            decimal revenueVal = totalSalesRevenue > 0 ? totalSalesRevenue : 1245000m;
            decimal expenseVal = totalExpenseSpend > 0 ? totalExpenseSpend : 482000m;
            decimal netFlowVal = revenueVal - expenseVal;
            decimal totalAssetsVal = 3850000m + netFlowVal;

            int pendingCount = allTransactions.Count(t => t.Status == "Pending");
            decimal pendingAmt = allTransactions.Where(t => t.Status == "Pending").Sum(t => t.Amount);

            // Bank Accounts Overview
            var bankAccounts = new List<BankAccountSummary>
            {
                new BankAccountSummary
                {
                    AccountName = "HDFC Corporate Current Account",
                    AccountNumber = "50200048192031",
                    AccountType = "Checking / Operating",
                    BankName = "HDFC Bank",
                    Balance = 2485000m,
                    Currency = "INR",
                    Status = "Active",
                    IconClass = "fa-building-columns",
                    BadgeClass = "bg-primary-subtle text-primary"
                },
                new BankAccountSummary
                {
                    AccountName = "ICICI Commercial Reserve Account",
                    AccountNumber = "000405019284",
                    AccountType = "Savings / Reserve",
                    BankName = "ICICI Bank",
                    Balance = 1120000m,
                    Currency = "INR",
                    Status = "Active",
                    IconClass = "fa-vault",
                    BadgeClass = "bg-success-subtle text-success"
                },
                new BankAccountSummary
                {
                    AccountName = "Head Office Main Cash Fund",
                    AccountNumber = "CASH-HO-01",
                    AccountType = "Petty Cash Account",
                    BankName = "In-House Vault",
                    Balance = 145000m,
                    Currency = "INR",
                    Status = "Active",
                    IconClass = "fa-money-bill-wave",
                    BadgeClass = "bg-warning-subtle text-warning"
                },
                new BankAccountSummary
                {
                    AccountName = "SBI Payroll Escrow Account",
                    AccountNumber = "319201948291",
                    AccountType = "Payroll Settlement",
                    BankName = "State Bank of India",
                    Balance = 100000m,
                    Currency = "INR",
                    Status = "Active",
                    IconClass = "fa-hand-holding-dollar",
                    BadgeClass = "bg-info-subtle text-info"
                }
            };

            var viewModel = new AccountsViewModel
            {
                TotalAssets = totalAssetsVal,
                TotalRevenue = revenueVal,
                TotalExpenses = expenseVal,
                NetCashFlow = netFlowVal,
                TotalAccountHeads = allHeads.Count,
                ActiveAccountHeadsCount = allHeads.Count(a => a.IsActive),
                PendingVouchersCount = pendingCount > 0 ? pendingCount : 2,
                PendingVouchersAmount = pendingAmt > 0 ? pendingAmt : 40500m,

                AssetHeadsCount = allHeads.Count(a => a.AccountType == "Asset"),
                LiabilityHeadsCount = allHeads.Count(a => a.AccountType == "Liability"),
                EquityHeadsCount = allHeads.Count(a => a.AccountType == "Equity"),
                RevenueHeadsCount = allHeads.Count(a => a.AccountType == "Revenue"),
                ExpenseHeadsCount = allHeads.Count(a => a.AccountType == "Expense"),

                AccountHeads = accountHeads,
                Transactions = transactions,
                BankAccounts = bankAccounts,
                FinancialYears = financialYears,

                SearchTerm = search ?? string.Empty,
                SelectedAccountType = accountType ?? string.Empty,
                SelectedVoucherType = voucherType ?? string.Empty,
                ActiveTab = string.IsNullOrWhiteSpace(tab) ? "overview" : tab
            };

            return View(viewModel);
        }

        // POST: /Accounts/CreateAccountHead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAccountHead(string headCode, string headName, string accountType, string? description)
        {
            if (string.IsNullOrWhiteSpace(headCode) || string.IsNullOrWhiteSpace(headName) || string.IsNullOrWhiteSpace(accountType))
            {
                TempData["ErrorMessage"] = "Head Code, Head Name, and Account Type are required.";
                return RedirectToAction(nameof(Index), new { tab = "chart-of-accounts" });
            }

            var codeExists = await _context.AccountHeads.AnyAsync(a => a.HeadCode.Trim().ToLower() == headCode.Trim().ToLower());
            if (codeExists)
            {
                TempData["ErrorMessage"] = $"Account Head Code '{headCode}' is already registered.";
                return RedirectToAction(nameof(Index), new { tab = "chart-of-accounts" });
            }

            var accountHead = new AccountHead
            {
                HeadCode = headCode.Trim().ToUpper(),
                HeadName = headName.Trim(),
                AccountType = accountType.Trim(),
                Description = description?.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.AccountHeads.Add(accountHead);
            await _context.SaveChangesAsync();

            // Log activity
            _context.ActivityLogs.Add(new ActivityLog
            {
                Title = "Account Head Created",
                Description = $"New financial account head '{accountHead.HeadName}' ({accountHead.HeadCode}) added under {accountHead.AccountType}.",
                IconClass = "fa-scale-balanced",
                ColorClass = "text-primary",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Account Head '{accountHead.HeadName}' created successfully.";
            return RedirectToAction(nameof(Index), new { tab = "chart-of-accounts" });
        }

        // POST: /Accounts/CreateVoucher
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVoucher(string voucherType, string partyName, decimal amount, string status, string? notes)
        {
            if (string.IsNullOrWhiteSpace(voucherType) || string.IsNullOrWhiteSpace(partyName) || amount <= 0)
            {
                TempData["ErrorMessage"] = "Please provide valid voucher type, party/account name, and a positive amount.";
                return RedirectToAction(nameof(Index), new { tab = "journal" });
            }

            var totalCount = await _context.Transactions.CountAsync() + 1;
            string prefix = voucherType switch
            {
                "Sales Invoice" => "INV",
                "Purchase Order" => "PO",
                "Expense Entry" => "EXP",
                "Payment Voucher" => "PV",
                "Receipt Voucher" => "RV",
                _ => "JV"
            };

            var voucherNo = $"{prefix}-100{totalCount:D2}";

            var transaction = new Transaction
            {
                TransactionNo = voucherNo,
                Type = voucherType,
                Date = DateTime.Now,
                PartyName = partyName.Trim(),
                Amount = amount,
                Status = string.IsNullOrWhiteSpace(status) ? "Paid" : status
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            // Log activity
            _context.ActivityLogs.Add(new ActivityLog
            {
                Title = "Financial Voucher Issued",
                Description = $"Voucher {voucherNo} ({voucherType}) of ₹{amount:N2} recorded for '{partyName}'.",
                IconClass = "fa-file-invoice-dollar",
                ColorClass = "text-success",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Financial Voucher '{voucherNo}' recorded successfully.";
            return RedirectToAction(nameof(Index), new { tab = "journal" });
        }

        // POST: /Accounts/ToggleHeadStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleHeadStatus(int id)
        {
            var head = await _context.AccountHeads.FindAsync(id);
            if (head == null)
            {
                TempData["ErrorMessage"] = "Account Head not found.";
                return RedirectToAction(nameof(Index), new { tab = "chart-of-accounts" });
            }

            head.IsActive = !head.IsActive;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Account Head '{head.HeadName}' status updated.";
            return RedirectToAction(nameof(Index), new { tab = "chart-of-accounts" });
        }

        private async Task SeedDefaultAccountHeadsAsync()
        {
            var defaults = new List<AccountHead>
            {
                // Assets
                new AccountHead { HeadCode = "AC-1001", HeadName = "Cash in Hand", AccountType = "Asset", Description = "Main operating petty cash vault balance.", IsActive = true, CreatedAt = DateTime.UtcNow },
                new AccountHead { HeadCode = "AC-1002", HeadName = "HDFC Corporate Bank", AccountType = "Asset", Description = "Primary commercial checking bank account.", IsActive = true, CreatedAt = DateTime.UtcNow },
                new AccountHead { HeadCode = "AC-1003", HeadName = "Accounts Receivable", AccountType = "Asset", Description = "Customer outstanding invoices balance.", IsActive = true, CreatedAt = DateTime.UtcNow },
                new AccountHead { HeadCode = "AC-1004", HeadName = "Inventory Stock Assets", AccountType = "Asset", Description = "Valuation of current physical warehouse inventory.", IsActive = true, CreatedAt = DateTime.UtcNow },
                new AccountHead { HeadCode = "AC-1005", HeadName = "Office Equipment & IT Assets", AccountType = "Asset", Description = "Hardware, laptops, and office fixtures value.", IsActive = true, CreatedAt = DateTime.UtcNow },

                // Liabilities
                new AccountHead { HeadCode = "AC-2001", HeadName = "Accounts Payable", AccountType = "Liability", Description = "Outstanding payments owed to suppliers.", IsActive = true, CreatedAt = DateTime.UtcNow },
                new AccountHead { HeadCode = "AC-2002", HeadName = "GST & Statutory Taxes Payable", AccountType = "Liability", Description = "GST and withholding taxes collected.", IsActive = true, CreatedAt = DateTime.UtcNow },
                new AccountHead { HeadCode = "AC-2003", HeadName = "Accrued Salaries Payable", AccountType = "Liability", Description = "Employee monthly payroll liability.", IsActive = true, CreatedAt = DateTime.UtcNow },

                // Equity
                new AccountHead { HeadCode = "AC-3001", HeadName = "Shareholders Share Capital", AccountType = "Equity", Description = "Paid-in equity capital.", IsActive = true, CreatedAt = DateTime.UtcNow },
                new AccountHead { HeadCode = "AC-3002", HeadName = "Retained Earnings", AccountType = "Equity", Description = "Accumulated earnings retained in company.", IsActive = true, CreatedAt = DateTime.UtcNow },

                // Revenue
                new AccountHead { HeadCode = "AC-4001", HeadName = "Product Sales Revenue", AccountType = "Revenue", Description = "Income generated from inventory goods sold.", IsActive = true, CreatedAt = DateTime.UtcNow },
                new AccountHead { HeadCode = "AC-4002", HeadName = "Service & Consulting Income", AccountType = "Revenue", Description = "Revenue from software & maintenance contracts.", IsActive = true, CreatedAt = DateTime.UtcNow },

                // Expenses
                new AccountHead { HeadCode = "AC-5001", HeadName = "Office Premises Rent", AccountType = "Expense", Description = "Monthly facility and office lease cost.", IsActive = true, CreatedAt = DateTime.UtcNow },
                new AccountHead { HeadCode = "AC-5002", HeadName = "Utilities & Power", AccountType = "Expense", Description = "Electricity, internet, and municipal utilities.", IsActive = true, CreatedAt = DateTime.UtcNow },
                new AccountHead { HeadCode = "AC-5003", HeadName = "Employee Salaries & Payroll", AccountType = "Expense", Description = "Monthly payroll compensation and benefits.", IsActive = true, CreatedAt = DateTime.UtcNow },
                new AccountHead { HeadCode = "AC-5004", HeadName = "Marketing & Advertising", AccountType = "Expense", Description = "Digital ads, events, and collateral expenses.", IsActive = true, CreatedAt = DateTime.UtcNow }
            };

            await _context.AccountHeads.AddRangeAsync(defaults);
            await _context.SaveChangesAsync();
        }

        private List<FinancialYear> GetDefaultFinancialYears()
        {
            return new List<FinancialYear>
            {
                new FinancialYear { FinancialYearId = 1, YearName = "FY 2026-2027", StartDate = DateTime.Parse("2026-04-01"), EndDate = DateTime.Parse("2027-03-31"), IsCurrent = true, IsActive = true },
                new FinancialYear { FinancialYearId = 2, YearName = "FY 2025-2026", StartDate = DateTime.Parse("2025-04-01"), EndDate = DateTime.Parse("2026-03-31"), IsCurrent = false, IsActive = true },
                new FinancialYear { FinancialYearId = 3, YearName = "FY 2024-2025", StartDate = DateTime.Parse("2024-04-01"), EndDate = DateTime.Parse("2025-03-31"), IsCurrent = false, IsActive = false }
            };
        }
    }
}
