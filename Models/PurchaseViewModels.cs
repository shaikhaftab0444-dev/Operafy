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

    public class POLineItem
    {
        public string Item { get; set; } = string.Empty;
        public string Specification { get; set; } = string.Empty;
        public string Hsn { get; set; } = string.Empty;
        public int Qty { get; set; }
        public string UnitPrice { get; set; } = string.Empty;
        public string Tax { get; set; } = "18% GST";
        public string Total { get; set; } = string.Empty;
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
        public string? DeliveryDate { get; set; }
        public string? Department { get; set; }
        public string? RequestedBy { get; set; }
        public string? ItemSummary { get; set; }
        public string? VendorGstin { get; set; }
        public string? VendorPan { get; set; }
        public string? VendorAddress { get; set; }
        public string? VendorEmail { get; set; }
        public string? VendorPhone { get; set; }
        public string? ShippingAddress { get; set; }
        public string? Warehouse { get; set; }
        public string? ReceivingGate { get; set; }
        public string? Subtotal { get; set; }
        public string? TaxAmount { get; set; }
        public string? Carrier { get; set; }
        public string? TrackingNumber { get; set; }
        public string? AuthorizedSignatory { get; set; }
        public List<POLineItem> Items { get; set; } = new();
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
        public string? Gstin { get; set; }
        public string? Pan { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? PaymentTerms { get; set; }
        public string? RiskGrade { get; set; }
        public string? AuditorRemarks { get; set; }
        public string? NextAuditDate { get; set; }
    }

    public class GRNLineItem
    {
        public string Item { get; set; } = string.Empty;
        public string Specification { get; set; } = string.Empty;
        public string Hsn { get; set; } = string.Empty;
        public int PoQty { get; set; }
        public int ReceivedQty { get; set; }
        public int AcceptedQty { get; set; }
        public int QuarantineQty { get; set; }
        public int RejectedQty { get; set; }
        public string Unit { get; set; } = "Units";
        public string Verdict { get; set; } = "PASSED QC";
        public string Remarks { get; set; } = string.Empty;
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
        public string? DeliveryChallanNo { get; set; }
        public string? Carrier { get; set; }
        public string? VehicleNumber { get; set; }
        public string? ReceivingGate { get; set; }
        public string? StorageBin { get; set; }
        public string? InspectorRemarks { get; set; }
        public List<GRNLineItem> Items { get; set; } = new();
    }

    public class GRNSlipDetailViewModel
    {
        public string GRNNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string PONumber { get; set; } = string.Empty;
        public string VendorName { get; set; } = string.Empty;
        public string VendorGstin { get; set; } = string.Empty;
        public string VendorAddress { get; set; } = string.Empty;
        public string ReceivedDate { get; set; } = string.Empty;
        public string Warehouse { get; set; } = string.Empty;
        public string StorageBin { get; set; } = string.Empty;
        public string ReceivingGate { get; set; } = string.Empty;
        public int ItemsReceived { get; set; }
        public string InspectedBy { get; set; } = string.Empty;
        public string DeliveryChallanNo { get; set; } = string.Empty;
        public string ChallanDate { get; set; } = string.Empty;
        public string Carrier { get; set; } = string.Empty;
        public string VehicleNumber { get; set; } = string.Empty;
        public string ThreeWayMatchStatus { get; set; } = string.Empty;
        public string Variance { get; set; } = "0.00%";
        public string InspectorRemarks { get; set; } = string.Empty;
        public string SapMovementType { get; set; } = "101 (Goods Receipt for PO)";
        public string BillOfLading { get; set; } = string.Empty;
        public List<GRNLineItem> Items { get; set; } = new();
    }

    public class VendorAuditOrderItem
    {
        public string PONumber { get; set; } = string.Empty;
        public string OrderDate { get; set; } = string.Empty;
        public string TotalAmount { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int ItemsCount { get; set; } = 1;
        public string PaymentTerms { get; set; } = string.Empty;
    }

    public class VendorAuditFindingItem
    {
        public string Date { get; set; } = string.Empty;
        public string AuditType { get; set; } = string.Empty;
        public string Auditor { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public string Score { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class VendorAuditDossier
    {
        public string VendorName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Rating { get; set; } = "4.8 / 5.0";
        public string Status { get; set; } = "Preferred Partner";
        public string SpendYTD { get; set; } = "₹ 0.00";
        public string OnTimeDeliveryRate { get; set; } = "95%";
        public string QualityScore { get; set; } = "98%";
        public string DefectRatio { get; set; } = "0.20%";
        public string PriceCompetitiveness { get; set; } = "96%";
        public string RiskGrade { get; set; } = "Low Risk (Grade AAA)";
        
        // Statutory & Corporate Identifiers
        public string Gstin { get; set; } = string.Empty;
        public string GstinStatus { get; set; } = "Active & Verified (GSTN Portal)";
        public string Pan { get; set; } = string.Empty;
        public string UdyamRegNo { get; set; } = string.Empty;
        public string CinNumber { get; set; } = string.Empty;
        
        // Contact & Location
        public string ContactPerson { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string RegisteredAddress { get; set; } = string.Empty;
        public string PlantLocation { get; set; } = string.Empty;
        
        // Banking & Commercial Terms
        public string BankName { get; set; } = string.Empty;
        public string BankAccountMasked { get; set; } = string.Empty;
        public string IfscCode { get; set; } = string.Empty;
        public string PaymentTerms { get; set; } = "Net 30 Days";
        public string LeadTimeDays { get; set; } = "4-7 Business Days";
        public string ThreeWayMatchStatus { get; set; } = "Automated Gate Verification Enabled";
        
        // Quality & Compliance Accreditations
        public List<string> Certifications { get; set; } = new();
        
        // Order Execution Records
        public List<VendorAuditOrderItem> RecentOrders { get; set; } = new();
        
        // Auditor Assessment & Sign-Off
        public string LeadAuditor { get; set; } = "Rajesh Khanna, Lead SRM Quality Auditor";
        public string AuditDate { get; set; } = string.Empty;
        public string AuditValidUntil { get; set; } = string.Empty;
        public string NextAuditDate { get; set; } = string.Empty;
        public string AuditorRemarks { get; set; } = string.Empty;
        public List<VendorAuditFindingItem> AuditHistory { get; set; } = new();
    }
}
