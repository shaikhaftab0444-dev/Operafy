using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin")]
    public class AdminUserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminUserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /AdminUser/Directory
        [HttpGet]
        public async Task<IActionResult> Directory()
        {
            var users = await _context.Users.Include(u => u.Role).Include(u => u.Branch).ToListAsync();
            return View(users);
        }

        // GET: /AdminUser/AddEdit
        [HttpGet]
        public async Task<IActionResult> AddEdit()
        {
            ViewBag.Roles = await _context.Roles.ToListAsync();
            ViewBag.Branches = await _context.Branches.ToListAsync();
            return View();
        }

        // GET: /AdminUser/PasswordResets
        [HttpGet]
        public async Task<IActionResult> PasswordResets()
        {
            var resets = await _context.AdminPasswordResets.OrderByDescending(r => r.ResetId).ToListAsync();
            return View(resets);
        }

        // POST: /AdminUser/ApproveReset
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveReset(int id)
        {
            var reset = await _context.AdminPasswordResets.FindAsync(id);
            if (reset != null)
            {
                reset.Status = "Completed";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(PasswordResets));
        }

        // GET: /AdminUser/Locks
        [HttpGet]
        public async Task<IActionResult> Locks()
        {
            var users = await _context.Users.Include(u => u.Role).ToListAsync();
            return View(users);
        }

        // POST: /AdminUser/ToggleLock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLock(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Locks));
        }
    }
}
