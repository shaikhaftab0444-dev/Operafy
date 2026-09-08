using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System.Threading.Tasks;
using System.Security.Claims;
using System;
using System.Linq;
using System.Collections.Generic;

namespace ERP_System.Controllers
{
    [Authorize]
    public class ESSAttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ESSAttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out int id) ? id : 1;
        }

        // GET: /ESSAttendance
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(ClockInOut));
        }

        // GET: /ESSAttendance/ClockInOut
        [HttpGet]
        public async Task<IActionResult> ClockInOut()
        {
            int userId = GetCurrentUserId();
            var today = DateTime.Today;
            var nowTime = DateTime.Now.TimeOfDay;

            var user = await _context.Users
                .Include(u => u.Shift)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            var todayPunch = await _context.ESSPunches
                .FirstOrDefaultAsync(p => p.UserId == userId && p.Date == today);

            var recentPunches = await _context.ESSPunches
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.Date)
                .Take(7)
                .ToListAsync();

            bool isWindowOpen = true;
            string windowMessage = "";

            if (user?.Shift != null && todayPunch == null)
            {
                var shiftStart = user.Shift.StartTime;
                var shiftEnd = user.Shift.EndTime;
                var allowedStart = shiftStart.Subtract(TimeSpan.FromMinutes(30));

                bool inWindow = shiftEnd > allowedStart
                    ? (nowTime >= allowedStart && nowTime <= shiftEnd)
                    : (nowTime >= allowedStart || nowTime <= shiftEnd);

                if (!inWindow)
                {
                    isWindowOpen = false;
                    windowMessage = $"Shift window inactive. Your shift is scheduled from {DateTime.Today.Add(shiftStart):hh:mm tt} to {DateTime.Today.Add(shiftEnd):hh:mm tt}. Punching opens at {DateTime.Today.Add(allowedStart):hh:mm tt}.";
                }
            }

            ViewBag.IsWindowOpen = isWindowOpen;
            ViewBag.WindowMessage = windowMessage;
            ViewBag.TodayPunch = todayPunch;
            ViewBag.UserShift = user?.Shift;

            return View(recentPunches);
        }

        // POST: /ESSAttendance/CheckIn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn()
        {
            int userId = GetCurrentUserId();
            var today = DateTime.Today;
            var now = DateTime.Now;
            var nowTime = now.TimeOfDay;
            var user = await _context.Users.Include(u => u.Shift).FirstOrDefaultAsync(u => u.UserId == userId);

            // Shift Window Validation
            if (user?.Shift != null)
            {
                var shiftStart = user.Shift.StartTime;
                var shiftEnd = user.Shift.EndTime;
                var allowedStart = shiftStart.Subtract(TimeSpan.FromMinutes(30));

                bool inWindow = shiftEnd > allowedStart
                    ? (nowTime >= allowedStart && nowTime <= shiftEnd)
                    : (nowTime >= allowedStart || nowTime <= shiftEnd);

                if (!inWindow)
                {
                    TempData["ErrorMessage"] = $"Punch not permitted outside assigned shift hours ({DateTime.Today.Add(shiftStart):hh:mm tt} - {DateTime.Today.Add(shiftEnd):hh:mm tt}).";
                    return RedirectToAction(nameof(ClockInOut));
                }
            }

            await CheckInInternal(user, userId, today, now);

            TempData["SuccessMessage"] = "Clock-in recorded successfully.";
            return RedirectToAction(nameof(ClockInOut));
        }

        // POST: /ESSAttendance/PunchToggle (AJAX support)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PunchToggle()
        {
            int userId = GetCurrentUserId();
            var today = DateTime.Today;
            var now = DateTime.Now;
            var nowTime = now.TimeOfDay;
            var user = await _context.Users.Include(u => u.Shift).FirstOrDefaultAsync(u => u.UserId == userId);

            var existingPunch = await _context.ESSPunches
                .FirstOrDefaultAsync(p => p.UserId == userId && p.Date == today);

            if (existingPunch == null)
            {
                if (user?.Shift != null)
                {
                    var shiftStart = user.Shift.StartTime;
                    var shiftEnd = user.Shift.EndTime;
                    var allowedStart = shiftStart.Subtract(TimeSpan.FromMinutes(30));

                    bool inWindow = shiftEnd > allowedStart
                        ? (nowTime >= allowedStart && nowTime <= shiftEnd)
                        : (nowTime >= allowedStart || nowTime <= shiftEnd);

                    if (!inWindow)
                    {
                        return Json(new { success = false, message = "Punch not permitted outside assigned shift hours." });
                    }
                }

                await CheckInInternal(user, userId, today, now);
                return Json(new { success = true, message = "Clock-in recorded successfully.", isClockedIn = true });
            }
            else if (existingPunch.CheckOutTime == null)
            {
                await CheckOutInternal(user, userId, today, now);
                return Json(new { success = true, message = "Clock-out recorded successfully.", isClockedIn = false, isCompleted = true });
            }
            else
            {
                return Json(new { success = false, message = "Shift already completed for today." });
            }
        }

        private async Task CheckInInternal(User? user, int userId, DateTime today, DateTime now)
        {
            TimeSpan cutoff = new TimeSpan(10, 0, 0);
            if (user?.Shift != null)
            {
                cutoff = user.Shift.StartTime.Add(TimeSpan.FromMinutes(user.Shift.GracePeriodMinutes));
            }

            string status = now.TimeOfDay > cutoff ? "Late Check-in" : "Present (On Time)";

            var existingPunch = await _context.ESSPunches
                .FirstOrDefaultAsync(p => p.UserId == userId && p.Date == today);

            if (existingPunch == null)
            {
                var punch = new ESSPunch
                {
                    UserId = userId,
                    Date = today,
                    CheckInTime = now,
                    PunchSource = "Web Clock"
                };
                _context.ESSPunches.Add(punch);
            }

            var existingLog = await _context.HRAttendanceLogs
                .FirstOrDefaultAsync(l => l.UserId == userId && l.Date.Date == today);

            if (existingLog == null)
            {
                var newLog = new HRAttendanceLog
                {
                    UserId = userId,
                    EmployeeCode = user?.UserCode ?? $"EMP-{userId:D3}",
                    EmployeeName = user?.FullName ?? User.Identity?.Name ?? "Employee",
                    Date = today,
                    CheckInTime = now,
                    CheckOutTime = null,
                    WorkHours = "0h 0m",
                    PunchSource = "Web Clock",
                    Status = status,
                    Remarks = status == "Late Check-in" ? "Web Clock Check-in (Late arrival)" : "Web Clock Check-in"
                };
                await _context.HRAttendanceLogs.AddAsync(newLog);
            }
            else if (existingLog.CheckInTime == null)
            {
                existingLog.CheckInTime = now;
                existingLog.PunchSource = "Web Clock";
                existingLog.Status = status;
                _context.HRAttendanceLogs.Update(existingLog);
            }

            await _context.SaveChangesAsync();
        }

        // POST: /ESSAttendance/CheckOut
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut()
        {
            int userId = GetCurrentUserId();
            var today = DateTime.Today;
            var now = DateTime.Now;
            var user = await _context.Users.FindAsync(userId);

            await CheckOutInternal(user, userId, today, now);

            TempData["SuccessMessage"] = "Clock-out recorded successfully.";
            return RedirectToAction(nameof(ClockInOut));
        }

        private async Task CheckOutInternal(User? user, int userId, DateTime today, DateTime now)
        {
            var existingPunch = await _context.ESSPunches
                .FirstOrDefaultAsync(p => p.UserId == userId && p.Date == today);

            if (existingPunch != null && existingPunch.CheckOutTime == null)
            {
                existingPunch.CheckOutTime = now;
                _context.ESSPunches.Update(existingPunch);
            }

            var existingLog = await _context.HRAttendanceLogs
                .FirstOrDefaultAsync(l => l.UserId == userId && l.Date.Date == today);

            if (existingLog != null)
            {
                if (existingLog.CheckOutTime == null)
                {
                    existingLog.CheckOutTime = now;
                    if (existingLog.CheckInTime.HasValue)
                    {
                        var diff = now - existingLog.CheckInTime.Value;
                        existingLog.WorkHours = $"{(int)diff.TotalHours}h {diff.Minutes}m";
                    }
                    _context.HRAttendanceLogs.Update(existingLog);
                }
            }
            else
            {
                var newLog = new HRAttendanceLog
                {
                    UserId = userId,
                    EmployeeCode = user?.UserCode ?? $"EMP-{userId:D3}",
                    EmployeeName = user?.FullName ?? User.Identity?.Name ?? "Employee",
                    Date = today,
                    CheckInTime = now.AddHours(-8),
                    CheckOutTime = now,
                    WorkHours = "8h 0m",
                    PunchSource = "Web Clock",
                    Status = "Present (On Time)",
                    Remarks = "Web Clock Check-out"
                };
                await _context.HRAttendanceLogs.AddAsync(newLog);
            }

            await _context.SaveChangesAsync();
        }

        // GET: /ESSAttendance/Calendar
        [HttpGet]
        public async Task<IActionResult> Calendar(int? month, int? year)
        {
            int userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);

            var targetYear = year ?? DateTime.Today.Year;
            var targetMonth = month ?? DateTime.Today.Month;

            DateTime joiningDate = user?.JoiningDate ?? user?.CreatedAt.Date ?? new DateTime(targetYear, targetMonth, 1);

            var punches = await _context.ESSPunches
                .Where(p => p.UserId == userId && p.Date.Year == targetYear && p.Date.Month == targetMonth)
                .ToListAsync();

            ViewBag.JoiningDate = joiningDate;
            ViewBag.TargetMonth = targetMonth;
            ViewBag.TargetYear = targetYear;
            ViewBag.User = user;

            return View(punches);
        }

        // GET: /ESSAttendance/Regularization
        [HttpGet]
        public async Task<IActionResult> Regularization()
        {
            int userId = GetCurrentUserId();
            var requests = await _context.HRAttendanceRegularizations
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return View(requests);
        }

        // POST: /ESSAttendance/RequestRegularization
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestRegularization(DateTime Date, TimeSpan CheckInTime, TimeSpan CheckOutTime, string Reason)
        {
            int userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);

            DateTime dummyDate = DateTime.Today;
            string requestedTime = $"{dummyDate.Add(CheckInTime):hh:mm tt} - {dummyDate.Add(CheckOutTime):hh:mm tt}";

            var regularizationReq = new HRAttendanceRegularization
            {
                UserId = userId,
                EmployeeName = User.Identity?.Name ?? user?.FullName ?? "Employee",
                CorrectionDate = Date.Date,
                IncorrectPunch = "Missing Check-out / Punch Error",
                RequestedCorrectTime = requestedTime,
                Reason = Reason,
                RequestDate = DateTime.Today,
                Status = "Pending",
                ManagerStatus = "Pending",
                CreatedAt = DateTime.UtcNow,
                AdminRemarks = null
            };

            await _context.HRAttendanceRegularizations.AddAsync(regularizationReq);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Attendance regularization request submitted to HR for review.";
            return RedirectToAction(nameof(Regularization));
        }
    }
}
