using ERP_System.Data;
using ERP_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin")]
    public class FinancialYearController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FinancialYearController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /FinancialYear
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var years = await _context.FinancialYears
                .OrderByDescending(y => y.StartDate)
                .ToListAsync();

            return View(years);
        }

        // GET: /FinancialYear/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new FinancialYear { StartDate = DateTime.Today, EndDate = DateTime.Today.AddYears(1).AddDays(-1) });
        }

        // POST: /FinancialYear/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FinancialYear model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedAt = DateTime.UtcNow;
                model.IsActive = true;

                // If this is set to current, clear others
                if (model.IsCurrent)
                {
                    var activeYears = await _context.FinancialYears.Where(y => y.IsCurrent).ToListAsync();
                    foreach (var year in activeYears)
                    {
                        year.IsCurrent = false;
                    }
                }

                _context.FinancialYears.Add(model);
                await _context.SaveChangesAsync();

                // Log activity
                _context.ActivityLogs.Add(new ActivityLog
                {
                    Title = "Financial Year Registered",
                    Description = $"New financial year '{model.YearName}' was registered in the calendar.",
                    IconClass = "fa-calendar-plus",
                    ColorClass = "bg-primary text-white",
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Financial Year '{model.YearName}' registered successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // POST: /FinancialYear/SetCurrent/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetCurrent(int id)
        {
            var year = await _context.FinancialYears.FindAsync(id);
            if (year == null)
            {
                return NotFound();
            }

            // Clear current flag from all other years
            var allYears = await _context.FinancialYears.ToListAsync();
            foreach (var y in allYears)
            {
                y.IsCurrent = (y.FinancialYearId == id);
                y.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Financial Year '{year.YearName}' is now set as the active operational year.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /FinancialYear/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var year = await _context.FinancialYears.FindAsync(id);
            if (year == null)
            {
                return NotFound();
            }

            // Cannot deactivate the currently active financial year
            if (year.IsCurrent && year.IsActive)
            {
                TempData["ErrorMessage"] = "Cannot deactivate the currently active operational financial year.";
                return RedirectToAction(nameof(Index));
            }

            year.IsActive = !year.IsActive;
            year.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Financial Year '{year.YearName}' status toggled successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
