using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,Sales Manager,Sales Executive,Manager")]
    public class SalesManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SalesManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /SalesManagement/Leads
        [HttpGet]
        public async Task<IActionResult> Leads()
        {
            var leads = await _context.Leads.OrderByDescending(l => l.LeadId).ToListAsync();
            return View(leads);
        }

        // POST: /SalesManagement/CreateLead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLead(Lead lead)
        {
            if (ModelState.IsValid)
            {
                lead.CreatedAt = DateTime.Now;
                _context.Leads.Add(lead);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Leads));
            }
            var leads = await _context.Leads.OrderByDescending(l => l.LeadId).ToListAsync();
            return View(nameof(Leads), leads);
        }

        // GET: /SalesManagement/Quotations
        [HttpGet]
        public async Task<IActionResult> Quotations()
        {
            var quotations = await _context.Quotations.OrderByDescending(q => q.QuotationId).ToListAsync();
            return View(quotations);
        }

        // POST: /SalesManagement/CreateQuotation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQuotation(Quotation quote)
        {
            if (ModelState.IsValid)
            {
                // Generate QuoteNo
                int lastId = await _context.Quotations.MaxAsync(q => (int?)q.QuotationId) ?? 5010;
                quote.QuoteNo = $"QTN-{lastId + 1}";
                _context.Quotations.Add(quote);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Quotations));
            }
            var quotations = await _context.Quotations.OrderByDescending(q => q.QuotationId).ToListAsync();
            return View(nameof(Quotations), quotations);
        }

        // GET: /SalesManagement/Orders
        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            var orders = await _context.SalesOrders.OrderByDescending(o => o.SalesOrderId).ToListAsync();
            return View(orders);
        }

        // POST: /SalesManagement/CreateOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder(SalesOrder order)
        {
            if (ModelState.IsValid)
            {
                // Generate OrderNo
                int lastId = await _context.SalesOrders.MaxAsync(o => (int?)o.SalesOrderId) ?? 9040;
                order.OrderNo = $"SO-{lastId + 1}";
                order.OrderDate = DateTime.Now;
                _context.SalesOrders.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Orders));
            }
            var orders = await _context.SalesOrders.OrderByDescending(o => o.SalesOrderId).ToListAsync();
            return View(nameof(Orders), orders);
        }

        // GET: /SalesManagement/Returns
        [HttpGet]
        public async Task<IActionResult> Returns()
        {
            var returnsList = await _context.SalesReturns.OrderByDescending(r => r.SalesReturnId).ToListAsync();
            return View(returnsList);
        }

        // POST: /SalesManagement/CreateReturn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReturn(SalesReturn returnEntry)
        {
            if (ModelState.IsValid)
            {
                // Generate ReturnNo
                int lastId = await _context.SalesReturns.MaxAsync(r => (int?)r.SalesReturnId) ?? 7000;
                returnEntry.ReturnNo = $"SR-{lastId + 1}";
                returnEntry.ReturnDate = DateTime.Now;
                _context.SalesReturns.Add(returnEntry);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Returns));
            }
            var returnsList = await _context.SalesReturns.OrderByDescending(r => r.SalesReturnId).ToListAsync();
            return View(nameof(Returns), returnsList);
        }

        // GET: /SalesManagement/Receivables
        [HttpGet]
        public async Task<IActionResult> Receivables()
        {
            var receivables = await _context.PaymentReceipts.OrderByDescending(p => p.PaymentReceiptId).ToListAsync();
            return View(receivables);
        }

        // POST: /SalesManagement/CreateReceivable
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReceivable(PaymentReceipt receipt)
        {
            if (ModelState.IsValid)
            {
                receipt.InvoiceDate = DateTime.Now;
                _context.PaymentReceipts.Add(receipt);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Receivables));
            }
            var receivables = await _context.PaymentReceipts.OrderByDescending(p => p.PaymentReceiptId).ToListAsync();
            return View(nameof(Receivables), receivables);
        }
    }
}
