using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,Inventory Manager")]
    public class InvAuditsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvAuditsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /InvAudits/StockTake
        [HttpGet]
        public async Task<IActionResult> StockTake()
        {
            var audits = await _context.InvStockAudits.OrderByDescending(a => a.AuditId).ToListAsync();
            return View(audits);
        }

        // POST: /InvAudits/CreateAudit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAudit(InvStockAudit audit)
        {
            if (ModelState.IsValid || audit.AuditorName != null)
            {
                audit.AuditDate = DateTime.Now;
                audit.Status = "Pending Review";
                
                // Generate Audit No
                int count = await _context.InvStockAudits.CountAsync();
                audit.AuditNo = $"AUD-600{(count + 21).ToString("D2")}";

                _context.InvStockAudits.Add(audit);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(StockTake));
            }
            var audits = await _context.InvStockAudits.OrderByDescending(a => a.AuditId).ToListAsync();
            return View(nameof(StockTake), audits);
        }

        // GET: /InvAudits/Reconciliation
        [HttpGet]
        public IActionResult Reconciliation()
        {
            return View();
        }

        // GET: /InvAudits/ScrapWriteOff
        [HttpGet]
        public async Task<IActionResult> ScrapWriteOff()
        {
            var scraps = await _context.InvScrapWriteOffs.OrderByDescending(s => s.ScrapId).ToListAsync();
            return View(scraps);
        }

        // POST: /InvAudits/CreateScrapWriteOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateScrapWriteOff(InvScrapWriteOff scrap)
        {
            if (ModelState.IsValid || (scrap.ItemName != null && scrap.Reason != null))
            {
                scrap.WriteOffDate = DateTime.Now;
                
                // Generate Scrap No
                int count = await _context.InvScrapWriteOffs.CountAsync();
                scrap.ScrapNo = $"SCR-30{(count + 41).ToString("D2")}";

                _context.InvScrapWriteOffs.Add(scrap);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ScrapWriteOff));
            }
            var scraps = await _context.InvScrapWriteOffs.OrderByDescending(s => s.ScrapId).ToListAsync();
            return View(nameof(ScrapWriteOff), scraps);
        }
    }
}
