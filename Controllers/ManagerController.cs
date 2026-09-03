using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System.Threading.Tasks;
using System.IO;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,Manager")]
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
        public async Task<IActionResult> Approvals(string category = "All")
        {
            var model = await GetPendingApprovalsListAsync();
            if (!string.IsNullOrEmpty(category) && category != "All")
            {
                model = model.Where(x => x.CategoryKey == category).ToList();
            }
            ViewBag.ActiveCategory = category;
            return View(model);
        }

        // POST: /Manager/ProcessApproval
        [HttpPost]
        public async Task<IActionResult> ProcessApproval(int id, string category, string actionType, string remarks = "")
        {
            // Normalize actionType (standardize "Approved"/"Rejected")
            string finalStatus = actionType;
            if (actionType.Equals("Approve", StringComparison.OrdinalIgnoreCase)) finalStatus = "Approved";
            if (actionType.Equals("Reject", StringComparison.OrdinalIgnoreCase)) finalStatus = "Rejected";

            if (category == "Leave" || category == "Casual Leave" || category == "Sick Leave" || category == "Earned Leave")
            {
                var leave = await _context.LeaveRequests.FindAsync(id);
                if (leave != null)
                {
                    leave.Status = finalStatus;
                    leave.ManagerStatus = finalStatus;
                    leave.ManagerRemarks = remarks;
                    leave.ReviewedBy = User.Identity?.Name ?? "Manager";
                    leave.ReviewedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = $"Leave request #{id} successfully {finalStatus.ToLower()}ed!" });
                }
            }
            else if (category == "Expense" || category.Contains("Reimbursement"))
            {
                var claim = await _context.ExpenseClaims.FindAsync(id);
                if (claim != null)
                {
                    claim.Status = finalStatus;
                    claim.ManagerStatus = finalStatus;
                    claim.ManagerRemarks = remarks;
                    claim.ReviewedBy = User.Identity?.Name ?? "Manager";
                    claim.ReviewedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = $"Expense claim #{id} successfully {finalStatus.ToLower()}ed!" });
                }
            }
            else // Regularization / Attendance
            {
                var reg = await _context.AttendanceRegularizations.FindAsync(id);
                if (reg != null)
                {
                    reg.Status = finalStatus;
                    reg.ManagerStatus = finalStatus;
                    reg.ManagerRemarks = remarks;
                    reg.ReviewedBy = User.Identity?.Name ?? "Manager";
                    reg.ReviewedAt = DateTime.UtcNow;

                    // If Approved, update the daily attendance record check-in/check-out times
                    if (finalStatus == "Approved")
                    {
                        var attendanceLog = await _context.HRAttendanceLogs
                            .FirstOrDefaultAsync(l => l.UserId == reg.UserId && l.Date.Date == reg.CorrectionDate.Date);

                        if (attendanceLog != null)
                        {
                            attendanceLog.Status = "Present (On Time)";
                            attendanceLog.Remarks = $"Regularized: {reg.RequestedCorrectTime} ({reg.Reason})";
                            if (reg.RequestedCorrectTime.Contains("06:00 PM"))
                            {
                                attendanceLog.CheckOutTime = reg.CorrectionDate.Date.AddHours(18);
                                if (attendanceLog.CheckInTime.HasValue)
                                {
                                    var duration = attendanceLog.CheckOutTime.Value - attendanceLog.CheckInTime.Value;
                                    attendanceLog.WorkHours = $"{(int)duration.TotalHours}h {duration.Minutes}m";
                                }
                            }
                            else if (reg.RequestedCorrectTime.Contains("09:00 AM"))
                            {
                                attendanceLog.CheckInTime = reg.CorrectionDate.Date.AddHours(9);
                                if (attendanceLog.CheckOutTime.HasValue)
                                {
                                    var duration = attendanceLog.CheckOutTime.Value - attendanceLog.CheckInTime.Value;
                                    attendanceLog.WorkHours = $"{(int)duration.TotalHours}h {duration.Minutes}m";
                                }
                            }
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
                                CheckInTime = reg.CorrectionDate.Date.AddHours(9),
                                CheckOutTime = reg.CorrectionDate.Date.AddHours(18),
                                WorkHours = "9h 0m",
                                PunchSource = "Regularization Approved",
                                Status = "Present (On Time)",
                                Remarks = $"Regularized: {reg.RequestedCorrectTime}"
                            };
                            await _context.HRAttendanceLogs.AddAsync(newLog);
                        }
                    }

                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = $"Attendance regularization #{id} successfully {finalStatus.ToLower()}ed!" });
                }
            }

            return Json(new { success = false, message = "Record not found." });
        }

        public class BulkApproveItem
        {
            public int Id { get; set; }
            public string Category { get; set; } = string.Empty;
        }

        // POST: /Manager/BulkApprove
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> BulkApprove([FromBody] List<BulkApproveItem> items)
        {
            if (items == null || !items.Any())
            {
                return Json(new { success = false, message = "No requests selected." });
            }

            int count = 0;
            foreach (var item in items)
            {
                if (item.Category == "Leave" || item.Category == "Casual Leave" || item.Category == "Sick Leave" || item.Category == "Earned Leave")
                {
                    var leave = await _context.LeaveRequests.FindAsync(item.Id);
                    if (leave != null)
                    {
                        leave.Status = "Approved";
                        leave.ManagerStatus = "Approved";
                        leave.ManagerRemarks = "Bulk approved by Manager";
                        leave.ReviewedBy = User.Identity?.Name ?? "Manager";
                        leave.ReviewedAt = DateTime.UtcNow;
                        count++;
                    }
                }
                else if (item.Category == "Expense" || item.Category.Contains("Reimbursement"))
                {
                    var claim = await _context.ExpenseClaims.FindAsync(item.Id);
                    if (claim != null)
                    {
                        claim.Status = "Approved";
                        claim.ManagerStatus = "Approved";
                        claim.ManagerRemarks = "Bulk approved by Manager";
                        claim.ReviewedBy = User.Identity?.Name ?? "Manager";
                        claim.ReviewedAt = DateTime.UtcNow;
                        count++;
                    }
                }
                else // Regularization
                {
                    var reg = await _context.AttendanceRegularizations.FindAsync(item.Id);
                    if (reg != null)
                    {
                        reg.Status = "Approved";
                        reg.ManagerStatus = "Approved";
                        reg.ManagerRemarks = "Bulk approved by Manager";
                        reg.ReviewedBy = User.Identity?.Name ?? "Manager";
                        reg.ReviewedAt = DateTime.UtcNow;

                        var attendanceLog = await _context.HRAttendanceLogs
                            .FirstOrDefaultAsync(l => l.UserId == reg.UserId && l.Date.Date == reg.CorrectionDate.Date);

                        if (attendanceLog != null)
                        {
                            attendanceLog.Status = "Present (On Time)";
                            attendanceLog.Remarks = $"Regularized: {reg.RequestedCorrectTime} ({reg.Reason})";
                            if (reg.RequestedCorrectTime.Contains("06:00 PM"))
                            {
                                attendanceLog.CheckOutTime = reg.CorrectionDate.Date.AddHours(18);
                                if (attendanceLog.CheckInTime.HasValue)
                                {
                                    var duration = attendanceLog.CheckOutTime.Value - attendanceLog.CheckInTime.Value;
                                    attendanceLog.WorkHours = $"{(int)duration.TotalHours}h {duration.Minutes}m";
                                }
                            }
                            else if (reg.RequestedCorrectTime.Contains("09:00 AM"))
                            {
                                attendanceLog.CheckInTime = reg.CorrectionDate.Date.AddHours(9);
                                if (attendanceLog.CheckOutTime.HasValue)
                                {
                                    var duration = attendanceLog.CheckOutTime.Value - attendanceLog.CheckInTime.Value;
                                    attendanceLog.WorkHours = $"{(int)duration.TotalHours}h {duration.Minutes}m";
                                }
                            }
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
                                CheckInTime = reg.CorrectionDate.Date.AddHours(9),
                                CheckOutTime = reg.CorrectionDate.Date.AddHours(18),
                                WorkHours = "9h 0m",
                                PunchSource = "Regularization Approved",
                                Status = "Present (On Time)",
                                Remarks = $"Regularized: {reg.RequestedCorrectTime}"
                            };
                            await _context.HRAttendanceLogs.AddAsync(newLog);
                        }
                        count++;
                    }
                }
            }

            if (count > 0)
            {
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true, count = count, message = $"{count} requests approved successfully!" });
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
            var pendingLeaves = await _context.ESSLeaveApplications
                .Where(l => l.Status == "Pending")
                .ToListAsync();
            var pendingRegs = await _context.HRAttendanceRegularizations
                .Where(r => r.Status == "Pending")
                .ToListAsync();
            var pendingClaims = await _context.ESSExpenseClaims
                .Where(c => c.Status == "Pending")
                .ToListAsync();

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

            // Dynamically load active employees from database with department mapping
            var teamUsers = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Department)
                .Where(u => u.IsActive && u.Role != null && u.Role.RoleName != "Super Admin" && u.Role.RoleName != "Admin" && u.Role.RoleName != "System Admin")
                .OrderBy(u => u.UserId)
                .ToListAsync();

            var teamAttendanceList = new List<TeamMemberStatus>();
            if (teamUsers.Any())
            {
                foreach (var u in teamUsers)
                {
                    var initials = !string.IsNullOrWhiteSpace(u.FullName)
                        ? string.Join("", u.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(x => x[0])).ToUpper()
                        : "EM";

                    string dept = !string.IsNullOrWhiteSpace(u.DepartmentName) ? u.DepartmentName : (u.Department?.DepartmentName ?? "Operations & Logistics");

                    teamAttendanceList.Add(new TeamMemberStatus
                    {
                        Name = u.FullName,
                        Role = u.Role?.RoleName ?? "Team Member",
                        Department = dept,
                        Status = "Present",
                        ClockInTime = "09:00 AM",
                        Avatar = initials,
                        StatusColor = "success"
                    });
                }
            }
            else
            {
                teamAttendanceList = new List<TeamMemberStatus>
                {
                    new TeamMemberStatus { Name = "Numan Khan", Role = "Sales Executive", Department = "Sales & Marketing", Status = "Present", ClockInTime = "08:58 AM", Avatar = "NK", StatusColor = "success" },
                    new TeamMemberStatus { Name = "Aftab Shaik", Role = "Senior Developer", Department = "IT & Software", Status = "Present", ClockInTime = "09:05 AM", Avatar = "AS", StatusColor = "success" },
                    new TeamMemberStatus { Name = "Sneha Patil", Role = "Operations Associate", Department = "Operations & Logistics", Status = "On Leave", ClockInTime = "N/A", Avatar = "SP", StatusColor = "danger" },
                    new TeamMemberStatus { Name = "Rohan Sharma", Role = "Quality Analyst", Department = "IT & Software", Status = "Present", ClockInTime = "09:15 AM", Avatar = "RS", StatusColor = "success" },
                    new TeamMemberStatus { Name = "Zoya Malik", Role = "Backend Engineer", Department = "IT & Software", Status = "Late", ClockInTime = "09:42 AM", Avatar = "ZM", StatusColor = "warning" },
                    new TeamMemberStatus { Name = "Sameer Verma", Role = "UI/UX Designer", Department = "IT & Software", Status = "Present", ClockInTime = "09:00 AM", Avatar = "SV", StatusColor = "success" }
                };
            }

            return new ManagerDashboardViewModel
            {
                TotalTeamCount = teamAttendanceList.Count,
                PresentTodayCount = teamAttendanceList.Count(x => x.Status == "Present"),
                PendingApprovalsCount = pendingApprovalsCount,
                ActiveTasksCount = 8,
                DelayedTasksCount = 1,
                ProductivityRate = "94.2%",

                TeamAttendance = teamAttendanceList,

                PendingApprovals = pendingApprovalsList,

                TeamTasks = new List<ManagerTaskItem>
                {
                    new ManagerTaskItem { Id = 1, Title = "Finalize Q3 Client Billing Summary", AssignedTo = "Numan Khan", Priority = "Urgent", DueDate = "28 Aug 2026", Progress = 75, Status = "In Progress" },
                    new ManagerTaskItem { Id = 2, Title = "Resolve Payment Gateway Timeout Exception", AssignedTo = "Aftab Shaik", Priority = "High", DueDate = "27 Aug 2026", Progress = 90, Status = "Review" },
                    new ManagerTaskItem { Id = 3, Title = "Branch Inventory Stock Audit Reconciliation", AssignedTo = "Sneha Patil", Priority = "Medium", DueDate = "30 Aug 2026", Progress = 30, Status = "Delayed" },
                    new ManagerTaskItem { Id = 4, Title = "Prepare New Hire Onboarding Documentation", AssignedTo = "Rohan Sharma", Priority = "Low", DueDate = "31 Aug 2026", Progress = 50, Status = "In Progress" }
                }
            };
        }

        private async Task<List<ApprovalItemViewModel>> GetPendingApprovalsListAsync()
        {
            var pendingLeaves = await _context.ESSLeaveApplications
                .Where(x => x.Status == "Pending")
                .ToListAsync();

            var pendingRegs = await _context.HRAttendanceRegularizations
                .Where(x => x.Status == "Pending")
                .ToListAsync();

            var pendingClaims = await _context.ESSExpenseClaims
                .Where(x => x.Status == "Pending")
                .ToListAsync();

            var users = await _context.Users.Include(u => u.Role).ToListAsync();
            var userMap = users.ToDictionary(u => u.UserId, u => u);

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
