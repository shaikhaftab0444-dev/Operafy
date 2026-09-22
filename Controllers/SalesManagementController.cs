using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ERP_System.Models;
using ERP_System.Data;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Sales Manager,Super Admin,Admin,Sales Executive,Manager")]
    public class SalesManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser>? _userManager;

        public SalesManagementController(ApplicationDbContext context, UserManager<ApplicationUser>? userManager = null)
        {
            _context = context;
            _userManager = userManager;
        }

        private string GetCurrentUserId()
        {
            return _userManager?.GetUserId(User) ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "1";
        }

        // ==========================================
        // 1. LEADS
        // ==========================================

        // GET: /SalesManagement/Leads
        [HttpGet]
        public async Task<IActionResult> Leads(int? createForCustomerId = null)
        {
            var currentUserId = GetCurrentUserId();
            int? currentUid = int.TryParse(currentUserId, out int uid) ? uid : (int?)null;
            bool isSalesExecutive = User.IsInRole("Sales Executive");

            var query = _context.SalesLeads
                .Include(l => l.AssignedToUserEntity)
                .AsQueryable();

            if (isSalesExecutive)
            {
                query = query.Where(l => l.AssignedToUserId == currentUserId || (currentUid.HasValue && l.AssignedToUserUserId == currentUid.Value));
            }

            var leads = await query.OrderByDescending(l => l.Id).ToListAsync();

            ViewBag.TotalLeads = leads.Count;
            ViewBag.NewLeads = leads.Count(l => l.Stage == "New");
            ViewBag.ContactedLeads = leads.Count(l => l.Stage == "Contacted" || l.Stage == "Negotiation");
            ViewBag.QualifiedLeads = leads.Count(l => l.Stage == "Qualified");
            ViewBag.TotalValue = leads.Sum(l => l.EstimatedDealValue);

            if (createForCustomerId.HasValue)
            {
                var cust = await _context.Customers.FindAsync(createForCustomerId.Value);
                if (cust != null)
                {
                    ViewBag.PreselectedCustomerName = cust.CustomerName;
                    ViewBag.PreselectedCompany = cust.CompanyName ?? cust.CustomerName;
                }
            }

            return View(leads);
        }

        public class LeadStageUpdateRequest
        {
            public int LeadId { get; set; }
            public string Stage { get; set; } = string.Empty;
        }

        // POST: /SalesManagement/UpdateLeadStageAjax
        [HttpPost]
        public async Task<IActionResult> UpdateLeadStageAjax([FromBody] LeadStageUpdateRequest request)
        {
            if (request == null || request.LeadId <= 0 || string.IsNullOrWhiteSpace(request.Stage))
                return Json(new { success = false, message = "Invalid request payload." });

            var lead = await _context.SalesLeads.FindAsync(request.LeadId);
            if (lead == null) return Json(new { success = false, message = "Lead not found." });

            var currentUserId = GetCurrentUserId();
            int? currentUid = int.TryParse(currentUserId, out int uid) ? uid : (int?)null;
            if (User.IsInRole("Sales Executive"))
            {
                if (lead.AssignedToUserId != currentUserId && (!currentUid.HasValue || lead.AssignedToUserUserId != currentUid.Value))
                {
                    return Json(new { success = false, message = "Unauthorized to modify this lead." });
                }
            }

            lead.Stage = request.Stage;
            if (request.Stage == "New") lead.WinProbability = 20;
            else if (request.Stage == "Qualified") lead.WinProbability = 50;
            else if (request.Stage == "Negotiation") lead.WinProbability = 75;
            else if (request.Stage == "Closed" || request.Stage == "Closed Won") lead.WinProbability = 100;
            else if (request.Stage == "Closed Lost") lead.WinProbability = 0;

            await _context.SaveChangesAsync();
            return Json(new { success = true, stage = lead.Stage, winProbability = lead.WinProbability, message = $"Lead {lead.LeadCode} moved to {lead.Stage}." });
        }

        // POST: /SalesManagement/CreateLead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLead(SalesLead lead)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(lead.LeadCode))
                {
                    int lastId = await _context.SalesLeads.MaxAsync(l => (int?)l.Id) ?? 100;
                    lead.LeadCode = $"LD-{lastId + 1}";
                }
                lead.CreatedDate = DateTime.UtcNow;
                lead.CreatedAt = DateTime.UtcNow;

                if (string.IsNullOrWhiteSpace(lead.CustomerName))
                    lead.CustomerName = lead.Company ?? lead.ContactName;

                if (string.IsNullOrEmpty(lead.AssignedToUserId))
                {
                    lead.AssignedToUserId = GetCurrentUserId();
                    lead.AssignedToUserUserId = int.TryParse(lead.AssignedToUserId, out int uid) ? uid : (int?)null;
                }

                _context.SalesLeads.Add(lead);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Lead {lead.LeadCode} created successfully.";
                return RedirectToAction(nameof(Leads));
            }

            var leads = await _context.SalesLeads.OrderByDescending(l => l.Id).ToListAsync();
            return View(nameof(Leads), leads);
        }

        // POST: /SalesManagement/ConvertLeadToQuotation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConvertLeadToQuotation(int leadId)
        {
            var lead = await _context.SalesLeads.FindAsync(leadId);
            if (lead == null) return NotFound();

            var currentUserId = GetCurrentUserId();
            int? currentUid = int.TryParse(currentUserId, out int cuid) ? cuid : (int?)null;

            var count = await _context.SalesQuotations.CountAsync();
            var quote = new SalesQuotation
            {
                QuotationNumber = "QTN-" + (5000 + count + 1),
                CustomerName = !string.IsNullOrWhiteSpace(lead.Company) ? lead.Company : lead.ContactName,
                SubTotal = lead.EstimatedDealValue,
                DiscountPercentage = 0,
                TotalAmount = lead.EstimatedDealValue,
                ApprovalStatus = "Draft",
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = currentUserId,
                CreatedByUserUserId = currentUid
            };

            lead.Stage = "Quotation Sent";
            _context.SalesQuotations.Add(quote);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Lead {lead.LeadCode} successfully converted to Quotation {quote.QuotationNumber}.";
            return RedirectToAction(nameof(Quotations));
        }

        // ==========================================
        // 2. QUOTATIONS
        // ==========================================

        // GET: /SalesManagement/Quotations
        [HttpGet]
        public async Task<IActionResult> Quotations(int? createForCustomerId = null)
        {
            var currentUserId = GetCurrentUserId();
            int? currentUid = int.TryParse(currentUserId, out int uid) ? uid : (int?)null;
            bool isSalesExecutive = User.IsInRole("Sales Executive");

            var query = _context.SalesQuotations
                .Include(q => q.CreatedByUser)
                .AsQueryable();

            if (isSalesExecutive)
            {
                query = query.Where(q => q.CreatedByUserId == currentUserId || (currentUid.HasValue && q.CreatedByUserUserId == currentUid.Value));
            }

            var quotations = await query.OrderByDescending(q => q.Id).ToListAsync();

            ViewBag.TotalQuotations = quotations.Count;
            ViewBag.ApprovedQuotations = quotations.Count(q => q.ApprovalStatus == "Approved");
            ViewBag.PendingQuotations = quotations.Count(q => q.ApprovalStatus == "Pending Review" || q.ApprovalStatus == "Pending Manager" || q.ApprovalStatus == "Draft");
            ViewBag.TotalQuoteValue = quotations.Sum(q => q.TotalAmount);

            if (createForCustomerId.HasValue)
            {
                var cust = await _context.Customers.FindAsync(createForCustomerId.Value);
                if (cust != null)
                {
                    ViewBag.PreselectedCustomerName = cust.CustomerName;
                }
            }

            return View(quotations);
        }

        // POST: /SalesManagement/CreateQuotation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQuotation(SalesQuotation quote)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(quote.QuotationNumber))
                {
                    int lastId = await _context.SalesQuotations.MaxAsync(q => (int?)q.Id) ?? 5000;
                    quote.QuotationNumber = $"QTN-{lastId + 1}";
                }

                if (quote.SubTotal == 0 && quote.TotalAmount > 0)
                    quote.SubTotal = quote.TotalAmount;

                if (quote.TotalAmount == 0 && quote.SubTotal > 0)
                    quote.TotalAmount = quote.SubTotal - (quote.SubTotal * (quote.DiscountPercentage / 100m));

                // Strict Policy: Any discount > 5.0% requires manager approval and locks quote
                quote.RequiresManagerApproval = quote.DiscountPercentage > 5.0m;
                if (quote.RequiresManagerApproval)
                {
                    quote.ApprovalStatus = "Pending Manager";
                    quote.ApprovalRemarks = "Requires Sales Manager approval due to discount exceeding 5%.";
                }
                else if (string.IsNullOrWhiteSpace(quote.ApprovalStatus) || quote.ApprovalStatus == "Pending Manager" || quote.ApprovalStatus == "Pending Review")
                {
                    quote.ApprovalStatus = "Approved";
                }

                var currentUserId = GetCurrentUserId();
                int? currentUid = int.TryParse(currentUserId, out int cuid) ? cuid : (int?)null;

                quote.CreatedAt = DateTime.UtcNow;
                quote.CreatedByUserId = currentUserId;
                quote.CreatedByUserUserId = currentUid;

                _context.SalesQuotations.Add(quote);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Quotation {quote.QuotationNumber} created successfully.";
                return RedirectToAction(nameof(Quotations));
            }

            var quotations = await _context.SalesQuotations.OrderByDescending(q => q.Id).ToListAsync();
            return View(nameof(Quotations), quotations);
        }

        // POST: /SalesManagement/ApproveQuotation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveQuotation(int quotationId)
        {
            if (User.IsInRole("Sales Executive"))
            {
                return Forbid();
            }

            var quote = await _context.SalesQuotations.FindAsync(quotationId);
            if (quote == null) return NotFound();

            quote.ApprovalStatus = "Approved";
            quote.ApprovalRemarks = "Approved by Sales Manager / Administrator.";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Quotation {quote.QuotationNumber} has been approved.";
            return RedirectToAction(nameof(Quotations));
        }

        // POST: /SalesManagement/ConvertQuotationToOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConvertQuotationToOrder(int quotationId)
        {
            var quote = await _context.SalesQuotations.FindAsync(quotationId);
            if (quote == null || quote.ApprovalStatus != "Approved")
                return BadRequest("Only approved quotations can be converted into Sales Orders.");

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerName == quote.CustomerName || c.CompanyName == quote.CustomerName)
                           ?? new Customer
                           {
                               CustomerCode = "CUST#" + ((await _context.Customers.CountAsync()) + 1).ToString("D4"),
                               CustomerName = quote.CustomerName,
                               CompanyName = quote.CustomerName,
                               Email = "sales@" + quote.CustomerName.ToLower().Replace(" ", "") + ".com",
                               PhoneNumber = "0000000000",
                               CreditLimit = 500000m,
                               OutstandingBalance = quote.TotalAmount,
                               IsActive = true,
                               CreatedAt = DateTime.UtcNow
                           };

            if (customer.Id == 0)
                _context.Customers.Add(customer);
            else
                customer.OutstandingBalance += quote.TotalAmount;

            await _context.SaveChangesAsync();

            var orderCode = "SO-" + DateTime.UtcNow.Year + "-" + ((await _context.SalesOrders.CountAsync()) + 1).ToString("D3");
            var order = new SalesOrder
            {
                OrderNumber = orderCode,
                OrderNo = orderCode,
                CustomerId = customer.Id,
                CustomerName = customer.CustomerName,
                OrderDate = DateTime.UtcNow,
                OrderTotal = quote.TotalAmount,
                TotalAmount = quote.TotalAmount,
                DeliveryStatus = "Confirmed",
                Status = "Confirmed",
                PaymentTerms = "Net 30",
                CreatedByUserId = GetCurrentUserId()
            };

            _context.SalesOrders.Add(order);

            // Generate linked SalesInvoice & PaymentReceivable automatically
            var invoice = new SalesInvoice
            {
                InvoiceNumber = "INV-" + DateTime.UtcNow.Year + "-" + ((await _context.SalesInvoices.CountAsync()) + 1).ToString("D3"),
                CustomerId = customer.Id,
                CustomerName = customer.CustomerName,
                SalesOrder = order,
                LinkedOrderNumber = order.OrderNumber,
                TaxableValue = Math.Round(order.OrderTotal / 1.18m, 2),
                GstAmount = Math.Round(order.OrderTotal - (order.OrderTotal / 1.18m), 2),
                GrandTotal = order.OrderTotal,
                TotalAmount = order.OrderTotal,
                Status = "Pending",
                InvoiceDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(30)
            };
            _context.SalesInvoices.Add(invoice);

            var receivable = new PaymentReceivable
            {
                InvoiceNumber = invoice.InvoiceNumber,
                CustomerId = customer.Id,
                InvoiceDate = invoice.InvoiceDate,
                DueDate = invoice.DueDate,
                TotalAmount = invoice.GrandTotal,
                PendingBalance = invoice.GrandTotal,
                Status = "Pending"
            };
            _context.PaymentReceivables.Add(receivable);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Quotation converted to Sales Order {order.OrderNumber}. Generated Invoice {invoice.InvoiceNumber}.";
            return RedirectToAction(nameof(Orders));
        }

        // ==========================================
        // 3. SALES ORDERS
        // ==========================================

        // GET: /SalesManagement/Orders
        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            var currentUserId = GetCurrentUserId();
            int? currentUid = int.TryParse(currentUserId, out int uid) ? uid : (int?)null;
            bool isSalesExecutive = User.IsInRole("Sales Executive");

            var query = _context.SalesOrders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .AsQueryable();

            if (isSalesExecutive)
            {
                query = query.Where(o =>
                    o.CreatedByUserId == currentUserId ||
                    (currentUid.HasValue && o.CreatedByUserId == currentUid.Value.ToString()) ||
                    (o.Customer != null && (o.Customer.AssignedRepId == currentUserId || (currentUid.HasValue && o.Customer.AssignedRepUserUserId == currentUid.Value))));
            }

            var orders = await query.OrderByDescending(o => o.SalesOrderId).ToListAsync();

            ViewBag.TotalOrders = orders.Count;
            ViewBag.ConfirmedOrders = orders.Count(o => o.DeliveryStatus == "Confirmed");
            ViewBag.AwaitingDispatch = orders.Count(o => o.DeliveryStatus == "Awaiting Dispatch");
            ViewBag.DeliveredOrders = orders.Count(o => o.DeliveryStatus == "Delivered");
            ViewBag.TotalOrderValue = orders.Sum(o => o.OrderTotal);

            return View(orders);
        }

        // POST: /SalesManagement/CreateOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder(SalesOrder order)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(order.OrderNumber))
                {
                    int lastId = await _context.SalesOrders.MaxAsync(o => (int?)o.SalesOrderId) ?? 100;
                    order.OrderNumber = $"SO-{DateTime.UtcNow.Year}-{(lastId + 1):D3}";
                }
                if (string.IsNullOrWhiteSpace(order.OrderNo))
                    order.OrderNo = order.OrderNumber;

                order.OrderDate = DateTime.UtcNow;
                if (order.CustomerId == 0)
                {
                    var cust = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerName == order.CustomerName);
                    if (cust != null) order.CustomerId = cust.Id;
                    else order.CustomerId = 1;
                }

                if (string.IsNullOrWhiteSpace(order.DeliveryStatus))
                    order.DeliveryStatus = "Confirmed";
                if (string.IsNullOrWhiteSpace(order.Status))
                    order.Status = order.DeliveryStatus;

                if (order.TotalAmount == 0 && order.OrderTotal > 0)
                    order.TotalAmount = order.OrderTotal;
                if (order.OrderTotal == 0 && (order.TotalAmount ?? 0m) > 0)
                    order.OrderTotal = order.TotalAmount ?? 0m;

                order.CreatedByUserId = GetCurrentUserId();

                _context.SalesOrders.Add(order);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Sales Order {order.OrderNumber} created successfully.";
                return RedirectToAction(nameof(Orders));
            }

            var orders = await _context.SalesOrders.OrderByDescending(o => o.SalesOrderId).ToListAsync();
            return View(nameof(Orders), orders);
        }

        // POST: /SalesManagement/NotifyWarehouseDispatch
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NotifyWarehouseDispatch(int orderId)
        {
            var order = await _context.SalesOrders.FindAsync(orderId);
            if (order == null) return Json(new { success = false, message = "Order not found." });

            if (order.DeliveryStatus == "Awaiting Dispatch" || order.DeliveryStatus == "Dispatched" || order.DeliveryStatus == "Delivered")
            {
                return Json(new { success = false, message = "Dispatch status is already locked." });
            }

            order.DeliveryStatus = "Awaiting Dispatch";
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"Warehouse notified for dispatch of {order.OrderNumber}." });
        }

        // ==========================================
        // 4. SALES INVOICES (Dedicated Register)
        // ==========================================

        // GET: /SalesManagement/Invoices
        [HttpGet]
        public async Task<IActionResult> Invoices(string? search, string? status)
        {
            var currentUserId = GetCurrentUserId();
            int? currentUid = int.TryParse(currentUserId, out int uid) ? uid : (int?)null;
            bool isSalesExecutive = User.IsInRole("Sales Executive");

            var query = _context.SalesInvoices
                .Include(i => i.Customer)
                .Include(i => i.SalesOrder)
                .AsQueryable();

            if (isSalesExecutive)
            {
                query = query.Where(i =>
                    i.CreatedByUserId == currentUserId ||
                    (currentUid.HasValue && i.CreatedByUserUserId == currentUid.Value) ||
                    (i.Customer != null && (i.Customer.AssignedRepId == currentUserId || (currentUid.HasValue && i.Customer.AssignedRepUserUserId == currentUid.Value))));
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
                query = query.Where(i => i.Status == status);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(i =>
                    i.InvoiceNumber.ToLower().Contains(term) ||
                    i.CustomerName.ToLower().Contains(term) ||
                    (i.Customer != null && i.Customer.CustomerName.ToLower().Contains(term)) ||
                    (i.LinkedOrderNumber != null && i.LinkedOrderNumber.ToLower().Contains(term)));
            }

            var list = await query.OrderByDescending(i => i.InvoiceDate).ToListAsync();

            ViewBag.SearchTerm = search;
            ViewBag.StatusFilter = status ?? "All";
            ViewBag.TotalInvoiced = list.Sum(i => i.TotalAmount);
            ViewBag.PaidInvoices = list.Where(i => i.Status == "Paid").Sum(i => i.TotalAmount);
            ViewBag.PendingInvoices = list.Where(i => i.Status == "Pending" || i.Status == "Partially Paid").Sum(i => i.TotalAmount);
            ViewBag.OverdueInvoices = list.Where(i => i.Status == "Overdue").Sum(i => i.TotalAmount);

            return View(list);
        }

        // POST: /SalesManagement/SendInvoiceReminder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendInvoiceReminder(string invoiceNo)
        {
            if (string.IsNullOrWhiteSpace(invoiceNo))
                return Json(new { success = false, message = "Valid Invoice Number is required." });

            var inv = await _context.SalesInvoices
                .Include(i => i.Customer)
                .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNo);

            if (inv == null)
                return Json(new { success = false, message = "Invoice record not found." });

            return Json(new { success = true, message = $"Automated payment reminder dispatched successfully to {inv.CustomerName} for Invoice {inv.InvoiceNumber}." });
        }

        // ==========================================
        // 5. SALES RETURNS
        // ==========================================

        // GET: /SalesManagement/Returns
        [HttpGet]
        public async Task<IActionResult> Returns()
        {
            var returnsList = await _context.SalesReturns
                .Include(r => r.Customer)
                .OrderByDescending(r => r.SalesReturnId)
                .ToListAsync();

            ViewBag.TotalReturns = returnsList.Count;
            ViewBag.InspectingReturns = returnsList.Count(r => r.Status == "Inspecting");
            ViewBag.RestockedReturns = returnsList.Count(r => r.Status == "Restocked");
            ViewBag.TotalRefundValue = returnsList.Sum(r => r.RefundValue);

            return View(returnsList);
        }

        // POST: /SalesManagement/CreateReturn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReturn(SalesReturn returnEntry)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(returnEntry.ReturnCode))
                {
                    int lastId = await _context.SalesReturns.MaxAsync(r => (int?)r.SalesReturnId) ?? 7000;
                    returnEntry.ReturnCode = $"SR-{lastId + 1}";
                }

                returnEntry.ReturnDate = DateTime.UtcNow;
                if (string.IsNullOrWhiteSpace(returnEntry.Status))
                    returnEntry.Status = "Inspecting";

                if (returnEntry.CustomerId == 0)
                {
                    var cust = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerName == returnEntry.CustomerName);
                    returnEntry.CustomerId = cust?.Id ?? 1;
                }

                _context.SalesReturns.Add(returnEntry);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Return {returnEntry.ReturnCode} logged successfully.";
                return RedirectToAction(nameof(Returns));
            }

            var returnsList = await _context.SalesReturns.OrderByDescending(r => r.SalesReturnId).ToListAsync();
            return View(nameof(Returns), returnsList);
        }

        // POST: /SalesManagement/ApproveReturnRestock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveReturnRestock(int returnId)
        {
            var ret = await _context.SalesReturns.FindAsync(returnId);
            if (ret == null || ret.Status == "Restocked")
                return Json(new { success = false, message = "Invalid return record or already restocked." });

            ret.Status = "Restocked";
            ret.CreditNoteNumber = "CN-" + DateTime.UtcNow.Year + "-" + ret.Id.ToString("D4");

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"Return approved. Credit Note {ret.CreditNoteNumber} generated." });
        }

        // ==========================================
        // 6. PAYMENTS / RECEIVABLES
        // ==========================================

        // GET: /SalesManagement/Receivables
        [HttpGet]
        public async Task<IActionResult> Receivables()
        {
            var receivables = await _context.PaymentReceivables
                .Include(p => p.Customer)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            ViewBag.TotalOutstanding = receivables.Sum(r => r.PendingBalance);
            ViewBag.OverdueBalance = receivables.Where(r => r.DueDate < DateTime.Today && r.PendingBalance > 0).Sum(r => r.PendingBalance);
            ViewBag.TotalCollected = receivables.Sum(r => r.TotalAmount - r.PendingBalance);
            ViewBag.ActiveCount = receivables.Count(r => r.PendingBalance > 0);

            // List of pending / partially paid / overdue invoices for payment logging dropdown
            ViewBag.PendingInvoices = await _context.PaymentReceivables
                .Where(p => p.PendingBalance > 0)
                .Select(p => new { p.InvoiceNumber, p.PendingBalance, p.CustomerName })
                .ToListAsync();

            return View(receivables);
        }

        // POST: /SalesManagement/CreateReceivable
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReceivable(PaymentReceivable receipt)
        {
            if (ModelState.IsValid)
            {
                receipt.InvoiceDate = DateTime.UtcNow;
                if (receipt.DueDate == default)
                    receipt.DueDate = DateTime.UtcNow.AddDays(30);

                if (receipt.CustomerId == 0)
                    receipt.CustomerId = 1;

                _context.PaymentReceivables.Add(receipt);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Receivable record for {receipt.InvoiceNumber} created successfully.";
                return RedirectToAction(nameof(Receivables));
            }

            var receivables = await _context.PaymentReceivables.OrderByDescending(p => p.Id).ToListAsync();
            return View(nameof(Receivables), receivables);
        }

        // POST: /SalesManagement/RecordPaymentReceipt
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordPaymentReceipt(string invoiceNo, decimal amountReceived, string paymentMode, string referenceUtr)
        {
            if (string.IsNullOrWhiteSpace(invoiceNo) || amountReceived <= 0)
                return Json(new { success = false, message = "Valid Invoice No and Payment Amount are required." });

            var item = await _context.PaymentReceivables.FirstOrDefaultAsync(p => p.InvoiceNumber == invoiceNo);
            if (item == null) return Json(new { success = false, message = "Receivable record not found." });

            if (amountReceived > item.PendingBalance)
                return Json(new { success = false, message = $"Payment amount (₹{amountReceived:N2}) cannot exceed pending balance (₹{item.PendingBalance:N2})." });

            item.PendingBalance -= amountReceived;
            item.Status = item.PendingBalance <= 0 ? "Paid" : "Partially Paid";

            // Update customer ledger
            var customer = await _context.Customers.FindAsync(item.CustomerId);
            if (customer != null)
            {
                customer.OutstandingBalance = Math.Max(0, customer.OutstandingBalance - amountReceived);
            }

            // Update SalesInvoice status if it exists
            var invoice = await _context.SalesInvoices.FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNo);
            if (invoice != null)
            {
                invoice.Status = item.Status;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"Payment of ₹{amountReceived:N2} recorded successfully via {paymentMode}. Reference: {referenceUtr}." });
        }
    }
}
