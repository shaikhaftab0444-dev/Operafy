using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Auditor,Super Admin,Admin")]
    public class AuditorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Auditor or /Auditor/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var flagged = await _context.AuditFlaggedItems
                .OrderByDescending(f => f.FlaggedOn)
                .Take(10)
                .ToListAsync();

            var recentLogs = await _context.SystemAuditTrails
                .Include(a => a.PerformedByUser)
                .OrderByDescending(a => a.Timestamp)
                .Take(8)
                .ToListAsync();

            var totalPending = flagged.Count(f => f.Status == "Pending Review");
            var highRisk = flagged.Count(f => f.Severity == "High" && f.Status == "Pending Review");
            var totalAmount = flagged.Where(f => f.Status == "Pending Review").Sum(f => f.DiscrepancyAmount);

            var vm = new AuditorDashboardViewModel
            {
                TotalAuditsConducted = await _context.AuditFlaggedItems.CountAsync(f => f.Status == "Cleared") + 45,
                PendingDiscrepanciesCount = totalPending,
                HighRiskFlagsCount = highRisk,
                TotalDiscrepancyAmount = string.Format("₹ {0:N2}", totalAmount),
                FlaggedItems = flagged,
                RecentAuditLogs = recentLogs
            };

            return View(vm);
        }

        // GET: /Auditor/Logs
        [HttpGet]
        public async Task<IActionResult> Logs(string entity = "", string search = "")
        {
            var query = _context.SystemAuditTrails.Include(a => a.PerformedByUser).AsQueryable();

            if (!string.IsNullOrEmpty(entity) && entity != "All")
            {
                query = query.Where(q => q.EntityName == entity);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(q => q.ChangesSummary.Contains(search) || q.RecordId.Contains(search) || q.ActionType.Contains(search));
            }

            var list = await query.OrderByDescending(q => q.Timestamp).Take(50).ToListAsync();
            ViewBag.SelectedEntity = entity;
            ViewBag.SearchTerm = search;
            return View(list);
        }

        // GET: /Auditor/FinancialReview
        [HttpGet]
        public async Task<IActionResult> FinancialReview()
        {
            var items = await _context.AuditFlaggedItems
                .Where(f => f.Module == "Finance")
                .OrderByDescending(f => f.FlaggedOn)
                .ToListAsync();
            return View(items);
        }

        // POST: /Auditor/ResolveAuditFlag
        [HttpPost]
        public async Task<IActionResult> ResolveAuditFlag([FromBody] UpdateAuditStatusInput input)
        {
            if (input == null || input.FlagId <= 0)
                return Json(new { success = false, message = "Invalid audit flag parameters." });

            var item = await _context.AuditFlaggedItems.FindAsync(input.FlagId);
            if (item == null)
                return Json(new { success = false, message = "Flagged record not found." });

            var oldStatus = item.Status;
            item.Status = input.NewStatus;

            // Log entry in SystemAuditTrails
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1";
            int.TryParse(currentUserId, out int parsedUserId);

            _context.SystemAuditTrails.Add(new SystemAuditTrail
            {
                EntityName = "AuditFlaggedItem",
                RecordId = $"FLAG-{item.Id}",
                ActionType = input.NewStatus == "Cleared" ? "APPROVAL" : "UPDATE",
                PerformedByUserId = currentUserId,
                PerformedByUserUserId = parsedUserId > 0 ? parsedUserId : 1,
                ChangesSummary = $"Audit flag #{item.Id} ({item.ReferenceNumber}) status updated from '{oldStatus}' to '{input.NewStatus}'. Note: {input.AuditNote ?? "Resolved in Auditor Center"}",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"Item #{input.FlagId} ({item.ReferenceNumber}) status updated to {input.NewStatus}." });
        }

        // POST: /Auditor/FlagNewItem
        [HttpPost]
        public async Task<IActionResult> FlagNewItem([FromBody] CreateAuditFlagInput input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.ReferenceNumber))
                return Json(new { success = false, message = "Reference ID / number is required." });

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1";
            int.TryParse(currentUserId, out int parsedUserId);

            var flag = new AuditFlaggedItem
            {
                Module = input.Module ?? "Finance",
                ReferenceNumber = input.ReferenceNumber.Trim(),
                Description = input.Description ?? "Discrepancy logged by auditor",
                DiscrepancyAmount = input.DiscrepancyAmount,
                Severity = input.Severity ?? "Medium",
                Status = "Pending Review",
                FlaggedByUserId = currentUserId,
                FlaggedOn = DateTime.UtcNow
            };

            _context.AuditFlaggedItems.Add(flag);
            await _context.SaveChangesAsync();

            // Log entry in SystemAuditTrails
            _context.SystemAuditTrails.Add(new SystemAuditTrail
            {
                EntityName = flag.Module,
                RecordId = flag.ReferenceNumber,
                ActionType = "CREATE",
                PerformedByUserId = currentUserId,
                PerformedByUserUserId = parsedUserId > 0 ? parsedUserId : 1,
                ChangesSummary = $"New discrepancy flag raised on {flag.Module} [{flag.ReferenceNumber}] - Amount: ₹{flag.DiscrepancyAmount:N2}, Severity: {flag.Severity}",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Transaction discrepancy logged successfully!" });
        }
    }
}
