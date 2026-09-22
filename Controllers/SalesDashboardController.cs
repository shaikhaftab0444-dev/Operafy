using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ERP_System.Data;
using ERP_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Sales Executive,Super Admin,Admin,Sales Manager,Manager")]
    public class SalesDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser>? _userManager;

        public SalesDashboardController(ApplicationDbContext context, UserManager<ApplicationUser>? userManager = null)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /SalesDashboard
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            string? currentUserId = null;
            int? currentUid = null;

            // Attempt resolving from UserManager if injected
            if (_userManager != null)
            {
                try
                {
                    var appUser = await _userManager.GetUserAsync(User);
                    if (appUser != null)
                    {
                        currentUserId = appUser.Id;
                        if (int.TryParse(appUser.Id, out int pid)) currentUid = pid;
                    }
                }
                catch { }
            }

            // Fallback to Claims (Cookie authentication)
            if (string.IsNullOrEmpty(currentUserId))
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(idClaim))
                {
                    currentUserId = idClaim;
                    if (int.TryParse(idClaim, out int pid)) currentUid = pid;
                }
                else
                {
                    var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
                    if (!string.IsNullOrEmpty(emailClaim))
                    {
                        var userObj = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailClaim);
                        if (userObj != null)
                        {
                            currentUserId = userObj.UserId.ToString();
                            currentUid = userObj.UserId;
                        }
                    }
                }
            }

            // If user is unauthenticated, challenge
            if (string.IsNullOrEmpty(currentUserId) && !(User.Identity?.IsAuthenticated == true))
            {
                return Challenge();
            }

            // 1. Quota & Target Tracking
            var execProfile = await _context.SalesExecutiveProfiles
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.UserId == currentUserId || (currentUid.HasValue && e.UserUserId == currentUid.Value));

            // If a Super Admin or Manager accesses the dashboard without being mapped to an executive profile,
            // fall back to the first active sales executive profile so the cockpit demonstrates live operational data
            if (execProfile == null)
            {
                execProfile = await _context.SalesExecutiveProfiles
                    .Include(e => e.User)
                    .FirstOrDefaultAsync(e => e.Status == "Active");

                if (execProfile != null)
                {
                    currentUserId = execProfile.UserId;
                    currentUid = execProfile.UserUserId;
                }
            }

            decimal monthlyTarget = execProfile?.MonthlyTarget ?? 1500000m;
            var currentMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            var myMonthlyInvoices = await _context.SalesInvoices
                .Where(i => (i.CreatedByUserId == currentUserId || (currentUid.HasValue && i.CreatedByUserUserId == currentUid.Value))
                         && i.InvoiceDate >= currentMonthStart 
                         && i.Status == "Paid")
                .ToListAsync();

            decimal achievedRevenue = myMonthlyInvoices.Sum(i => i.TotalAmount);
            decimal targetAchievementPct = monthlyTarget > 0 ? (achievedRevenue / monthlyTarget) * 100m : 0m;

            // 2. Active Pipeline (Open Leads assigned strictly to this executive)
            var myLeads = await _context.SalesLeads
                .Where(l => l.AssignedToUserId == currentUserId || (currentUid.HasValue && l.AssignedToUserUserId == currentUid.Value))
                .ToListAsync();

            var activePipeline = myLeads.Where(l => l.Stage != "Closed Won" && l.Stage != "Closed Lost").ToList();
            decimal activePipelineValue = activePipeline.Sum(l => l.EstimatedDealValue);

            // 3. Pending Manager Approvals (Quotations created by this executive requiring approval)
            var pendingQuotations = await _context.SalesQuotations
                .Where(q => (q.CreatedByUserId == currentUserId || (currentUid.HasValue && q.CreatedByUserUserId == currentUid.Value))
                         && q.ApprovalStatus == "Pending Review")
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            // 4. Today's Follow-Ups (Assigned leads in active actionable stages)
            var todaysFollowUps = myLeads
                .Where(l => l.Stage == "New" || l.Stage == "Negotiation" || l.Stage == "Qualified" || l.Stage == "Quotation Sent")
                .OrderByDescending(l => l.CreatedDate)
                .Take(5)
                .ToList();

            // 5. Recent Invoices (Strictly belonging to this executive)
            var recentInvoices = await _context.SalesInvoices
                .Where(i => i.CreatedByUserId == currentUserId || (currentUid.HasValue && i.CreatedByUserUserId == currentUid.Value))
                .OrderByDescending(i => i.InvoiceDate)
                .Take(5)
                .ToListAsync();

            // Assign to ViewBag
            ViewBag.MonthlyTarget = monthlyTarget;
            ViewBag.AchievedRevenue = achievedRevenue;
            ViewBag.TargetAchievementPct = Math.Round(targetAchievementPct, 1);

            ViewBag.ActiveLeadsCount = activePipeline.Count;
            ViewBag.ActivePipelineValue = activePipelineValue;

            ViewBag.PendingQuotations = pendingQuotations;
            ViewBag.TodaysFollowUps = todaysFollowUps;
            ViewBag.RecentInvoices = recentInvoices;

            ViewBag.ExecutiveName = execProfile?.User?.FullName ?? User.Identity?.Name ?? "Sales Executive";
            ViewBag.ExecCode = execProfile?.ExecCode ?? "EX-104";
            ViewBag.Region = execProfile?.Region ?? "North India";

            return View();
        }

        // POST: /SalesDashboard/UpdateLeadStage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateLeadStage(int leadId, string stage)
        {
            var lead = await _context.SalesLeads.FindAsync(leadId);
            if (lead != null)
            {
                lead.Stage = stage;
                if (stage == "Closed Won") lead.WinProbability = 100;
                else if (stage == "Negotiation") lead.WinProbability = 80;
                else if (stage == "Quotation Sent") lead.WinProbability = 65;
                else if (stage == "Qualified") lead.WinProbability = 50;
                else if (stage == "Contacted") lead.WinProbability = 35;
                else if (stage == "Closed Lost") lead.WinProbability = 0;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Lead {lead.LeadCode} status updated to {stage}.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
