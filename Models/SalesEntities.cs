using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{

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

    [Table("erp_SalesTargets")]
    public class SalesTarget
    {
        [Key]
        public int Id { get; set; }

        public int? ExecutiveUserId { get; set; }

        [ForeignKey("ExecutiveUserId")]
        public virtual User? ExecutiveUser { get; set; }

        public int Month { get; set; } = DateTime.Today.Month;
        public int Year { get; set; } = DateTime.Today.Year;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TargetAmount { get; set; } = 0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? AchievedAmount { get; set; } = 0m;
    }

    [Table("erp_HierarchicalTasks")]
    public class HierarchicalTask
    {
        [Key]
        public int Id { get; set; }
        public string? Title { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
        public string? TaskType { get; set; } = "MANAGER_TO_EMPLOYEE";
        public string? AssignedByUserId { get; set; }
        public string? AssignedToUserId { get; set; }
        public string? Priority { get; set; } = "Medium";
        public string? Status { get; set; } = "Pending";
        public int ProgressPercentage { get; set; } = 0;
        public DateTime? DueDate { get; set; } = DateTime.Today.AddDays(7);
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
