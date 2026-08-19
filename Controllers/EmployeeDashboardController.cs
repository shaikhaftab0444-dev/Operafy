using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using ERP_System.Data;

namespace ERP_System.Controllers
{
    [Authorize]
    public class EmployeeDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Employee
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Find current logged in user from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            User? currentUser = null;

            if (int.TryParse(userIdClaim, out int parsedId))
            {
                currentUser = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.UserId == parsedId);
            }

            // Fetch activity logs for staff update feed
            var activityLogs = await _context.ActivityLogs
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Fetch recent transactions for reference
            var transactions = await _context.Transactions
                .OrderByDescending(t => t.Date)
                .Take(5)
                .ToListAsync();

            // Fetch payslips for this logged-in user
            var payslips = new System.Collections.Generic.List<Payslip>();
            if (currentUser != null)
            {
                payslips = await _context.Payslips
                    .Where(p => p.UserId == currentUser.UserId)
                    .OrderByDescending(p => p.PaymentDate)
                    .ToListAsync();
            }

            ViewBag.CurrentUser = currentUser;
            ViewBag.ActivityLogs = activityLogs;
            ViewBag.Transactions = transactions;
            ViewBag.MyPayslips = payslips;

            return View();
        }
    }
}
