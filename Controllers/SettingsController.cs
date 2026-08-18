using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;

namespace ERP_System.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Settings
        [HttpGet]
        public IActionResult Index()
        {
            var model = new SettingsViewModel
            {
                SystemName = "ERP System Solutions",
                CurrencySymbol = "₹ (INR)",
                DateFormat = "DD/MM/YYYY",
                TimeZone = "Asia/Kolkata (IST +5:30)",
                DefaultPageSize = 15,
                EnableTwoFactor = false,
                SessionTimeoutMinutes = 30,
                MaxFailedLoginAttempts = 5,
                EnablePasswordExpiry = true,
                EnableEmailNotifications = true,
                EnableDesktopNotifications = true,
                EnableDailyDigest = true,
                ThemeMode = "Light",
                SidebarLayout = "Expanded",
                PrimaryAccentColor = "#3b82f6",
                SmtpHost = "smtp.company-erp.com",
                SmtpPort = 587,
                SenderEmail = "noreply@company-erp.com",
                EnableSmtpSsl = true
            };

            return View(model);
        }

        // POST: /Settings/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(SettingsViewModel model, string section = "General")
        {
            if (ModelState.IsValid)
            {
                // Record activity log
                _context.ActivityLogs.Add(new ActivityLog
                {
                    Title = "Settings Updated",
                    Description = $"{section} settings were updated successfully.",
                    IconClass = "fa-sliders",
                    ColorClass = "text-primary",
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"{section} settings updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Failed to update settings. Please check form inputs.";
            return View("Index", model);
        }
    }
}
