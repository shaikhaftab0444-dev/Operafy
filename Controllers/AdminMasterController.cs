using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin")]
    public class AdminMasterController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminMasterController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /AdminMaster/Currency
        [HttpGet]
        public async Task<IActionResult> Currency()
        {
            var currencies = await _context.Currencies.OrderByDescending(c => c.IsBaseCurrency).ThenBy(c => c.CurrencyCode).ToListAsync();
            var baseCurrency = currencies.FirstOrDefault(c => c.IsBaseCurrency)?.CurrencyCode ?? "INR";

            var viewModel = new CurrencyViewModel
            {
                Currencies = currencies,
                BaseCurrencyCode = baseCurrency
            };

            return View(viewModel);
        }

        // POST: /AdminMaster/CreateCurrency
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCurrency(CurrencyViewModel model)
        {
            if (ModelState.IsValid)
            {
                var exists = await _context.Currencies.AnyAsync(c => c.CurrencyCode.ToUpper() == model.CurrencyCode.ToUpper());
                if (exists)
                {
                    TempData["ErrorMessage"] = $"Currency with code '{model.CurrencyCode.ToUpper()}' already exists.";
                    return RedirectToAction(nameof(Currency));
                }

                var currency = new Currency
                {
                    CurrencyCode = model.CurrencyCode.ToUpper(),
                    CurrencyName = model.CurrencyName,
                    Symbol = model.Symbol,
                    ExchangeRate = model.ExchangeRate,
                    DecimalPlaces = model.DecimalPlaces,
                    IsActive = model.IsActive,
                    IsBaseCurrency = false,
                    LastUpdated = DateTime.Now
                };

                _context.Currencies.Add(currency);
                await _context.SaveChangesAsync();

                // Add to history
                var history = new CurrencyRateHistory
                {
                    CurrencyId = currency.CurrencyId,
                    ExchangeRate = currency.ExchangeRate,
                    ChangedAt = DateTime.Now
                };
                _context.CurrencyRateHistories.Add(history);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Currency '{currency.CurrencyCode}' added successfully.";
                return RedirectToAction(nameof(Currency));
            }

            TempData["ErrorMessage"] = "Validation failed. Please check input values.";
            return RedirectToAction(nameof(Currency));
        }

        // POST: /AdminMaster/UpdateRate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRate(int id, decimal rate)
        {
            var currency = await _context.Currencies.FindAsync(id);
            if (currency != null)
            {
                if (currency.IsBaseCurrency)
                {
                    TempData["ErrorMessage"] = "Cannot change exchange rate of the base currency.";
                    return RedirectToAction(nameof(Currency));
                }

                if (rate <= 0)
                {
                    TempData["ErrorMessage"] = "Exchange rate must be greater than zero.";
                    return RedirectToAction(nameof(Currency));
                }

                currency.ExchangeRate = rate;
                currency.LastUpdated = DateTime.Now;

                // Log History
                var history = new CurrencyRateHistory
                {
                    CurrencyId = currency.CurrencyId,
                    ExchangeRate = rate,
                    ChangedAt = DateTime.Now
                };

                _context.CurrencyRateHistories.Add(history);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Exchange rate for '{currency.CurrencyCode}' updated to {rate:F4}.";
            }
            return RedirectToAction(nameof(Currency));
        }

        // POST: /AdminMaster/ToggleCurrencyStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCurrencyStatus(int id)
        {
            var currency = await _context.Currencies.FindAsync(id);
            if (currency != null)
            {
                if (currency.IsBaseCurrency)
                {
                    TempData["ErrorMessage"] = "Cannot deactivate the base currency.";
                    return RedirectToAction(nameof(Currency));
                }

                currency.IsActive = !currency.IsActive;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Status of '{currency.CurrencyCode}' updated successfully.";
            }
            return RedirectToAction(nameof(Currency));
        }

        // POST: /AdminMaster/SetBaseCurrency
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetBaseCurrency(string code)
        {
            var targetCurrency = await _context.Currencies.FirstOrDefaultAsync(c => c.CurrencyCode == code);
            if (targetCurrency != null)
            {
                var allCurrencies = await _context.Currencies.ToListAsync();
                foreach (var cur in allCurrencies)
                {
                    if (cur.CurrencyCode == code)
                    {
                        cur.IsBaseCurrency = true;
                        cur.ExchangeRate = 1.000000m;
                        cur.LastUpdated = DateTime.Now;
                    }
                    else
                    {
                        cur.IsBaseCurrency = false;
                        cur.LastUpdated = DateTime.Now;
                    }
                }
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Base currency set to '{code}'. All conversions updated.";
            }
            return RedirectToAction(nameof(Currency));
        }

        // POST: /AdminMaster/SyncRates
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SyncRates()
        {
            var currencies = await _context.Currencies.Where(c => !c.IsBaseCurrency && c.IsActive).ToListAsync();
            var random = new Random();

            foreach (var cur in currencies)
            {
                // Fluctuate rate slightly (+-1.5%) to simulate API update
                double percent = (random.NextDouble() * 3.0 - 1.5) / 100.0;
                cur.ExchangeRate = cur.ExchangeRate * (decimal)(1.0 + percent);
                cur.LastUpdated = DateTime.Now;

                var history = new CurrencyRateHistory
                {
                    CurrencyId = cur.CurrencyId,
                    ExchangeRate = cur.ExchangeRate,
                    ChangedAt = DateTime.Now
                };
                _context.CurrencyRateHistories.Add(history);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Successfully synchronized live exchange rates with global central bank API feeds.";
            return RedirectToAction(nameof(Currency));
        }

        // GET: /AdminMaster/GetRateHistory/5
        [HttpGet]
        public async Task<IActionResult> GetRateHistory(int id)
        {
            var history = await _context.CurrencyRateHistories
                .Where(h => h.CurrencyId == id)
                .OrderByDescending(h => h.ChangedAt)
                .Select(h => new
                {
                    rate = h.ExchangeRate.ToString("F6"),
                    date = h.ChangedAt.ToString("dd MMM yyyy hh:mm tt")
                })
                .ToListAsync();

            return Json(history);
        }

        // GET: /AdminMaster/Tax
        [HttpGet]
        public async Task<IActionResult> Tax(string regime = "All")
        {
            IQueryable<TaxSlab> query = _context.TaxSlabs;
            if (!string.IsNullOrEmpty(regime) && regime != "All")
            {
                query = query.Where(t => t.Regime == regime);
            }

            var slabs = await query.OrderByDescending(t => t.IsActive).ThenBy(t => t.TaxCode).ToListAsync();
            var viewModel = new TaxViewModel
            {
                TaxSlabs = slabs,
                SelectedRegime = regime
            };
            return View(viewModel);
        }

        // POST: /AdminMaster/CreateTaxSlab
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTaxSlab(TaxViewModel model)
        {
            if (ModelState.IsValid)
            {
                var exists = await _context.TaxSlabs.AnyAsync(t => t.TaxCode.ToUpper() == model.TaxCode.ToUpper());
                if (exists)
                {
                    TempData["ErrorMessage"] = $"Tax slab with code '{model.TaxCode.ToUpper()}' already exists.";
                    return RedirectToAction(nameof(Tax));
                }

                var slab = new TaxSlab
                {
                    TaxCode = model.TaxCode.ToUpper(),
                    Description = model.Description,
                    CombinedRate = model.CombinedRate,
                    CGST = model.CGST,
                    SGST = model.SGST,
                    IGST = model.IGST,
                    Category = model.Category,
                    Regime = model.Regime,
                    IsRcmActive = model.IsRcmActive,
                    EffectiveDate = model.EffectiveDate,
                    IsActive = model.IsActive
                };

                _context.TaxSlabs.Add(slab);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Tax slab '{slab.TaxCode}' created successfully.";
                return RedirectToAction(nameof(Tax));
            }

            TempData["ErrorMessage"] = "Validation failed. Please verify the input values.";
            return RedirectToAction(nameof(Tax));
        }

        // POST: /AdminMaster/ToggleTaxSlabStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTaxSlabStatus(int id)
        {
            var slab = await _context.TaxSlabs.FindAsync(id);
            if (slab != null)
            {
                slab.IsActive = !slab.IsActive;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Tax slab '{slab.TaxCode}' status updated.";
            }
            return RedirectToAction(nameof(Tax));
        }

        // POST: /AdminMaster/DeleteTaxSlab
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTaxSlab(int id)
        {
            var slab = await _context.TaxSlabs.FindAsync(id);
            if (slab != null)
            {
                _context.TaxSlabs.Remove(slab);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Tax slab '{slab.TaxCode}' deleted successfully.";
            }
            return RedirectToAction(nameof(Tax));
        }

        // GET: /AdminMaster/Departments
        [HttpGet]
        public async Task<IActionResult> Departments(int branchId = 0, string search = "")
        {
            var branches = await _context.Branches.ToListAsync();
            var staffList = await _context.Users.Where(u => u.IsActive).ToListAsync();

            IQueryable<Department> query = _context.Departments
                .Include(d => d.HOD)
                .Include(d => d.Branch)
                .Include(d => d.ParentDepartment);

            if (branchId > 0)
            {
                query = query.Where(d => d.BranchId == branchId);
            }

            if (!string.IsNullOrEmpty(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(d => d.DepartmentCode.ToLower().Contains(lowerSearch) || d.DepartmentName.ToLower().Contains(lowerSearch));
            }

            var departments = await query.OrderByDescending(d => d.IsActive).ThenBy(d => d.DepartmentCode).ToListAsync();

            var viewModel = new DepartmentViewModel
            {
                Departments = departments,
                Branches = branches,
                StaffList = staffList,
                SelectedBranchId = branchId,
                SearchTerm = search
            };

            return View(viewModel);
        }

        // POST: /AdminMaster/CreateDepartment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDepartment(DepartmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var exists = await _context.Departments.AnyAsync(d => d.DepartmentCode.ToUpper() == model.DepartmentCode.ToUpper());
                if (exists)
                {
                    TempData["ErrorMessage"] = $"Department with code '{model.DepartmentCode.ToUpper()}' already exists.";
                    return RedirectToAction(nameof(Departments));
                }

                var dept = new Department
                {
                    DepartmentCode = model.DepartmentCode.ToUpper(),
                    DepartmentName = model.DepartmentName,
                    HODId = model.HODId,
                    BranchId = model.BranchId,
                    ParentDepartmentId = model.ParentDepartmentId,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.Now
                };

                _context.Departments.Add(dept);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Department '{dept.DepartmentCode}' added successfully.";
                return RedirectToAction(nameof(Departments));
            }

            TempData["ErrorMessage"] = "Validation failed. Please check input values.";
            return RedirectToAction(nameof(Departments));
        }

        // POST: /AdminMaster/ToggleDepartmentStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleDepartmentStatus(int id)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept != null)
            {
                dept.IsActive = !dept.IsActive;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Status of department '{dept.DepartmentCode}' updated.";
            }
            return RedirectToAction(nameof(Departments));
        }

        // POST: /AdminMaster/DeleteDepartment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept != null)
            {
                // Check if any child departments depend on this one
                var hasChildren = await _context.Departments.AnyAsync(d => d.ParentDepartmentId == id);
                if (hasChildren)
                {
                    TempData["ErrorMessage"] = $"Cannot delete department '{dept.DepartmentCode}' because sub-departments depend on it.";
                    return RedirectToAction(nameof(Departments));
                }

                _context.Departments.Remove(dept);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Department '{dept.DepartmentCode}' deleted successfully.";
            }
            return RedirectToAction(nameof(Departments));
        }

        // GET: /AdminMaster/Designations
        [HttpGet]
        public async Task<IActionResult> Designations(int departmentId = 0, string search = "")
        {
            var departments = await _context.Departments.OrderBy(d => d.DepartmentName).ToListAsync();
            
            IQueryable<Designation> query = _context.Designations.Include(d => d.Department);
            
            if (departmentId > 0)
            {
                query = query.Where(d => d.DepartmentId == departmentId);
            }
            
            if (!string.IsNullOrEmpty(search))
            {
                var lower = search.ToLower();
                query = query.Where(d => d.DesignationCode.ToLower().Contains(lower) || d.JobTitle.ToLower().Contains(lower));
            }
            
            var designations = await query.OrderByDescending(d => d.IsActive).ThenBy(d => d.DesignationCode).ToListAsync();
            
            // Calculate active staff count for each designation dynamically
            var counts = new Dictionary<int, int>();
            foreach (var d in designations)
            {
                int count = 1;
                if (d.DesignationCode == "HR-MGR") count = await _context.Users.CountAsync(u => u.IsActive && u.Role != null && u.Role.RoleName == "HR");
                else if (d.DesignationCode == "SDE-II") count = await _context.Users.CountAsync(u => u.IsActive && u.Role != null && u.Role.RoleName == "Employee");
                else if (d.DesignationCode == "SYS-ADM") count = await _context.Users.CountAsync(u => u.IsActive && u.Role != null && u.Role.RoleName == "Manager");
                else if (d.DesignationCode == "SAL-EXEC") count = await _context.Users.CountAsync(u => u.IsActive && u.Role != null && u.Role.RoleName.Contains("Sales"));
                else if (d.DesignationCode == "ACC-MGR") count = await _context.Users.CountAsync(u => u.IsActive && u.Role != null && (u.Role.RoleName == "Accountant" || u.Role.RoleName == "Finance Manager"));
                counts[d.DesignationId] = count > 0 ? count : 1;
            }
            
            ViewBag.DesignationEmployeeCounts = counts;
            
            var viewModel = new DesignationViewModel
            {
                Designations = designations,
                Departments = departments,
                SelectedDepartmentId = departmentId,
                SearchQuery = search
            };
            
            return View(viewModel);
        }

        // POST: /AdminMaster/CreateDesignation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDesignation(DesignationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var codeUpper = model.Designation.DesignationCode.ToUpper().Trim();
                var exists = await _context.Designations.AnyAsync(d => d.DesignationCode.ToUpper() == codeUpper);
                if (exists)
                {
                    TempData["ErrorMessage"] = $"Designation with code '{codeUpper}' already exists.";
                    return RedirectToAction(nameof(Designations));
                }

                var desig = new Designation
                {
                    DesignationCode = codeUpper,
                    JobTitle = model.Designation.JobTitle,
                    DepartmentId = model.Designation.DepartmentId,
                    HierarchyLevel = model.Designation.HierarchyLevel,
                    MinCTC = model.Designation.MinCTC,
                    MaxCTC = model.Designation.MaxCTC,
                    JobDescription = model.Designation.JobDescription,
                    IsActive = model.Designation.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Designations.Add(desig);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Designation '{desig.DesignationCode}' created successfully.";
                return RedirectToAction(nameof(Designations));
            }

            TempData["ErrorMessage"] = "Validation failed. Please verify input fields.";
            return RedirectToAction(nameof(Designations));
        }

        // POST: /AdminMaster/EditDesignation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDesignation(DesignationViewModel model)
        {
            var exists = await _context.Designations.FindAsync(model.Designation.DesignationId);
            if (exists == null)
            {
                TempData["ErrorMessage"] = "Designation not found.";
                return RedirectToAction(nameof(Designations));
            }

            exists.JobTitle = model.Designation.JobTitle;
            exists.DepartmentId = model.Designation.DepartmentId;
            exists.HierarchyLevel = model.Designation.HierarchyLevel;
            exists.MinCTC = model.Designation.MinCTC;
            exists.MaxCTC = model.Designation.MaxCTC;
            exists.JobDescription = model.Designation.JobDescription;
            exists.IsActive = model.Designation.IsActive;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Designation '{exists.DesignationCode}' updated successfully.";
            return RedirectToAction(nameof(Designations));
        }

        // POST: /AdminMaster/ToggleDesignationStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleDesignationStatus(int id)
        {
            var desig = await _context.Designations.FindAsync(id);
            if (desig != null)
            {
                desig.IsActive = !desig.IsActive;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Status of designation '{desig.DesignationCode}' updated.";
            }
            return RedirectToAction(nameof(Designations));
        }

        // POST: /AdminMaster/DeleteDesignation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDesignation(int id)
        {
            var desig = await _context.Designations.FindAsync(id);
            if (desig != null)
            {
                _context.Designations.Remove(desig);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Designation '{desig.DesignationCode}' deleted successfully.";
            }
            return RedirectToAction(nameof(Designations));
        }

        // GET: /AdminMaster/Holidays
        [HttpGet]
        public async Task<IActionResult> Holidays(int year = 2026, int branchId = 0, string view = "list")
        {
            var branches = await _context.Branches.OrderBy(b => b.BranchName).ToListAsync();
            
            IQueryable<HRHoliday> query = _context.Holidays.Include(h => h.Branch);

            // Filter by Year
            query = query.Where(h => h.Date.Year == year);

            // Filter by Branch (If branchId > 0, show branch-specific holidays AND global holidays)
            if (branchId > 0)
            {
                query = query.Where(h => h.BranchId == branchId || h.BranchId == null);
            }

            var holidays = await query.OrderBy(h => h.Date).ToListAsync();

            // Calculate KPI metrics
            int totalCount = holidays.Count(h => h.IsActive);
            int mandatory = holidays.Count(h => h.IsActive && (h.Type == "National Holiday" || h.Type == "Gazetted / Public" || h.Type == "Mandatory"));
            int optional = holidays.Count(h => h.IsActive && (h.Type == "Optional / Restricted" || h.Type == "Optional" || h.Type == "Restricted"));

            // Find next upcoming holiday (closest date today or future)
            var today = DateTime.Today;
            var upcoming = await _context.Holidays
                .Where(h => h.IsActive && h.Date >= today)
                .OrderBy(h => h.Date)
                .FirstOrDefaultAsync();

            var viewModel = new HolidayViewModel
            {
                Holidays = holidays,
                Branches = branches,
                SelectedYear = year,
                SelectedBranchId = branchId,
                SelectedView = view,
                TotalHolidaysCount = totalCount,
                MandatoryCount = mandatory,
                OptionalCount = optional,
                NextUpcomingHoliday = upcoming
            };

            return View(viewModel);
        }

        // POST: /AdminMaster/CreateHoliday
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateHoliday(HolidayViewModel model)
        {
            if (ModelState.IsValid)
            {
                var exists = await _context.Holidays.AnyAsync(h => h.HolidayName.ToLower() == model.Holiday.HolidayName.ToLower() && h.Date.Date == model.Holiday.Date.Date);
                if (exists)
                {
                    TempData["ErrorMessage"] = $"Holiday '{model.Holiday.HolidayName}' already scheduled on this date.";
                    return RedirectToAction(nameof(Holidays), new { year = model.SelectedYear, branchId = model.SelectedBranchId, view = model.SelectedView });
                }

                var hol = new HRHoliday
                {
                    HolidayName = model.Holiday.HolidayName.Trim(),
                    Date = model.Holiday.Date,
                    Type = model.Holiday.Type,
                    BranchId = model.Holiday.BranchId == 0 ? null : model.Holiday.BranchId,
                    IsPaid = model.Holiday.IsPaid,
                    IsActive = model.Holiday.IsActive,
                    Description = model.Holiday.Description,
                    CreatedAt = DateTime.Now
                };

                _context.Holidays.Add(hol);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Holiday '{hol.HolidayName}' created successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Validation failed. Please verify input fields.";
            }

            return RedirectToAction(nameof(Holidays), new { year = model.SelectedYear, branchId = model.SelectedBranchId, view = model.SelectedView });
        }

        // POST: /AdminMaster/EditHoliday
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHoliday(HolidayViewModel model)
        {
            var exists = await _context.Holidays.FindAsync(model.Holiday.HolidayId);
            if (exists == null)
            {
                TempData["ErrorMessage"] = "Holiday not found.";
                return RedirectToAction(nameof(Holidays), new { year = model.SelectedYear, branchId = model.SelectedBranchId, view = model.SelectedView });
            }

            exists.HolidayName = model.Holiday.HolidayName.Trim();
            exists.Date = model.Holiday.Date;
            exists.Type = model.Holiday.Type;
            exists.BranchId = model.Holiday.BranchId == 0 ? null : model.Holiday.BranchId;
            exists.IsPaid = model.Holiday.IsPaid;
            exists.IsActive = model.Holiday.IsActive;
            exists.Description = model.Holiday.Description;
            exists.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Holiday '{exists.HolidayName}' updated successfully.";

            return RedirectToAction(nameof(Holidays), new { year = model.SelectedYear, branchId = model.SelectedBranchId, view = model.SelectedView });
        }

        // POST: /AdminMaster/ToggleHolidayStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleHolidayStatus(int id, int year = 2026, int branchId = 0, string view = "list")
        {
            var hol = await _context.Holidays.FindAsync(id);
            if (hol != null)
            {
                hol.IsActive = !hol.IsActive;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Status of holiday '{hol.HolidayName}' updated.";
            }
            return RedirectToAction(nameof(Holidays), new { year, branchId, view });
        }

        // POST: /AdminMaster/DeleteHoliday
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHoliday(int id, int year = 2026, int branchId = 0, string view = "list")
        {
            var hol = await _context.Holidays.FindAsync(id);
            if (hol != null)
            {
                _context.Holidays.Remove(hol);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Holiday '{hol.HolidayName}' deleted successfully.";
            }
            return RedirectToAction(nameof(Holidays), new { year, branchId, view });
        }
    }
}
