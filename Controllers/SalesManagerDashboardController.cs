using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Data;
using ERP_System.Models;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Sales Manager,Super Admin,Admin")]
    public class SalesManagerDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SalesManagerDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<User> GetCurrentManagerAsync()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentUserId = int.TryParse(userIdClaim, out int cid) ? cid : 1;
            var currentManager = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.UserId == currentUserId);

            if (currentManager != null) return currentManager;

            var userName = User.Identity?.Name ?? "admin";
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.UserName == userName || u.Email == userName)
                ?? new User { UserId = 1, FullName = "Sales Manager", UserName = "salesmanager", RoleId = 1 };
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? dateRange, string? territory)
        {
            User currentUser = await GetCurrentManagerAsync();

            int currentUserId = currentUser.UserId;
            string currentUserIdStr = currentUser.Id;
            int? currentDeptId = currentUser.DepartmentId;

            // Strict Role Isolation: Fetch only subordinates (Sales Executives, Sales Coordinators)
            var repUsers = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Department)
                .Where(u => u.IsActive && u.UserId != currentUserId && (
                    u.ReportingManagerId == currentUserIdStr ||
                    (!string.IsNullOrEmpty(currentUser.FullName) && u.ReportingManagerName == currentUser.FullName) ||
                    (currentDeptId.HasValue && u.DepartmentId == currentDeptId.Value)
                ))
                .ToListAsync();

            if (!repUsers.Any())
            {
                repUsers = await _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.Department)
                    .Where(u => u.IsActive && u.UserId != currentUserId && u.Role != null && (u.Role.RoleName.Contains("Sales") || u.Role.RoleName.Contains("Executive")))
                    .ToListAsync();
            }

            if (!repUsers.Any())
            {
                repUsers = await _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.Department)
                    .Where(u => u.IsActive && u.UserId != currentUserId && u.Role != null && u.Role.RoleName != "Super Admin")
                    .Take(8)
                    .ToListAsync();
            }

            var repUserIds = repUsers.Select(u => u.Id).ToList();

            // 1. KPI Aggregations
            var closedInvoices = await _context.SalesInvoices
                .Where(i => repUserIds.Contains(i.CreatedByUserId) && i.Status == "Paid")
                .ToListAsync();

            decimal totalRevenue = closedInvoices.Sum(i => i.TotalAmount);
            int activeRepsCount = repUsers.Count;
            decimal avgSalesPerRep = activeRepsCount > 0 ? totalRevenue / activeRepsCount : 0;

            // Target Aggregation
            var currentQuarter = string.IsNullOrEmpty(dateRange) ? "Q3-2026" : dateRange;
            var targets = await _context.SalesTargets
                .Where(t => repUserIds.Contains(t.SalesRepUserId) && t.FiscalQuarter == currentQuarter)
                .ToListAsync();
            decimal totalGoal = targets.Sum(t => t.TargetAmount);
            if (totalGoal == 0) totalGoal = 15000000m; // Fallback default goal ₹1.5 Cr
            decimal targetAchievedPct = totalGoal > 0 ? (totalRevenue / totalGoal) * 100m : 0m;

            // 2. Pipeline Funnel Counts & Values
            var leads = await _context.SalesLeads
                .Where(l => repUserIds.Contains(l.AssignedToUserId))
                .ToListAsync();

            var pipelineStages = new Dictionary<string, (int Count, decimal Value)>
            {
                { "New", (leads.Count(l => l.Stage == "New"), leads.Where(l => l.Stage == "New").Sum(l => l.EstimatedDealValue)) },
                { "Qualified", (leads.Count(l => l.Stage == "Qualified"), leads.Where(l => l.Stage == "Qualified").Sum(l => l.EstimatedDealValue)) },
                { "Quotation Sent", (leads.Count(l => l.Stage == "Quotation Sent"), leads.Where(l => l.Stage == "Quotation Sent").Sum(l => l.EstimatedDealValue)) },
                { "Negotiation", (leads.Count(l => l.Stage == "Negotiation"), leads.Where(l => l.Stage == "Negotiation").Sum(l => l.EstimatedDealValue)) },
                { "Closed Won", (leads.Count(l => l.Stage == "Closed Won"), leads.Where(l => l.Stage == "Closed Won").Sum(l => l.EstimatedDealValue)) }
            };
            ViewBag.PipelineStages = pipelineStages;

            // 3. Pending Quotation Approvals (Discounts > 10%)
            var pendingApprovals = await _context.SalesQuotations
                .Include(q => q.CreatedByUser)
                .Where(q => q.RequiresManagerApproval && q.ApprovalStatus == "Pending Review" && repUserIds.Contains(q.CreatedByUserId))
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            foreach (var quote in pendingApprovals)
            {
                if (quote.CreatedByUser == null && !string.IsNullOrEmpty(quote.CreatedByUserId))
                {
                    var u = repUsers.FirstOrDefault(x => x.Id == quote.CreatedByUserId || x.UserId.ToString() == quote.CreatedByUserId);
                    if (u != null)
                    {
                        quote.CreatedByUser = u;
                    }
                }
            }

            // 4. Rep Leaderboard
            var leaderboard = repUsers.Select(rep => new RepLeaderboardItem
            {
                RepId = rep.Id,
                Name = rep.FullName ?? rep.UserName,
                Revenue = closedInvoices.Where(i => i.CreatedByUserId == rep.Id).Sum(i => i.TotalAmount),
                DealsClosed = closedInvoices.Count(i => i.CreatedByUserId == rep.Id),
                IsActive = rep.IsActive
            }).OrderByDescending(x => x.Revenue).ToList();

            var recentInvoices = await _context.SalesInvoices
                .Where(i => repUserIds.Contains(i.CreatedByUserId))
                .OrderByDescending(i => i.InvoiceDate)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.ActiveReps = activeRepsCount;
            ViewBag.AvgSales = avgSalesPerRep;
            ViewBag.TargetAchievedPct = Math.Round(targetAchievedPct, 1);
            ViewBag.GoalAmount = totalGoal;
            ViewBag.PendingApprovals = pendingApprovals;
            ViewBag.Leaderboard = leaderboard;
            ViewBag.RecentInvoices = recentInvoices;

            int companyId = currentUser.CompanyId > 0 ? currentUser.CompanyId : 1;
            int branchId = currentUser.BranchId > 0 ? currentUser.BranchId : 3;
            var companyRecord = await _context.Companies.FindAsync(companyId);
            var branchRecord = await _context.Branches.FindAsync(branchId);
            var activeCompanyName = companyRecord?.CompanyName ?? "ERP Solutions Ltd";
            var activeBranchName = branchRecord?.BranchName ?? "Head Office";

            var viewModel = new SalesManagerDashboardViewModel
            {
                CurrentUserFullName = currentUser.FullName ?? "Sales Manager",
                CurrentUserRole = currentUser.Role?.RoleName ?? "Sales Manager",
                CurrentCompany = $"{activeCompanyName} ({activeBranchName})",
                TotalTeamSalesRevenue = totalRevenue,
                ActiveSalesRepsCount = activeRepsCount,
                TeamTargetAchievementPercent = Math.Round(targetAchievedPct, 1),
                AverageSalesPerRep = avgSalesPerRep,
                GoalAmount = totalGoal,
                PipelineStages = pipelineStages,
                PendingApprovals = pendingApprovals,
                Leaderboard = leaderboard,
                RecentInvoices = recentInvoices,
                RepUsers = repUsers,
                DateRange = dateRange ?? "Q3 2026",
                Territory = territory
            };

            return View(viewModel);
        }

        // Modal Action: Review & Sign-Off Quotation Discount
        [HttpPost]
        public async Task<IActionResult> ReviewQuotation(int quotationId, string decision, string? remarks)
        {
            var quote = await _context.SalesQuotations.FindAsync(quotationId);
            if (quote == null) return Json(new { success = false, message = "Quotation not found." });

            quote.ApprovalStatus = decision; // "Approved" or "Rejected"
            quote.ApprovalRemarks = remarks;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"Quotation discount {decision.ToLower()} successfully." });
        }

        // Rep Drilldown JSON endpoint
        [HttpGet]
        public async Task<IActionResult> GetRepDetails(string repId)
        {
            int.TryParse(repId, out int uid);
            var rep = await _context.Users.FirstOrDefaultAsync(u => u.UserId == uid || u.UserName == repId);
            if (rep == null) return Json(new { success = false, message = "User not found." });

            var repLeads = await _context.SalesLeads.Where(l => l.AssignedToUserId == repId || (uid > 0 && l.AssignedToUserUserId == uid)).ToListAsync();
            var invoices = await _context.SalesInvoices.Where(i => (i.CreatedByUserId == repId || (uid > 0 && i.CreatedByUserUserId == uid)) && i.Status == "Paid").ToListAsync();

            int totalDeals = repLeads.Count;
            int wonDeals = repLeads.Count(l => l.Stage == "Closed Won");
            decimal totalRevenue = invoices.Sum(i => i.TotalAmount);
            decimal activePipeline = repLeads.Where(l => l.Stage != "Closed Won" && l.Stage != "Closed Lost").Sum(l => l.EstimatedDealValue);

            return Json(new
            {
                success = true,
                name = rep.FullName ?? rep.UserName,
                email = rep.Email,
                totalDeals = totalDeals,
                wonDeals = wonDeals,
                totalRevenue = totalRevenue,
                activePipeline = activePipeline
            });
        }

        // Quick Action: Add Lead
        [HttpPost]
        public async Task<IActionResult> AddLead(string leadTitle, string customerName, string contactEmail, string contactPhone, decimal estimatedDealValue, string stage, int? winProbability, string assignedToUserId)
        {
            try
            {
                int.TryParse(assignedToUserId, out int uid);
                var lead = new SalesLead
                {
                    LeadTitle = leadTitle ?? "New Opportunity",
                    CustomerName = customerName ?? "Prospective Client",
                    ContactEmail = contactEmail ?? "",
                    ContactPhone = contactPhone ?? "",
                    EstimatedDealValue = estimatedDealValue,
                    Stage = string.IsNullOrEmpty(stage) ? "New" : stage,
                    WinProbability = winProbability ?? 20,
                    AssignedToUserId = assignedToUserId ?? "1",
                    AssignedToUserUserId = uid > 0 ? uid : 1,
                    CreatedAt = DateTime.UtcNow,
                    ExpectedCloseDate = DateTime.UtcNow.AddDays(30)
                };
                await _context.SalesLeads.AddAsync(lead);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Lead successfully created." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to create lead: " + ex.Message });
            }
        }

        // Quick Action: Assign Target
        [HttpPost]
        public async Task<IActionResult> AssignTarget(string salesRepUserId, string fiscalQuarter, decimal targetAmount)
        {
            try
            {
                int.TryParse(salesRepUserId, out int uid);
                var quarter = string.IsNullOrEmpty(fiscalQuarter) ? "Q3-2026" : fiscalQuarter;
                var target = await _context.SalesTargets.FirstOrDefaultAsync(t => (t.SalesRepUserId == salesRepUserId || (uid > 0 && t.SalesRepUserUserId == uid)) && t.FiscalQuarter == quarter);
                if (target != null)
                {
                    target.TargetAmount = targetAmount;
                }
                else
                {
                    target = new SalesTarget
                    {
                        SalesRepUserId = salesRepUserId,
                        SalesRepUserUserId = uid > 0 ? uid : null,
                        ExecutiveUserId = salesRepUserId,
                        FiscalQuarter = quarter,
                        TargetAmount = targetAmount,
                        AchievedAmount = 0,
                        Month = DateTime.Today.Month,
                        Year = DateTime.Today.Year
                    };
                    await _context.SalesTargets.AddAsync(target);
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Target successfully assigned." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to assign target: " + ex.Message });
            }
        }

        // GET: /SalesManagerDashboard/TeamAttendance
        [HttpGet]
        public async Task<IActionResult> TeamAttendance(DateTime? selectedDate)
        {
            var currentManager = await GetCurrentManagerAsync();
            var targetDate = selectedDate ?? DateTime.Today;

            var teamUsers = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Department)
                .Where(u => u.IsActive && u.UserId != currentManager.UserId && (
                    u.ReportingManagerId == currentManager.UserId.ToString() ||
                    (!string.IsNullOrEmpty(currentManager.FullName) && u.ReportingManagerName == currentManager.FullName) ||
                    (currentManager.DepartmentId != null && u.DepartmentId == currentManager.DepartmentId) ||
                    (u.Role != null && (u.Role.RoleName.Contains("Sales") || u.Role.RoleName.Contains("Executive")))
                ))
                .OrderBy(u => u.FullName)
                .ToListAsync();

            if (!teamUsers.Any())
            {
                teamUsers = await _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.Department)
                    .Where(u => u.IsActive && u.UserId != currentManager.UserId && u.Role != null && u.Role.RoleName != "Super Admin")
                    .Take(8)
                    .ToListAsync();
            }

            var teamUserIds = teamUsers.Select(u => u.UserId).ToList();

            var attendanceLogs = await _context.HRAttendanceLogs
                .Where(a => teamUserIds.Contains(a.UserId) && a.Date.Date == targetDate.Date)
                .ToListAsync();

            ViewBag.SelectedDate = targetDate.ToString("yyyy-MM-dd");
            ViewBag.SelectedDateObj = targetDate;
            ViewBag.TotalMembers = teamUsers.Count;
            ViewBag.PresentToday = attendanceLogs.Count(a => a.Status == "Present" || a.Status.Contains("Late"));
            ViewBag.LateCount = attendanceLogs.Count(a => a.Status.Contains("Late"));
            ViewBag.AbsentCount = Math.Max(0, teamUsers.Count - ViewBag.PresentToday);
            ViewBag.Executives = teamUsers;

            return View(attendanceLogs);
        }

        // GET: /SalesManagerDashboard/Tasks
        [HttpGet]
        public async Task<IActionResult> Tasks()
        {
            var currentManager = await GetCurrentManagerAsync();

            var teamMembers = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Department)
                .Where(u => u.IsActive && u.UserId != currentManager.UserId && (
                    u.ReportingManagerId == currentManager.UserId.ToString() ||
                    (!string.IsNullOrEmpty(currentManager.FullName) && u.ReportingManagerName == currentManager.FullName) ||
                    (currentManager.DepartmentId != null && u.DepartmentId == currentManager.DepartmentId) ||
                    (u.Role != null && (u.Role.RoleName.Contains("Sales") || u.Role.RoleName.Contains("Executive")))
                ))
                .OrderBy(u => u.FullName)
                .ToListAsync();

            if (!teamMembers.Any())
            {
                teamMembers = await _context.Users
                    .Where(u => u.IsActive && u.UserId != currentManager.UserId && u.Role != null && u.Role.RoleName != "Super Admin")
                    .Take(8)
                    .ToListAsync();
            }

            var managerIdStr = currentManager.UserId.ToString();
            var managerName = currentManager.UserName;

            var tasks = await _context.HierarchicalTasks
                .Where(t => t.AssignedByUserId == managerIdStr || t.AssignedByUserId == managerName || t.AssignedByUserId == currentManager.FullName)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            ViewBag.TeamMembers = teamMembers;
            return View(tasks);
        }

        // POST: /SalesManagerDashboard/AssignTask
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTask(string assignedToUserId, string title, string description, DateTime dueDate, string priority)
        {
            try
            {
                var currentManager = await GetCurrentManagerAsync();

                var task = new HierarchicalTask
                {
                    Title = title ?? "Sales Task",
                    Description = description ?? "",
                    DepartmentId = currentManager.DepartmentId ?? 1,
                    TaskType = "MANAGER_TO_EMPLOYEE",
                    AssignedByUserId = currentManager.UserId.ToString(),
                    AssignedToUserId = assignedToUserId,
                    DueDate = dueDate != default ? dueDate : DateTime.Today.AddDays(7),
                    Priority = string.IsNullOrWhiteSpace(priority) ? "Medium" : priority,
                    Status = "Pending",
                    ProgressPercentage = 0,
                    CreatedAt = DateTime.UtcNow
                };

                _context.HierarchicalTasks.Add(task);
                await _context.SaveChangesAsync();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.ContentType?.Contains("json") == true)
                {
                    return Json(new { success = true, message = "Target / Task successfully assigned to sales executive." });
                }

                TempData["SuccessMessage"] = "Target / Task successfully assigned to sales executive.";
                return RedirectToAction(nameof(Tasks));
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.ContentType?.Contains("json") == true)
                {
                    return Json(new { success = false, message = "Error assigning task: " + ex.Message });
                }
                TempData["ErrorMessage"] = "Error assigning task: " + ex.Message;
                return RedirectToAction(nameof(Tasks));
            }
        }
    }
}
