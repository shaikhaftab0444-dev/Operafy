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
        [StringLength(20)]
        public string OpeningTime { get; set; } = "09:00 AM";

        [Required]
        [StringLength(20)]
        public string ClosingTime { get; set; } = "06:00 PM";

        [Required]
        [StringLength(50)]
        public string OffDay { get; set; } = "Sunday";
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
    }
}
