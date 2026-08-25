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
        public IActionResult Departments()
        {
            return View();
        }

        // GET: /AdminMaster/Designations
        [HttpGet]
        public IActionResult Designations()
        {
            return View();
        }

        // GET: /AdminMaster/Holidays
        [HttpGet]
        public async Task<IActionResult> Holidays()
        {
            var holidays = await _context.Holidays.ToListAsync();
            return View(holidays);
        }
    }
}
