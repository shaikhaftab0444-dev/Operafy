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

        // POST: /Purchase/UpdatePRStatus
        [HttpPost]
        [Authorize(Roles = "Purchase Manager,Super Admin,Admin")]
        public async Task<IActionResult> UpdatePRStatus(int prId, string status)
        {
            var approverName = User.Identity?.Name ?? "Chief Procurement Officer";
            var result = status.Equals("Approved", StringComparison.OrdinalIgnoreCase)
                ? PurchaseDataStore.ApproveRequisition(prId, approverName)
                : PurchaseDataStore.RejectRequisition(prId, approverName);

            if (result == null)
            {
                return Json(new { success = false, message = "Requisition not found." });
            }

            return Json(new { success = true, message = $"Purchase Requisition #{prId} has been marked as {status}." });
        }

        // POST: /Purchase/ApproveRequisition (backwards compatibility)
        [HttpPost]
        [Authorize(Roles = "Purchase Manager,Super Admin,Admin")]
        public IActionResult ApproveRequisition(int id, string actionType)
        {
            var approverName = User.Identity?.Name ?? "Chief Procurement Officer";
            var result = (actionType == "Approved")
                ? PurchaseDataStore.ApproveRequisition(id, approverName)
                : PurchaseDataStore.RejectRequisition(id, approverName);

            if (result == null)
            {
                return Json(new { success = false, message = "Requisition not found." });
            }

            return Json(new { success = true, id = id, action = actionType, message = $"Requisition #{id} marked as {actionType}." });
        }

        // GET: /Purchase/CreatePOFromPR
        [HttpGet]
        [Authorize(Roles = "Purchase Manager,Super Admin,Admin")]
        public IActionResult CreatePOFromPR(int prId)
        {
            var req = PurchaseDataStore.GetRequisitions().FirstOrDefault(r => r.Id == prId);
            if (req != null)
            {
                if (req.Status != "Approved")
                {
                    PurchaseDataStore.ApproveRequisition(prId, User.Identity?.Name ?? "Purchase Manager");
                }
                TempData["SuccessMessage"] = $"PR #{prId} ({req.ItemSummary}) successfully converted to Purchase Order.";
            }
            return RedirectToAction("Orders");
        }

        // POST: /Purchase/SubmitPR
        [HttpPost]
        public IActionResult SubmitPR(RequisitionItem item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.ItemSummary))
            {
                return Json(new { success = false, message = "Invalid requisition details." });
            }

            if (string.IsNullOrWhiteSpace(item.RequestedOn))
            {
                item.RequestedOn = DateTime.Now.ToString("dd MMM yyyy");
            }
            item.Status = "Pending";

            var created = PurchaseDataStore.AddRequisition(item);
            return Json(new
            {
                success = true,
                id = created.Id,
                message = $"Purchase Requisition #{created.Id} submitted successfully."
            });
        }

        // GET: /Purchase/Orders
        [HttpGet]
        public IActionResult Orders()
        {
            var model = PurchaseDataStore.GetOrders();
            return View(model);
        }

        // GET: /Purchase/OrderDetails
        [HttpGet]
        public IActionResult OrderDetails(string? poNumber)
        {
            if (string.IsNullOrWhiteSpace(poNumber))
            {
                var firstPo = PurchaseDataStore.GetOrders().FirstOrDefault();
                poNumber = firstPo?.PONumber ?? "PO-2026-0891";
            }

            var po = PurchaseDataStore.GetOrderByNumber(poNumber)
                     ?? PurchaseDataStore.GetOrders().FirstOrDefault(p => p.PONumber.Equals(poNumber, StringComparison.OrdinalIgnoreCase))
                     ?? new PurchaseOrderItem
                     {
                         PONumber = poNumber,
                         VendorName = "TechInfra Solutions Ltd",
                         ItemsCount = 1,
                         TotalAmount = "₹ 1,45,000.00",
                         OrderDate = DateTime.Now.ToString("dd MMM yyyy"),
                         Status = "Confirmed",
                         PaymentTerms = "Net 30"
                     };

            PurchaseDataStore.EnsurePOEnriched(po);
            return View("OrderDetails", po);
        }

        // GET: /Purchase/ViewOrder (friendly alias)
        [HttpGet]
        public IActionResult ViewOrder(string? poNumber)
        {
            return RedirectToAction("OrderDetails", new { poNumber });
        }

        // GET: /Purchase/Vendors
        [HttpGet]
        public IActionResult Vendors()
        {
            var model = PurchaseDataStore.GetVendors();
            return View(model);
        }

        // GET: /Purchase/VendorAudit
        [HttpGet]
        public IActionResult VendorAudit(string? vendorName)
        {
            if (string.IsNullOrWhiteSpace(vendorName))
            {
                var firstVendor = PurchaseDataStore.GetVendors().FirstOrDefault();
                vendorName = firstVendor?.Name ?? "TechInfra Solutions Ltd";
            }

            var dossier = PurchaseDataStore.GetVendorAuditDossier(vendorName);
            return View("VendorAudit", dossier);
        }

        // GET: /Purchase/AuditReport (friendly alias)
        [HttpGet]
        public IActionResult AuditReport(string? vendorName)
        {
            return RedirectToAction("VendorAudit", new { vendorName });
        }

        // GET: /Purchase/Receipts
        [HttpGet]
        public IActionResult Receipts()
        {
            var model = PurchaseDataStore.GetReceipts();
            return View(model);
        }

        // GET: /Purchase/Slip
        [HttpGet]
        public IActionResult Slip(string? grnNumber)
        {
            if (string.IsNullOrWhiteSpace(grnNumber))
            {
                var firstGrn = PurchaseDataStore.GetReceipts().FirstOrDefault();
                grnNumber = firstGrn?.GRNNumber ?? "GRN-2026-0411";
            }

            var slip = PurchaseDataStore.GetGRNSlipDetails(grnNumber);
            return View("Slip", slip);
        }

        // GET: /Purchase/GRNSlip (friendly alias)
        [HttpGet]
        public IActionResult GRNSlip(string? grnNumber)
        {
            return RedirectToAction("Slip", new { grnNumber });
        }

        // GET: /Purchase/ReceiptDetails (friendly alias)
        [HttpGet]
        public IActionResult ReceiptDetails(string? grnNumber)
        {
            return RedirectToAction("Slip", new { grnNumber });
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
            var po = PurchaseDataStore.GetOrderByNumber(poNumber)
                     ?? PurchaseDataStore.GetOrders().FirstOrDefault()
                     ?? new PurchaseOrderItem
                     {
                         PONumber = poNumber ?? "PO-2026-0891",
                         VendorName = "TechInfra Solutions Ltd",
                         ItemsCount = 3,
                         TotalAmount = "₹ 4,20,000.00",
                         OrderDate = DateTime.Now.ToString("dd MMM yyyy"),
                         Status = "Confirmed",
                         PaymentTerms = "Net 30"
                     };

            PurchaseDataStore.EnsurePOEnriched(po);

            // Compute status timeline index
            int stepIndex = 1;
            if (string.Equals(po.Status, "Confirmed", StringComparison.OrdinalIgnoreCase)) stepIndex = 2;
            else if (string.Equals(po.Status, "In Transit", StringComparison.OrdinalIgnoreCase)) stepIndex = 3;
            else if (string.Equals(po.Status, "Delivered", StringComparison.OrdinalIgnoreCase)) stepIndex = 4;

            return Json(new
            {
                success = true,
                po = po,
                poNumber = po.PONumber,
                vendorName = po.VendorName,
                orderDate = po.OrderDate,
                deliveryDate = po.DeliveryDate,
                status = po.Status,
                stepIndex = stepIndex,
                paymentTerms = po.PaymentTerms,
                gstin = po.VendorGstin,
                pan = po.VendorPan,
                vendorEmail = po.VendorEmail,
                vendorPhone = po.VendorPhone,
                vendorAddress = po.VendorAddress,
                billingAddress = "Operafy Systems Corporate HQ, 4th Floor, Tech Park Central, Bangalore - 560100",
                shippingAddress = po.ShippingAddress,
                warehouse = po.Warehouse,
                receivingGate = po.ReceivingGate,
                carrier = po.Carrier,
                trackingNumber = po.TrackingNumber,
                department = po.Department ?? "Corporate Procurement",
                requestedBy = po.RequestedBy ?? "Aftab Shaik",
                items = po.Items.Select(i => new
                {
                    item = i.Item,
                    specification = i.Specification,
                    hsn = i.Hsn,
                    qty = i.Qty,
                    unitPrice = i.UnitPrice,
                    tax = i.Tax,
                    total = i.Total
                }).ToList(),
                subtotal = po.Subtotal,
                taxAmount = po.TaxAmount,
                grandTotal = po.TotalAmount,
                authorizedSignatory = po.AuthorizedSignatory
            });
        }

        // POST: /Purchase/UpdatePOStatus
        [HttpPost]
        public IActionResult UpdatePOStatus(string poNumber, string status)
        {
            if (string.IsNullOrWhiteSpace(poNumber) || string.IsNullOrWhiteSpace(status))
            {
                return Json(new { success = false, message = "PO Number and Status are required." });
            }

            var updated = PurchaseDataStore.UpdateOrderStatus(poNumber, status);
            if (updated == null)
            {
                return Json(new { success = false, message = $"Purchase Order {poNumber} not found." });
            }

            var allOrders = PurchaseDataStore.GetOrders();
            int inTransitCount = allOrders.Count(p => p.Status == "In Transit");
            int deliveredCount = allOrders.Count(p => p.Status == "Delivered");
            int confirmedCount = allOrders.Count(p => p.Status == "Confirmed");

            return Json(new
            {
                success = true,
                poNumber = updated.PONumber,
                status = updated.Status,
                inTransitCount = inTransitCount,
                deliveredCount = deliveredCount,
                confirmedCount = confirmedCount,
                message = $"Purchase Order {updated.PONumber} status updated to '{updated.Status}'."
            });
        }

        // POST: /Purchase/QuickGenerateGRN
        [HttpPost]
        public IActionResult QuickGenerateGRN(string poNumber)
        {
            var po = PurchaseDataStore.GetOrderByNumber(poNumber);
            if (po == null)
            {
                return Json(new { success = false, message = $"Purchase Order {poNumber} not found." });
            }

            var grnNumber = "GRN-2026-0" + new Random().Next(420, 999);
            var grn = new GRNItem
            {
                GRNNumber = grnNumber,
                PONumber = po.PONumber,
                VendorName = po.VendorName,
                ReceivedDate = DateTime.Now.ToString("dd MMM yyyy"),
                Status = "Inspected & Accepted",
                Warehouse = !string.IsNullOrWhiteSpace(po.Warehouse) ? po.Warehouse : "WH-Main Logistics Bay 3",
                ItemsReceived = po.ItemsCount > 0 ? po.ItemsCount : 1,
                InspectedBy = User.Identity?.Name ?? "Aftab Shaik"
            };

            PurchaseDataStore.AddReceipt(grn);
            PurchaseDataStore.UpdateOrderStatus(po.PONumber, "Delivered");

            return Json(new
            {
                success = true,
                grnNumber = grn.GRNNumber,
                poNumber = po.PONumber,
                newStatus = "Delivered",
                message = $"Goods Receipt Note {grn.GRNNumber} created successfully for {po.PONumber}. Order marked as Delivered & 3-Way Matched."
            });
        }

        // GET: /Purchase/GetGRNDetails
        [HttpGet]
        public IActionResult GetGRNDetails(string grnNumber)
        {
            var slip = PurchaseDataStore.GetGRNSlipDetails(grnNumber);
            return Json(new
            {
                success = true,
                slip = slip,
                grn = new
                {
                    grnNumber = slip.GRNNumber,
                    poNumber = slip.PONumber,
                    vendorName = slip.VendorName,
                    receivedDate = slip.ReceivedDate,
                    warehouse = slip.Warehouse,
                    itemsReceived = slip.ItemsReceived,
                    inspectedBy = slip.InspectedBy,
                    status = slip.Status
                },
                grnNumber = slip.GRNNumber,
                poNumber = slip.PONumber,
                vendorName = slip.VendorName,
                vendorGstin = slip.VendorGstin,
                vendorAddress = slip.VendorAddress,
                receivedDate = slip.ReceivedDate,
                inspectionDate = slip.ReceivedDate,
                warehouse = slip.Warehouse,
                storageBin = slip.StorageBin,
                receivingGate = slip.ReceivingGate,
                itemsReceived = slip.ItemsReceived,
                inspectedBy = slip.InspectedBy,
                deliveryChallanNo = slip.DeliveryChallanNo,
                challanDate = slip.ChallanDate,
                transporter = slip.Carrier,
                carrier = slip.Carrier,
                vehicleNumber = slip.VehicleNumber,
                threeWayMatchStatus = slip.ThreeWayMatchStatus,
                variance = slip.Variance,
                inspectorRemarks = slip.InspectorRemarks,
                sapMovementType = slip.SapMovementType,
                billOfLading = slip.BillOfLading,
                items = slip.Items.Select(i => new
                {
                    item = i.Item,
                    specification = i.Specification,
                    hsn = i.Hsn,
                    poQty = i.PoQty,
                    expectedQty = i.PoQty,
                    receivedQty = i.ReceivedQty,
                    acceptedQty = i.AcceptedQty,
                    quarantineQty = i.QuarantineQty,
                    rejectedQty = i.RejectedQty,
                    unit = i.Unit,
                    verdict = i.Verdict,
                    remarks = i.Remarks
                }).ToList()
            });
        }

        // POST: /Purchase/UpdateGRNStatus
        [HttpPost]
        public IActionResult UpdateGRNStatus(string grnNumber, string status, string? remarks)
        {
            if (string.IsNullOrWhiteSpace(grnNumber) || string.IsNullOrWhiteSpace(status))
            {
                return Json(new { success = false, message = "GRN Number and status are required." });
            }

            bool updated = PurchaseDataStore.UpdateGRNStatus(grnNumber, status, remarks);
            if (!updated)
            {
                return Json(new { success = false, message = $"Goods Receipt Note {grnNumber} not found." });
            }

            var receipts = PurchaseDataStore.GetReceipts();
            int quarantineCount = receipts.Count(g => g.Status == "Quality Quarantine");
            int matchedCount = receipts.Count(g => g.Status == "3-Way Matched");
            int acceptedCount = receipts.Count(g => g.Status == "Inspected & Accepted" || g.Status == "Completed & Stocked");

            return Json(new
            {
                success = true,
                grnNumber = grnNumber,
                newStatus = status,
                quarantineCount = quarantineCount,
                matchedCount = matchedCount,
                acceptedCount = acceptedCount,
                message = $"Goods Receipt {grnNumber} successfully updated to '{status}'. Material moved to warehouse bin."
            });
        }

        // GET: /Purchase/GetVendorAudit
        [HttpGet]
        public IActionResult GetVendorAudit(string vendorName)
        {
            var dossier = PurchaseDataStore.GetVendorAuditDossier(vendorName);
            return Json(new
            {
                success = true,
                dossier = dossier,
                vendor = new
                {
                    name = dossier.VendorName,
                    category = dossier.Category,
                    rating = dossier.Rating,
                    status = dossier.Status,
                    spendYtd = dossier.SpendYTD,
                    onTimeDeliveryRate = dossier.OnTimeDeliveryRate,
                    qualityScore = dossier.QualityScore,
                    defectRatio = dossier.DefectRatio,
                    priceCompetitiveness = dossier.PriceCompetitiveness,
                    riskGrade = dossier.RiskGrade
                },
                gstin = dossier.Gstin,
                gstinStatus = dossier.GstinStatus,
                pan = dossier.Pan,
                udyamRegNo = dossier.UdyamRegNo,
                cinNumber = dossier.CinNumber,
                contactPerson = dossier.ContactPerson,
                contactEmail = dossier.ContactEmail,
                contactPhone = dossier.ContactPhone,
                registeredAddress = dossier.RegisteredAddress,
                plantLocation = dossier.PlantLocation,
                bankName = dossier.BankName,
                bankAccountMasked = dossier.BankAccountMasked,
                ifscCode = dossier.IfscCode,
                paymentTerms = dossier.PaymentTerms,
                leadTimeDays = dossier.LeadTimeDays,
                rejectionRate = dossier.DefectRatio,
                certifications = dossier.Certifications,
                recentOrders = dossier.RecentOrders,
                recentPOs = dossier.RecentOrders.Select(p => $"{p.PONumber} ({p.TotalAmount} - {p.Status})").ToList(),
                leadAuditor = dossier.LeadAuditor,
                auditDate = dossier.AuditDate,
                auditValidUntil = dossier.AuditValidUntil,
                nextAuditDate = dossier.NextAuditDate,
                auditorRemarks = dossier.AuditorRemarks,
                auditHistory = dossier.AuditHistory
            });
        }

        // POST: /Purchase/UpdateVendorAuditStatus
        [HttpPost]
        public IActionResult UpdateVendorAuditStatus(string vendorName, string status)
        {
            if (string.IsNullOrWhiteSpace(vendorName) || string.IsNullOrWhiteSpace(status))
            {
                return Json(new { success = false, message = "Vendor name and status are required." });
            }

            bool updated = PurchaseDataStore.UpdateVendorStatus(vendorName, status);
            if (!updated)
            {
                return Json(new { success = false, message = $"Vendor '{vendorName}' not found." });
            }

            var vendors = PurchaseDataStore.GetVendors();
            int activeCount = vendors.Count(v => v.Status != "Blacklisted" && v.Status != "Suspended");

            return Json(new
            {
                success = true,
                vendorName = vendorName,
                newStatus = status,
                activeVendorsCount = activeCount,
                message = $"Vendor '{vendorName}' status updated to '{status}'."
            });
        }

        // POST: /Purchase/SaveVendorAuditRemarks
        [HttpPost]
        public IActionResult SaveVendorAuditRemarks(string vendorName, string remarks, string nextAuditDate, string riskGrade)
        {
            if (string.IsNullOrWhiteSpace(vendorName))
            {
                return Json(new { success = false, message = "Vendor name is required." });
            }

            bool saved = PurchaseDataStore.SaveVendorAuditFinding(vendorName, remarks, nextAuditDate, riskGrade);
            return Json(new
            {
                success = saved,
                vendorName = vendorName,
                message = saved ? "Audit findings & schedule recorded successfully." : "Vendor not found."
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
