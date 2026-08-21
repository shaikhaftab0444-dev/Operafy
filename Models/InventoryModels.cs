using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_InvWarehouses")]
    public class InvWarehouse
    {
        [Key]
        public int WarehouseId { get; set; }

        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Location { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    [Table("erp_InvGrns")]
    public class InvGrn
    {
        [Key]
        public int GrnId { get; set; }

        [Required]
        [StringLength(50)]
        public string GrnNo { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string SupplierName { get; set; } = string.Empty;

        [Required]
        public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(100)]
        public string ReceivedBy { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Completed"; // Completed, Pending Verification
    }

    [Table("erp_InvTransfers")]
    public class InvTransfer
    {
        [Key]
        public int TransferId { get; set; }

        [Required]
        [StringLength(50)]
        public string TransferNo { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FromWarehouse { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ToWarehouse { get; set; } = string.Empty;

        [Required]
        public DateTime TransferDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Transferred"; // Transferred, In Transit, Pending
    }

    [Table("erp_InvStockAudits")]
    public class InvStockAudit
    {
        [Key]
        public int AuditId { get; set; }

        [Required]
        [StringLength(50)]
        public string AuditNo { get; set; } = string.Empty;

        [Required]
        public DateTime AuditDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(100)]
        public string AuditorName { get; set; } = string.Empty;

        public bool DiscrepancyFound { get; set; } = false;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Reconciled"; // Reconciled, Draft, Pending Review
    }

    [Table("erp_InvScrapWriteOffs")]
    public class InvScrapWriteOff
    {
        [Key]
        public int ScrapId { get; set; }

        [Required]
        [StringLength(50)]
        public string ScrapNo { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string ItemName { get; set; } = string.Empty;

        [Required]
        public int QtyScrapped { get; set; }

        [Required]
        [StringLength(255)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public DateTime WriteOffDate { get; set; } = DateTime.UtcNow;
    }
}
