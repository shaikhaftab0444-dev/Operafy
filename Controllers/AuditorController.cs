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
                .OrderByDescending(f => f.LoggedAt)
                .Take(10)
                .ToListAsync();

            var recentLogs = await _context.SystemAuditTrails
                .Include(a => a.PerformedByUser)
                .OrderByDescending(a => a.Timestamp)
                .Take(8)
                .ToListAsync();

            var recentMutations = await _context.SystemMutationLogs
                .OrderByDescending(m => m.Timestamp)
                .Take(8)
                .ToListAsync();

            var totalPending = await _context.AuditFlaggedItems.CountAsync(f => f.Status == "Pending Review");
            var highRisk = await _context.AuditFlaggedItems.CountAsync(f => (f.Severity == "High" || f.Severity == "Critical") && f.Status == "Pending Review");
            var totalAmount = await _context.AuditFlaggedItems
                .Where(f => f.Status == "Pending Review")
                .SumAsync(f => (decimal?)f.DiscrepancyAmount) ?? 0m;
            var totalCleared = await _context.AuditFlaggedItems.CountAsync(f => f.Status == "Cleared");

            var vm = new AuditorDashboardViewModel
            {
                TotalAuditsConducted = totalCleared,
                PendingDiscrepanciesCount = totalPending,
                HighRiskFlagsCount = highRisk,
                TotalDiscrepancyAmount = string.Format("₹ {0:N2}", totalAmount),
                FlaggedItems = flagged,
                RecentAuditLogs = recentLogs,
                RecentMutationLogs = recentMutations
            };

            return View(vm);
        }

        // GET: /Auditor/Logs
        [HttpGet]
        public async Task<IActionResult> Logs(string entity = "", string search = "")
        {
            var query = _context.SystemMutationLogs.AsQueryable();

            if (!string.IsNullOrEmpty(entity) && entity != "All")
            {
                query = query.Where(q => q.EntityName == entity);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(q => q.ChangesSummary.Contains(search) 
                                      || q.RecordId.Contains(search) 
                                      || q.ActionType.Contains(search)
                                      || q.EntityName.Contains(search));
            }

            var list = await query.OrderByDescending(q => q.Timestamp).Take(100).ToListAsync();

            // Populate user display names
            var userIds = list.Where(l => !string.IsNullOrEmpty(l.PerformedByUserId))
                              .Select(l => l.PerformedByUserId)
                              .Distinct()
                              .ToList();

            var users = await _context.Users
                .Where(u => userIds.Contains(u.UserId.ToString()))
                .ToDictionaryAsync(u => u.UserId.ToString(), u => u.FullName);

            ViewBag.UsersDict = users;
            ViewBag.SelectedEntity = entity;
            ViewBag.SearchTerm = search;

            // KPI stats across mutation logs
            ViewBag.TotalLogsCount = await _context.SystemMutationLogs.CountAsync();
            ViewBag.ApprovalLogsCount = await _context.SystemMutationLogs.CountAsync(x => x.ActionType == "APPROVAL");
            ViewBag.UpdateLogsCount = await _context.SystemMutationLogs.CountAsync(x => x.ActionType == "UPDATE");
            ViewBag.OverrideLogsCount = await _context.SystemMutationLogs.CountAsync(x => x.ActionType == "FORCE_OVERRIDE" || x.ActionType == "DELETE");

            return View(list);
        }

        // GET: /Auditor/GetLogDiff?id=5
        [HttpGet]
        public async Task<IActionResult> GetLogDiff(int id)
        {
            var log = await _context.SystemMutationLogs.FindAsync(id);
            if (log == null)
            {
                return Json(new { success = false, message = "Audit mutation log entry not found." });
            }

            var userName = "System";
            if (!string.IsNullOrEmpty(log.PerformedByUserId) && int.TryParse(log.PerformedByUserId, out int uid))
            {
                var user = await _context.Users.FindAsync(uid);
                if (user != null)
                {
                    userName = user.FullName;
                }
            }

            return Json(new
            {
                success = true,
                id = log.Id,
                entityName = log.EntityName,
                recordId = log.RecordId,
                actionType = log.ActionType,
                changesSummary = log.ChangesSummary,
                oldValues = log.OldValuesJson ?? "{}",
                newValues = log.NewValuesJson ?? "{}",
                timestamp = log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                performedBy = userName,
                ipAddress = log.IpAddress ?? "127.0.0.1"
            });
        }

        // GET: /Auditor/FinancialReview
        [HttpGet]
        public async Task<IActionResult> FinancialReview()
        {
            // Query only mapped properties (ModuleName and ReferenceNumber)
            var financeFlags = await _context.AuditFlaggedItems
                .Include(f => f.AuditedByUser)
                .Where(f => f.ModuleName == "Finance" 
                         || f.ReferenceNumber.StartsWith("INV") 
                         || f.ReferenceNumber.StartsWith("VND") 
                         || f.ReferenceNumber.StartsWith("JV"))
                .OrderByDescending(f => f.LoggedAt)
                .ToListAsync();

            ViewBag.TotalVouchers = financeFlags.Count;
            ViewBag.PendingCount = financeFlags.Count(f => f.Status == "Pending Review" || f.Status == "Pending");
            ViewBag.ClearedCount = financeFlags.Count(f => f.Status == "Cleared");
            ViewBag.EscalatedCount = financeFlags.Count(f => f.Status == "Escalated");

            return View(financeFlags);
        }

        // GET: /Auditor/GeneralLedger
        [HttpGet]
        public async Task<IActionResult> GeneralLedger(string quarter = "All", string search = "")
        {
            var query = _context.JournalVouchers.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim();
                query = query.Where(v => v.VoucherNumber.Contains(search)
                                      || v.DebitAccount.Contains(search)
                                      || v.CreditAccount.Contains(search)
                                      || (v.Narration != null && v.Narration.Contains(search)));
            }

            if (!string.IsNullOrEmpty(quarter) && quarter != "All")
            {
                int year = DateTime.Today.Year;
                if (quarter == "Q1")
                    query = query.Where(v => v.VoucherDate >= new DateTime(year, 1, 1) && v.VoucherDate <= new DateTime(year, 3, 31));
                else if (quarter == "Q2")
                    query = query.Where(v => v.VoucherDate >= new DateTime(year, 4, 1) && v.VoucherDate <= new DateTime(year, 6, 30));
                else if (quarter == "Q3")
                    query = query.Where(v => v.VoucherDate >= new DateTime(year, 7, 1) && v.VoucherDate <= new DateTime(year, 9, 30));
                else if (quarter == "Q4")
                    query = query.Where(v => v.VoucherDate >= new DateTime(year, 10, 1) && v.VoucherDate <= new DateTime(year, 12, 31));
            }

            var vouchers = await query.OrderByDescending(v => v.VoucherDate).ThenByDescending(v => v.Id).ToListAsync();

            // Seed initial sample vouchers if table is completely empty
            if (!vouchers.Any() && string.IsNullOrEmpty(search) && quarter == "All")
            {
                var sampleVouchers = new List<JournalVoucher>
                {
                    new JournalVoucher
                    {
                        VoucherNumber = "JV-2026-001",
                        VoucherDate = DateTime.Today.AddDays(-1),
                        VoucherType = "Journal",
                        DebitAccount = "Office Rent Expense A/c",
                        CreditAccount = "HDFC Current Bank A/c",
                        Amount = 75000.00m,
                        Narration = "Head office monthly lease amortization",
                        Status = "Posted",
                        CreatedByUserId = "1",
                        CreatedAt = DateTime.UtcNow.AddDays(-1)
                    },
                    new JournalVoucher
                    {
                        VoucherNumber = "JV-2026-002",
                        VoucherDate = DateTime.Today.AddDays(-2),
                        VoucherType = "Payment",
                        DebitAccount = "Accounts Payable - Apex Global Supplies",
                        CreditAccount = "ICICI Operating A/c",
                        Amount = 145000.00m,
                        Narration = "Settlement against invoice PO-9021",
                        Status = "Posted",
                        CreatedByUserId = "1",
                        CreatedAt = DateTime.UtcNow.AddDays(-2)
                    },
                    new JournalVoucher
                    {
                        VoucherNumber = "JV-2026-003",
                        VoucherDate = DateTime.Today.AddDays(-3),
                        VoucherType = "Receipt",
                        DebitAccount = "SBI Corporate Escrow A/c",
                        CreditAccount = "Accounts Receivable - Zenith Technologies",
                        Amount = 210000.00m,
                        Narration = "Customer milestone billing remittance",
                        Status = "Posted",
                        CreatedByUserId = "1",
                        CreatedAt = DateTime.UtcNow.AddDays(-3)
                    },
                    new JournalVoucher
                    {
                        VoucherNumber = "JV-2026-004",
                        VoucherDate = DateTime.Today.AddDays(-4),
                        VoucherType = "Journal",
                        DebitAccount = "IT Infrastructure & Software Assets",
                        CreditAccount = "Vendor: CloudTech Systems",
                        Amount = 98500.00m,
                        Narration = "Annual enterprise cloud server license capitalization",
                        Status = "Posted",
                        CreatedByUserId = "1",
                        CreatedAt = DateTime.UtcNow.AddDays(-4)
                    },
                    new JournalVoucher
                    {
                        VoucherNumber = "JV-2026-005",
                        VoucherDate = DateTime.Today.AddDays(-5),
                        VoucherType = "Payment",
                        DebitAccount = "Staff Salaries Payable A/c",
                        CreditAccount = "HDFC Current Bank A/c",
                        Amount = 385000.00m,
                        Narration = "Executive & engineering payroll distribution",
                        Status = "Posted",
                        CreatedByUserId = "1",
                        CreatedAt = DateTime.UtcNow.AddDays(-5)
                    },
                    new JournalVoucher
                    {
                        VoucherNumber = "JV-2026-006",
                        VoucherDate = DateTime.Today.AddDays(-6),
                        VoucherType = "Journal",
                        DebitAccount = "Finished Goods Inventory A/c",
                        CreditAccount = "Cost of Goods Sold (COGS)",
                        Amount = 125000.00m,
                        Narration = "Quarterly stock variance reconciliation & restock",
                        Status = "Posted",
                        CreatedByUserId = "1",
                        CreatedAt = DateTime.UtcNow.AddDays(-6)
                    }
                };
                await _context.JournalVouchers.AddRangeAsync(sampleVouchers);
                await _context.SaveChangesAsync();
                vouchers = sampleVouchers;
            }

            decimal totalDebit = vouchers.Sum(v => v.Amount);
            decimal totalCredit = vouchers.Sum(v => v.Amount);
            decimal variance = Math.Abs(totalDebit - totalCredit);

            var vm = new AuditorGeneralLedgerViewModel
            {
                TotalDebit = totalDebit,
                TotalCredit = totalCredit,
                Variance = variance,
                SelectedQuarter = quarter,
                SearchQuery = search,
                Vouchers = vouchers
            };

            return View(vm);
        }

        // POST: /Auditor/UpdateFlagStatus
        [HttpPost]
        public async Task<IActionResult> UpdateFlagStatus([FromBody] UpdateFlagStatusInput input)
        {
            if (input == null || input.FlagId <= 0)
                return Json(new { success = false, message = "Invalid audit flag parameters." });

            var item = await _context.AuditFlaggedItems.FindAsync(input.FlagId);
            if (item == null)
                return Json(new { success = false, message = "Flagged record not found." });

            var oldStatus = item.Status;
            var decision = input.Decision;
            if (string.IsNullOrEmpty(decision)) decision = "Cleared";

            item.Status = decision;
            item.ResolutionNotes = input.ResolutionNotes ?? (decision == "Cleared" ? "Verified and passed by Auditor." : "Escalated for senior administrative audit review.");
            item.ResolvedAt = DateTime.UtcNow;

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1";
            item.AuditedByUserId = currentUserId;

            // Record Mutation Log with JSON snapshot
            var oldValues = System.Text.Json.JsonSerializer.Serialize(new
            {
                item.Id,
                item.ReferenceNumber,
                Module = item.ModuleName,
                item.DiscrepancyAmount,
                Status = oldStatus,
                item.Severity
            });

            var newValues = System.Text.Json.JsonSerializer.Serialize(new
            {
                item.Id,
                item.ReferenceNumber,
                Module = item.ModuleName,
                item.DiscrepancyAmount,
                Status = decision,
                item.Severity,
                item.ResolutionNotes,
                AuditedByUserId = currentUserId,
                ResolvedAt = item.ResolvedAt
            });

            var mutationLog = new SystemMutationLog
            {
                EntityName = "AuditFlaggedItem",
                RecordId = $"FLAG-{item.Id}",
                ActionType = decision == "Cleared" ? "APPROVAL" : "UPDATE",
                ChangesSummary = $"Audit flag #{item.Id} ({item.ReferenceNumber}) status updated from '{oldStatus}' to '{decision}'. Note: {item.ResolutionNotes}",
                OldValuesJson = oldValues,
                NewValuesJson = newValues,
                PerformedByUserId = currentUserId,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                Timestamp = DateTime.UtcNow
            };
            _context.SystemMutationLogs.Add(mutationLog);

            // Also keep SystemAuditTrails in sync
            int.TryParse(currentUserId, out int parsedUserId);
            _context.SystemAuditTrails.Add(new SystemAuditTrail
            {
                EntityName = item.ModuleName,
                RecordId = item.ReferenceNumber,
                ActionType = decision == "Cleared" ? "APPROVAL" : "UPDATE",
                PerformedByUserId = currentUserId,
                PerformedByUserUserId = parsedUserId > 0 ? parsedUserId : 1,
                ChangesSummary = $"Audit flag #{item.Id} ({item.ReferenceNumber}) status updated from '{oldStatus}' to '{decision}'. Note: {item.ResolutionNotes}",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"Voucher #{item.ReferenceNumber} marked as '{decision}'." });
        }

        // POST: /Auditor/ResolveAuditFlag (Backward compatibility)
        [HttpPost]
        public async Task<IActionResult> ResolveAuditFlag([FromBody] UpdateAuditStatusInput input)
        {
            if (input == null) return Json(new { success = false, message = "Invalid payload" });
            return await UpdateFlagStatus(new UpdateFlagStatusInput
            {
                FlagId = input.FlagId,
                Decision = input.NewStatus,
                ResolutionNotes = input.AuditNote
            });
        }

        // POST: /Auditor/RaiseFlag
        [HttpPost]
        public async Task<IActionResult> RaiseFlag([FromBody] CreateAuditFlagInput input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.ReferenceNumber))
                return Json(new { success = false, message = "Reference ID / number is required." });

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1";
            int.TryParse(currentUserId, out int parsedUserId);

            var flag = new AuditFlaggedItem
            {
                ModuleName = input.Module ?? "Finance",
                ReferenceNumber = input.ReferenceNumber.Trim(),
                VarianceDescription = input.Description ?? "Discrepancy logged by auditor",
                DiscrepancyAmount = input.DiscrepancyAmount,
                Severity = input.Severity ?? "Medium",
                Status = "Pending Review",
                FlaggedByUserId = currentUserId,
                LoggedAt = DateTime.UtcNow
            };

            _context.AuditFlaggedItems.Add(flag);
            await _context.SaveChangesAsync();

            var newJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                flag.Id,
                flag.ReferenceNumber,
                Module = flag.ModuleName,
                flag.DiscrepancyAmount,
                flag.Severity,
                flag.Status,
                flag.LoggedAt
            });

            _context.SystemMutationLogs.Add(new SystemMutationLog
            {
                EntityName = flag.ModuleName,
                RecordId = flag.ReferenceNumber,
                ActionType = "CREATE",
                ChangesSummary = $"New discrepancy flag raised on {flag.ModuleName} [{flag.ReferenceNumber}] - Amount: ₹{flag.DiscrepancyAmount:N2}, Severity: {flag.Severity}",
                OldValuesJson = null,
                NewValuesJson = newJson,
                PerformedByUserId = currentUserId,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                Timestamp = DateTime.UtcNow
            });

            _context.SystemAuditTrails.Add(new SystemAuditTrail
            {
                EntityName = flag.ModuleName,
                RecordId = flag.ReferenceNumber,
                ActionType = "CREATE",
                PerformedByUserId = currentUserId,
                PerformedByUserUserId = parsedUserId > 0 ? parsedUserId : 1,
                ChangesSummary = $"New discrepancy flag raised on {flag.ModuleName} [{flag.ReferenceNumber}] - Amount: ₹{flag.DiscrepancyAmount:N2}, Severity: {flag.Severity}",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Transaction discrepancy logged successfully!" });
        }

        // POST: /Auditor/FlagNewItem (Backward compatibility)
        [HttpPost]
        public async Task<IActionResult> FlagNewItem([FromBody] CreateAuditFlagInput input)
        {
            return await RaiseFlag(input);
        }
    }
}
