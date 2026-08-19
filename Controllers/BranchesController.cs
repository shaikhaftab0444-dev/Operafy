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
    [Authorize(Roles = "Super Admin,Admin,HR,Manager")]
    public class BranchesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BranchesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper to resolve company context from cookie or user claims
        private int GetActiveCompanyId()
        {
            int companyId = 1;
            var companyClaim = User.FindFirst("Company")?.Value;
            if (int.TryParse(companyClaim, out int cId))
            {
                companyId = cId;
            }

            if ((User.IsInRole("Super Admin") || User.IsInRole("Admin")) &&
                Request.Cookies.TryGetValue("ActiveCompanyId", out string? cookieCompanyId) &&
                int.TryParse(cookieCompanyId, out int ccId))
            {
                companyId = ccId;
            }

            return companyId;
        }

        // GET: /Branches
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int companyId = GetActiveCompanyId();
            var company = await _context.Companies.FindAsync(companyId);
            
            var branches = await _context.Branches
                .Include(b => b.Company)
                .Where(b => b.CompanyId == companyId)
                .OrderBy(b => b.BranchName)
                .ToListAsync();

            ViewBag.ActiveCompany = company;
            return View(branches);
        }

        // GET: /Branches/Create
        [HttpGet]
        [Authorize(Roles = "Super Admin,Admin")]
        public IActionResult Create()
        {
            int companyId = GetActiveCompanyId();
            return View(new Branch { CompanyId = companyId });
        }

        // POST: /Branches/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Super Admin,Admin")]
        public async Task<IActionResult> Create(Branch model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedAt = DateTime.UtcNow;
                _context.Branches.Add(model);
                await _context.SaveChangesAsync();

                // Log activity
                _context.ActivityLogs.Add(new ActivityLog
                {
                    Title = "Branch Registered",
                    Description = $"New branch '{model.BranchName}' ({model.BranchCode}) was registered.",
                    IconClass = "fa-building",
                    ColorClass = "bg-primary text-white",
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Branch '{model.BranchName}' created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: /Branches/Edit/5
        [HttpGet]
        [Authorize(Roles = "Super Admin,Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null)
            {
                return NotFound();
            }
            return View(branch);
        }

        // POST: /Branches/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Super Admin,Admin")]
        public async Task<IActionResult> Edit(int id, Branch model)
        {
            if (id != model.BranchId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Branches.FindAsync(id);
                    if (existing == null) return NotFound();

                    existing.BranchName = model.BranchName;
                    existing.BranchCode = model.BranchCode;
                    existing.Email = model.Email;
                    existing.Phone = model.Phone;
                    existing.Mobile = model.Mobile;
                    existing.GSTNumber = model.GSTNumber;
                    existing.AddressLine1 = model.AddressLine1;
                    existing.AddressLine2 = model.AddressLine2;
                    existing.City = model.City;
                    existing.State = model.State;
                    existing.Country = model.Country;
                    existing.PostalCode = model.PostalCode;
                    existing.IsActive = model.IsActive;
                    existing.UpdatedAt = DateTime.UtcNow;

                    _context.Branches.Update(existing);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Branch '{model.BranchName}' updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Branches.Any(e => e.BranchId == id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // POST: /Branches/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Super Admin,Admin")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null)
            {
                return NotFound();
            }

            branch.IsActive = !branch.IsActive;
            branch.UpdatedAt = DateTime.UtcNow;
            _context.Branches.Update(branch);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Branch status toggled successfully. Current status is {(branch.IsActive ? "Active" : "Inactive")}.";
            return RedirectToAction(nameof(Index));
        }
    }
}
