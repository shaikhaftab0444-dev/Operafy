using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin")]
    public class SuperAdminOrgController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SuperAdminOrgController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /SuperAdminOrg/Companies
        [HttpGet]
        public async Task<IActionResult> Companies()
        {
            var companies = await _context.Companies.ToListAsync();
            return View(companies);
        }

        // POST: /SuperAdminOrg/CreateCompany
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCompany(Company company)
        {
            if (ModelState.IsValid || (company.CompanyName != null && company.CompanyCode != null))
            {
                company.AddressLine1 ??= "HQ Office Address";
                company.City ??= "City Office";
                company.State ??= "State Office";
                company.Country ??= "Country Office";
                company.IsActive = true;
                company.CreatedAt = DateTime.Now;
                _context.Companies.Add(company);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Companies));
            }
            var companies = await _context.Companies.ToListAsync();
            return View(nameof(Companies), companies);
        }

        // POST: /SuperAdminOrg/ToggleCompanyActive
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCompanyActive(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                company.IsActive = !company.IsActive;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Companies));
        }

        // GET: /SuperAdminOrg/Branches
        [HttpGet]
        public async Task<IActionResult> Branches()
        {
            var branches = await _context.Branches.ToListAsync();
            return View(branches);
        }

        // POST: /SuperAdminOrg/CreateBranch
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBranch(Branch branch)
        {
            if (ModelState.IsValid || (branch.BranchName != null && branch.BranchCode != null))
            {
                branch.IsActive = true;
                _context.Branches.Add(branch);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Branches));
            }
            var branches = await _context.Branches.ToListAsync();
            return View(nameof(Branches), branches);
        }

        // GET: /SuperAdminOrg/FinancialYears
        [HttpGet]
        public async Task<IActionResult> FinancialYears()
        {
            var years = await _context.FinancialYears.ToListAsync();
            return View(years);
        }

        // POST: /SuperAdminOrg/CreateFinancialYear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFinancialYear(FinancialYear year)
        {
            if (ModelState.IsValid || year.YearName != null)
            {
                year.IsActive = true;
                _context.FinancialYears.Add(year);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(FinancialYears));
            }
            var years = await _context.FinancialYears.ToListAsync();
            return View(nameof(FinancialYears), years);
        }

        // GET: /SuperAdminOrg/ExchangeRates
        [HttpGet]
        public IActionResult ExchangeRates()
        {
            return View();
        }
    }
}
