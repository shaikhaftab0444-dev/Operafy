using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_AdminPasswordResets")]
    public class AdminPasswordReset
    {
        [Key]
        public int ResetId { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Completed, Rejected
    }

    [Table("erp_AdminBranchHours")]
    public class AdminBranchHour
    {
        [Key]
        public int HourId { get; set; }

        [Required]
        [StringLength(100)]
        public string BranchName { get; set; } = string.Empty;

        [Required]
        public int BranchId { get; set; }

        [ForeignKey("BranchId")]
        public Branch? Branch { get; set; }

        [Required]
        [StringLength(20)]
        public string OpeningTime { get; set; } = "09:00 AM";

        [Required]
        [StringLength(20)]
        public string ClosingTime { get; set; } = "06:00 PM";

        [Required]
        [StringLength(50)]
        public string OffDay { get; set; } = "Sunday";

        public int GracePeriod { get; set; } = 15;

        public int BreakDuration { get; set; } = 45;

        public decimal HalfDayMinHours { get; set; } = 4.5m;

        public bool IsContinuousShift { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime EffectiveDate { get; set; } = DateTime.Today;
    }

    [Table("erp_AdminBackupLogs")]
    public class AdminBackupLog
    {
        [Key]
        public int BackupId { get; set; }

        [Required]
        [StringLength(255)]
        public string Filename { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string BackupSize { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Success"; // Success, Failed

        [Required]
        [StringLength(50)]
        public string BackupType { get; set; } = "Manual Trigger"; // Manual Trigger, Auto Schedule

        [Required]
        [StringLength(100)]
        public string TriggeredBy { get; set; } = "Super Admin";

        [Required]
        [StringLength(100)]
        public string StorageLocation { get; set; } = "Local Disk"; // Local Disk, Azure Storage
    }

    [Table("erp_AdminLoginAudits")]
    public class AdminLoginAudit
    {
        [Key]
        public int AuditId { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string IpAddress { get; set; } = string.Empty;

        [Required]
        public DateTime LoginTime { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Success"; // Success, Failed
    }

    [Table("erp_AdminAnnouncements")]
    public class AdminAnnouncement
    {
        [Key]
        public int AnnouncementId { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(100)]
        public string PostedBy { get; set; } = "System Admin";

        [Required]
        [StringLength(50)]
        public string Priority { get; set; } = "Normal"; // Normal, High, Urgent / Broadcast

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = "General"; // Maintenance, HR Policy, Compliance, Holiday & Events, Corporate News, IT Infrastructure

        public bool IsPinned { get; set; } = false;

        [StringLength(255)]
        public string? AttachmentName { get; set; }

        [StringLength(255)]
        public string? AttachmentUrl { get; set; }

        [Required]
        [StringLength(100)]
        public string TargetAudience { get; set; } = "All Staff"; // All Staff, Managers & Admins, Sales Department, Accounts Team

        [Required]
        [StringLength(100)]
        public string TargetBranch { get; set; } = "All Branches";

        public DateTime? ExpiryDate { get; set; }
    }
}
