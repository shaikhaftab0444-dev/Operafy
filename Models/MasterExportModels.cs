using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_ExportAuditLogs")]
    public class ExportAuditLog
    {
        [Key]
        public int AuditId { get; set; }

        [Required]
        [StringLength(100)]
        public string DatasetName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string FileFormat { get; set; } = string.Empty;

        public int RecordsCount { get; set; }

        [Required]
        [StringLength(100)]
        public string ExportedBy { get; set; } = string.Empty;

        public DateTime ExportedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        public string IpAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Success";
    }

    public class MasterExportViewModel
    {
        public List<ExportAuditLog> AuditLogs { get; set; } = new List<ExportAuditLog>();
        
        public int EmployeeCount { get; set; }
        public int ItemCount { get; set; }
        public int CustomerCount { get; set; }
        public int VendorCount { get; set; }
        public int AccountEntryCount { get; set; }
        public int TaxSlabCount { get; set; }
        
        public List<Branch> Branches { get; set; } = new List<Branch>();
    }
}
