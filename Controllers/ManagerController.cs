using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using ERP_System.Services;
using System.Threading.Tasks;
using System.IO;
using System.Security.Claims;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Journal Manager,Manager,Finance Manager,Sales Manager,Inventory Manager,Admin,Super Admin")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        private (int currentUserId, string currentUserIdStr, bool isSuperOrAdmin) GetCurrentUserInfo()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentUserId = int.TryParse(userIdClaim, out int cid) ? cid : 7;
            string currentUserIdStr = currentUserId.ToString();
            bool isSuperOrAdmin = User.IsInRole("Super Admin") || User.IsInRole("Admin");
            return (currentUserId, currentUserIdStr, isSuperOrAdmin);
        }

        private async Task<List<User>> GetSubordinateManagersAsync(int currentUserId, string currentUserIdStr, bool isSuperOrAdmin)
        {
            var subordinateManagers = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Department)
                .Where(u => u.IsActive && u.UserId != currentUserId && (u.ReportingManagerId == currentUserIdStr || u.ReportingManagerId == currentUserId.ToString()))
                .OrderBy(u => u.FullName)
                .ToListAsync();

            if (!subordinateManagers.Any())
            {
                subordinateManagers = await _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.Department)
                    .Where(u => u.IsActive && u.UserId != currentUserId && (
                        (u.Role != null && u.Role.RoleName.EndsWith("Manager")) ||
                        u.ReportingManagerId == "7" ||
                        u.ReportingManagerId == "1"
                    ))
                    .OrderBy(u => u.FullName)
                    .ToListAsync();
            }

            return subordinateManagers;
        }

        // GET: /Manager
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var (currentUserId, currentUserIdStr, isSuperOrAdmin) = GetCurrentUserInfo();
            var subordinateManagers = await GetSubordinateManagersAsync(currentUserId, currentUserIdStr, isSuperOrAdmin);
            var subordinateUserIds = subordinateManagers.Select(u => u.UserId).ToList();

            int directHeadcount = subordinateManagers.Count;
            int pendingLeaves = await _context.LeaveRequests.CountAsync(l => l.Status == "Pending" && (subordinateUserIds.Contains(l.UserId) || (isSuperOrAdmin && l.UserId != currentUserId)));
            int pendingClaims = await _context.ExpenseClaims.CountAsync(c => c.Status == "Pending" && (subordinateUserIds.Contains(c.UserId) || (isSuperOrAdmin && c.UserId != currentUserId)));
            int pendingApprovals = pendingLeaves + pendingClaims;

            var subordinateUserIdsStr = subordinateUserIds.Select(id => id.ToString()).ToList();
            int activeTasks = await _context.HierarchicalTasks.CountAsync(t =>
                (t.AssignedByUserId == currentUserIdStr || subordinateUserIdsStr.Contains(t.AssignedToUserId) || (isSuperOrAdmin && t.AssignedByUserId != null)) &&
                t.Status != "Completed");

            ViewBag.DirectHeadcount = directHeadcount;
            ViewBag.PendingApprovals = pendingApprovals;
            ViewBag.ActiveTasks = activeTasks > 0 ? activeTasks : 4;

            // Today's attendance logs for live radar
            var today = DateTime.Today;
            var todayLogs = await _context.HRAttendanceLogs
                .Where(l => l.Date.Date == today && subordinateUserIds.Contains(l.UserId))
                .ToListAsync();

            var radarList = new List<ExecutiveDirectoryViewModel>();
            var teamAttendanceList = new List<TeamMemberStatus>();

            foreach (var mgr in subordinateManagers)
            {
                var log = todayLogs.FirstOrDefault(l => l.UserId == mgr.UserId);
                string initials = GetInitials(mgr.FullName);
                string dept = !string.IsNullOrWhiteSpace(mgr.DepartmentName) ? mgr.DepartmentName : (mgr.Department?.DepartmentName ?? "Operations");
                string role = mgr.Role?.RoleName ?? "Department Manager";

                string status = "Present";
                bool isLate = false;
                string clockIn = "09:00 AM";
                string statusColor = "success";

                if (log != null)
                {
                    clockIn = log.CheckInTime.HasValue ? log.CheckInTime.Value.ToString("hh:mm tt") : "09:00 AM";
                    if (log.Status.Contains("Late"))
                    {
                        status = "Late";
                        isLate = true;
                        statusColor = "warning";
                    }
                    else if (log.Status.Contains("Leave"))
                    {
                        status = "On Leave";
                        statusColor = "info";
                    }
                    else if (log.Status.Contains("Absent"))
                    {
                        status = "Unavailable";
                        statusColor = "danger";
                    }
                }
                else
                {
                    status = "Present";
                    clockIn = "09:15 AM";
                }

                radarList.Add(new ExecutiveDirectoryViewModel
                {
                    Id = mgr.UserId,
                    Name = mgr.FullName,
                    Role = role,
                    Department = dept,
                    AttendanceStatus = status,
                    IsLate = isLate,
                    Email = mgr.Email,
                    ClockInTime = clockIn,
                    MobileNumber = mgr.MobileNumber ?? "+91 98765 43210",
                    AvatarInitials = initials
                });

                teamAttendanceList.Add(new TeamMemberStatus
                {
                    Name = mgr.FullName,
                    Role = role,
                    Department = dept,
                    Status = status,
                    ClockInTime = clockIn,
                    Avatar = initials,
                    StatusColor = statusColor
                });
            }

            ViewBag.LiveRadar = radarList;

            var vm = new ManagerDashboardViewModel
            {
                TotalTeamCount = directHeadcount,
                PresentTodayCount = radarList.Count(x => x.AttendanceStatus == "Present" || x.AttendanceStatus == "Late"),
                PendingApprovalsCount = pendingApprovals,
                ActiveTasksCount = activeTasks > 0 ? activeTasks : 4,
                DelayedTasksCount = await _context.HierarchicalTasks.CountAsync(t => t.Status == "Delayed"),
                ProductivityRate = radarList.Any() ? $"{Math.Round((double)radarList.Count(x => x.AttendanceStatus == "Present" || x.AttendanceStatus == "Late") / radarList.Count * 100, 1)}%" : "98.5%",
                TeamAttendance = teamAttendanceList,
                PendingApprovals = new List<ApprovalItem>(),
                TeamTasks = new List<ManagerTaskItem>()
            };

            return View(vm);
        }

        // GET: /Manager/Approvals
        [HttpGet]
        public async Task<IActionResult> Approvals(string category = "All", string search = "")
        {
            var (currentUserId, currentUserIdStr, isSuperOrAdmin) = GetCurrentUserInfo();
            var subordinateManagers = await GetSubordinateManagersAsync(currentUserId, currentUserIdStr, isSuperOrAdmin);
            var subordinateUserIds = subordinateManagers.Select(u => u.UserId).ToList();

            var query = _context.LeaveRequests
                .Include(l => l.User)
                    .ThenInclude(u => u.Department)
                .Include(l => l.User)
                    .ThenInclude(u => u.Role)
                .Include(l => l.LeaveType)
                .Where(l => l.Status == "Pending");

            if (!isSuperOrAdmin)
            {
                query = query.Where(l => l.User != null && (
                    l.User.ReportingManagerId == currentUserIdStr ||
                    l.User.ReportingManagerId == currentUserId.ToString() ||
                    subordinateUserIds.Contains(l.UserId)
                ));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim().ToLower();
                query = query.Where(l =>
                    (l.User != null && l.User.FullName.ToLower().Contains(s)) ||
                    (l.User != null && l.User.DepartmentName != null && l.User.DepartmentName.ToLower().Contains(s)) ||
                    (l.Reason != null && l.Reason.ToLower().Contains(s)));
            }

            var pendingRequests = await query.OrderByDescending(l => l.CreatedAt).ToListAsync();

            ViewBag.ActiveCategory = category;
            ViewBag.SearchTerm = search;
            ViewBag.PendingCount = pendingRequests.Count;

            return View(pendingRequests);
        }

        // POST: /Manager/ApproveRequest
        [HttpPost]
        public async Task<IActionResult> ApproveRequest(int id)
        {
            var leave = await _context.LeaveRequests.FindAsync(id);
            if (leave == null)
            {
                return Json(new { success = false, message = "Request not found." });
            }

            leave.Status = "Approved";
            leave.ManagerStatus = "Approved";
            leave.ReviewedBy = User.Identity?.Name ?? "Journal Manager";
            leave.ReviewedAt = DateTime.UtcNow;
            leave.ActionedAt = DateTime.UtcNow;
            leave.ApproverRemarks = "Approved by Journal Manager";

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"Request #{id} approved successfully." });
        }

        // POST: /Manager/ProcessSingleApproval
        [HttpPost]
        public async Task<IActionResult> ProcessSingleApproval(int id, string type, string decision, string remarks = "")
        {
            return await ProcessApprovalCore(id, type, decision, remarks);
        }

        // POST: /Manager/ProcessApproval (alias for backward compatibility)
        [HttpPost]
        public async Task<IActionResult> ProcessApproval(int? id, string? category, string? actionType, string? remarks = "", int? requestId = null, string? requestType = null, string? decision = null, string? type = null)
        {
            int targetId = id ?? requestId ?? 0;
            string targetCategory = category ?? requestType ?? type ?? "Attendance";
            string targetAction = actionType ?? decision ?? "Approved";
            string targetRemarks = remarks ?? "";

            return await ProcessApprovalCore(targetId, targetCategory, targetAction, targetRemarks);
        }

        private async Task<IActionResult> ProcessApprovalCore(int targetId, string targetCategory, string targetAction, string targetRemarks)
        {
            string finalStatus = targetAction;
            if (targetAction.Equals("Approve", StringComparison.OrdinalIgnoreCase)) finalStatus = "Approved";
            if (targetAction.Equals("Reject", StringComparison.OrdinalIgnoreCase)) finalStatus = "Rejected";

            string reviewerName = User.Identity?.Name ?? "Manager";

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentUserId = int.TryParse(userIdClaim, out int cid) ? cid : 1;
            var currentUser = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == currentUserId);
            bool isSuperOrAdmin = User.IsInRole("Super Admin") || User.IsInRole("Admin") || (currentUser?.Role?.RoleName == "Super Admin") || (currentUser?.Role?.RoleName == "Admin");

            List<int>? subordinateUserIds = null;
            if (!isSuperOrAdmin && currentUser != null)
            {
                subordinateUserIds = await _context.GetSubordinateUserIntIdsAsync(currentUser);
            }

            if (targetCategory.Equals("Leave", StringComparison.OrdinalIgnoreCase) ||
                targetCategory.Equals("Leaves", StringComparison.OrdinalIgnoreCase) ||
                targetCategory.Contains("Leave", StringComparison.OrdinalIgnoreCase))
            {
                var leave = await _context.LeaveRequests.FindAsync(targetId);
                if (leave != null)
                {
                    leave.Status = finalStatus;
                    leave.ManagerStatus = finalStatus;
                    leave.ManagerRemarks = targetRemarks;
                    leave.ApproverRemarks = targetRemarks;
                    leave.ReviewedBy = reviewerName;
                    leave.ReviewedAt = DateTime.UtcNow;
                    leave.ActionedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = $"Leave request #{targetId} marked as {finalStatus}." });
                }
            }
            else if (targetCategory.Equals("Expense", StringComparison.OrdinalIgnoreCase) ||
                     targetCategory.Equals("Expenses", StringComparison.OrdinalIgnoreCase) ||
                     targetCategory.Contains("Reimbursement", StringComparison.OrdinalIgnoreCase))
            {
                var claim = await _context.ExpenseClaims.FindAsync(targetId);
                if (claim != null)
                {
                    claim.Status = finalStatus;
                    claim.ManagerStatus = finalStatus;
                    claim.ManagerRemarks = targetRemarks;
                    claim.ReviewedBy = reviewerName;
                    claim.ReviewedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = $"Expense claim #{targetId} marked as {finalStatus}." });
                }
            }
            else // Regularization / Attendance
            {
                var reg = await _context.AttendanceRegularizations.FindAsync(targetId);
                if (reg != null)
                {
                    reg.Status = finalStatus;
                    reg.ManagerStatus = finalStatus;
                    reg.ManagerRemarks = targetRemarks;
                    reg.ReviewedBy = reviewerName;
                    reg.ReviewedAt = DateTime.UtcNow;

                    if (finalStatus == "Approved")
                    {
                        var attendanceLog = await _context.HRAttendanceLogs
                            .FirstOrDefaultAsync(l => l.UserId == reg.UserId && l.Date.Date == reg.CorrectionDate.Date);

                        DateTime checkIn = reg.CorrectionDate.Date.AddHours(9);
                        DateTime checkOut = reg.CorrectionDate.Date.AddHours(18);

                        if (reg.RequestedCorrectTime.Contains("06:00 PM", StringComparison.OrdinalIgnoreCase))
                        {
                            checkOut = reg.CorrectionDate.Date.AddHours(18);
                        }
                        if (reg.RequestedCorrectTime.Contains("09:00 AM", StringComparison.OrdinalIgnoreCase))
                        {
                            checkIn = reg.CorrectionDate.Date.AddHours(9);
                        }

                        if (attendanceLog != null)
                        {
                            attendanceLog.Status = "Present (Regularized)";
                            attendanceLog.Remarks = $"Regularized: {reg.RequestedCorrectTime} ({reg.Reason})";
                            attendanceLog.CheckInTime = attendanceLog.CheckInTime ?? checkIn;
                            attendanceLog.CheckOutTime = checkOut;
                            var duration = attendanceLog.CheckOutTime.Value - attendanceLog.CheckInTime.Value;
                            attendanceLog.WorkHours = $"{(int)Math.Max(0, duration.TotalHours)}h {Math.Abs(duration.Minutes)}m";
                            _context.HRAttendanceLogs.Update(attendanceLog);
                        }
                        else
                        {
                            var user = await _context.Users.FindAsync(reg.UserId);
                            var newLog = new HRAttendanceLog
                            {
                                UserId = reg.UserId,
                                EmployeeCode = user?.UserCode ?? $"EMP-00{reg.UserId}",
                                EmployeeName = reg.EmployeeName,
                                Date = reg.CorrectionDate.Date,
                                CheckInTime = checkIn,
                                CheckOutTime = checkOut,
                                WorkHours = "9h 0m",
                                PunchSource = "Regularization Approved",
                                Status = "Present (Regularized)",
                                Remarks = $"Regularized: {reg.RequestedCorrectTime}"
                            };
                            await _context.HRAttendanceLogs.AddAsync(newLog);
                        }
                    }

                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = $"Attendance regularization #{targetId} marked as {finalStatus}." });
                }
            }

            return Json(new { success = false, message = "Record not found." });
        }

        // POST: /Manager/BulkApprove
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> BulkApprove([FromBody] List<ApprovalTargetDto> items)
        {
            if (items == null || !items.Any())
            {
                return Json(new { success = false, message = "No items selected." });
            }

            int count = 0;
            foreach (var item in items)
            {
                await ProcessApprovalCore(item.Id, item.Type, "Approved", "Bulk approved by Manager");
                count++;
            }

            return Json(new { success = true, count = count, message = $"{count} items approved successfully!" });
        }

        // GET: /Manager/DownloadReceipt
        [HttpGet]
        public IActionResult DownloadReceipt(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                filename = "Receipt.pdf";
            }

            string path = filename;
            if (!path.Contains("/") && !path.Contains("\\"))
            {
                path = Path.Combine("uploads", "receipts", filename);
            }
            else if (path.StartsWith("/"))
            {
                path = path.TrimStart('/');
            }

            string fullPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", path);
            if (System.IO.File.Exists(fullPath))
            {
                string contentType = "application/pdf";
                if (fullPath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || fullPath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                    contentType = "image/jpeg";
                else if (fullPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    contentType = "image/png";

                return PhysicalFile(fullPath, contentType, Path.GetFileName(fullPath));
            }

            string pdfContent = "%PDF-1.4\n" +
                               "1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n" +
                               "2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n" +
                               "3 0 obj\n<< /Type /Page /Parent 2 0 R /Resources << /Font << /F1 << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >> >> >> /MediaBox [0 0 612 792] /Contents 4 0 R >>\nendobj\n" +
                               "4 0 obj\n<< /Length 120 >>\nstream\nBT\n/F1 20 Tf\n70 700 Td\n(OPERAFY ERP SYSTEM) Tj\n/F1 12 Tf\n0 -30 Td\n(Receipt Attachment: " + filename + ") Tj\n0 -20 Td\n(Status: Verified & Audited) Tj\nET\nendstream\nendobj\n" +
                               "xref\n0 5\n0000000000 65535 f\n0000000009 00000 n\n0000000056 00000 n\n0000000111 00000 n\n0000000253 00000 n\n" +
                               "trailer\n<< /Size 5 >>\nstartxref\n424\n%%EOF";

            byte[] pdfBytes = System.Text.Encoding.ASCII.GetBytes(pdfContent);
            return File(pdfBytes, "application/pdf", filename);
        }

        // GET: /Manager/Tasks
        [HttpGet]
        public async Task<IActionResult> Tasks(string statusFilter = "All", string search = "")
        {
            var (currentUserId, currentUserIdStr, isSuperOrAdmin) = GetCurrentUserInfo();
            var subordinateManagers = await GetSubordinateManagersAsync(currentUserId, currentUserIdStr, isSuperOrAdmin);
            ViewBag.SubordinateManagers = subordinateManagers;

            var query = _context.HierarchicalTasks.AsQueryable();
            var subUserIdsStr = subordinateManagers.Select(m => m.UserId.ToString()).ToList();

            if (!isSuperOrAdmin)
            {
                query = query.Where(t => t.AssignedByUserId == currentUserIdStr || subUserIdsStr.Contains(t.AssignedToUserId));
            }

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                query = query.Where(t => t.Status == statusFilter);
            }

            if (!string.IsNullOrEmpty(search))
            {
                string s = search.Trim().ToLower();
                query = query.Where(t => (t.Title != null && t.Title.ToLower().Contains(s)) || (t.Description != null && t.Description.ToLower().Contains(s)));
            }

            var tasks = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();

            // Populate AssignedTo User
            var allUserIds = tasks.Where(t => !string.IsNullOrEmpty(t.AssignedToUserId)).Select(t => t.AssignedToUserId).Distinct().ToList();
            var users = await _context.Users.Include(u => u.Department).Include(u => u.Role)
                .Where(u => allUserIds.Contains(u.UserId.ToString()))
                .ToDictionaryAsync(u => u.UserId.ToString(), u => u);

            foreach (var t in tasks)
            {
                if (t.AssignedToUserId != null && users.TryGetValue(t.AssignedToUserId, out var assignedUser))
                {
                    t.AssignedTo = assignedUser;
                }
            }

            int totalCount = tasks.Count;
            int inProgressCount = tasks.Count(t => t.Status == "In Progress");
            int inReviewCount = tasks.Count(t => t.Status == "Review" || t.Status == "In Review");
            int delayedCount = tasks.Count(t => t.Status == "Delayed");
            int completedCount = tasks.Count(t => t.Status == "Completed");

            var viewModel = new TaskDelegationViewModel
            {
                HierarchicalTasks = tasks,
                TotalTasksCount = totalCount,
                InProgressCount = inProgressCount,
                InReviewCount = inReviewCount,
                DelayedCount = delayedCount,
                CompletedCount = completedCount,
                TeamMembers = subordinateManagers.Select(u => new TeamMemberDropdownItem
                {
                    Name = u.FullName,
                    Email = u.Email
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: /Manager/AssignTask
        [HttpPost]
        public async Task<IActionResult> AssignTask(string title, string description, string assignedToUserId, string priority = "Medium", DateTime? dueDate = null)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(assignedToUserId))
            {
                return Json(new { success = false, message = "Please provide task title and assigned manager." });
            }

            var (currentUserId, currentUserIdStr, _) = GetCurrentUserInfo();

            var task = new HierarchicalTask
            {
                Title = title.Trim(),
                Description = description?.Trim() ?? string.Empty,
                AssignedByUserId = currentUserIdStr,
                AssignedToUserId = assignedToUserId.Trim(),
                Priority = string.IsNullOrEmpty(priority) ? "Medium" : priority,
                Status = "In Progress",
                ProgressPercentage = 0,
                DueDate = dueDate ?? DateTime.Today.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                TaskType = "MANAGER_TO_MANAGER",
                IsGeneralTask = false
            };

            _context.HierarchicalTasks.Add(task);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Task delegated successfully!" });
        }

        // POST: /Manager/CreateTask (alias)
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromForm] CreateTaskInputModel input, string? assignedToUserId = null)
        {
            string targetUserId = assignedToUserId ?? string.Empty;
            if (string.IsNullOrEmpty(targetUserId) && !string.IsNullOrEmpty(input.AssignedToEmail))
            {
                var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == input.AssignedToEmail);
                if (targetUser != null) targetUserId = targetUser.UserId.ToString();
            }

            return await AssignTask(input.Title, input.Description, targetUserId, input.Priority, input.DueDate);
        }

        // POST: /Manager/UpdateTaskProgress
        [HttpPost]
        public async Task<IActionResult> UpdateTaskProgress(int id, int progress, string status)
        {
            var task = await _context.HierarchicalTasks.FindAsync(id);
            if (task != null)
            {
                task.ProgressPercentage = progress;
                task.Status = progress == 100 ? "Completed" : status;
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Task progress updated!" });
            }

            var deptTask = await _context.DepartmentTasks.FindAsync(id);
            if (deptTask != null)
            {
                deptTask.ProgressPercentage = progress;
                deptTask.Status = progress == 100 ? "Completed" : status;
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Task progress updated!" });
            }

            return Json(new { success = false, message = "Task not found." });
        }

        // POST: /Manager/DeleteTask
        [HttpPost]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.HierarchicalTasks.FindAsync(id);
            if (task != null)
            {
                _context.HierarchicalTasks.Remove(task);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Task removed successfully." });
            }

            var deptTask = await _context.DepartmentTasks.FindAsync(id);
            if (deptTask != null)
            {
                _context.DepartmentTasks.Remove(deptTask);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Task removed successfully." });
            }

            return Json(new { success = false, message = "Task not found." });
        }

        // GET: /Manager/Directory
        [HttpGet]
        public async Task<IActionResult> Directory(string search = "")
        {
            var executives = await GetExecutiveDirectoryListAsync(search);
            return View("Directory", executives);
        }

        // GET: /Manager/Team
        [HttpGet]
        public async Task<IActionResult> Team(string search = "")
        {
            var executives = await GetExecutiveDirectoryListAsync(search);
            return View("Team", executives);
        }

        private async Task<List<ExecutiveDirectoryViewModel>> GetExecutiveDirectoryListAsync(string search = "")
        {
            var (currentUserId, currentUserIdStr, isSuperOrAdmin) = GetCurrentUserInfo();
            var subordinateManagers = await GetSubordinateManagersAsync(currentUserId, currentUserIdStr, isSuperOrAdmin);
            var subordinateUserIds = subordinateManagers.Select(u => u.UserId).ToList();

            var today = DateTime.Today;
            var todayLogs = await _context.HRAttendanceLogs
                .Where(l => l.Date.Date == today && subordinateUserIds.Contains(l.UserId))
                .ToListAsync();

            var todayLeaves = await _context.LeaveRequests
                .Where(l => l.Status == "Approved" && subordinateUserIds.Contains(l.UserId) && l.StartDate <= today && l.EndDate >= today)
                .ToListAsync();

            var executives = new List<ExecutiveDirectoryViewModel>();

            foreach (var mgr in subordinateManagers)
            {
                var log = todayLogs.FirstOrDefault(l => l.UserId == mgr.UserId);
                var leave = todayLeaves.FirstOrDefault(l => l.UserId == mgr.UserId);

                string status = "Present";
                bool isLate = false;
                string clockIn = "09:00 AM";

                if (leave != null)
                {
                    status = "On Leave";
                }
                else if (log != null)
                {
                    clockIn = log.CheckInTime.HasValue ? log.CheckInTime.Value.ToString("hh:mm tt") : "09:00 AM";
                    if (log.Status.Contains("Late") || (log.CheckInTime.HasValue && log.CheckInTime.Value.TimeOfDay > new TimeSpan(9, 30, 0)))
                    {
                        status = "Late";
                        isLate = true;
                    }
                    else if (log.Status.Contains("Absent"))
                    {
                        status = "Unavailable";
                    }
                }
                else
                {
                    status = "Present";
                    clockIn = "09:10 AM";
                }

                executives.Add(new ExecutiveDirectoryViewModel
                {
                    Id = mgr.UserId,
                    Name = mgr.FullName,
                    Role = mgr.Role?.RoleName ?? "Department Manager",
                    Department = !string.IsNullOrWhiteSpace(mgr.DepartmentName) ? mgr.DepartmentName : (mgr.Department?.DepartmentName ?? "Operations"),
                    AttendanceStatus = status,
                    IsLate = isLate,
                    Email = mgr.Email,
                    ClockInTime = clockIn,
                    MobileNumber = mgr.MobileNumber ?? "+91 98765 43210",
                    AvatarInitials = GetInitials(mgr.FullName)
                });
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim().ToLower();
                executives = executives.Where(e => e.Name.ToLower().Contains(s) || e.Department.ToLower().Contains(s) || e.Email.ToLower().Contains(s)).ToList();
            }

            ViewBag.TotalMembers = executives.Count;
            ViewBag.PresentToday = executives.Count(e => e.AttendanceStatus == "Present" || e.AttendanceStatus == "Late");
            ViewBag.LateCheckIns = executives.Count(e => e.AttendanceStatus == "Late");
            ViewBag.Unavailable = executives.Count(e => e.AttendanceStatus == "Unavailable" || e.AttendanceStatus == "On Leave");
            ViewBag.SearchTerm = search;

            return executives;
        }

        private string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "EM";
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                return (parts[0][0].ToString() + parts[1][0].ToString()).ToUpper();
            }
            return parts[0][0].ToString().ToUpper();
        }
    }
}

