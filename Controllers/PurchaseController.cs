using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP_System.Models;
using ERP_System.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,Purchase Manager")]
    public class PurchaseController : Controller
    {
        // GET: /Purchase
        [HttpGet]
        public IActionResult Index()
        {
            var vm = GetPopulatedPurchaseDashboardVM();
            return View(vm);
        }

        // GET: /Purchase/Requisitions
        [HttpGet]
        public IActionResult Requisitions()
        {
            var model = PurchaseDataStore.GetRequisitions();
            return View(model);
        }

        // GET: /Purchase/Orders
        [HttpGet]
        public IActionResult Orders()
        {
            var model = PurchaseDataStore.GetOrders();
            return View(model);
        }

        // GET: /Purchase/Vendors
        [HttpGet]
        public IActionResult Vendors()
        {
            var model = PurchaseDataStore.GetVendors();
            return View(model);
        }

        // GET: /Purchase/Receipts
        [HttpGet]
        public IActionResult Receipts()
        {
            var model = PurchaseDataStore.GetReceipts();
            return View(model);
        }

        // POST: /Purchase/CreatePO
        [HttpPost]
        public IActionResult CreatePO(CreatePurchaseOrderInput input)
        {
            var po = PurchaseDataStore.AddOrder(input);
            return Json(new 
            { 
                success = true, 
                poNumber = po.PONumber, 
                vendor = po.VendorName,
                deliveryDate = !string.IsNullOrWhiteSpace(input?.DeliveryDate) ? input.DeliveryDate : DateTime.Now.AddDays(7).ToString("dd MMM yyyy"),
                paymentTerms = po.PaymentTerms,
                message = $"Purchase Order {po.PONumber} created and dispatched to vendor {po.VendorName}." 
            });
        }

        // POST: /Purchase/ApproveRequisition
        [HttpPost]
        public IActionResult ApproveRequisition(int id, string actionType)
        {
            var userName = User.Identity?.Name ?? "Chief Procurement Officer";
            if (string.Equals(actionType, "Approved", StringComparison.OrdinalIgnoreCase))
            {
                var req = PurchaseDataStore.ApproveRequisition(id, userName);
                if (req == null)
                    return Json(new { success = false, message = $"Requisition #{id} not found." });

                return Json(new 
                { 
                    success = true, 
                    id = id, 
                    status = "Approved",
                    message = $"Requisition #{id} approved and released. Official PO generated in Purchase Orders registry." 
                });
            }
            else
            {
                var req = PurchaseDataStore.RejectRequisition(id, userName);
                if (req == null)
                    return Json(new { success = false, message = $"Requisition #{id} not found." });

                return Json(new 
                { 
                    success = true, 
                    id = id, 
                    status = "Rejected",
                    message = $"Requisition #{id} has been formally rejected." 
                });
            }
        }

        // POST: /Purchase/SubmitPR
        [HttpPost]
        public IActionResult SubmitPR(RequisitionItem input)
        {
            var req = PurchaseDataStore.AddRequisition(input);
            return Json(new
            {
                success = true,
                id = req.Id,
                data = req,
                message = $"Purchase Requisition #{req.Id} submitted successfully to approval queue."
            });
        }

        // POST: /Purchase/OnboardVendor
        [HttpPost]
        public IActionResult OnboardVendor(VendorScorecardItem input)
        {
            if (string.IsNullOrWhiteSpace(input.Rating)) input.Rating = "4.5 / 5.0";
            if (string.IsNullOrWhiteSpace(input.OnTimeDeliveryRate)) input.OnTimeDeliveryRate = "95%";
            if (string.IsNullOrWhiteSpace(input.QualityScore)) input.QualityScore = "98%";
            if (string.IsNullOrWhiteSpace(input.Status)) input.Status = "Approved Partner";
            
            var v = PurchaseDataStore.AddVendor(input);
            return Json(new
            {
                success = true,
                vendor = v,
                message = $"Vendor partner '{v.Name}' successfully onboarded and compliance verified."
            });
        }

        // POST: /Purchase/CreateGRN
        [HttpPost]
        public IActionResult CreateGRN(GRNItem input)
        {
            var grnNo = "GRN-2026-0" + new Random().Next(415, 999);
            input.GRNNumber = grnNo;
            input.ReceivedDate = DateTime.Now.ToString("dd MMM yyyy");
            if (string.IsNullOrWhiteSpace(input.Status)) input.Status = "Inspected & Accepted";
            
            var grn = PurchaseDataStore.AddReceipt(input);
            return Json(new
            {
                success = true,
                grnNumber = grn.GRNNumber,
                grn = grn,
                message = $"Goods Receipt Note {grn.GRNNumber} generated for PO {grn.PONumber}. Material cleared for receiving bay."
            });
        }

        // GET: /Purchase/ExportPOLedger
        [HttpGet]
        public IActionResult ExportPOLedger()
        {
            var orders = PurchaseDataStore.GetOrders();
            var sb = new StringBuilder();
            sb.AppendLine("PO Number,Vendor Partner,Line Items,Total Value,Order Date,Shipping Status,Payment Terms");
            foreach (var po in orders)
            {
                sb.AppendLine($"\"{po.PONumber}\",\"{po.VendorName}\",{po.ItemsCount},\"{po.TotalAmount}\",\"{po.OrderDate}\",\"{po.Status}\",\"{po.PaymentTerms}\"");
            }
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"PurchaseOrders_Ledger_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        // GET: /Purchase/GetPODetails
        [HttpGet]
        public IActionResult GetPODetails(string poNumber)
        {
            var po = PurchaseDataStore.GetOrders().FirstOrDefault(p => p.PONumber.Equals(poNumber, StringComparison.OrdinalIgnoreCase))
                     ?? new PurchaseOrderItem { PONumber = poNumber, VendorName = "TechInfra Solutions Ltd", ItemsCount = 3, TotalAmount = "₹ 3,96,480.00", OrderDate = DateTime.Now.ToString("dd MMM yyyy"), Status = "Confirmed", PaymentTerms = "Net 30" };
            return Json(new
            {
                success = true,
                po = po,
                gstin = "27AAACN8921K1Z2",
                billingAddress = "Operafy Corporate HQ, 4th Floor, Tech Park Central, Bangalore - 560100",
                shippingAddress = "Warehouse Bay 3, Operafy Central Depot, Nelamangala - 562123",
                items = new[]
                {
                    new { Item = "Dell UltraSharp 27\" Monitors (4K UHD)", Hsn = "8471", Qty = 5, UnitPrice = "₹ 24,000.00", Tax = "18% GST", Total = "₹ 1,20,000.00" },
                    new { Item = "High-Speed Dual Band Managed Switches", Hsn = "8517", Qty = 4, UnitPrice = "₹ 45,000.00", Tax = "18% GST", Total = "₹ 1,80,000.00" },
                    new { Item = "Category-6e Shielded Network Spools (305m)", Hsn = "8544", Qty = 3, UnitPrice = "₹ 12,000.00", Tax = "18% GST", Total = "₹ 36,000.00" }
                },
                subtotal = "₹ 3,36,000.00",
                taxAmount = "₹ 60,480.00",
                grandTotal = po.TotalAmount,
                authorizedSignatory = "Aftab Shaik (Chief Procurement Officer)"
            });
        }

        // GET: /Purchase/GetGRNDetails
        [HttpGet]
        public IActionResult GetGRNDetails(string grnNumber)
        {
            var grn = PurchaseDataStore.GetReceipts().FirstOrDefault(g => g.GRNNumber.Equals(grnNumber, StringComparison.OrdinalIgnoreCase))
                      ?? new GRNItem { GRNNumber = grnNumber, Status = "Inspected & Accepted", PONumber = "PO-2026-0893", VendorName = "National Paper Mills", ReceivedDate = DateTime.Now.ToString("dd MMM yyyy"), Warehouse = "WH-Main Bay A", ItemsReceived = 50, InspectedBy = "Rajesh K." };
            return Json(new
            {
                success = true,
                grn = grn,
                inspectionDate = grn.ReceivedDate,
                deliveryChallanNo = "DC-2026-" + new Random().Next(10000, 99999),
                transporter = "BlueDart Express Freight (Vehicle: KA-01-EA-9821)",
                items = new[]
                {
                    new { Item = "Heavy Corrugated Packaging Cartons (Grade A)", ExpectedQty = grn.ItemsReceived, ReceivedQty = grn.ItemsReceived, AcceptedQty = grn.ItemsReceived, RejectedQty = 0, Remarks = "Passed Tensile & Bursting Test" }
                },
                threeWayMatchStatus = "100% Matched (PO, GRN & Invoice Aligned)",
                variance = "0.00%",
                inspectorRemarks = "Physical count and quality verification completed with zero defect tolerance."
            });
        }

        // GET: /Purchase/GetVendorAudit
        [HttpGet]
        public IActionResult GetVendorAudit(string vendorName)
        {
            var vendor = PurchaseDataStore.GetVendors().FirstOrDefault(v => v.Name.Equals(vendorName, StringComparison.OrdinalIgnoreCase))
                         ?? new VendorScorecardItem { Name = vendorName, Rating = "4.8 / 5.0", Category = "Enterprise Partner", OnTimeDeliveryRate = "98%", QualityScore = "99%", SpendYTD = "₹ 24,50,000.00", Status = "Preferred Partner" };
            return Json(new
            {
                success = true,
                vendor = vendor,
                gstin = "27AAACN" + new Random().Next(1000, 9999) + "K1Z" + new Random().Next(1, 9),
                pan = "AAACN" + new Random().Next(1000, 9999) + "K",
                contactEmail = "procurement-support@" + vendor.Name.ToLower().Replace(" ", "").Replace(".", "") + ".com",
                contactPhone = "+91 98450 " + new Random().Next(10000, 99999),
                leadTimeDays = "4-7 Business Days",
                rejectionRate = "0.3%",
                certifications = new[] { "ISO 9001:2015 Certified", "RoHS Compliant", "Dun & Bradstreet 5A1", "CMMI Level 3" },
                recentPOs = new[] { "PO-2026-0891 (₹ 4.20L - On-Time)", "PO-2026-0872 (₹ 2.80L - On-Time)", "PO-2026-0855 (₹ 6.10L - On-Time)" },
                paymentTerms = "Net 30 Days (Direct NEFT / RTGS)"
            });
        }

        // POST: /Purchase/ResetDemoData
        [HttpPost]
        public IActionResult ResetDemoData()
        {
            PurchaseDataStore.ResetToDefaults();
            return Json(new { success = true, message = "Purchase management data reset to default demo records." });
        }

        private PurchaseDashboardViewModel GetPopulatedPurchaseDashboardVM()
        {
            var reqs = PurchaseDataStore.GetRequisitions();
            var orders = PurchaseDataStore.GetOrders();
            var vendors = PurchaseDataStore.GetVendors();

            return new PurchaseDashboardViewModel
            {
                TotalProcurementSpend = "₹ 38,40,000.00",
                ActiveOrdersCount = orders.Count(p => p.Status != "Delivered"),
                PendingRequisitionsCount = reqs.Count(r => r.Status == "Pending" || string.IsNullOrEmpty(r.Status)),
                ApprovedVendorsCount = vendors.Count,
                PendingApprovals = reqs.Where(r => r.Status == "Pending" || string.IsNullOrEmpty(r.Status)).Take(5).ToList(),
                RecentPurchaseOrders = orders.Take(5).ToList()
            };
        }
    }
}
