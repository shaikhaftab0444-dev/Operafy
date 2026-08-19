using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using System.Linq;
using System.Threading.Tasks;
using ERP_System.Data;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,HR")]
    public class HRController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HRController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /HR
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var totalEmployees = await _context.Users.CountAsync();
            var activeEmployees = await _context.Users.CountAsync(u => u.IsActive);
            var inactiveEmployees = await _context.Users.CountAsync(u => !u.IsActive);
            var lockedAccounts = await _context.Users.CountAsync(u => u.IsLocked);

            var recentHires = await _context.Users
                .Include(u => u.Role)
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .ToListAsync();

            var employeesList = await _context.Users
                .Include(u => u.Role)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var roleDistribution = await _context.Users
                .Include(u => u.Role)
                .GroupBy(u => u.Role != null ? u.Role.RoleName : "Unassigned")
                .Select(g => new RoleDistributionItem
                {
                    RoleName = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var model = new HRDashboardViewModel
            {
                TotalEmployees = totalEmployees,
                ActiveEmployees = activeEmployees,
                InactiveEmployees = inactiveEmployees,
                LockedAccounts = lockedAccounts,
                RecentHires = recentHires,
                EmployeesList = employeesList,
                RoleDistribution = roleDistribution
            };

            return View(model);
        }
    }
}
