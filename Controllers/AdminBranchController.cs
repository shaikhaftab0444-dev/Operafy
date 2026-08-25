using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System.Threading.Tasks;
using System.Linq;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin")]
    public class AdminBranchController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Helpers.ICurrencyService _currencyService;

        public AdminBranchController(ApplicationDbContext context, Helpers.ICurrencyService currencyService)
        {
            _context = context;
            _currencyService = currencyService;
        }

        // GET: /AdminBranch/Details
        [HttpGet]
        public async Task<IActionResult> Details()
        {
            var branches = await _context.Branches.ToListAsync();
            return View(branches);
        }

        // POST: /AdminBranch/CreateBranch
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBranch(Branch branch)
        {
            if (ModelState.IsValid || (branch.BranchName != null && branch.BranchCode != null))
            {
                branch.IsActive = true;
                _context.Branches.Add(branch);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details));
            }
            var branches = await _context.Branches.ToListAsync();
            return View(nameof(Details), branches);
        }

        // GET: /AdminBranch/Hours
        [HttpGet]
        public async Task<IActionResult> Hours(string search = "")
        {
            var branches = await _context.Branches.OrderBy(b => b.BranchName).ToListAsync();
            
            IQueryable<AdminBranchHour> query = _context.AdminBranchHours.Include(h => h.Branch);

            if (!string.IsNullOrEmpty(search))
            {
                var lower = search.ToLower();
                query = query.Where(h => h.BranchName.ToLower().Contains(lower) || (h.Branch != null && h.Branch.BranchName.ToLower().Contains(lower)));
            }

            var hours = await query.OrderByDescending(h => h.IsActive).ThenBy(h => h.BranchName).ToListAsync();

            var viewModel = new BranchWorkingHoursViewModel
            {
                Hours = hours,
                Branches = branches,
                SearchTerm = search
            };

            return View(viewModel);
        }

        // POST: /AdminBranch/SaveHours
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveHours(BranchWorkingHoursViewModel model)
        {
            var branch = await _context.Branches.FindAsync(model.Hour.BranchId);
            if (branch == null)
            {
                TempData["ErrorMessage"] = "Selected branch is invalid.";
                return RedirectToAction(nameof(Hours));
            }

            if (model.Hour.HourId == 0)
            {
                // Create
                var exists = await _context.AdminBranchHours.AnyAsync(h => h.BranchId == model.Hour.BranchId);
                if (exists)
                {
                    TempData["ErrorMessage"] = $"Working hours for branch '{branch.BranchName}' already configured.";
                    return RedirectToAction(nameof(Hours));
                }

                var newHour = new AdminBranchHour
                {
                    BranchId = model.Hour.BranchId,
                    BranchName = branch.BranchName,
                    OpeningTime = model.Hour.OpeningTime,
                    ClosingTime = model.Hour.ClosingTime,
                    OffDay = model.Hour.OffDay,
                    GracePeriod = model.Hour.GracePeriod,
                    BreakDuration = model.Hour.BreakDuration,
                    HalfDayMinHours = model.Hour.HalfDayMinHours,
                    IsContinuousShift = model.Hour.IsContinuousShift,
                    IsActive = model.Hour.IsActive,
                    EffectiveDate = model.Hour.EffectiveDate
                };

                _context.AdminBranchHours.Add(newHour);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Working hours for '{branch.BranchName}' established successfully.";
            }
            else
            {
                // Update
                var existing = await _context.AdminBranchHours.FindAsync(model.Hour.HourId);
                if (existing == null)
                {
                    TempData["ErrorMessage"] = "Working hour setting not found.";
                    return RedirectToAction(nameof(Hours));
                }

                existing.BranchId = model.Hour.BranchId;
                existing.BranchName = branch.BranchName;
                existing.OpeningTime = model.Hour.OpeningTime;
                existing.ClosingTime = model.Hour.ClosingTime;
                existing.OffDay = model.Hour.OffDay;
                existing.GracePeriod = model.Hour.GracePeriod;
                existing.BreakDuration = model.Hour.BreakDuration;
                existing.HalfDayMinHours = model.Hour.HalfDayMinHours;
                existing.IsContinuousShift = model.Hour.IsContinuousShift;
                existing.IsActive = model.Hour.IsActive;
                existing.EffectiveDate = model.Hour.EffectiveDate;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Working hours for '{branch.BranchName}' modified successfully.";
            }

            return RedirectToAction(nameof(Hours));
        }

        // POST: /AdminBranch/ToggleHoursStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleHoursStatus(int id)
        {
            var hour = await _context.AdminBranchHours.FindAsync(id);
            if (hour != null)
            {
                hour.IsActive = !hour.IsActive;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Working hours status for '{hour.BranchName}' updated.";
            }
            return RedirectToAction(nameof(Hours));
        }

        // POST: /AdminBranch/DeleteHours
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHours(int id)
        {
            var hour = await _context.AdminBranchHours.FindAsync(id);
            if (hour != null)
            {
                _context.AdminBranchHours.Remove(hour);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Working hours configuration for '{hour.BranchName}' removed.";
            }
            return RedirectToAction(nameof(Hours));
        }

        // GET: /AdminBranch/Regional
        [HttpGet]
        public async Task<IActionResult> Regional()
        {
            var settings = await _context.RegionalConfigurations.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new RegionalConfiguration
                {
                    Country = "India",
                    CurrencyCode = "INR",
                    CurrencySymbol = "₹",
                    NumberSystem = "Lakhs/Crores",
                    DateFormat = "DD/MM/YYYY",
                    Timezone = "India Standard Time",
                    TaxSystem = "GST",
                    FinancialYearCycle = "April 1 - March 31"
                };
            }
            return View(settings);
        }

        // POST: /AdminBranch/SaveRegionalSettings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRegionalSettings(RegionalConfiguration model)
        {
            if (ModelState.IsValid)
            {
                var settings = await _context.RegionalConfigurations.FirstOrDefaultAsync();
                if (settings == null)
                {
                    settings = new RegionalConfiguration();
                    _context.RegionalConfigurations.Add(settings);
                }

                settings.Country = model.Country;
                settings.CurrencyCode = model.CurrencyCode;
                settings.CurrencySymbol = model.CurrencySymbol;
                settings.NumberSystem = model.NumberSystem;
                settings.DateFormat = model.DateFormat;
                settings.Timezone = model.Timezone;
                settings.TaxSystem = model.TaxSystem;
                settings.FinancialYearCycle = model.FinancialYearCycle;
                settings.LastUpdated = System.DateTime.UtcNow;

                await _context.SaveChangesAsync();
                
                // Refresh static cache in the Currency Service immediately
                _currencyService.RefreshCache();

                TempData["SuccessMessage"] = "Regional configuration settings updated and propagated globally.";
                return RedirectToAction(nameof(Regional));
            }

            TempData["ErrorMessage"] = "Validation failed. Please verify regional configurations inputs.";
            return RedirectToAction(nameof(Regional));
        }

        // GET: /AdminBranch/GetCountryDefaults
        [HttpGet]
        public IActionResult GetCountryDefaults(string country)
        {
            if (string.IsNullOrEmpty(country)) return BadRequest();

            object? defaults = country.ToLower() switch
            {
                "india" => new
                {
                    CurrencyCode = "INR",
                    CurrencySymbol = "₹",
                    NumberSystem = "Lakhs/Crores",
                    DateFormat = "DD/MM/YYYY",
                    Timezone = "India Standard Time",
                    TaxSystem = "GST (CGST, SGST, IGST)",
                    FinancialYearCycle = "April 1 - March 31"
                },
                "united states" => new
                {
                    CurrencyCode = "USD",
                    CurrencySymbol = "$",
                    NumberSystem = "Millions/Billions",
                    DateFormat = "MM/DD/YYYY",
                    Timezone = "Eastern Standard Time",
                    TaxSystem = "Sales Tax",
                    FinancialYearCycle = "January 1 - December 31"
                },
                "united arab emirates" => new
                {
                    CurrencyCode = "AED",
                    CurrencySymbol = "د.إ",
                    NumberSystem = "Millions/Billions",
                    DateFormat = "DD/MM/YYYY",
                    Timezone = "Arabian Standard Time",
                    TaxSystem = "VAT (5%)",
                    FinancialYearCycle = "January 1 - December 31"
                },
                "united kingdom" => new
                {
                    CurrencyCode = "GBP",
                    CurrencySymbol = "£",
                    NumberSystem = "Millions/Billions",
                    DateFormat = "DD/MM/YYYY",
                    Timezone = "GMT Standard Time",
                    TaxSystem = "VAT (20%)",
                    FinancialYearCycle = "January 1 - December 31"
                },
                "saudi arabia" => new
                {
                    CurrencyCode = "SAR",
                    CurrencySymbol = "﷼",
                    NumberSystem = "Millions/Billions",
                    DateFormat = "DD/MM/YYYY",
                    Timezone = "Arab Standard Time",
                    TaxSystem = "VAT (15%)",
                    FinancialYearCycle = "January 1 - December 31"
                },
                _ => null
            };

            if (defaults == null) return NotFound();
            return Json(defaults);
        }
    }
}
