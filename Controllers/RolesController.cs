using ERP_System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin")]
    public class RolesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RolesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Roles
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var roles = await _context.Roles.OrderBy(r => r.RoleName).ToListAsync();
            
            // Count users in each role
            var userCountsList = await _context.Users
                .Where(u => u.RoleId != null)
                .GroupBy(u => u.RoleId)
                .Select(g => new { RoleId = g.Key, Count = g.Count() })
                .ToListAsync();

            var userCounts = new Dictionary<int, int>();
            foreach (var item in userCountsList)
            {
                if (item.RoleId.HasValue)
                {
                    userCounts[item.RoleId.Value] = item.Count;
                }
            }

            ViewBag.UserCounts = userCounts;
            return View(roles);
        }
    }
}
