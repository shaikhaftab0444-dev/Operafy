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
    [Authorize(Roles = "Super Admin,Admin,HR,Manager,Employee")]
    public class HRLeaveController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HRLeaveController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /HRLeave/Policy
        [HttpGet]
        public IActionResult Policy()
        {
            return View();
        }

        // GET: /HRLeave/Applications
        [HttpGet]
        public async Task<IActionResult> Applications(string? statusFilter, string? searchTerm)
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
                            Status = leave.Status
                        };

            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
            {
                query = query.Where(l => l.Status == statusFilter);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(l => l.EmployeeName.ToLower().Contains(term) || l.LeaveType.ToLower().Contains(term));
            }

            var result = await query.OrderByDescending(l => l.StartDate).ToListAsync();
            var employees = await _context.Users.Where(u => u.IsActive).OrderBy(u => u.FullName).ToListAsync();
            ViewBag.Employees = employees;
            ViewBag.StatusFilter = statusFilter ?? "All";
            ViewBag.SearchTerm = searchTerm ?? "";

            return View(result);
        }

        // GET: /HRLeave/Approvals
        [HttpGet]
        public async Task<IActionResult> Approvals()
        {
            var pendingLeaves = await (from leave in _context.ESSLeaveApplications
                                       where leave.Status == "Pending" || leave.Status == "Pending Approver"
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
                                           Status = leave.Status
                                       }).OrderByDescending(l => l.StartDate).ToListAsync();

            return View(pendingLeaves);
        }

        // POST: /HRLeave/ApproveLeave
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveLeave(int leaveId, string? remarks)
        {
            var leave = await _context.ESSLeaveApplications.FindAsync(leaveId);
            if (leave == null)
            {
                TempData["ErrorMessage"] = "Leave application not found.";
                return RedirectToAction(nameof(Applications));
            }

            leave.Status = "Approved";
            _context.ESSLeaveApplications.Update(leave);

            // Automatically update attendance logs for the employee for approved leave days!
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
            TempData["SuccessMessage"] = $"Leave application for {empName} approved successfully.";
            return RedirectToAction(nameof(Applications));
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
                _context.ESSLeaveApplications.Update(leave);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Leave application rejected.";
            }
            return RedirectToAction(nameof(Applications));
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

            int totalDays = (endDate.Date - startDate.Date).Days + 1;
            if (totalDays <= 0) totalDays = 1;

            var leave = new ESSLeaveApplication
            {
                UserId = userId,
                LeaveType = leaveType,
                StartDate = startDate.Date,
                EndDate = endDate.Date,
                TotalDays = totalDays,
                Reason = reason,
                Status = "Approved"
            };

            await _context.ESSLeaveApplications.AddAsync(leave);

            // Update daily attendance logs
            for (var dt = startDate.Date; dt <= endDate.Date; dt = dt.AddDays(1))
            {
                var existingLog = await _context.HRAttendanceLogs
                    .FirstOrDefaultAsync(l => l.UserId == userId && l.Date.Date == dt);

                if (existingLog != null)
                {
                    existingLog.Status = "On Leave";
                    existingLog.Remarks = $"Approved {leaveType}: {reason}";
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
                        Remarks = $"Approved {leaveType}"
                    };
                    await _context.HRAttendanceLogs.AddAsync(leaveLog);
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Leave created and approved for {user.FullName}.";
            return RedirectToAction(nameof(Applications));
        }

        // GET: /HRLeave/BalanceLedger
        [HttpGet]
        public async Task<IActionResult> BalanceLedger()
        {
            var users = await _context.Users.Where(u => u.IsActive).OrderBy(u => u.FullName).ToListAsync();
            var approvedLeaves = await _context.ESSLeaveApplications.Where(l => l.Status == "Approved").ToListAsync();

            ViewBag.ApprovedLeaves = approvedLeaves;
            return View(users);
        }

        // GET: /HRLeave/HolidayList
        [HttpGet]
        public async Task<IActionResult> HolidayList()
        {
            var holidays = await _context.Holidays
                .OrderBy(h => h.Date)
                .ToListAsync();

            return View(holidays);
        }

        // POST: /HRLeave/CreateHoliday
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateHoliday(string HolidayName, DateTime Date, string Type, string? Description)
        {
            var holiday = new HRHoliday
            {
                HolidayName = HolidayName,
                Date = Date,
                Type = Type,
                Description = Description,
                CreatedAt = DateTime.Now
            };

            _context.Holidays.Add(holiday);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(HolidayList));
        }

        // GET: /HRLeave/EditHoliday
        [HttpGet]
        public IActionResult EditHoliday()
        {
            return RedirectToAction(nameof(HolidayList));
        }

        // POST: /HRLeave/EditHoliday
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHoliday(int HolidayId, string HolidayName, DateTime Date, string Type, string? Description)
        {
            var holiday = await _context.Holidays.FindAsync(HolidayId);
            if (holiday != null)
            {
                holiday.HolidayName = HolidayName;
                holiday.Date = Date;
                holiday.Type = Type;
                holiday.Description = Description;
                holiday.UpdatedAt = DateTime.Now;

                _context.Holidays.Update(holiday);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(HolidayList));
        }

        // POST: /HRLeave/DeleteHoliday
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHoliday(int HolidayId)
        {
            var holiday = await _context.Holidays.FindAsync(HolidayId);
            if (holiday != null)
            {
                _context.Holidays.Remove(holiday);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(HolidayList));
        }
    }
}
