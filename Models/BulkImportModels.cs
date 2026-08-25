using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_ImportLogs")]
    public class ImportLog
    {
        [Key]
        public int LogId { get; set; }

        [Required]
        [StringLength(100)]
        public string ModuleType { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Filename { get; set; } = string.Empty;

        public int TotalRows { get; set; }
        public int SuccessRows { get; set; }
        public int FailedRows { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Success"; // Success, Failed, Partial

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(255)]
        public string? LogFileUrl { get; set; }
    }

    public class BulkImportViewModel
    {
        public string SelectedModule { get; set; } = "Items & SKUs Catalog";
        public string DuplicateStrategy { get; set; } = "Skip Existing"; // Skip Existing, Overwrite Existing, Reject Entire File
        public List<ImportLog> ImportLogs { get; set; } = new List<ImportLog>();
    }
}
