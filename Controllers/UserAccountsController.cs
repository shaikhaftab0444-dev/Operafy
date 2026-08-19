using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using ERP_System.Data;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin")]
    public class UserAccountsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserAccountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Users
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return View(users);
        }

        // GET: /Users/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = await _context.Roles
                .Where(r => r.IsActive)
                .ToListAsync();
            return View();
        }

        // POST: /Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if username already exists
                var existingUser = await _context.Users.AnyAsync(u => u.UserName == model.UserName || u.Email == model.Email);
                if (existingUser)
                {
                    ModelState.AddModelError("", "Username or Email already exists in the database.");
                    ViewBag.Roles = await _context.Roles.Where(r => r.IsActive).ToListAsync();
                    return View(model);
                }

                var user = new User
                {
                    UserName = model.UserName,
                    FullName = model.FullName,
                    Email = model.Email,
                    MobileNumber = model.MobileNumber,
                    RoleId = model.RoleId,
                    UserCode = "USR" + new Random().Next(1000, 9999).ToString(),
                    CompanyId = 1, // Seeded Company (AIT Technologies Pvt Ltd)
                    BranchId = 3,  // Seeded Branch (Head Office)
                    IsActive = true,
                    IsLocked = false,
                    IsEmailVerified = true,
                    CreatedAt = DateTime.UtcNow
                };

                var hasher = new PasswordHasher<User>();
                user.PasswordHash = hasher.HashPassword(user, model.Password);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"User '{model.FullName}' created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = await _context.Roles.Where(r => r.IsActive).ToListAsync();
            return View(model);
        }

        // POST: /Users/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Cannot deactivate the seeded Admin account (UserId = 1) to prevent locking out the admin
            if (user.UserId == 1)
            {
                TempData["ErrorMessage"] = "Cannot modify the root Super Admin account.";
                return RedirectToAction(nameof(Index));
            }

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"User '{user.FullName}' status updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /UserAccounts/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (user.UserId == 1)
            {
                TempData["ErrorMessage"] = "Cannot delete the root Super Admin account.";
                return RedirectToAction(nameof(Index));
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"User '{user.FullName}' has been deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }

    public class CreateUserViewModel
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        [Display(Name = "Mobile Number")]
        public string? MobileNumber { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Display(Name = "System Role")]
        public int RoleId { get; set; }
    }
}
