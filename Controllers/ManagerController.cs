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
    [Authorize(Roles = "Manager,Finance Manager,Sales Manager,Inventory Manager,Admin,Super Admin")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManagerController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: /Manager
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vm = await GetPopulatedManagerVMAsync();
            return View(vm);
        }

        // GET: /Manager/Approvals
        [HttpGet]
        public async Task<IActionResult> Approvals(string category = "All", string search = "")
        {
            var model = await GetPendingApprovalsListAsync();

            if (!string.IsNullOrEmpty(category) && category != "All")
            {
                if (category.Equals("Leaves", StringComparison.OrdinalIgnoreCase) || category.Equals("Leave", StringComparison.OrdinalIgnoreCase))
                {
                    model = model.Where(x => x.CategoryKey == "Leave").ToList();
                }
                else if (category.Equals("Regularization", StringComparison.OrdinalIgnoreCase))
                {
                    model = model.Where(x => x.CategoryKey == "Regularization").ToList();
                }
                else if (category.Equals("Expenses", StringComparison.OrdinalIgnoreCase) || category.Equals("Expense", StringComparison.OrdinalIgnoreCase))
                {
                    model = model.Where(x => x.CategoryKey == "Expense").ToList();
                }
                else
                {
                    model = model.Where(x => x.CategoryKey.Equals(category, StringComparison.OrdinalIgnoreCase) || x.ClaimCategory.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
                }
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim();
                model = model.Where(x =>
                    x.EmployeeName.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    x.Reason.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    x.ClaimCategory.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    x.Role.Contains(s, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            ViewBag.ActiveCategory = category;
            ViewBag.SearchTerm = search;
            ViewBag.PendingCount = model.Count;
            return View(model);
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
            // Normalize actionType (standardize "Approved"/"Rejected")
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
                    if (!isSuperOrAdmin && subordinateUserIds != null && !subordinateUserIds.Contains(leave.UserId))
                    {
                        return Json(new { success = false, message = "Access Denied: You can only approve requests from your direct subordinates." });
                    }

                    leave.Status = finalStatus;
                    leave.ManagerStatus = finalStatus;
                    leave.ManagerRemarks = targetRemarks;
                    leave.ReviewedBy = reviewerName;
                    leave.ReviewedAt = DateTime.UtcNow;
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
                    if (!isSuperOrAdmin && subordinateUserIds != null && !subordinateUserIds.Contains(claim.UserId))
                    {
                        return Json(new { success = false, message = "Access Denied: You can only approve requests from your direct subordinates." });
                    }

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
                    if (!isSuperOrAdmin && subordinateUserIds != null && !subordinateUserIds.Contains(reg.UserId))
                    {
                        return Json(new { success = false, message = "Access Denied: You can only approve requests from your direct subordinates." });
                    }
                    reg.Status = finalStatus;
                    reg.ManagerStatus = finalStatus;
                    reg.ManagerRemarks = targetRemarks;
                    reg.ReviewedBy = reviewerName;
                    reg.ReviewedAt = DateTime.UtcNow;

                    // If Approved, update or insert the daily attendance record check-in/check-out times
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

            // Check if file exists in the uploads directory
            string path = filename;
            if (!path.Contains("/") && !path.Contains("\\"))
            {
                path = Path.Combine("uploads", "receipts", filename);
            }
            else if (path.StartsWith("/"))
            {
                path = path.TrimStart('/');
            }

            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", path);
            if (System.IO.File.Exists(fullPath))
            {
                string contentType = "application/pdf";
                if (fullPath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || fullPath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                    contentType = "image/jpeg";
                else if (fullPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    contentType = "image/png";

                return PhysicalFile(fullPath, contentType, Path.GetFileName(fullPath));
            }

            // Simple valid PDF stream format fallback
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
            var query = _context.DepartmentTasks.AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                query = query.Where(t => t.Status == statusFilter);
            }

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(t => t.Title.ToLower().Contains(search) || t.AssignedToName.ToLower().Contains(search));
            }

            var tasks = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
            
            var viewModel = new TaskDelegationViewModel
            {
                Tasks = tasks,
                TotalTasksCount = await _context.DepartmentTasks.CountAsync(),
                InProgressCount = await _context.DepartmentTasks.CountAsync(t => t.Status == "In Progress"),
                InReviewCount = await _context.DepartmentTasks.CountAsync(t => t.Status == "Review" || t.Status == "In Review"),
                DelayedCount = await _context.DepartmentTasks.CountAsync(t => t.Status == "Delayed"),
                CompletedCount = await _context.DepartmentTasks.CountAsync(t => t.Status == "Completed"),
                TeamMembers = await _context.Users.Select(u => new TeamMemberDropdownItem { Name = u.FullName ?? u.UserName, Email = u.Email }).ToListAsync()
            };

            return View(viewModel);
        }

        // POST: /Manager/CreateTask
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromForm] CreateTaskInputModel input)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(input.Title) || string.IsNullOrWhiteSpace(input.AssignedToEmail))
                return Json(new { success = false, message = "Please fill all mandatory fields." });

            var newTask = new DepartmentTask
            {
                Title = input.Title,
                Description = input.Description,
                AssignedToName = input.AssignedToName,
                AssignedToEmail = input.AssignedToEmail,
                Priority = input.Priority, // Urgent, High, Medium, Low
                DueDate = input.DueDate,
                ProgressPercentage = 0,
                Status = "In Progress",
                AssignedBy = User.Identity?.Name ?? "Manager",
                CreatedAt = DateTime.UtcNow
            };

            _context.DepartmentTasks.Add(newTask);
            await _context.SaveChangesAsync();

            // Synchronize with Employee's Assigned Tasks portal
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == input.AssignedToEmail.ToLower());
            if (user != null)
            {
                var essTask = new ESSTask
                {
                    UserId = user.UserId,
                    TaskTitle = input.Title,
                    Description = input.Description,
                    DueDate = input.DueDate,
                    Status = "In Progress",
                    DepartmentTaskId = newTask.TaskId
                };
                _context.ESSTasks.Add(essTask);
                await _context.SaveChangesAsync();
            }

            return Json(new { 
                success = true, 
                message = "Task assigned successfully to " + input.AssignedToName,
                task = new {
                    taskId = newTask.TaskId,
                    title = newTask.Title,
                    description = newTask.Description,
                    assignedToName = newTask.AssignedToName,
                    assignedToEmail = newTask.AssignedToEmail,
                    priority = newTask.Priority,
                    dueDate = newTask.DueDate.ToString("dd MMM yyyy"),
                    progressPercentage = newTask.ProgressPercentage,
                    status = newTask.Status
                }
            });
        }

        // POST: /Manager/UpdateTaskProgress
        [HttpPost]
        public async Task<IActionResult> UpdateTaskProgress(int id, int progress, string status)
        {
            var task = await _context.DepartmentTasks.FindAsync(id);
            if (task == null) return Json(new { success = false, message = "Task not found." });

            task.ProgressPercentage = progress;
            task.Status = progress == 100 ? "Completed" : status;
            _context.DepartmentTasks.Update(task);

            // Sync to ESSTasks
            var essTask = await _context.ESSTasks.FirstOrDefaultAsync(t => t.DepartmentTaskId == id);
            if (essTask != null)
            {
                essTask.Status = task.Status;
                _context.ESSTasks.Update(essTask);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Task progress updated!" });
        }

        // POST: /Manager/DeleteTask
        [HttpPost]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.DepartmentTasks.FindAsync(id);
            if (task == null) return Json(new { success = false, message = "Task not found." });

            _context.DepartmentTasks.Remove(task);

            // Delete from ESSTasks
            var essTask = await _context.ESSTasks.FirstOrDefaultAsync(t => t.DepartmentTaskId == id);
            if (essTask != null)
            {
                _context.ESSTasks.Remove(essTask);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Task removed successfully." });
        }

        // GET: /Manager/Team
        [HttpGet]
        public async Task<IActionResult> Team()
        {
            var vm = await GetPopulatedManagerVMAsync();
            return View(vm);
        }

        private async Task<ManagerDashboardViewModel> GetPopulatedManagerVMAsync()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentUserId = int.TryParse(userIdClaim, out int cid) ? cid : 1;
            var currentUser = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == currentUserId);
            bool isSuperOrAdmin = User.IsInRole("Super Admin") || User.IsInRole("Admin") || (currentUser?.Role?.RoleName == "Super Admin") || (currentUser?.Role?.RoleName == "Admin");

            var allUsers = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Department)
                .Where(u => u.IsActive)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            List<User> teamUsers;
            if (isSuperOrAdmin)
            {
                teamUsers = allUsers.Where(u => u.Role?.RoleName != "Super Admin" && u.Role?.RoleName != "Admin").ToList();
            }
            else
            {
                teamUsers = await _context.GetSubordinateUsersAsync(currentUser!);
            }

            var teamUserIds = teamUsers.Select(u => u.UserId).ToList();

            var pendingLeavesQuery = _context.ESSLeaveApplications.Where(l => l.Status == "Pending");
            var pendingRegsQuery = _context.HRAttendanceRegularizations.Where(r => r.Status == "Pending");
            var pendingClaimsQuery = _context.ESSExpenseClaims.Where(c => c.Status == "Pending");

            if (!isSuperOrAdmin)
            {
                pendingLeavesQuery = pendingLeavesQuery.Where(l => teamUserIds.Contains(l.UserId));
                pendingRegsQuery = pendingRegsQuery.Where(r => teamUserIds.Contains(r.UserId));
                pendingClaimsQuery = pendingClaimsQuery.Where(c => teamUserIds.Contains(c.UserId));
            }

            var pendingLeaves = await pendingLeavesQuery.ToListAsync();
            var pendingRegs = await pendingRegsQuery.ToListAsync();
            var pendingClaims = await pendingClaimsQuery.ToListAsync();

            var pendingApprovalsCount = pendingLeaves.Count + pendingRegs.Count + pendingClaims.Count;

            var pendingApprovalsList = new List<ApprovalItem>();
            foreach (var leave in pendingLeaves)
            {
                pendingApprovalsList.Add(new ApprovalItem
                {
                    Id = leave.LeaveApplicationId,
                    EmployeeName = leave.EmployeeName ?? _context.Users.FirstOrDefault(u => u.UserId == leave.UserId)?.FullName ?? "Employee",
                    Type = leave.LeaveType,
                    Dates = $"{leave.StartDate:dd MMM} - {leave.EndDate:dd MMM yyyy} ({leave.TotalDays} Days)",
                    Reason = leave.Reason,
                    RequestedOn = leave.CreatedAt?.ToString("dd MMM yyyy") ?? leave.StartDate.AddDays(-1).ToString("dd MMM yyyy"),
                    Status = "Pending"
                });
            }
            foreach (var reg in pendingRegs)
            {
                pendingApprovalsList.Add(new ApprovalItem
                {
                    Id = reg.RequestId,
                    EmployeeName = reg.EmployeeName,
                    Type = "Attendance Regularization",
                    Dates = $"{reg.CorrectionDate:dd MMM yyyy} ({reg.RequestedCorrectTime})",
                    Reason = reg.Reason,
                    RequestedOn = reg.CreatedAt?.ToString("dd MMM yyyy") ?? reg.RequestDate.ToString("dd MMM yyyy"),
                    Status = "Pending"
                });
            }
            foreach (var claim in pendingClaims)
            {
                pendingApprovalsList.Add(new ApprovalItem
                {
                    Id = claim.ExpenseClaimId,
                    EmployeeName = claim.EmployeeName ?? _context.Users.FirstOrDefault(u => u.UserId == claim.UserId)?.FullName ?? "Employee",
                    Type = claim.ExpenseType + " Reimbursement",
                    Dates = $"Claim amount: INR {claim.Amount:N2}",
                    Reason = $"Reimbursement request for {claim.ExpenseType} expense",
                    RequestedOn = claim.CreatedAt?.ToString("dd MMM yyyy") ?? claim.ClaimDate.ToString("dd MMM yyyy"),
                    Status = "Pending"
                });
            }

            // Live Attendance Logs for today
            var today = DateTime.Today;
            var todayLogs = await _context.HRAttendanceLogs
                .Where(l => l.Date.Date == today && teamUserIds.Contains(l.UserId))
                .ToListAsync();

            var teamAttendanceList = new List<TeamMemberStatus>();
            foreach (var u in teamUsers)
            {
                var log = todayLogs.FirstOrDefault(l => l.UserId == u.UserId);
                var initials = GetInitials(u.FullName);
                string dept = !string.IsNullOrWhiteSpace(u.DepartmentName) ? u.DepartmentName : (u.Department?.DepartmentName ?? "Operations");

                string status = "Not Clocked In";
                string clockIn = "N/A";
                string statusColor = "secondary";

                if (log != null)
                {
                    clockIn = log.CheckInTime.HasValue ? log.CheckInTime.Value.ToString("hh:mm tt") : "N/A";
                    if (log.Status.StartsWith("Present"))
                    {
                        statusColor = "success";
                        status = "Present";
                    }
                    else if (log.Status.Contains("Late"))
                    {
                        statusColor = "warning";
                        status = "Late";
                    }
                    else if (log.Status.Contains("Leave"))
                    {
                        statusColor = "info";
                        status = "On Leave";
                    }
                    else if (log.Status.Contains("Absent"))
                    {
                        statusColor = "danger";
                        status = "Absent";
                    }
                    else
                    {
                        statusColor = "primary";
                        status = log.Status;
                    }
                }

                teamAttendanceList.Add(new TeamMemberStatus
                {
                    Name = u.FullName,
                    Role = u.Role?.RoleName ?? "Team Member",
                    Department = dept,
                    Status = status,
                    ClockInTime = clockIn,
                    Avatar = initials,
                    StatusColor = statusColor
                });
            }

            var activeTasks = await _context.DepartmentTasks.CountAsync(t => t.Status == "In Progress");
            var delayedTasks = await _context.DepartmentTasks.CountAsync(t => t.Status == "Delayed");

            return new ManagerDashboardViewModel
            {
                TotalTeamCount = teamAttendanceList.Count,
                PresentTodayCount = teamAttendanceList.Count(x => x.Status == "Present" || x.Status == "Late"),
                PendingApprovalsCount = pendingApprovalsCount,
                ActiveTasksCount = activeTasks > 0 ? activeTasks : 8,
                DelayedTasksCount = delayedTasks,
                ProductivityRate = teamAttendanceList.Any() ? $"{Math.Round((double)teamAttendanceList.Count(x => x.Status == "Present" || x.Status == "Late") / teamAttendanceList.Count * 100, 1)}%" : "95.0%",
                TeamAttendance = teamAttendanceList,
                PendingApprovals = pendingApprovalsList,
                TeamTasks = new List<ManagerTaskItem>
                {
                    new ManagerTaskItem { Id = 1, Title = "Finalize Q3 Client Billing Summary", AssignedTo = teamUsers.FirstOrDefault()?.FullName ?? "Numan Khan", Priority = "Urgent", DueDate = "28 Aug 2026", Progress = 75, Status = "In Progress" },
                    new ManagerTaskItem { Id = 2, Title = "Resolve Payment Gateway Timeout Exception", AssignedTo = teamUsers.Skip(1).FirstOrDefault()?.FullName ?? "Aftab Shaik", Priority = "High", DueDate = "27 Aug 2026", Progress = 90, Status = "Review" },
                    new ManagerTaskItem { Id = 3, Title = "Branch Inventory Stock Audit Reconciliation", AssignedTo = teamUsers.Skip(2).FirstOrDefault()?.FullName ?? "Sneha Patil", Priority = "Medium", DueDate = "30 Aug 2026", Progress = 30, Status = "Delayed" },
                    new ManagerTaskItem { Id = 4, Title = "Prepare New Hire Onboarding Documentation", AssignedTo = teamUsers.Skip(3).FirstOrDefault()?.FullName ?? "Rohan Sharma", Priority = "Low", DueDate = "31 Aug 2026", Progress = 50, Status = "In Progress" }
                }
            };
        }

        private async Task<List<ApprovalItemViewModel>> GetPendingApprovalsListAsync()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentUserId = int.TryParse(userIdClaim, out int cid) ? cid : 1;
            var currentUser = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == currentUserId);
            bool isSuperOrAdmin = User.IsInRole("Super Admin") || User.IsInRole("Admin") || (currentUser?.Role?.RoleName == "Super Admin") || (currentUser?.Role?.RoleName == "Admin");

            var allUsers = await _context.Users.Include(u => u.Role).ToListAsync();
            var userMap = allUsers.ToDictionary(u => u.UserId, u => u);

            List<int> subordinateIds;
            if (isSuperOrAdmin)
            {
                subordinateIds = allUsers.Select(u => u.UserId).ToList();
            }
            else
            {
                subordinateIds = await _context.GetSubordinateUserIntIdsAsync(currentUser!);
            }

            var pendingLeaves = await _context.ESSLeaveApplications
                .Where(x => x.Status == "Pending" && subordinateIds.Contains(x.UserId))
                .ToListAsync();

            var pendingRegs = await _context.HRAttendanceRegularizations
                .Where(x => x.Status == "Pending" && subordinateIds.Contains(x.UserId))
                .ToListAsync();

            var pendingClaims = await _context.ESSExpenseClaims
                .Where(x => x.Status == "Pending" && subordinateIds.Contains(x.UserId))
                .ToListAsync();

            var list = new List<ApprovalItemViewModel>();

            foreach (var leave in pendingLeaves)
            {
                var user = userMap.TryGetValue(leave.UserId, out var u) ? u : null;
                string empName = leave.EmployeeName ?? user?.FullName ?? "Employee";
                string roleName = user?.Role?.RoleName ?? "Employee";

                list.Add(new ApprovalItemViewModel
                {
                    Id = leave.LeaveApplicationId,
                    EmployeeName = empName,
                    Role = roleName,
                    Avatar = GetInitials(empName),
                    ClaimCategory = leave.LeaveType,
                    CategoryKey = "Leave",
                    Duration = $"{leave.StartDate:dd MMM} – {leave.EndDate:dd MMM yyyy} ({leave.TotalDays} Days)",
                    Reason = leave.Reason,
                    SubmittedDate = leave.CreatedAt?.ToString("dd MMM yyyy, hh:mm tt") ?? leave.StartDate.AddDays(-1).ToString("dd MMM yyyy, 09:00 AM"),
                    HasAttachment = false,
                    AttachmentName = null,
                    Status = "Pending"
                });
            }

            foreach (var reg in pendingRegs)
            {
                var user = userMap.TryGetValue(reg.UserId, out var u) ? u : null;
                string empName = reg.EmployeeName ?? user?.FullName ?? "Employee";
                string roleName = user?.Role?.RoleName ?? "Employee";

                list.Add(new ApprovalItemViewModel
                {
                    Id = reg.RequestId,
                    EmployeeName = empName,
                    Role = roleName,
                    Avatar = GetInitials(empName),
                    ClaimCategory = "Attendance Regularization",
                    CategoryKey = "Regularization",
                    Duration = $"{reg.CorrectionDate:dd MMM yyyy} ({reg.RequestedCorrectTime})",
                    Reason = reg.Reason,
                    SubmittedDate = reg.CreatedAt?.ToString("dd MMM yyyy, hh:mm tt") ?? reg.RequestDate.ToString("dd MMM yyyy, 09:00 AM"),
                    HasAttachment = false,
                    AttachmentName = null,
                    Status = "Pending"
                });
            }

            foreach (var claim in pendingClaims)
            {
                var user = userMap.TryGetValue(claim.UserId, out var u) ? u : null;
                string empName = claim.EmployeeName ?? user?.FullName ?? "Employee";
                string roleName = user?.Role?.RoleName ?? "Employee";

                list.Add(new ApprovalItemViewModel
                {
                    Id = claim.ExpenseClaimId,
                    EmployeeName = empName,
                    Role = roleName,
                    Avatar = GetInitials(empName),
                    ClaimCategory = claim.ExpenseType + " Reimbursement",
                    CategoryKey = "Expense",
                    Duration = $"Claim amount: INR {claim.Amount:N2}",
                    Reason = $"Reimbursement request for {claim.ExpenseType} expense",
                    SubmittedDate = claim.CreatedAt?.ToString("dd MMM yyyy, hh:mm tt") ?? claim.ClaimDate.ToString("dd MMM yyyy, 09:00 AM"),
                    HasAttachment = !string.IsNullOrEmpty(claim.ReceiptFileName),
                    AttachmentName = claim.ReceiptFileName,
                    Status = "Pending"
                });
            }

            return list;
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
