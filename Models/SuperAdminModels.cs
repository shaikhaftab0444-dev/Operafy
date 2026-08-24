using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_SuperAdminErrorLogs")]
    public class SuperAdminErrorLog
    {
        [Key]
        public int ErrorLogId { get; set; }

        [Required]
        [StringLength(255)]
        public string ErrorMessage { get; set; } = string.Empty;

        public string StackTrace { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    [Table("erp_SuperAdminMaintenances")]
    public class SuperAdminMaintenance
    {
        [Key]
        public int MaintenanceId { get; set; }

        public bool IsMaintenanceMode { get; set; } = false;

        [Required]
        [StringLength(255)]
        public string CustomMessage { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string SetBy { get; set; } = "Super Admin";
    }

    [Table("erp_SuperAdminIntegrations")]
    public class SuperAdminIntegration
    {
        [Key]
        public int IntegrationId { get; set; }

        [Required]
        [StringLength(100)]
        public string ProviderName { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string ApiKey { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string ApiUrl { get; set; } = string.Empty;
    }

    [Table("erp_SuperAdminRestorePoints")]
    public class SuperAdminRestorePoint
    {
        [Key]
        public int RestorePointId { get; set; }

        [Required]
        [StringLength(100)]
        public string PointName { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(255)]
        public string Description { get; set; } = string.Empty;
    }

    [Table("erp_SuperAdminPriceOverrides")]
    public class SuperAdminPriceOverride
    {
        [Key]
        public int OverrideId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        [Required]
        [StringLength(100)]
        public string VendorName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal CustomPrice { get; set; }

        [Required]
        [StringLength(100)]
        public string ApprovedBy { get; set; } = "Super Admin";
    }
}
