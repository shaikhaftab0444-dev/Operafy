using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{


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

    [Table("erp_SalesReturns")]
    public class SalesReturn
    {
        [Key]
        [Column("SalesReturnId")]
        public int SalesReturnId { get; set; }

        [NotMapped]
        public int Id { get => SalesReturnId; set => SalesReturnId = value; }

        public string ReturnCode { get; set; } = string.Empty; // e.g. "SR-7001"

        [NotMapped]
        public string ReturnNo { get => ReturnCode; set => ReturnCode = value; }

        public string OriginalInvoiceNumber { get; set; } = string.Empty;

        [NotMapped]
        public string OriginalInvoiceNo { get => OriginalInvoiceNumber; set => OriginalInvoiceNumber = value; }

        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public DateTime ReturnDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal RefundValue { get; set; }

        public string ReturnReason { get; set; } = "Defective"; // "Defective", "Transit Damage", "Wrong Item", "Customer Rejection"

        public string Status { get; set; } = "Inspecting"; // "Inspecting", "Restocked", "Refunded", "Rejected"

        public string? CreditNoteNumber { get; set; } // e.g. "CN-2026-0001"
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
