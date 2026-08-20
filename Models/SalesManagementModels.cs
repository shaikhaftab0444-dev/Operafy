using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_Leads")]
    public class Lead
    {
        [Key]
        public int LeadId { get; set; }

        [Required]
        [StringLength(100)]
        public string ContactName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Company { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Source { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "New"; // New, Contacted, Qualified, Lost
    }

    [Table("erp_Quotations")]
    public class Quotation
    {
        [Key]
        public int QuotationId { get; set; }

        [Required]
        [StringLength(50)]
        public string QuoteNo { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        public DateTime ExpiryDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedAmount { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Draft"; // Draft, Sent, Approved
    }

    [Table("erp_SalesOrders")]
    public class SalesOrder
    {
        [Key]
        public int SalesOrderId { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderNo { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OrderTotal { get; set; }

        [Required]
        [StringLength(50)]
        public string DeliveryStatus { get; set; } = "Processing"; // Processing, Dispatched, Delivered
    }

    [Table("erp_SalesReturns")]
    public class SalesReturn
    {
        [Key]
        public int SalesReturnId { get; set; }

        [Required]
        [StringLength(50)]
        public string ReturnNo { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string OriginalInvoiceNo { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        public DateTime ReturnDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RefundValue { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Inspecting"; // Inspecting, Refunded, Cancelled
    }

    [Table("erp_PaymentReceipts")]
    public class PaymentReceipt
    {
        [Key]
        public int PaymentReceiptId { get; set; }

        [Required]
        [StringLength(50)]
        public string InvoiceNo { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        public DateTime InvoiceDate { get; set; }

        public DateTime DueDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PendingBalance { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Overdue, Paid
    }
}
