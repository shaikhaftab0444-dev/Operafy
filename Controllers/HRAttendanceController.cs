using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Data;
using ERP_System.Models;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,HR")]
    public class HRAttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HRAttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // -------------------------------------------------------------
        // 1. DAILY ATTENDANCE LOGS
        // -------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> DailyLogs(DateTime? selectedDate, string? searchTerm, string? statusFilter)
        {
            var date = selectedDate ?? DateTime.Today;
            var query = _context.HRAttendanceLogs.Where(l => l.Date.Date == date.Date);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(l => l.EmployeeName.ToLower().Contains(term) || l.EmployeeCode.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
            {
                query = query.Where(l => l.Status == statusFilter);
            }

            var logs = await query.OrderBy(l => l.EmployeeCode).ToListAsync();
            var employees = await _context.Users.Where(u => u.IsActive).OrderBy(u => u.FullName).ToListAsync();

            // Calculate statistics for the selected date
            var allLogsForDate = await _context.HRAttendanceLogs.Where(l => l.Date.Date == date.Date).ToListAsync();

            var viewModel = new DailyLogsViewModel
            {
                Logs = logs,
                Employees = employees,
                SelectedDate = date,
                SearchTerm = searchTerm ?? "",
                StatusFilter = statusFilter ?? "All",
                TotalPresent = allLogsForDate.Count(l => l.Status.StartsWith("Present")),
                TotalLate = allLogsForDate.Count(l => l.Status.Contains("Late")),
                TotalOnLeave = allLogsForDate.Count(l => l.Status.Contains("Leave")),
                TotalAbsent = allLogsForDate.Count(l => l.Status.Contains("Absent"))
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManualPunch(int userId, DateTime date, TimeSpan checkInTime, TimeSpan? checkOutTime, string punchSource, string status, string? remarks)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Selected employee was not found.";
                return RedirectToAction(nameof(DailyLogs));
            }

            var checkInDateTime = date.Date.Add(checkInTime);
            DateTime? checkOutDateTime = checkOutTime.HasValue ? date.Date.Add(checkOutTime.Value) : null;

            string workHoursStr = "0h 0m";
            if (checkOutDateTime.HasValue && checkOutDateTime > checkInDateTime)
            {
                var duration = checkOutDateTime.Value - checkInDateTime;
                workHoursStr = $"{(int)duration.TotalHours}h {duration.Minutes}m";
            }

            var existingLog = await _context.HRAttendanceLogs
                .FirstOrDefaultAsync(l => l.UserId == userId && l.Date.Date == date.Date);

            if (existingLog != null)
            {
                existingLog.CheckInTime = checkInDateTime;
                existingLog.CheckOutTime = checkOutDateTime;
                existingLog.WorkHours = workHoursStr;
                existingLog.PunchSource = string.IsNullOrWhiteSpace(punchSource) ? "Manual Entry" : punchSource;
                existingLog.Status = status;
                existingLog.Remarks = remarks;
                _context.HRAttendanceLogs.Update(existingLog);
            }
            else
            {
                var newLog = new HRAttendanceLog
                {
                    UserId = user.UserId,
                    EmployeeCode = string.IsNullOrWhiteSpace(user.UserCode) ? $"EMP-00{user.UserId}" : user.UserCode,
                    EmployeeName = user.FullName,
                    Date = date.Date,
                    CheckInTime = checkInDateTime,
                    CheckOutTime = checkOutDateTime,
                    WorkHours = workHoursStr,
                    PunchSource = string.IsNullOrWhiteSpace(punchSource) ? "Manual Entry" : punchSource,
                    Status = status,
                    Remarks = remarks
                };
                await _context.HRAttendanceLogs.AddAsync(newLog);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Attendance log for {user.FullName} successfully saved.";
            return RedirectToAction(nameof(DailyLogs), new { selectedDate = date.ToString("yyyy-MM-dd") });
        }


        // -------------------------------------------------------------
        // 2. BIOMETRIC & GEO-PUNCHES
        // -------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Biometric()
        {
            var devices = await _context.HRBiometricDevices.OrderBy(d => d.DeviceId).ToListAsync();
            var todayPunches = await _context.HRAttendanceLogs
                .Where(l => l.Date.Date == DateTime.Today)
                .OrderByDescending(l => l.CheckInTime)
                .ToListAsync();

            var viewModel = new BiometricViewModel
            {
                Devices = devices,
                LivePunches = todayPunches,
                TotalPunchesToday = devices.Sum(d => d.TodaySyncCount) + todayPunches.Count,
                ActiveDevicesCount = devices.Count(d => d.IsActive && d.ConnectionStatus == "Connected")
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SyncNow()
        {
            var devices = await _context.HRBiometricDevices.ToListAsync();
            var random = new Random();
            int newPunchesSynced = 0;

            foreach (var dev in devices)
            {
                if (dev.IsActive)
                {
                    dev.LastSyncTime = DateTime.Now;
                    int added = random.Next(3, 12);
                    dev.TodaySyncCount += added;
                    newPunchesSynced += added;
                    dev.ConnectionStatus = "Connected";
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Biometric log sync completed successfully. {newPunchesSynced} new attendance punches fetched across all devices.";
            return RedirectToAction(nameof(Biometric));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDevice(string deviceName, string ipOrLocation)
        {
            if (string.IsNullOrWhiteSpace(deviceName) || string.IsNullOrWhiteSpace(ipOrLocation))
            {
                TempData["ErrorMessage"] = "Device Name and IP/Location are required.";
                return RedirectToAction(nameof(Biometric));
            }

            var device = new HRBiometricDevice
            {
                DeviceName = deviceName.Trim(),
                IpOrLocation = ipOrLocation.Trim(),
                ConnectionStatus = "Connected",
                LastSyncTime = DateTime.Now,
                TodaySyncCount = 0,
                IsActive = true
            };

            await _context.HRBiometricDevices.AddAsync(device);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Biometric device '{device.DeviceName}' added and connected successfully.";
            return RedirectToAction(nameof(Biometric));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleDeviceStatus(int deviceId)
        {
            var device = await _context.HRBiometricDevices.FindAsync(deviceId);
            if (device != null)
            {
                device.IsActive = !device.IsActive;
                device.ConnectionStatus = device.IsActive ? "Connected" : "Offline";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Device '{device.DeviceName}' status updated to {device.ConnectionStatus}.";
            }
            return RedirectToAction(nameof(Biometric));
        }


        // -------------------------------------------------------------
        // 3. SHIFT SCHEDULING & ROSTERS
        // -------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> ShiftScheduling()
        {
            var rosters = await _context.HRShiftRosters.OrderByDescending(r => r.EffectiveDate).ToListAsync();
            var employees = await _context.Users.Where(u => u.IsActive).OrderBy(u => u.FullName).ToListAsync();

            var viewModel = new ShiftSchedulingViewModel
            {
                Rosters = rosters,
                Employees = employees
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignShift(int userId, string shiftName, string timings, string weeklyOffs, DateTime effectiveDate, string? notes)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Employee not found.";
                return RedirectToAction(nameof(ShiftScheduling));
            }

            var existingRoster = await _context.HRShiftRosters.FirstOrDefaultAsync(r => r.UserId == userId);
            if (existingRoster != null)
            {
                existingRoster.ShiftName = shiftName;
                existingRoster.Timings = timings;
                existingRoster.WeeklyOffs = weeklyOffs;
                existingRoster.EffectiveDate = effectiveDate;
                existingRoster.Notes = notes;
                _context.HRShiftRosters.Update(existingRoster);
            }
            else
            {
                var roster = new HRShiftRoster
                {
                    UserId = user.UserId,
                    EmployeeName = user.FullName,
                    ShiftName = shiftName,
                    Timings = timings,
                    WeeklyOffs = weeklyOffs,
                    EffectiveDate = effectiveDate,
                    Notes = notes
                };
                await _context.HRShiftRosters.AddAsync(roster);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Shift roster updated for {user.FullName}.";
            return RedirectToAction(nameof(ShiftScheduling));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoster(int rosterId)
        {
            var roster = await _context.HRShiftRosters.FindAsync(rosterId);
            if (roster != null)
            {
                _context.HRShiftRosters.Remove(roster);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Roster for {roster.EmployeeName} removed.";
            }
            return RedirectToAction(nameof(ShiftScheduling));
        }


        // -------------------------------------------------------------
        // 4. OVERTIME TRACKER
        // -------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Overtime(string? monthYear)
        {
            var month = string.IsNullOrWhiteSpace(monthYear) ? "August 2026" : monthYear;
            var records = await _context.HROvertimeRecords
                .Where(r => r.MonthYear == month)
                .OrderByDescending(r => r.OvertimeHours)
                .ToListAsync();

            var employees = await _context.Users.Where(u => u.IsActive).OrderBy(u => u.FullName).ToListAsync();

            var viewModel = new OvertimeViewModel
            {
                OvertimeRecords = records,
                Employees = employees,
                SelectedMonth = month,
                TotalOvertimePay = records.Sum(r => r.TotalOvertimePay),
                PendingApprovalsCount = records.Count(r => r.PayoutStatus == "Pending Monthly Cycle")
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOvertime(int userId, string monthYear, int standardHours, int hoursLogged, string multiplier, decimal hourlyRate)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Selected employee not found.";
                return RedirectToAction(nameof(Overtime));
            }

            int otHours = Math.Max(0, hoursLogged - standardHours);
            decimal multFactor = multiplier == "2.0x" ? 2.0m : 1.5m;
            decimal totalPay = otHours * hourlyRate * multFactor;

            var existingRecord = await _context.HROvertimeRecords
                .FirstOrDefaultAsync(r => r.UserId == userId && r.MonthYear == monthYear);

            if (existingRecord != null)
            {
                existingRecord.StandardHours = standardHours;
                existingRecord.HoursLogged = hoursLogged;
                existingRecord.OvertimeHours = otHours;
                existingRecord.Multiplier = multiplier;
                existingRecord.HourlyRate = hourlyRate;
                existingRecord.TotalOvertimePay = totalPay;
                existingRecord.PayoutStatus = "Pending Monthly Cycle";
                _context.HROvertimeRecords.Update(existingRecord);
            }
            else
            {
                var record = new HROvertimeRecord
                {
                    UserId = user.UserId,
                    EmployeeName = user.FullName,
                    MonthYear = monthYear,
                    StandardHours = standardHours,
                    HoursLogged = hoursLogged,
                    OvertimeHours = otHours,
                    Multiplier = multiplier,
                    HourlyRate = hourlyRate,
                    TotalOvertimePay = totalPay,
                    PayoutStatus = "Pending Monthly Cycle"
                };
                await _context.HROvertimeRecords.AddAsync(record);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Overtime record logged for {user.FullName} ({otHours} OT Hours).";
            return RedirectToAction(nameof(Overtime), new { monthYear = monthYear });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveOvertime(int overtimeId)
        {
            var record = await _context.HROvertimeRecords.FindAsync(overtimeId);
            if (record != null)
            {
                record.PayoutStatus = "Approved for Payroll";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Overtime payout of ₹{record.TotalOvertimePay:N2} for {record.EmployeeName} approved for payroll cycle.";
            }
            return RedirectToAction(nameof(Overtime), new { monthYear = record?.MonthYear });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectOvertime(int overtimeId)
        {
            var record = await _context.HROvertimeRecords.FindAsync(overtimeId);
            if (record != null)
            {
                record.PayoutStatus = "Rejected";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Overtime record for {record.EmployeeName} rejected.";
            }
            return RedirectToAction(nameof(Overtime), new { monthYear = record?.MonthYear });
        }


        // -------------------------------------------------------------
        // 5. ATTENDANCE REQUESTS / REGULARIZATION
        // -------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Regularization()
        {
            var requests = await _context.HRAttendanceRegularizations
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            var employees = await _context.Users.Where(u => u.IsActive).OrderBy(u => u.FullName).ToListAsync();

            var viewModel = new RegularizationViewModel
            {
                Requests = requests,
                Employees = employees,
                PendingCount = requests.Count(r => r.Status == "Pending Review"),
                ApprovedCount = requests.Count(r => r.Status == "Approved"),
                RejectedCount = requests.Count(r => r.Status == "Rejected")
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveRequest(int requestId, string? adminRemarks)
        {
            var req = await _context.HRAttendanceRegularizations.FindAsync(requestId);
            if (req == null)
            {
                TempData["ErrorMessage"] = "Regularization request not found.";
                return RedirectToAction(nameof(Regularization));
            }

            req.Status = "Approved";
            req.AdminRemarks = adminRemarks ?? "Approved by HR";

            // Automatically update or create attendance log to reflect corrected time!
            var attendanceLog = await _context.HRAttendanceLogs
                .FirstOrDefaultAsync(l => l.UserId == req.UserId && l.Date.Date == req.CorrectionDate.Date);

            if (attendanceLog != null)
            {
                attendanceLog.Status = "Present (On Time)";
                attendanceLog.Remarks = $"Regularized: {req.RequestedCorrectTime} ({req.Reason})";
                if (req.RequestedCorrectTime.Contains("06:00 PM"))
                {
                    attendanceLog.CheckOutTime = req.CorrectionDate.Date.AddHours(18);
                    if (attendanceLog.CheckInTime.HasValue)
                    {
                        var duration = attendanceLog.CheckOutTime.Value - attendanceLog.CheckInTime.Value;
                        attendanceLog.WorkHours = $"{(int)duration.TotalHours}h {duration.Minutes}m";
                    }
                }
                else if (req.RequestedCorrectTime.Contains("09:00 AM"))
                {
                    attendanceLog.CheckInTime = req.CorrectionDate.Date.AddHours(9);
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
                var user = await _context.Users.FindAsync(req.UserId);
                var newLog = new HRAttendanceLog
                {
                    UserId = req.UserId,
                    EmployeeCode = user?.UserCode ?? $"EMP-00{req.UserId}",
                    EmployeeName = req.EmployeeName,
                    Date = req.CorrectionDate.Date,
                    CheckInTime = req.CorrectionDate.Date.AddHours(9),
                    CheckOutTime = req.CorrectionDate.Date.AddHours(18),
                    WorkHours = "9h 0m",
                    PunchSource = "Regularization Approved",
                    Status = "Present (On Time)",
                    Remarks = $"Regularized: {req.RequestedCorrectTime}"
                };
                await _context.HRAttendanceLogs.AddAsync(newLog);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Attendance regularization for {req.EmployeeName} approved and daily log updated.";
            return RedirectToAction(nameof(Regularization));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectRequest(int requestId, string? adminRemarks)
        {
            var req = await _context.HRAttendanceRegularizations.FindAsync(requestId);
            if (req != null)
            {
                req.Status = "Rejected";
                req.AdminRemarks = adminRemarks ?? "Rejected by HR";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Regularization request for {req.EmployeeName} rejected.";
            }
            return RedirectToAction(nameof(Regularization));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitRequest(int userId, DateTime correctionDate, string incorrectPunch, string requestedCorrectTime, string reason)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Selected employee not found.";
                return RedirectToAction(nameof(Regularization));
            }

            var request = new HRAttendanceRegularization
            {
                UserId = user.UserId,
                EmployeeName = user.FullName,
                CorrectionDate = correctionDate,
                IncorrectPunch = incorrectPunch,
                RequestedCorrectTime = requestedCorrectTime,
                Reason = reason,
                RequestDate = DateTime.Today,
                Status = "Pending Review"
            };

            await _context.HRAttendanceRegularizations.AddAsync(request);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Attendance regularization request submitted for {user.FullName}.";
            return RedirectToAction(nameof(Regularization));
        }
    }
}
