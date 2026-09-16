using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_WarehouseLocations")]
    public class WarehouseLocation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string WarehouseCode { get; set; } = string.Empty; // e.g. "WH-001"

        [Required]
        [StringLength(150)]
        public string WarehouseName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string WarehouseType { get; set; } = "Finished Goods"; // "Finished Goods", "Raw Materials", "Transit Hub", "Cold Storage"

        [Required]
        [StringLength(300)]
        public string LocationAddress { get; set; } = string.Empty;

        [StringLength(50)]
        public string? SupervisorUserId { get; set; }

        public int? SupervisorId { get; set; }

        [ForeignKey("SupervisorId")]
        public virtual User? Supervisor { get; set; }

        public int MaxCapacityUnits { get; set; } = 5000;

        public bool IsActive { get; set; } = true;

        public virtual ICollection<BinRackMaster> StoragePositions { get; set; } = new List<BinRackMaster>();
    }

    [Table("erp_BinRackMasters")]
    public class BinRackMaster
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        [ForeignKey("WarehouseId")]
        public virtual WarehouseLocation? Warehouse { get; set; }

        [Required]
        [StringLength(50)]
        public string RackCode { get; set; } = string.Empty; // e.g. "RACK-A1"

        [Required]
        [StringLength(50)]
        public string BinLevel { get; set; } = string.Empty; // e.g. "BIN-ROW-3"

        public int? AssignedCatalogItemId { get; set; }

        [ForeignKey("AssignedCatalogItemId")]
        public virtual CatalogItem? AssignedItem { get; set; }

        public int MaxCapacityVolume { get; set; } = 500; // e.g. 500 Units/Ltr

        public int CurrentOccupancyVolume { get; set; } = 0; // e.g. 350

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Available"; // "Available", "Full", "Maintenance"
    }

    [Table("erp_GoodsReceiptNotes")]
    public class GoodsReceiptNote
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string GrnNumber { get; set; } = string.Empty; // e.g. "GRN-2026-0003"

        [StringLength(50)]
        public string? PurchaseOrderNumber { get; set; } // e.g. "PO-2026-0891"

        [Required]
        [StringLength(150)]
        public string SupplierName { get; set; } = string.Empty;

        public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string? ReceivedByUserId { get; set; }

        public int? ReceivedById { get; set; }

        [ForeignKey("ReceivedById")]
        public virtual User? ReceivedByUser { get; set; }

        [Required]
        public int DestinationWarehouseId { get; set; }

        [ForeignKey("DestinationWarehouseId")]
        public virtual WarehouseLocation? DestinationWarehouse { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending verification"; // "Pending verification", "Completed", "Rejected"

        [StringLength(1000)]
        public string? Remarks { get; set; }

        public virtual ICollection<GrnLineItem> LineItems { get; set; } = new List<GrnLineItem>();
    }

    [Table("erp_GrnLineItems")]
    public class GrnLineItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int GrnId { get; set; }

        [ForeignKey("GrnId")]
        public virtual GoodsReceiptNote? Grn { get; set; }

        [Required]
        public int CatalogItemId { get; set; }

        [ForeignKey("CatalogItemId")]
        public virtual CatalogItem? CatalogItem { get; set; }

        public int OrderedQuantity { get; set; } = 0;

        public int ReceivedQuantity { get; set; } = 0;

        public int RejectedQuantity { get; set; } = 0;

        public int? TargetBinId { get; set; }

        [ForeignKey("TargetBinId")]
        public virtual BinRackMaster? TargetBin { get; set; }

        [StringLength(100)]
        public string? BatchNumber { get; set; }
    }

    [Table("erp_MaterialDispatches")]
    public class MaterialDispatch
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string DispatchSlipNo { get; set; } = string.Empty; // e.g. "DSP-2026-9043"

        [StringLength(50)]
        public string? SalesOrderNumber { get; set; } // e.g. "SO-2026-104"

        [Required]
        [StringLength(150)]
        public string DestinationParty { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CarrierName { get; set; } = string.Empty; // e.g. "BlueDart Logistics"

        [Required]
        [StringLength(100)]
        public string TrackingNumber { get; set; } = string.Empty;

        [StringLength(100)]
        public string? EWayBillNumber { get; set; }

        public DateTime DispatchDate { get; set; } = DateTime.UtcNow;

        [Required]
        public int SourceWarehouseId { get; set; }

        [ForeignKey("SourceWarehouseId")]
        public virtual WarehouseLocation? SourceWarehouse { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Packing"; // "Packing", "Picked", "Dispatched", "Delivered", "Cancelled"

        public virtual ICollection<DispatchLineItem> LineItems { get; set; } = new List<DispatchLineItem>();
    }

    [Table("erp_DispatchLineItems")]
    public class DispatchLineItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DispatchId { get; set; }

        [ForeignKey("DispatchId")]
        public virtual MaterialDispatch? Dispatch { get; set; }

        [Required]
        public int CatalogItemId { get; set; }

        [ForeignKey("CatalogItemId")]
        public virtual CatalogItem? CatalogItem { get; set; }

        public int Quantity { get; set; } = 1;

        public int? PickedFromBinId { get; set; }

        [ForeignKey("PickedFromBinId")]
        public virtual BinRackMaster? PickedFromBin { get; set; }
    }

    [Table("erp_StockMovementLogs")]
    public class StockMovementLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int InventoryItemId { get; set; }

        [ForeignKey("InventoryItemId")]
        public virtual CatalogItem? InventoryItem { get; set; }

        [Required]
        [StringLength(50)]
        public string MovementType { get; set; } = "INWARD"; // "INWARD", "OUTWARD"

        public int Quantity { get; set; }

        [Required]
        [StringLength(100)]
        public string ReferenceDocument { get; set; } = string.Empty;

        [StringLength(100)]
        public string? HandledByUserId { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
