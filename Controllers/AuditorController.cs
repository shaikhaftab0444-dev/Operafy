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
    public class AuditorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Auditor
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var totalActivityLogsCount = await _context.ActivityLogs.CountAsync();
            var totalTransactionsCount = await _context.Transactions.CountAsync();
            var stockAdjustmentsCount = await _context.StockAdjustments.CountAsync();
            var totalRolePermissionsCount = await _context.RolePermissions.CountAsync();

            var totalRevenue = await _context.Transactions
                .Where(t => t.Type == "Sales Invoice" || t.Type == "Income")
                .SumAsync(t => t.Amount);

            var totalPurchases = await _context.Transactions
                .Where(t => t.Type == "Purchase Order" || t.Type == "Expense")
                .SumAsync(t => t.Amount);

            var recentActivityLogs = await _context.ActivityLogs
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .ToListAsync();

            var recentTransactions = await _context.Transactions
                .OrderByDescending(t => t.Date)
                .Take(8)
                .ToListAsync();

            var recentStockAdjustments = await _context.StockAdjustments
                .OrderByDescending(s => s.CreatedAt)
                .Take(8)
                .ToListAsync();

            var systemRolePermissions = await _context.RolePermissions
                .Include(rp => rp.Role)
                .OrderBy(rp => rp.RoleId)
                .Take(10)
                .ToListAsync();

            var viewModel = new AuditorDashboardViewModel
            {
                TotalActivityLogsCount = totalActivityLogsCount,
                TotalTransactionsCount = totalTransactionsCount,
                StockAdjustmentsCount = stockAdjustmentsCount,
                TotalRolePermissionsCount = totalRolePermissionsCount,
                TotalRevenue = totalRevenue,
                TotalPurchases = totalPurchases,
                NetMargin = totalRevenue - totalPurchases,
                RecentActivityLogs = recentActivityLogs,
                RecentTransactions = recentTransactions,
                RecentStockAdjustments = recentStockAdjustments,
                SystemRolePermissions = systemRolePermissions
            };

            return View(viewModel);
        }
    }
}
