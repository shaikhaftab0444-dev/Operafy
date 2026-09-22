using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_SalesLeads")]
    public class SalesLead
    {
        [Key]
        public int Id { get; set; }
        public string LeadCode { get; set; } = string.Empty; // e.g., "LD-1"
        public string LeadTitle { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;

        [NotMapped]
        public string CustomerName
        {
            get => !string.IsNullOrWhiteSpace(Company) ? Company : ContactName;
            set
            {
                if (string.IsNullOrWhiteSpace(Company)) Company = value;
                if (string.IsNullOrWhiteSpace(ContactName)) ContactName = value;
            }
        }

        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string Source { get; set; } = "Website Referral"; // "LinkedIn", "Direct Call", "Cold Email", "Website Referral"

        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedDealValue { get; set; }
        public string Stage { get; set; } = "New"; // "New", "Contacted", "Qualified", "Quotation Sent", "Negotiation", "Closed Won", "Closed Lost"
        public int WinProbability { get; set; } = 20; // 20, 40, 60, 80, 100
        
        public string? AssignedToUserId { get; set; }
        public int? AssignedToUserUserId { get; set; }

        [ForeignKey("AssignedToUserUserId")]
        public virtual User? AssignedToUserEntity { get; set; }

        [NotMapped]
        public virtual ApplicationUser? AssignedToUser
        {
            get
            {
                if (AssignedToUserEntity == null) return null;
                if (AssignedToUserEntity is ApplicationUser appUser) return appUser;
                return new ApplicationUser
                {
                    UserId = AssignedToUserEntity.UserId,
                    FullName = AssignedToUserEntity.FullName,
                    Email = AssignedToUserEntity.Email,
                    UserName = AssignedToUserEntity.UserName,
                    Role = AssignedToUserEntity.Role
                };
            }
            set => AssignedToUserEntity = value;
        }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public DateTime CreatedAt
        {
            get => CreatedDate;
            set => CreatedDate = value;
        }

        public DateTime? ExpectedCloseDate { get; set; }
    }

    [Table("erp_SalesQuotations")]
    public class SalesQuotation
    {
        [Key]
        public int Id { get; set; }
        public string QuotationNumber { get; set; } = string.Empty; // e.g. "QTN-5001"
        public string CustomerName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountPercentage { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public bool RequiresManagerApproval { get; set; } = false; // Triggered if DiscountPercentage > 10%
        public string ApprovalStatus { get; set; } = "Draft"; // "Draft", "Pending Review", "Approved", "Rejected"
        public string? ApprovalRemarks { get; set; }
        
        public string? CreatedByUserId { get; set; }
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

        // Legacy compatibility properties
        public string? ExecutiveUserId { get; set; }
        public int Month { get; set; } = DateTime.Today.Month;
        public int Year { get; set; } = DateTime.Today.Year;
    }

    [Table("erp_SalesInvoices")]
    public class SalesInvoice
    {
        [Key]
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty; // e.g. "INV-2026-001"
        
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public int? SalesOrderId { get; set; }
        [ForeignKey("SalesOrderId")]
        public virtual SalesOrder? SalesOrder { get; set; }

        public string? LinkedOrderNumber { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxableValue { get; set; } = 0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal GstAmount { get; set; } = 0m;

        private decimal _grandTotal = 0m;
        private decimal _totalAmount = 0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal GrandTotal
        {
            get => _grandTotal != 0m ? _grandTotal : _totalAmount;
            set
            {
                _grandTotal = value;
                if (_totalAmount == 0m) _totalAmount = value;
            }
        }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount
        {
            get => _totalAmount != 0m ? _totalAmount : _grandTotal;
            set
            {
                _totalAmount = value;
                if (_grandTotal == 0m) _grandTotal = value;
            }
        }

        public string Status { get; set; } = "Paid"; // "Paid", "Pending", "Overdue", "Partially Paid"

        public string CreatedByUserId { get; set; } = string.Empty;
        public int? CreatedByUserUserId { get; set; }
        [ForeignKey("CreatedByUserUserId")]
        public virtual User? CreatedByUser { get; set; }

        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(30);
    }

    [Table("erp_SalesOrders")]
    public class SalesOrder
    {
        [Key]
        [Column("SalesOrderId")]
        public int SalesOrderId { get; set; }

        [NotMapped]
        public int Id { get => SalesOrderId; set => SalesOrderId = value; }

        private string _orderNumber = string.Empty;
        public string OrderNumber
        {
            get => !string.IsNullOrEmpty(_orderNumber) ? _orderNumber : _orderNo;
            set
            {
                _orderNumber = value ?? string.Empty;
                if (string.IsNullOrEmpty(_orderNo)) _orderNo = _orderNumber;
            }
        }

        private string _orderNo = string.Empty;
        public string OrderNo
        {
            get => !string.IsNullOrEmpty(_orderNo) ? _orderNo : _orderNumber;
            set
            {
                _orderNo = value ?? string.Empty;
                if (string.IsNullOrEmpty(_orderNumber)) _orderNumber = _orderNo;
            }
        }

        [NotMapped]
        public string DisplayOrderNumber => !string.IsNullOrWhiteSpace(OrderNumber) ? OrderNumber : OrderNo;

        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        public string? CustomerName { get; set; } = string.Empty;

        public DateTime? OrderDate { get; set; } = DateTime.UtcNow;

        private decimal _orderTotal = 0m;
        [Column(TypeName = "decimal(18,2)")]
        public decimal OrderTotal
        {
            get => _orderTotal != 0m ? _orderTotal : (_totalAmount ?? 0m);
            set
            {
                _orderTotal = value;
                if ((_totalAmount ?? 0m) == 0m) _totalAmount = value;
            }
        }

        private decimal? _totalAmount = 0m;
        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalAmount
        {
            get => (_totalAmount ?? 0m) != 0m ? _totalAmount : _orderTotal;
            set
            {
                _totalAmount = value ?? 0m;
                if (_orderTotal == 0m) _orderTotal = value ?? 0m;
            }
        }

        private string _deliveryStatus = "Confirmed";
        public string DeliveryStatus
        {
            get => !string.IsNullOrEmpty(_deliveryStatus) ? _deliveryStatus : (_status ?? "Confirmed");
            set
            {
                _deliveryStatus = value ?? "Confirmed";
                if (string.IsNullOrEmpty(_status) || _status == "Confirmed") _status = _deliveryStatus;
            }
        }

        private string? _status = "Confirmed";
        public string? Status
        {
            get => !string.IsNullOrEmpty(_status) ? _status : _deliveryStatus;
            set
            {
                _status = value ?? "Confirmed";
                if (string.IsNullOrEmpty(_deliveryStatus) || _deliveryStatus == "Confirmed") _deliveryStatus = _status;
            }
        }

        public string PaymentTerms { get; set; } = "Net 30"; // "Immediate", "Net 30", "Net 60"
        public string? CreatedByUserId { get; set; }

        [NotMapped]
        public int? CreatedByUserUserId
        {
            get => int.TryParse(CreatedByUserId, out int uid) ? uid : null;
            set => CreatedByUserId = value?.ToString();
        }

        public virtual ICollection<SalesOrderItem> Items { get; set; } = new List<SalesOrderItem>();
    }

    [Table("erp_SalesOrderItems")]
    public class SalesOrderItem
    {
        [Key]
        public int Id { get; set; }

        public int SalesOrderId { get; set; }
        [ForeignKey("SalesOrderId")]
        public virtual SalesOrder? SalesOrder { get; set; }

        public int? ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        public string ItemDescription { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }
    }

    [Table("erp_PaymentReceivables")]
    public class PaymentReceivable
    {
        [Key]
        public int Id { get; set; }

        public string InvoiceNumber { get; set; } = string.Empty;

        [NotMapped]
        public string InvoiceNo { get => InvoiceNumber; set => InvoiceNumber = value; }

        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        [NotMapped]
        public string CustomerName => Customer?.CustomerName ?? string.Empty;

        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(30);

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PendingBalance { get; set; }

        public string Status { get; set; } = "Pending"; // "Pending", "Overdue", "Paid", "Partially Paid"
    }

    // Preserved for backward compatibility with existing components
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
}
