using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using ERP_System.Models;

namespace ERP_System.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Dashboard
        public async Task<IActionResult> Index()
        {
            var fullName = User.Identity?.Name ?? "User Account";
            var roleName = User.FindFirstValue(ClaimTypes.Role) ?? "Super Admin";
            var company = User.FindFirstValue("Company") ?? "ERP Solutions Ltd";

            var model = new DashboardViewModel
            {
                CurrentUserFullName = fullName,
                CurrentUserRole = roleName,
                CurrentCompany = company,
                
                // Fetch dynamic collections from SQL Server Db
                RecentTransactions = await _context.Transactions
                    .OrderByDescending(t => t.Date)
                    .Take(5)
                    .ToListAsync(),
                    
                TopProducts = await _context.Products
                    .OrderByDescending(p => p.SoldQty)
                    .Take(5)
                    .ToListAsync(),
                    
                RecentActivities = await _context.ActivityLogs
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }
    }
}
