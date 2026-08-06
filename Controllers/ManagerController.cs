using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,Manager")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Manager
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var totalTeamMembers = await _context.Users.CountAsync();
            var totalProducts = await _context.Products.CountAsync();
            var lowStockCount = await _context.Products.CountAsync(p => p.Status == "Low Stock" || p.Status == "Out of Stock");
            var pendingApprovalsCount = await _context.Transactions.CountAsync(t => t.Status == "Pending");

            var pendingTransactions = await _context.Transactions
                .Where(t => t.Status == "Pending")
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            var stockAlerts = await _context.Products
                .Where(p => p.Status == "Low Stock" || p.Status == "Out of Stock")
                .OrderBy(p => p.StockQty)
                .ToListAsync();

            var recentOperationsLog = await _context.ActivityLogs
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();

            var model = new ManagerDashboardViewModel
            {
                TotalTeamMembers = totalTeamMembers,
                TotalProducts = totalProducts,
                LowStockCount = lowStockCount,
                PendingApprovalsCount = pendingApprovalsCount,
                PendingTransactions = pendingTransactions,
                StockAlerts = stockAlerts,
                RecentOperationsLog = recentOperationsLog
            };

            return View(model);
        }

        // POST: /Manager/ApproveTransaction/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveTransaction(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            transaction.Status = "Paid"; // Approve and mark as Paid
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Transaction '{transaction.TransactionNo}' approved successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
