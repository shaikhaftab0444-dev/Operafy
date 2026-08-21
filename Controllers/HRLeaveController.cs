using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,HR")]
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
        public IActionResult Applications()
        {
            return View();
        }

        // GET: /HRLeave/BalanceLedger
        [HttpGet]
        public IActionResult BalanceLedger()
        {
            return View();
        }

        // GET: /HRLeave/Approvals
        [HttpGet]
        public IActionResult Approvals()
        {
            return View();
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
