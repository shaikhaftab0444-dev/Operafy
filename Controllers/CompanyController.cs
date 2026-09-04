using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using System;
using System.Threading.Tasks;
using ERP_System.Data;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,HR,Manager,Employee")]
    public class CompanyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CompanyController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Company
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var company = await _context.Companies.FirstOrDefaultAsync();
            if (company == null)
            {
                // Fallback / auto-creation logic if table is empty (though it has 1 row in database)
                company = new Company
                {
                    CompanyName = "AIT Technologies Pvt Ltd",
                    CompanyCode = "AIT001",
                    Email = "info@ait.com",
                    Phone = "+91 98765 43210",
                    Website = "www.aitech.com",
                    AddressLine1 = "AIT Campus, Tech Zone",
                    City = "Delhi",
                    State = "Delhi",
                    Country = "India",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Companies.Add(company);
                await _context.SaveChangesAsync();
            }

            return View(company);
        }

        // POST: /Company/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Company model)
        {
            if (ModelState.IsValid)
            {
                var company = await _context.Companies.FirstOrDefaultAsync(c => c.CompanyId == model.CompanyId);
                if (company == null)
                {
                    return NotFound();
                }

                company.CompanyName = model.CompanyName;
                company.CompanyCode = model.CompanyCode;
                company.Email = model.Email;
                company.Phone = model.Phone;
                company.Website = model.Website;
                company.AddressLine1 = model.AddressLine1;
                company.AddressLine2 = model.AddressLine2;
                company.City = model.City;
                company.State = model.State;
                company.Country = model.Country;
                company.PostalCode = model.PostalCode;
                company.Currency = model.Currency;
                company.TimeZone = model.TimeZone;
                company.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Company profile updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Failed to update company profile. Invalid form inputs.";
            return View("Index", model);
        }

        // GET: /Company/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Company());
        }

        // POST: /Company/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Company model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedAt = DateTime.UtcNow;
                model.IsActive = true;
                
                _context.Companies.Add(model);
                await _context.SaveChangesAsync();

                // Log activity
                _context.ActivityLogs.Add(new ActivityLog
                {
                    Title = "Company Registered",
                    Description = $"Company '{model.CompanyName}' ({model.CompanyCode}) was successfully registered.",
                    IconClass = "fa-building",
                    ColorClass = "bg-success text-white",
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Company '{model.CompanyName}' registered successfully.";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Failed to register company. Please verify the input values.";
            return View(model);
        }
    }
}
