using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ERP_System.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
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

            ViewBag.CurrentUser = currentUser;
            ViewBag.ActivityLogs = activityLogs;
            ViewBag.Transactions = transactions;

            return View();
        }
    }
}
