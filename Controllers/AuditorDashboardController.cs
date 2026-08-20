using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,Auditor")]
    public class AuditorDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditorDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /AuditorDashboard or /Auditor
        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? category)
        {
            var totalActivityLogsCount = await _context.ActivityLogs.CountAsync();
            var totalTransactionsCount = await _context.Transactions.CountAsync();
            var stockAdjustmentsCount = await _context.StockAdjustments.CountAsync();
            var totalRolePermissionsCount = await _context.RolePermissions.CountAsync();
            var totalUsersCount = await _context.Users.CountAsync();
            var totalRolesCount = await _context.Roles.CountAsync();

            var totalCompaniesCount = await _context.Companies.CountAsync();
            var totalBranchesCount = await _context.Branches.CountAsync();
            var totalSuppliersCount = await _context.Suppliers.CountAsync();
            var totalCustomersCount = await _context.Customers.CountAsync();

            var totalRevenue = await _context.Transactions
                .Where(t => t.Type == "Sales Invoice" || t.Type == "Income")
                .SumAsync(t => (decimal?)t.Amount) ?? 0m;

            var totalPurchases = await _context.Transactions
                .Where(t => t.Type == "Purchase Order" || t.Type == "Expense" || t.Type == "Expense Entry")
                .SumAsync(t => (decimal?)t.Amount) ?? 0m;

            var pendingTxnCount = await _context.Transactions
                .Where(t => t.Status == "Pending")
                .CountAsync();

            var highValuePendingTxnCount = await _context.Transactions
                .Where(t => t.Status == "Pending" && t.Amount >= 50000)
                .CountAsync();

            var today = DateTime.UtcNow.Date;
            var activityLogsTodayCount = await _context.ActivityLogs
                .Where(a => a.CreatedAt >= today)
                .CountAsync();

            var stockDecreaseCount = await _context.StockAdjustments
                .Where(s => s.AdjustmentType == "Decrease" || s.AdjustmentType == "Reduction" || s.QuantityChange < 0)
                .CountAsync();

            var stockIncreaseCount = await _context.StockAdjustments
                .Where(s => s.AdjustmentType == "Increase" || s.AdjustmentType == "Addition" || s.QuantityChange > 0)
                .CountAsync();

            // Event Category Breakdown
            var securityLogsCount = await _context.ActivityLogs
                .Where(a => a.Title.Contains("User") || a.Title.Contains("Role") || a.Title.Contains("Login") || a.Title.Contains("Permission") || a.Title.Contains("Password"))
                .CountAsync();

            var financialLogsCount = await _context.ActivityLogs
                .Where(a => a.Title.Contains("Invoice") || a.Title.Contains("Payment") || a.Title.Contains("Expense") || a.Title.Contains("Purchase") || a.Title.Contains("Transaction"))
                .CountAsync();

            var inventoryLogsCount = await _context.ActivityLogs
                .Where(a => a.Title.Contains("Stock") || a.Title.Contains("Product") || a.Title.Contains("Inventory") || a.Title.Contains("Item"))
                .CountAsync();

            var systemLogsCount = Math.Max(0, totalActivityLogsCount - (securityLogsCount + financialLogsCount + inventoryLogsCount));

            // Filtering query for activity logs
            var logsQuery = _context.ActivityLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                logsQuery = logsQuery.Where(l => l.Title.ToLower().Contains(s) || (l.Description != null && l.Description.ToLower().Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                if (category == "Security")
                {
                    logsQuery = logsQuery.Where(a => a.Title.Contains("User") || a.Title.Contains("Role") || a.Title.Contains("Login") || a.Title.Contains("Permission"));
                }
                else if (category == "Financial")
                {
                    logsQuery = logsQuery.Where(a => a.Title.Contains("Invoice") || a.Title.Contains("Payment") || a.Title.Contains("Expense") || a.Title.Contains("Purchase"));
                }
                else if (category == "Inventory")
                {
                    logsQuery = logsQuery.Where(a => a.Title.Contains("Stock") || a.Title.Contains("Product") || a.Title.Contains("Inventory"));
                }
            }

            var recentActivityLogs = await logsQuery
                .OrderByDescending(a => a.CreatedAt)
                .Take(15)
                .ToListAsync();

            var recentTransactions = await _context.Transactions
                .OrderByDescending(t => t.Date)
                .Take(10)
                .ToListAsync();

            var recentStockAdjustments = await _context.StockAdjustments
                .OrderByDescending(s => s.CreatedAt)
                .Take(10)
                .ToListAsync();

            var systemRolePermissions = await _context.RolePermissions
                .Include(rp => rp.Role)
                .OrderBy(rp => rp.RoleId)
                .Take(12)
                .ToListAsync();

            double complianceScore = 98.4;
            if (stockDecreaseCount > 5) complianceScore -= 2.0;
            if (pendingTxnCount > 10) complianceScore -= 3.5;
            if (complianceScore < 70) complianceScore = 70;

            var viewModel = new AuditorDashboardViewModel
            {
                TotalActivityLogsCount = totalActivityLogsCount,
                TotalTransactionsCount = totalTransactionsCount,
                StockAdjustmentsCount = stockAdjustmentsCount,
                TotalRolePermissionsCount = totalRolePermissionsCount,
                TotalUsersCount = totalUsersCount,
                TotalRolesCount = totalRolesCount,
                TotalCompaniesCount = totalCompaniesCount,
                TotalBranchesCount = totalBranchesCount,
                TotalSuppliersCount = totalSuppliersCount,
                TotalCustomersCount = totalCustomersCount,
                TotalRevenue = totalRevenue,
                TotalPurchases = totalPurchases,
                NetMargin = totalRevenue - totalPurchases,
                PendingTransactionsCount = pendingTxnCount,
                HighValuePendingTxnCount = highValuePendingTxnCount,
                ComplianceScore = Math.Round(complianceScore, 1),
                ActivityLogsTodayCount = activityLogsTodayCount,
                StockDecreaseCount = stockDecreaseCount,
                StockIncreaseCount = stockIncreaseCount,
                SecurityLogsCount = securityLogsCount,
                FinancialLogsCount = financialLogsCount,
                InventoryLogsCount = inventoryLogsCount,
                SystemLogsCount = systemLogsCount,
                SearchQuery = search,
                SelectedCategory = category,
                RecentActivityLogs = recentActivityLogs,
                RecentTransactions = recentTransactions,
                RecentStockAdjustments = recentStockAdjustments,
                SystemRolePermissions = systemRolePermissions
            };

            return View(viewModel);
        }
    }
}
