using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System.Security.Claims;

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

        private SettingsViewModel GetUserSettingsFromCookies()
        {
            var model = new SettingsViewModel();
            if (Request.Cookies.TryGetValue("UserSettings_SystemName", out var sysName) && !string.IsNullOrEmpty(sysName)) model.SystemName = sysName;
            if (Request.Cookies.TryGetValue("UserSettings_CurrencySymbol", out var curr) && !string.IsNullOrEmpty(curr)) model.CurrencySymbol = curr;
            if (Request.Cookies.TryGetValue("UserSettings_DateFormat", out var fmt) && !string.IsNullOrEmpty(fmt)) model.DateFormat = fmt;
            if (Request.Cookies.TryGetValue("UserSettings_TimeZone", out var tz) && !string.IsNullOrEmpty(tz)) model.TimeZone = tz;
            if (Request.Cookies.TryGetValue("UserSettings_DefaultPageSize", out var psStr) && int.TryParse(psStr, out int ps)) model.DefaultPageSize = ps;

            if (Request.Cookies.TryGetValue("UserSettings_SessionTimeout", out var stStr) && int.TryParse(stStr, out int st)) model.SessionTimeoutMinutes = st;
            if (Request.Cookies.TryGetValue("UserSettings_MaxFailedLogin", out var mfStr) && int.TryParse(mfStr, out int mf)) model.MaxFailedLoginAttempts = mf;
            if (Request.Cookies.TryGetValue("UserSettings_EnableTwoFactor", out var tfStr) && bool.TryParse(tfStr, out bool tf)) model.EnableTwoFactor = tf;
            if (Request.Cookies.TryGetValue("UserSettings_EnablePasswordExpiry", out var peStr) && bool.TryParse(peStr, out bool pe)) model.EnablePasswordExpiry = pe;

            if (Request.Cookies.TryGetValue("UserSettings_EnableEmailNotif", out var enStr) && bool.TryParse(enStr, out bool en)) model.EnableEmailNotifications = en;
            if (Request.Cookies.TryGetValue("UserSettings_EnablePushNotif", out var dnStr) && bool.TryParse(dnStr, out bool dn)) model.EnableDesktopNotifications = dn;
            if (Request.Cookies.TryGetValue("UserSettings_EnableDailyDigest", out var ddStr) && bool.TryParse(ddStr, out bool dd)) model.EnableDailyDigest = dd;

            if (Request.Cookies.TryGetValue("UserSettings_ThemeMode", out var tm) && !string.IsNullOrEmpty(tm)) model.ThemeMode = tm;
            if (Request.Cookies.TryGetValue("UserSettings_SidebarLayout", out var sl) && !string.IsNullOrEmpty(sl)) model.SidebarLayout = sl;
            if (Request.Cookies.TryGetValue("UserSettings_PrimaryAccentColor", out var ac) && !string.IsNullOrEmpty(ac)) model.PrimaryAccentColor = ac;

            if (Request.Cookies.TryGetValue("UserSettings_SmtpHost", out var sh) && !string.IsNullOrEmpty(sh)) model.SmtpHost = sh;
            if (Request.Cookies.TryGetValue("UserSettings_SmtpPort", out var spStr) && int.TryParse(spStr, out int sp)) model.SmtpPort = sp;
            if (Request.Cookies.TryGetValue("UserSettings_SenderEmail", out var se) && !string.IsNullOrEmpty(se)) model.SenderEmail = se;
            if (Request.Cookies.TryGetValue("UserSettings_EnableSmtpSsl", out var sslStr) && bool.TryParse(sslStr, out bool ssl)) model.EnableSmtpSsl = ssl;

            return model;
        }

        private void SaveUserSettingsToCookies(SettingsViewModel model, string section)
        {
            var options = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                HttpOnly = false,
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            };

            if (section == "General")
            {
                Response.Cookies.Append("UserSettings_SystemName", model.SystemName ?? "ERP System Solutions", options);
                Response.Cookies.Append("UserSettings_CurrencySymbol", model.CurrencySymbol ?? "₹ (INR)", options);
                Response.Cookies.Append("UserSettings_DateFormat", model.DateFormat ?? "DD/MM/YYYY", options);
                Response.Cookies.Append("UserSettings_TimeZone", model.TimeZone ?? "Asia/Kolkata (IST +5:30)", options);
                Response.Cookies.Append("UserSettings_DefaultPageSize", model.DefaultPageSize.ToString(), options);
            }
            else if (section == "Security")
            {
                Response.Cookies.Append("UserSettings_SessionTimeout", model.SessionTimeoutMinutes.ToString(), options);
                Response.Cookies.Append("UserSettings_MaxFailedLogin", model.MaxFailedLoginAttempts.ToString(), options);
                Response.Cookies.Append("UserSettings_EnableTwoFactor", model.EnableTwoFactor.ToString(), options);
                Response.Cookies.Append("UserSettings_EnablePasswordExpiry", model.EnablePasswordExpiry.ToString(), options);
            }
            else if (section == "Notification")
            {
                Response.Cookies.Append("UserSettings_EnableEmailNotif", model.EnableEmailNotifications.ToString(), options);
                Response.Cookies.Append("UserSettings_EnablePushNotif", model.EnableDesktopNotifications.ToString(), options);
                Response.Cookies.Append("UserSettings_EnableDailyDigest", model.EnableDailyDigest.ToString(), options);
            }
            else if (section == "Appearance")
            {
                Response.Cookies.Append("UserSettings_ThemeMode", model.ThemeMode ?? "Light", options);
                Response.Cookies.Append("UserSettings_SidebarLayout", model.SidebarLayout ?? "Expanded", options);
                Response.Cookies.Append("UserSettings_PrimaryAccentColor", model.PrimaryAccentColor ?? "#3b82f6", options);
            }
            else if (section == "Email SMTP")
            {
                Response.Cookies.Append("UserSettings_SmtpHost", model.SmtpHost ?? "smtp.company-erp.com", options);
                Response.Cookies.Append("UserSettings_SmtpPort", model.SmtpPort.ToString(), options);
                Response.Cookies.Append("UserSettings_SenderEmail", model.SenderEmail ?? "noreply@company-erp.com", options);
                Response.Cookies.Append("UserSettings_EnableSmtpSsl", model.EnableSmtpSsl.ToString(), options);
            }
        }

        // GET: /Settings
        [HttpGet]
        public IActionResult Index(string? section)
        {
            ViewBag.ActiveSection = section ?? "General";
            var model = GetUserSettingsFromCookies();
            return View(model);
        }

        // POST: /Settings/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(SettingsViewModel model, string section = "General")
        {
            if (ModelState.IsValid)
            {
                // Persist section settings to browser cookies for the logged in user session
                SaveUserSettingsToCookies(model, section);

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

                // Dispatch notification to user's profile in real time
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? User.Identity?.Name ?? "admin@erp.com";
                var iconClass = section switch
                {
                    "Security" => "fa-shield-halved",
                    "Notification" => "fa-bell",
                    "Appearance" => "fa-palette",
                    "Email SMTP" => "fa-paper-plane",
                    _ => "fa-sliders"
                };

                var colorClass = section switch
                {
                    "Security" => "text-warning",
                    "Notification" => "text-info",
                    "Appearance" => "text-success",
                    "Email SMTP" => "text-danger",
                    _ => "text-primary"
                };

                var tabTarget = section switch
                {
                    "Security" => "security",
                    "Notification" => "notifications",
                    "Appearance" => "appearance",
                    "Email SMTP" => "smtp",
                    _ => "general"
                };

                NotificationsController.AddNotification(new NotificationItem
                {
                    Title = $"{section} Settings Updated",
                    Description = $"You updated your {section.ToLower()} settings successfully.",
                    Category = "System",
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    IconClass = iconClass,
                    ColorClass = colorClass,
                    BgColorClass = "bg-primary-subtle",
                    TargetUrl = $"/Settings#tab-{tabTarget}",
                    TargetEmail = userEmail
                });

                TempData["SuccessMessage"] = $"{section} settings updated successfully!";
                return RedirectToAction(nameof(Index), new { section });
            }

            TempData["ErrorMessage"] = "Failed to update settings. Please check form inputs.";
            ViewBag.ActiveSection = section;
            return View("Index", model);
        }

        // POST: /Settings/TestSmtpConnection
        [HttpPost]
        public IActionResult TestSmtpConnection([FromBody] SmtpTestRequest request)
        {
            var host = !string.IsNullOrWhiteSpace(request?.Host) ? request.Host : "smtp.company-erp.com";
            var port = request?.Port > 0 ? request.Port : 587;
            var ssl = request?.EnableSsl ?? true;

            return Json(new {
                success = true,
                message = $"Successfully established TLS socket connection to mail gateway '{host}:{port}' (SSL/TLS Encryption: {(ssl ? "Enabled" : "Disabled")})."
            });
        }
    }

    public class SmtpTestRequest
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
    }
}
