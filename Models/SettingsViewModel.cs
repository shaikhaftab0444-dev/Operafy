using System.ComponentModel.DataAnnotations;

namespace ERP_System.Models
{
    public class SettingsViewModel
    {
        // General Settings
        [Required]
        [Display(Name = "System Name")]
        public string SystemName { get; set; } = "ERP System Solutions";

        [Required]
        [Display(Name = "Default Currency Symbol")]
        public string CurrencySymbol { get; set; } = "₹ (INR)";

        [Required]
        [Display(Name = "Date Format")]
        public string DateFormat { get; set; } = "DD/MM/YYYY";

        [Required]
        [Display(Name = "System TimeZone")]
        public string TimeZone { get; set; } = "Asia/Kolkata (IST +5:30)";

        [Range(5, 100)]
        [Display(Name = "Default Records Per Page")]
        public int DefaultPageSize { get; set; } = 15;

        // Security & Policy Settings
        [Display(Name = "Enable Two-Factor Authentication (2FA)")]
        public bool EnableTwoFactor { get; set; } = false;

        [Range(5, 240)]
        [Display(Name = "Session Timeout (Minutes)")]
        public int SessionTimeoutMinutes { get; set; } = 30;

        [Range(3, 10)]
        [Display(Name = "Max Failed Login Attempts")]
        public int MaxFailedLoginAttempts { get; set; } = 5;

        [Display(Name = "Require Password Expiry (Every 90 Days)")]
        public bool EnablePasswordExpiry { get; set; } = true;

        // Notification Settings
        [Display(Name = "Email Alerts & Notifications")]
        public bool EnableEmailNotifications { get; set; } = true;

        [Display(Name = "Desktop Push Notifications")]
        public bool EnableDesktopNotifications { get; set; } = true;

        [Display(Name = "Daily Executive Summary Email")]
        public bool EnableDailyDigest { get; set; } = true;

        // Appearance Settings
        [Display(Name = "UI Theme Mode")]
        public string ThemeMode { get; set; } = "Light";

        [Display(Name = "Sidebar Menu Layout")]
        public string SidebarLayout { get; set; } = "Expanded";

        [Display(Name = "Primary Accent Color")]
        public string PrimaryAccentColor { get; set; } = "#3b82f6";

        // SMTP Email Settings
        [Display(Name = "SMTP Host Server")]
        public string SmtpHost { get; set; } = "smtp.company-erp.com";

        [Display(Name = "SMTP Server Port")]
        public int SmtpPort { get; set; } = 587;

        [Display(Name = "Sender Email Address")]
        public string SenderEmail { get; set; } = "noreply@company-erp.com";

        [Display(Name = "Enable SSL/TLS Encryption")]
        public bool EnableSmtpSsl { get; set; } = true;
    }
}
