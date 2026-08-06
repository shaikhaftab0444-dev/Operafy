using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,HR,Manager")]
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Employees
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var employees = await _context.Users
                .Include(u => u.Role)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            ViewBag.Roles = await _context.Roles.Where(r => r.IsActive).ToListAsync();
            return View(employees);
        }

        // POST: /Employees/UpdateRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(int userId, int roleId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            if (userId == 1)
            {
                TempData["ErrorMessage"] = "Cannot modify the root Super Admin account role.";
                return RedirectToAction(nameof(Index));
            }

            user.RoleId = roleId;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Employee '{user.FullName}' role updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Employees/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (user.UserId == 1)
            {
                TempData["ErrorMessage"] = "Cannot deactivate the root Super Admin account.";
                return RedirectToAction(nameof(Index));
            }

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Employee '{user.FullName}' status changed successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
