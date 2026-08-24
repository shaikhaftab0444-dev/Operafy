using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System.Threading.Tasks;
using System.Linq;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin")]
    public class SuperAdminDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SuperAdminDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /SuperAdminDashboard
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.TotalCompanies = await _context.Companies.CountAsync();
            ViewBag.TotalBranches = await _context.Branches.CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.ActiveUsers = await _context.Users.Where(u => u.IsActive).CountAsync();
            ViewBag.RecentLogs = await _context.ActivityLogs.OrderByDescending(a => a.ActivityLogId).Take(5).ToListAsync();
            return View();
        }
    }
}
