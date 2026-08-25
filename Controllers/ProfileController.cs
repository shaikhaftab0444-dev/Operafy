using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System.Security.Claims;

namespace ERP_System.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: /Profile
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentLoggedInUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var company = await _context.Companies.FirstOrDefaultAsync(c => c.CompanyId == user.CompanyId)
                          ?? await _context.Companies.FirstOrDefaultAsync();

            var activities = await _context.ActivityLogs
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();

            var viewModel = new ProfileViewModel
            {
                User = user,
                Company = company,
                Branch = user.Branch,
                RecentActivities = activities,
                FullName = user.FullName,
                Email = user.Email,
                MobileNumber = user.MobileNumber,
                ProfilePhoto = user.ProfilePhoto ?? "/profile_images/admin-avatar.jpg"
            };

            return View(viewModel);
        }

        // POST: /Profile/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            var user = await GetCurrentLoggedInUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!string.IsNullOrWhiteSpace(model.FullName))
            {
                user.FullName = model.FullName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                // Check email uniqueness if changed
                if (user.Email != model.Email)
                {
                    var emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email && u.UserId != user.UserId);
                    if (emailExists)
                    {
                        TempData["ErrorMessage"] = "Email address is already in use by another user.";
                        return RedirectToAction(nameof(Index));
                    }
                    user.Email = model.Email.Trim();
                }
            }

            user.MobileNumber = model.MobileNumber?.Trim();

            // Handle Profile Image Upload
            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var fileExtension = Path.GetExtension(model.ProfileImage.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    TempData["ErrorMessage"] = "Invalid image file format. Allowed formats: JPG, JPEG, PNG, WEBP.";
                    return RedirectToAction(nameof(Index));
                }

                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "profile_images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"user_{user.UserId}_{DateTime.Now.Ticks}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfileImage.CopyToAsync(fileStream);
                }

                user.ProfilePhoto = $"/profile_images/{uniqueFileName}";
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Record activity log
            _context.ActivityLogs.Add(new ActivityLog
            {
                Title = "Profile Updated",
                Description = $"User '{user.FullName}' updated their profile information.",
                IconClass = "fa-user-check",
                ColorClass = "text-success",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            NotificationsController.AddNotification(new NotificationItem
            {
                Title = "Profile Updated",
                Description = "Your account profile information was updated successfully.",
                Category = "Security",
                CreatedAt = DateTime.Now,
                IsRead = false,
                IconClass = "fa-user-check",
                ColorClass = "text-success",
                BgColorClass = "bg-success-subtle",
                TargetUrl = "/Profile",
                TargetEmail = user.Email
            });

            TempData["SuccessMessage"] = "Profile details updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var user = await GetCurrentLoggedInUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                var hasher = new PasswordHasher<User>();
                var verifyResult = hasher.VerifyHashedPassword(user, user.PasswordHash, model.CurrentPassword);

                if (verifyResult == PasswordVerificationResult.Failed)
                {
                    TempData["ErrorMessage"] = "Incorrect current password. Please try again.";
                    return RedirectToAction(nameof(Index));
                }

                user.PasswordHash = hasher.HashPassword(user, model.NewPassword);
                user.LastPasswordChanged = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Record activity log
                _context.ActivityLogs.Add(new ActivityLog
                {
                    Title = "Password Changed",
                    Description = $"User '{user.FullName}' successfully changed their account password.",
                    IconClass = "fa-key",
                    ColorClass = "text-warning",
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                NotificationsController.AddNotification(new NotificationItem
                {
                    Title = "Password Changed",
                    Description = "Your account password was changed successfully.",
                    Category = "Security",
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    IconClass = "fa-key",
                    ColorClass = "text-warning",
                    BgColorClass = "bg-warning-subtle",
                    TargetUrl = "/Profile",
                    TargetEmail = user.Email
                });

                TempData["SuccessMessage"] = "Password updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Password update failed. Please verify password requirements and matching inputs.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<User?> GetCurrentLoggedInUserAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.Branch)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user != null) return user;
            }

            // Fallback to email claim
            var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(emailClaim))
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.Branch)
                    .FirstOrDefaultAsync(u => u.Email == emailClaim);

                if (user != null) return user;
            }

            // Fallback to first active admin/user in database
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Branch)
                .FirstOrDefaultAsync();
        }
    }
}
