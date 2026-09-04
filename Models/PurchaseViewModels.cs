using System;
using System.Collections.Generic;

namespace ERP_System.Models
{
    public class PurchaseDashboardViewModel
    {
        public string TotalProcurementSpend { get; set; } = "₹ 0.00";
        public int ActiveOrdersCount { get; set; }
        public int PendingRequisitionsCount { get; set; }
        public int ApprovedVendorsCount { get; set; }
        public List<RequisitionItem> PendingApprovals { get; set; } = new();
        public List<PurchaseOrderItem> RecentPurchaseOrders { get; set; } = new();
    }

    public class RequisitionItem
    {
        public int Id { get; set; }
        public string Department { get; set; } = string.Empty;
        public string RequestedBy { get; set; } = string.Empty;
        public string ItemSummary { get; set; } = string.Empty;
        public string EstimatedCost { get; set; } = string.Empty;
        public string Urgency { get; set; } = "Medium";
        public string RequestedOn { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public string? ApprovedOn { get; set; }
        public string? ApprovedBy { get; set; }
    }

    public class PurchaseOrderItem
    {
        public string PONumber { get; set; } = string.Empty;
        public string VendorName { get; set; } = string.Empty;
        public int ItemsCount { get; set; }
        public string TotalAmount { get; set; } = string.Empty;
        public string OrderDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string PaymentTerms { get; set; } = string.Empty;
    }

    public class CreatePurchaseOrderInput
    {
        public string VendorName { get; set; } = string.Empty;
        public string DeliveryDate { get; set; } = string.Empty;
        public string PaymentTerms { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? ItemSummary { get; set; }
        public string? EstimatedCost { get; set; }
    }

    public class VendorScorecardItem
    {
        public string Name { get; set; } = string.Empty;
        public string Rating { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string OnTimeDeliveryRate { get; set; } = "95%";
        public string QualityScore { get; set; } = "98%";
        public string SpendYTD { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
    }

    public class GRNItem
    {
        public string GRNNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string PONumber { get; set; } = string.Empty;
        public string VendorName { get; set; } = string.Empty;
        public string ReceivedDate { get; set; } = string.Empty;
        public string Warehouse { get; set; } = string.Empty;
        public int ItemsReceived { get; set; }
        public string InspectedBy { get; set; } = string.Empty;
    }
}
