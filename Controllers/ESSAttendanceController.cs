using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System.Threading.Tasks;
using System.Security.Claims;
using System;
using System.Linq;

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

        // GET: /ESSAttendance/ClockInOut
        [HttpGet]
        public async Task<IActionResult> ClockInOut()
        {
            int userId = GetCurrentUserId();
            var today = DateTime.Today;

            var todayPunch = await _context.ESSPunches
                .FirstOrDefaultAsync(p => p.UserId == userId && p.Date == today);

            var recentPunches = await _context.ESSPunches
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.Date)
                .Take(7)
                .ToListAsync();

            ViewBag.TodayPunch = todayPunch;
            return View(recentPunches);
        }

        // POST: /ESSAttendance/CheckIn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn()
        {
            int userId = GetCurrentUserId();
            var today = DateTime.Today;

            var existingPunch = await _context.ESSPunches
                .FirstOrDefaultAsync(p => p.UserId == userId && p.Date == today);

            if (existingPunch == null)
            {
                var punch = new ESSPunch
                {
                    UserId = userId,
                    Date = today,
                    CheckInTime = DateTime.Now,
                    PunchSource = "Web Clock"
                };
                _context.ESSPunches.Add(punch);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ClockInOut));
        }

        // POST: /ESSAttendance/CheckOut
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut()
        {
            int userId = GetCurrentUserId();
            var today = DateTime.Today;

            var existingPunch = await _context.ESSPunches
                .FirstOrDefaultAsync(p => p.UserId == userId && p.Date == today);

            if (existingPunch != null && existingPunch.CheckOutTime == null)
            {
                existingPunch.CheckOutTime = DateTime.Now;
                _context.ESSPunches.Update(existingPunch);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ClockInOut));
        }

        // GET: /ESSAttendance/Calendar
        [HttpGet]
        public async Task<IActionResult> Calendar()
        {
            int userId = GetCurrentUserId();
            var punches = await _context.ESSPunches
                .Where(p => p.UserId == userId && p.Date.Month == DateTime.Today.Month)
                .ToListAsync();
            return View(punches);
        }

        // GET: /ESSAttendance/Regularization
        [HttpGet]
        public async Task<IActionResult> Regularization()
        {
            int userId = GetCurrentUserId();
            var punches = await _context.ESSPunches
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.Date)
                .ToListAsync();
            return View(punches);
        }

        // POST: /ESSAttendance/RequestRegularization
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestRegularization(DateTime Date, TimeSpan CheckInTime, TimeSpan CheckOutTime, string Reason)
        {
            int userId = GetCurrentUserId();
            var existingPunch = await _context.ESSPunches
                .FirstOrDefaultAsync(p => p.UserId == userId && p.Date == Date.Date);

            var checkInDateTime = Date.Date + CheckInTime;
            var checkOutDateTime = Date.Date + CheckOutTime;

            if (existingPunch == null)
            {
                var punch = new ESSPunch
                {
                    UserId = userId,
                    Date = Date.Date,
                    CheckInTime = checkInDateTime,
                    CheckOutTime = checkOutDateTime,
                    PunchSource = "Regularization Request"
                };
                _context.ESSPunches.Add(punch);
            }
            else
            {
                existingPunch.CheckInTime = checkInDateTime;
                existingPunch.CheckOutTime = checkOutDateTime;
                existingPunch.PunchSource = "Regularized";
                _context.ESSPunches.Update(existingPunch);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Regularization));
        }
    }
}
