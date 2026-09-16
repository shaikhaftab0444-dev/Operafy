using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "HR,Super Admin,Admin,Manager")]
    public class HRLeaveController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HRLeaveController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================================
        // 1. LEAVE POLICY & TYPES (/HRLeave/Policy)
        // =========================================================================
        [HttpGet]
        public async Task<IActionResult> Policy()
        {
            await EnsureDefaultLeaveTypesAsync();

            var policies = await _context.LeaveTypes
                .Where(l => l.IsActive)
                .OrderBy(l => l.Name)
                .ToListAsync();

            ViewBag.TotalLeaveTypes = policies.Count;
            ViewBag.MaxYearlyLeave = policies.Sum(p => p.YearlyLimit);
            ViewBag.EncashableCount = policies.Count(p => p.IsEncashable);

            return View(policies);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLeaveType(string leaveName, string code, int yearlyLimit, int carryForwardLimit, bool isEncashable, string? eligibilityCriteria)
        {
            if (string.IsNullOrWhiteSpace(leaveName) || string.IsNullOrWhiteSpace(code))
            {
                TempData["ErrorMessage"] = "Leave Name and Code are required.";
                return RedirectToAction(nameof(Policy));
            }

            var cleanCode = code.Trim().ToUpper();
            var exists = await _context.LeaveTypes.AnyAsync(l => l.Code == cleanCode && l.IsActive);
            if (exists)
            {
                TempData["ErrorMessage"] = $"Leave code '{cleanCode}' already exists.";
                return RedirectToAction(nameof(Policy));
            }

            var type = new LeaveType
            {
                Name = leaveName.Trim(),
                Code = cleanCode,
                YearlyLimit = yearlyLimit > 0 ? yearlyLimit : 12,
                CarryForwardLimit = carryForwardLimit >= 0 ? carryForwardLimit : 0,
                IsEncashable = isEncashable,
                EligibilityCriteria = eligibilityCriteria?.Trim() ?? "Immediate upon confirmation",
                IsActive = true
            };

            _context.LeaveTypes.Add(type);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Leave Type '{type.Name} ({type.Code})' policy created successfully.";
            return RedirectToAction(nameof(Policy));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLeaveType(int id, string leaveName, string code, int yearlyLimit, int carryForwardLimit, bool isEncashable, string? eligibilityCriteria)
        {
            var policy = await _context.LeaveTypes.FindAsync(id);
            if (policy != null)
            {
                policy.Name = leaveName.Trim();
                policy.Code = code.Trim().ToUpper();
                policy.YearlyLimit = yearlyLimit;
                policy.CarryForwardLimit = carryForwardLimit;
                policy.IsEncashable = isEncashable;
                policy.EligibilityCriteria = eligibilityCriteria?.Trim();

                _context.LeaveTypes.Update(policy);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Leave Type '{policy.Name}' updated successfully.";
            }
            return RedirectToAction(nameof(Policy));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLeaveType(int id)
        {
            var policy = await _context.LeaveTypes.FindAsync(id);
            if (policy != null)
            {
                policy.IsActive = false;
                _context.LeaveTypes.Update(policy);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Leave Type '{policy.Name}' deleted.";
            }
            return RedirectToAction(nameof(Policy));
        }


        // =========================================================================
        // 2. LEAVE APPLICATIONS (/HRLeave/Applications)
        // =========================================================================
        [HttpGet]
        public async Task<IActionResult> Applications(string? searchEmployee = "", string? searchTerm = "", string? status = "All", string? statusFilter = "All")
        {
            await EnsureDefaultLeaveTypesAsync();

            var search = !string.IsNullOrWhiteSpace(searchEmployee) ? searchEmployee : (!string.IsNullOrWhiteSpace(searchTerm) ? searchTerm : "");
            var filterStatus = !string.IsNullOrWhiteSpace(status) && status != "All" ? status : (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All" ? statusFilter : "All");

            var query = from leave in _context.ESSLeaveApplications
                        join user in _context.Users on leave.UserId equals user.UserId into userGroup
                        from u in userGroup.DefaultIfEmpty()
                        select new HRLeaveApplicationViewModel
                        {
                            LeaveApplicationId = leave.LeaveApplicationId,
                            UserId = leave.UserId,
                            EmployeeName = u != null ? u.FullName : $"Employee #{leave.UserId}",
                            LeaveType = leave.LeaveType,
                            StartDate = leave.StartDate,
                            EndDate = leave.EndDate,
                            TotalDays = leave.TotalDays,
                            Reason = leave.Reason,
                            Status = leave.Status,
                            AdminRemarks = leave.ManagerRemarks
                        };

            if (filterStatus != "All")
            {
                query = query.Where(l => l.Status == filterStatus);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(l => l.EmployeeName.ToLower().Contains(term) || l.LeaveType.ToLower().Contains(term));
            }

            var applications = await query.OrderByDescending(l => l.StartDate).ToListAsync();

            // Overall counts
            var allLeaves = await _context.ESSLeaveApplications.ToListAsync();
            ViewBag.TotalCount = allLeaves.Count;
            ViewBag.PendingCount = allLeaves.Count(a => a.Status == "Pending" || a.Status == "Pending Approver");
            ViewBag.ApprovedCount = allLeaves.Count(a => a.Status == "Approved");
            ViewBag.RejectedCount = allLeaves.Count(a => a.Status == "Rejected");

            ViewBag.StatusFilter = filterStatus;
            ViewBag.SearchTerm = search;
            ViewBag.LeaveTypes = await _context.LeaveTypes.Where(t => t.IsActive).OrderBy(t => t.Name).ToListAsync();

            return View(applications);
        }

        // Scalable Debounce AJAX Search for 2,000+ Employees
        [HttpGet]
        public async Task<IActionResult> SearchEmployees(string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 1)
            {
                return Json(new List<object>());
            }

            var term = q.Trim().ToLower();
            var results = await _context.Users
                .Where(u => u.IsActive && (
                    u.FullName.ToLower().Contains(term) ||
                    u.UserCode.ToLower().Contains(term) ||
                    u.Email.ToLower().Contains(term) ||
                    u.UserName.ToLower().Contains(term)
                ))
                .OrderBy(u => u.FullName)
                .Take(25)
                .Select(u => new
                {
                    id = u.UserId,
                    name = u.FullName,
                    code = string.IsNullOrEmpty(u.UserCode) ? $"EMP-00{u.UserId}" : u.UserCode,
                    email = u.Email,
                    department = u.DepartmentName ?? (u.Department != null ? u.Department.DepartmentName : "General")
                })
                .ToListAsync();

            return Json(results);
        }

        // POST: /HRLeave/SubmitLeaveByHR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitLeaveByHR(int userId, string leaveType, DateTime startDate, DateTime endDate, string reason)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Employee not found.";
                return RedirectToAction(nameof(Applications));
            }

            int totalDays = Math.Max(1, (endDate.Date - startDate.Date).Days + 1);

            var leave = new ESSLeaveApplication
            {
                UserId = userId,
                EmployeeName = user.FullName,
                LeaveType = string.IsNullOrWhiteSpace(leaveType) ? "Casual Leave" : leaveType,
                StartDate = startDate.Date,
                EndDate = endDate.Date,
                TotalDays = totalDays,
                Reason = reason,
                Status = "Approved",
                ManagerRemarks = "Direct Log by HR/Admin",
                CreatedAt = DateTime.UtcNow,
                ReviewedBy = User.Identity?.Name ?? "HR Admin",
                ReviewedAt = DateTime.UtcNow
            };

            await _context.ESSLeaveApplications.AddAsync(leave);

            // Also synchronize with LeaveRequests table
            var matchingLeaveType = await _context.LeaveTypes.FirstOrDefaultAsync(t => t.Name == leaveType || t.Code == leaveType);
            var leaveReq = new LeaveRequest
            {
                UserId = userId,
                LeaveTypeId = matchingLeaveType?.Id ?? 1,
                StartDate = startDate.Date,
                EndDate = endDate.Date,
                TotalDays = totalDays,
                Reason = reason,
                Status = "Approved",
                ApproverRemarks = "Direct Log by HR/Admin",
                CreatedAt = DateTime.UtcNow,
                ActionedAt = DateTime.UtcNow,
                ActionedByUserId = User.Identity?.Name
            };
            await _context.LeaveRequests.AddAsync(leaveReq);

            // Update daily attendance logs to "On Leave"
            for (var dt = startDate.Date; dt <= endDate.Date; dt = dt.AddDays(1))
            {
                var existingLog = await _context.HRAttendanceLogs
                    .FirstOrDefaultAsync(l => l.UserId == userId && l.Date.Date == dt);

                if (existingLog != null)
                {
                    existingLog.Status = "On Leave";
                    existingLog.Remarks = $"Approved {leave.LeaveType}: {reason}";
                    _context.HRAttendanceLogs.Update(existingLog);
                }
                else
                {
                    var leaveLog = new HRAttendanceLog
                    {
                        UserId = userId,
                        EmployeeCode = user.UserCode ?? $"EMP-00{userId}",
                        EmployeeName = user.FullName,
                        Date = dt,
                        CheckInTime = null,
                        CheckOutTime = null,
                        WorkHours = "0h 0m",
                        PunchSource = "HR Entry",
                        Status = "On Leave",
                        Remarks = $"Approved {leave.LeaveType}"
                    };
                    await _context.HRAttendanceLogs.AddAsync(leaveLog);
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Leave for {user.FullName} ({totalDays} days) logged and approved successfully.";
            return RedirectToAction(nameof(Applications));
        }


        // =========================================================================
        // 3. LEAVE APPROVALS & DECISION PIPELINE (/HRLeave/Approvals)
        // =========================================================================
        [HttpGet]
        public async Task<IActionResult> Approvals(DateTime? fromDate, DateTime? toDate, string status = "Pending")
        {
            var query = from leave in _context.ESSLeaveApplications
                        join user in _context.Users on leave.UserId equals user.UserId into userGroup
                        from u in userGroup.DefaultIfEmpty()
                        select new HRLeaveApplicationViewModel
                        {
                            LeaveApplicationId = leave.LeaveApplicationId,
                            UserId = leave.UserId,
                            EmployeeName = u != null ? u.FullName : $"Employee #{leave.UserId}",
                            LeaveType = leave.LeaveType,
                            StartDate = leave.StartDate,
                            EndDate = leave.EndDate,
                            TotalDays = leave.TotalDays,
                            Reason = leave.Reason,
                            Status = leave.Status,
                            AdminRemarks = leave.ManagerRemarks
                        };

            if (status != "All")
            {
                if (status == "Pending")
                    query = query.Where(l => l.Status == "Pending" || l.Status == "Pending Approver");
                else
                    query = query.Where(l => l.Status == status);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(l => l.StartDate >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(l => l.EndDate <= toDate.Value.Date);
            }

            var result = await query.OrderByDescending(l => l.StartDate).ToListAsync();

            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.SelectedStatus = status;

            var allPending = await _context.ESSLeaveApplications.CountAsync(l => l.Status == "Pending" || l.Status == "Pending Approver");
            ViewBag.PendingCount = allPending;
            ViewBag.TotalDays = result.Sum(x => x.TotalDays);

            return View(result);
        }

        // POST: /HRLeave/ApproveLeave
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveLeave(int leaveId, string? remarks)
        {
            var leave = await _context.ESSLeaveApplications.FindAsync(leaveId);
            if (leave == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Leave application not found." });

                TempData["ErrorMessage"] = "Leave application not found.";
                return RedirectToAction(nameof(Approvals));
            }

            leave.Status = "Approved";
            leave.ManagerRemarks = string.IsNullOrWhiteSpace(remarks) ? "Approved by HR Admin" : remarks;
            leave.ReviewedBy = User.Identity?.Name ?? "HR Admin";
            leave.ReviewedAt = DateTime.UtcNow;
            _context.ESSLeaveApplications.Update(leave);

            // Synchronize Attendance Logs
            var user = await _context.Users.FindAsync(leave.UserId);
            var empCode = user?.UserCode ?? $"EMP-00{leave.UserId}";
            var empName = user?.FullName ?? $"Employee #{leave.UserId}";

            for (var dt = leave.StartDate.Date; dt <= leave.EndDate.Date; dt = dt.AddDays(1))
            {
                var existingLog = await _context.HRAttendanceLogs
                    .FirstOrDefaultAsync(l => l.UserId == leave.UserId && l.Date.Date == dt);

                if (existingLog != null)
                {
                    existingLog.Status = "On Leave";
                    existingLog.Remarks = $"Approved {leave.LeaveType}: {leave.Reason}";
                    _context.HRAttendanceLogs.Update(existingLog);
                }
                else
                {
                    var leaveLog = new HRAttendanceLog
                    {
                        UserId = leave.UserId,
                        EmployeeCode = empCode,
                        EmployeeName = empName,
                        Date = dt,
                        CheckInTime = null,
                        CheckOutTime = null,
                        WorkHours = "0h 0m",
                        PunchSource = "Leave Approved",
                        Status = "On Leave",
                        Remarks = $"Approved {leave.LeaveType}"
                    };
                    await _context.HRAttendanceLogs.AddAsync(leaveLog);
                }
            }

            await _context.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, message = $"Leave for {empName} approved successfully." });

            TempData["SuccessMessage"] = $"Leave application for {empName} approved successfully.";
            return RedirectToAction(nameof(Approvals));
        }

        // POST: /HRLeave/RejectLeave
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectLeave(int leaveId, string? remarks)
        {
            var leave = await _context.ESSLeaveApplications.FindAsync(leaveId);
            if (leave != null)
            {
                leave.Status = "Rejected";
                leave.ManagerRemarks = string.IsNullOrWhiteSpace(remarks) ? "Rejected by HR Admin" : remarks;
                leave.ReviewedBy = User.Identity?.Name ?? "HR Admin";
                leave.ReviewedAt = DateTime.UtcNow;
                _context.ESSLeaveApplications.Update(leave);
                await _context.SaveChangesAsync();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, message = "Leave application rejected." });

            TempData["SuccessMessage"] = "Leave application rejected.";
            return RedirectToAction(nameof(Approvals));
        }


        // =========================================================================
        // 4. LEAVE BALANCE LEDGER (/HRLeave/BalanceLedger)
        // =========================================================================
        [HttpGet]
        public async Task<IActionResult> BalanceLedger(string financialYear = "2026-2027", int? departmentId = null)
        {
            var departments = await _context.Departments.OrderBy(d => d.DepartmentName).ToListAsync();
            ViewBag.Departments = departments;
            ViewBag.SelectedFinancialYear = financialYear;
            ViewBag.SelectedDepartmentId = departmentId;

            var usersQuery = _context.Users.Where(u => u.IsActive).AsQueryable();
            if (departmentId.HasValue && departmentId.Value > 0)
            {
                usersQuery = usersQuery.Where(u => u.DepartmentId == departmentId.Value);
            }

            var users = await usersQuery.OrderBy(u => u.FullName).ToListAsync();
            var approvedLeaves = await _context.ESSLeaveApplications
                .Where(l => l.Status == "Approved")
                .ToListAsync();

            var leaveBalances = new List<UserLeaveBalanceViewModel>();

            foreach (var u in users)
            {
                var userLeaves = approvedLeaves.Where(l => l.UserId == u.UserId).ToList();

                int clTaken = userLeaves.Where(l => l.LeaveType.Contains("Casual") || l.LeaveType == "CL").Sum(l => l.TotalDays);
                int slTaken = userLeaves.Where(l => l.LeaveType.Contains("Sick") || l.LeaveType == "SL").Sum(l => l.TotalDays);
                int elTaken = userLeaves.Where(l => l.LeaveType.Contains("Earned") || l.LeaveType == "EL").Sum(l => l.TotalDays);

                leaveBalances.Add(new UserLeaveBalanceViewModel
                {
                    UserId = u.UserId,
                    EmployeeCode = string.IsNullOrEmpty(u.UserCode) ? $"EMP-00{u.UserId}" : u.UserCode,
                    FullName = u.FullName,
                    DepartmentName = u.DepartmentName ?? (departments.FirstOrDefault(d => d.DepartmentId == u.DepartmentId)?.DepartmentName ?? "General"),
                    FinancialYear = financialYear,
                    CasualLeaveAllowed = 12,
                    CasualLeaveTaken = clTaken,
                    SickLeaveAllowed = 8,
                    SickLeaveTaken = slTaken,
                    EarnedLeaveAllowed = 15,
                    EarnedLeaveTaken = elTaken
                });
            }

            ViewBag.TotalStaff = leaveBalances.Count;
            ViewBag.TotalAllocated = leaveBalances.Sum(b => b.TotalEntitled);
            ViewBag.TotalTaken = leaveBalances.Sum(b => b.TotalTaken);
            ViewBag.TotalAvailable = leaveBalances.Sum(b => b.TotalBalance);

            return View(leaveBalances);
        }


        // =========================================================================
        // 5. HOLIDAY CALENDAR (/HRLeave/HolidayList)
        // =========================================================================
        [HttpGet]
        public async Task<IActionResult> HolidayList(int? year = null, string? type = "All")
        {
            await EnsureDefaultHolidaysAsync();

            int selectedYear = year ?? DateTime.Today.Year;
            var query = _context.CompanyHolidays.Where(h => h.Year == selectedYear).AsQueryable();

            if (!string.IsNullOrWhiteSpace(type) && type != "All")
            {
                query = query.Where(h => h.Type == type);
            }

            var holidays = await query.OrderBy(h => h.HolidayDate).ToListAsync();

            // Counts for year
            var allYearHolidays = await _context.CompanyHolidays.Where(h => h.Year == selectedYear).ToListAsync();
            ViewBag.SelectedYear = selectedYear;
            ViewBag.SelectedType = type ?? "All";
            ViewBag.TotalHolidays = allYearHolidays.Count;
            ViewBag.MandatoryCount = allYearHolidays.Count(h => h.Type == "Mandatory");
            ViewBag.OptionalCount = allYearHolidays.Count(h => h.Type == "Optional");
            ViewBag.RestrictedCount = allYearHolidays.Count(h => h.Type == "Restricted");

            return View(holidays);
        }

        // POST: /HRLeave/CreateHoliday
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateHoliday(string HolidayName, DateTime Date, string Type, string? Description, int? Year)
        {
            if (string.IsNullOrWhiteSpace(HolidayName))
            {
                TempData["ErrorMessage"] = "Holiday Name is required.";
                return RedirectToAction(nameof(HolidayList));
            }

            int holidayYear = Year ?? Date.Year;
            var holiday = new CompanyHoliday
            {
                Name = HolidayName.Trim(),
                HolidayDate = Date.Date,
                DayOfWeek = Date.DayOfWeek.ToString(),
                Type = string.IsNullOrWhiteSpace(Type) ? "Mandatory" : Type,
                Description = Description?.Trim(),
                Year = holidayYear
            };

            await _context.CompanyHolidays.AddAsync(holiday);

            // Also add to HRHoliday table for master consistency
            var hrHoliday = new HRHoliday
            {
                HolidayName = HolidayName.Trim(),
                Date = Date.Date,
                Type = holiday.Type,
                Description = holiday.Description,
                CreatedAt = DateTime.Now
            };
            await _context.Holidays.AddAsync(hrHoliday);

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Holiday '{holiday.Name}' added to {holidayYear} calendar successfully.";
            return RedirectToAction(nameof(HolidayList), new { year = holidayYear });
        }

        // POST: /HRLeave/EditHoliday
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHoliday(int Id, string HolidayName, DateTime Date, string Type, string? Description, int? Year)
        {
            var holiday = await _context.CompanyHolidays.FindAsync(Id);
            if (holiday != null)
            {
                holiday.Name = HolidayName.Trim();
                holiday.HolidayDate = Date.Date;
                holiday.DayOfWeek = Date.DayOfWeek.ToString();
                holiday.Type = Type;
                holiday.Description = Description?.Trim();
                holiday.Year = Year ?? Date.Year;

                _context.CompanyHolidays.Update(holiday);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Holiday '{holiday.Name}' updated successfully.";
                return RedirectToAction(nameof(HolidayList), new { year = holiday.Year });
            }

            return RedirectToAction(nameof(HolidayList));
        }

        // POST: /HRLeave/DeleteHoliday
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHoliday(int Id)
        {
            var holiday = await _context.CompanyHolidays.FindAsync(Id);
            if (holiday != null)
            {
                int year = holiday.Year;
                _context.CompanyHolidays.Remove(holiday);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Holiday '{holiday.Name}' deleted.";
                return RedirectToAction(nameof(HolidayList), new { year = year });
            }

            return RedirectToAction(nameof(HolidayList));
        }


        // =========================================================================
        // HELPER SEED METHODS
        // =========================================================================
        private async Task EnsureDefaultLeaveTypesAsync()
        {
            if (!await _context.LeaveTypes.AnyAsync())
            {
                var defaultTypes = new List<LeaveType>
                {
                    new LeaveType { Name = "Casual Leave", Code = "CL", YearlyLimit = 12, CarryForwardLimit = 0, IsEncashable = false, EligibilityCriteria = "Immediate after probation", IsActive = true },
                    new LeaveType { Name = "Sick Leave", Code = "SL", YearlyLimit = 8, CarryForwardLimit = 4, IsEncashable = false, EligibilityCriteria = "Immediate upon joining", IsActive = true },
                    new LeaveType { Name = "Earned Leave", Code = "EL", YearlyLimit = 15, CarryForwardLimit = 30, IsEncashable = true, EligibilityCriteria = "Post 1 year continuous service", IsActive = true },
                    new LeaveType { Name = "Maternity / Paternity Leave", Code = "ML", YearlyLimit = 180, CarryForwardLimit = 0, IsEncashable = false, EligibilityCriteria = "Eligible full-time staff", IsActive = true }
                };
                await _context.LeaveTypes.AddRangeAsync(defaultTypes);
                await _context.SaveChangesAsync();
            }
        }

        private async Task EnsureDefaultHolidaysAsync()
        {
            if (!await _context.CompanyHolidays.AnyAsync())
            {
                int currentYear = DateTime.Today.Year;
                var defaultHolidays = new List<CompanyHoliday>
                {
                    new CompanyHoliday { Name = "New Year's Day", HolidayDate = new DateTime(currentYear, 1, 1), DayOfWeek = "Thursday", Type = "Mandatory", Description = "Global New Year Celebration", Year = currentYear },
                    new CompanyHoliday { Name = "Republic Day", HolidayDate = new DateTime(currentYear, 1, 26), DayOfWeek = "Monday", Type = "Mandatory", Description = "National Holiday", Year = currentYear },
                    new CompanyHoliday { Name = "Maha Shivratri", HolidayDate = new DateTime(currentYear, 2, 17), DayOfWeek = "Tuesday", Type = "Optional", Description = "Religious Festival", Year = currentYear },
                    new CompanyHoliday { Name = "Holi (Festival of Colors)", HolidayDate = new DateTime(currentYear, 3, 4), DayOfWeek = "Wednesday", Type = "Mandatory", Description = "Spring Festival of Colors", Year = currentYear },
                    new CompanyHoliday { Name = "Eid-ul-Fitr", HolidayDate = new DateTime(currentYear, 3, 21), DayOfWeek = "Saturday", Type = "Mandatory", Description = "Islamic Festive Day", Year = currentYear },
                    new CompanyHoliday { Name = "Maharashtra Day & Labour Day", HolidayDate = new DateTime(currentYear, 5, 1), DayOfWeek = "Friday", Type = "Mandatory", Description = "State & International Workers Day", Year = currentYear },
                    new CompanyHoliday { Name = "Independence Day", HolidayDate = new DateTime(currentYear, 8, 15), DayOfWeek = "Saturday", Type = "Mandatory", Description = "National Independence Celebration", Year = currentYear },
                    new CompanyHoliday { Name = "Ganesh Chaturthi", HolidayDate = new DateTime(currentYear, 9, 14), DayOfWeek = "Monday", Type = "Mandatory", Description = "Festival of Lord Ganesha", Year = currentYear },
                    new CompanyHoliday { Name = "Gandhi Jayanti", HolidayDate = new DateTime(currentYear, 10, 2), DayOfWeek = "Friday", Type = "Mandatory", Description = "Mahatma Gandhi Birth Anniversary", Year = currentYear },
                    new CompanyHoliday { Name = "Dussehra (Vijayadashami)", HolidayDate = new DateTime(currentYear, 10, 20), DayOfWeek = "Tuesday", Type = "Mandatory", Description = "Victory of Good over Evil", Year = currentYear },
                    new CompanyHoliday { Name = "Diwali (Festival of Lights)", HolidayDate = new DateTime(currentYear, 11, 8), DayOfWeek = "Sunday", Type = "Mandatory", Description = "Deepavali Festival", Year = currentYear },
                    new CompanyHoliday { Name = "Christmas Day", HolidayDate = new DateTime(currentYear, 12, 25), DayOfWeek = "Friday", Type = "Mandatory", Description = "Christmas Celebration", Year = currentYear }
                };
                await _context.CompanyHolidays.AddRangeAsync(defaultHolidays);
                await _context.SaveChangesAsync();
            }
        }
    }
}
