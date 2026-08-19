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
            // =====================================================
            // EMPLOYEE/STAFF ONLY
            // Exclude Super Admin and Admin completely
            // =====================================================
            var employeeQuery = _context.Users
                .Include(u => u.Role)
                .Where(u =>
                    u.Role != null &&
                    u.Role.RoleName != null &&
                    u.Role.RoleName.Trim() != "Admin" &&
                    u.Role.RoleName.Trim() != "Super Admin"
                );

            // =====================================================
            // KPI COUNTS
            // =====================================================
            var totalEmployees = await employeeQuery.CountAsync();

            var activeEmployees = await employeeQuery
                .CountAsync(u => u.IsActive);

            var inactiveEmployees = await employeeQuery
                .CountAsync(u => !u.IsActive);

            var lockedAccounts = await employeeQuery
                .CountAsync(u => u.IsLocked);

            // =====================================================
            // RECENT EMPLOYEES
            // =====================================================
            var recentHires = await employeeQuery
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .ToListAsync();

            // =====================================================
            // EMPLOYEE DIRECTORY
            // =====================================================
            var employeesList = await employeeQuery
                .OrderBy(u => u.FullName)
                .ToListAsync();

            // =====================================================
            // ROLE DISTRIBUTION
            // Admin and Super Admin are already excluded
            // =====================================================
            var roleDistribution = await employeeQuery
                .GroupBy(u => u.Role!.RoleName!.Trim())
                .Select(g => new RoleDistributionItem
                {
                    RoleName = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            // =====================================================
            // DASHBOARD VIEW MODEL
            // =====================================================
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