using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ERP_System.Models;
using ERP_System.Data;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.ComponentModel.DataAnnotations;

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
        public async Task<IActionResult> AddEdit(int? id)
        {
            ViewBag.Roles = await _context.Roles.Where(r => r.IsActive).ToListAsync();
            ViewBag.Branches = await _context.Branches.ToListAsync();

            var viewModel = new AdminUserAddEditViewModel();
            if (id.HasValue && id.Value > 0)
            {
                var existingUser = await _context.Users.FindAsync(id.Value);
                if (existingUser != null)
                {
                    viewModel.UserId = existingUser.UserId;
                    viewModel.FullName = existingUser.FullName;
                    viewModel.Email = existingUser.Email;
                    viewModel.UserName = existingUser.UserName;
                    viewModel.RoleId = existingUser.RoleId ?? 0;
                    viewModel.BranchId = existingUser.BranchId;
                }
            }

            return View(viewModel);
        }

        // POST: /AdminUser/AddEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEdit(AdminUserAddEditViewModel model)
        {
            if (model.UserId == 0 && string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError("Password", "Password is required for new users.");
            }

            if (ModelState.IsValid)
            {
                // Check for duplicate username or email (excluding current user if editing)
                bool exists = await _context.Users.AnyAsync(u => 
                    (u.UserName.ToLower() == model.UserName.ToLower() || u.Email.ToLower() == model.Email.ToLower()) && 
                    u.UserId != model.UserId);

                if (exists)
                {
                    ModelState.AddModelError(string.Empty, "Username or Email address already exists in the system.");
                    ViewBag.Roles = await _context.Roles.Where(r => r.IsActive).ToListAsync();
                    ViewBag.Branches = await _context.Branches.ToListAsync();
                    return View(model);
                }

                if (model.UserId == 0)
                {
                    // Create New User
                    var maxId = await _context.Users.MaxAsync(u => (int?)u.UserId) ?? 0;
                    var user = new User
                    {
                        FullName = model.FullName.Trim(),
                        Email = model.Email.Trim(),
                        UserName = model.UserName.Trim(),
                        RoleId = model.RoleId,
                        BranchId = model.BranchId,
                        CompanyId = 1, // Default company
                        UserCode = $"USR{(maxId + 1):D3}",
                        IsActive = true,
                        IsLocked = false,
                        IsEmailVerified = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var hasher = new PasswordHasher<User>();
                    user.PasswordHash = hasher.HashPassword(user, model.Password!);

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"User profile '{user.FullName}' created successfully in database (ID: {user.UserId}, Code: {user.UserCode}).";
                }
                else
                {
                    // Update Existing User
                    var existingUser = await _context.Users.FindAsync(model.UserId);
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    existingUser.FullName = model.FullName.Trim();
                    existingUser.Email = model.Email.Trim();
                    existingUser.UserName = model.UserName.Trim();
                    existingUser.RoleId = model.RoleId;
                    existingUser.BranchId = model.BranchId;
                    existingUser.UpdatedAt = DateTime.UtcNow;

                    if (!string.IsNullOrWhiteSpace(model.Password))
                    {
                        var hasher = new PasswordHasher<User>();
                        existingUser.PasswordHash = hasher.HashPassword(existingUser, model.Password);
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"User profile '{existingUser.FullName}' updated successfully.";
                }

                return RedirectToAction(nameof(Directory));
            }

            ViewBag.Roles = await _context.Roles.Where(r => r.IsActive).ToListAsync();
            ViewBag.Branches = await _context.Branches.ToListAsync();
            return View(model);
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

        // POST: /AdminUser/EmergencyReset
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmergencyReset(int id, string tempPassword)
        {
            if (string.IsNullOrEmpty(tempPassword))
            {
                return RedirectToAction(nameof(PasswordResets));
            }

            var reset = await _context.AdminPasswordResets.FindAsync(id);
            if (reset != null)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == reset.Email);
                if (user != null)
                {
                    var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
                    user.PasswordHash = hasher.HashPassword(user, tempPassword);

                    reset.Status = "Manual Override";
                    reset.RequestType = "Admin Override";
                    reset.DeliveryMethod = "Manual OTP";
                    
                    await _context.SaveChangesAsync();
                    TempData["EmergencySuccess"] = $"Emergency temporary password set successfully for user: {reset.Email}";
                }
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

    public class AdminUserAddEditViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(150, ErrorMessage = "Full Name cannot exceed 150 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(150)]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters.")]
        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Role selection is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid user role.")]
        [Display(Name = "Role")]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Office branch selection is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid branch location.")]
        [Display(Name = "Branch")]
        public int BranchId { get; set; }
    }
}
