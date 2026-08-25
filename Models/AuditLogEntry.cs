using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_AuditLogs")]
    public class AuditLogEntry
    {
        [Key]
        public int LogId { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string RoleName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Module { get; set; } = string.Empty; // Security & RBAC, Master Setup, Financials, HRMS, etc.

        [Required]
        [StringLength(255)]
        public string ActionSubject { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string IpAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Device { get; set; } = "Chrome / Win11";

        [Required]
        [StringLength(50)]
        public string Severity { get; set; } = "Success"; // Success, Warning, Security Alert
    }
}
