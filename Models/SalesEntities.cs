using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_SalesLeads")]
    public class SalesLead
    {
        [Key]
        public int Id { get; set; }
        public string LeadTitle { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedDealValue { get; set; }
        public string Stage { get; set; } = "New"; // "New", "Qualified", "Quotation Sent", "Negotiation", "Closed Won", "Closed Lost"
        public int WinProbability { get; set; } = 20; // 20%, 40%, 60%, 80%, 100%, 0%
        
        public string AssignedToUserId { get; set; } = string.Empty;
        public int? AssignedToUserUserId { get; set; }
        [ForeignKey("AssignedToUserUserId")]
        public virtual User? AssignedToUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpectedCloseDate { get; set; }
    }

    [Table("erp_SalesQuotations")]
    public class SalesQuotation
    {
        [Key]
        public int Id { get; set; }
        public string QuotationNumber { get; set; } = string.Empty; // e.g. "QT-2026-0042"
        public string CustomerName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountPercentage { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public bool RequiresManagerApproval { get; set; } = false; // Triggered if DiscountPercentage > 10%
        public string ApprovalStatus { get; set; } = "Approved"; // "Pending Review", "Approved", "Rejected"
        public string? ApprovalRemarks { get; set; }
        
        public string CreatedByUserId { get; set; } = string.Empty;
        public int? CreatedByUserUserId { get; set; }
        [ForeignKey("CreatedByUserUserId")]
        public virtual User? CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    [Table("erp_SalesTargets")]
    public class SalesTarget
    {
        [Key]
        public int Id { get; set; }

        public string SalesRepUserId { get; set; } = string.Empty;
        public int? SalesRepUserUserId { get; set; }

        [ForeignKey("SalesRepUserUserId")]
        public virtual User? ExecutiveUser { get; set; }

        [NotMapped]
        public virtual User? SalesRep { get => ExecutiveUser; set => ExecutiveUser = value; }

        public string FiscalQuarter { get; set; } = "Q3-2026"; // e.g. "Q3-2026"

        [Column(TypeName = "decimal(18,2)")]
        public decimal TargetAmount { get; set; } = 0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal AchievedAmount { get; set; } = 0m;

        // Legacy compatibility properties - typed as string to match NVARCHAR(50) database column
        public string? ExecutiveUserId { get; set; }
        public int Month { get; set; } = DateTime.Today.Month;
        public int Year { get; set; } = DateTime.Today.Year;
    }

    [Table("erp_SalesInvoices")]
    public class SalesInvoice
    {
        [Key]
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } = 0m;

        public string Status { get; set; } = "Paid"; // "Paid", "Pending", "Overdue"

        public string CreatedByUserId { get; set; } = string.Empty;
        public int? CreatedByUserUserId { get; set; }
        [ForeignKey("CreatedByUserUserId")]
        public virtual User? CreatedByUser { get; set; }

        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    }

    // Preserved for backward compatibility with existing SalesManagementController & views
    [Table("erp_Leads")]
    public class Lead
    {
        [Key]
        [Column("LeadId")]
        public int LeadId { get; set; }

        [NotMapped]
        public int Id { get => LeadId; set => LeadId = value; }

        public string? ClientName { get; set; } = string.Empty;

        [NotMapped]
        public string? ContactName { get => ClientName; set => ClientName = value; }

        public string? CompanyName { get; set; } = string.Empty;

        [NotMapped]
        public string? Company { get => CompanyName; set => CompanyName = value; }

        public string? Email { get; set; } = string.Empty;
        public string? Phone { get; set; } = string.Empty;
        public string? Source { get; set; } = "Inbound";

        [Column(TypeName = "decimal(18,2)")]
        public decimal? EstimatedValue { get; set; } = 0m;

        public string? Stage { get; set; } = "New"; // "New", "Contacted", "Proposal", "Negotiation", "Won", "Lost"

        [NotMapped]
        public string? Status { get => Stage; set => Stage = value; }

        public int? AssignedExecutiveId { get; set; }

        [ForeignKey("AssignedExecutiveId")]
        public virtual User? AssignedExecutive { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    }

    [Table("erp_SalesOrders")]
    public class SalesOrder
    {
        [Key]
        [Column("SalesOrderId")]
        public int SalesOrderId { get; set; }

        [NotMapped]
        public int Id { get => SalesOrderId; set => SalesOrderId = value; }

        public string? OrderNumber { get; set; } = string.Empty; // e.g. "SO-2026-0412"

        [NotMapped]
        public string? OrderNo { get => OrderNumber; set => OrderNumber = value; }

        public string? CustomerName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalAmount { get; set; } = 0m;

        [NotMapped]
        public decimal? OrderTotal { get => TotalAmount; set => TotalAmount = value; }

        public string? Status { get; set; } = "Confirmed"; // "Draft", "Confirmed", "Invoiced", "Cancelled"

        [NotMapped]
        public string? DeliveryStatus { get => Status; set => Status = value; }

        public int? CreatedByUserId { get; set; }
        public DateTime? OrderDate { get; set; } = DateTime.Today;
        public string? PaymentTerms { get; set; } = "Net 30"; // "Immediate", "Net 30"
    }
}
