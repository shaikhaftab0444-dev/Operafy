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
    [Authorize(Roles = "Super Admin,Admin,Inventory Manager,Purchase Manager,Manager")]
    public class SuppliersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SuppliersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Suppliers
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var suppliers = await _context.Suppliers
                .OrderBy(s => s.SupplierName)
                .ToListAsync();

            return View(suppliers);
        }

        // GET: /Suppliers/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Supplier { SupplierCode = "SUP-" + new Random().Next(1000, 9999) });
        }

        // POST: /Suppliers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier model)
        {
            if (ModelState.IsValid)
            {
                var codeExists = await _context.Suppliers.AnyAsync(s => s.SupplierCode == model.SupplierCode);
                if (codeExists)
                {
                    ModelState.AddModelError("SupplierCode", "This Supplier Code already exists.");
                    return View(model);
                }

                model.CreatedAt = DateTime.UtcNow;
                model.IsActive = true;

                _context.Suppliers.Add(model);
                await _context.SaveChangesAsync();

                // Log activity
                _context.ActivityLogs.Add(new ActivityLog
                {
                    Title = "Supplier Registered",
                    Description = $"Vendor '{model.SupplierName}' ({model.SupplierCode}) was successfully registered.",
                    IconClass = "fa-truck",
                    ColorClass = "bg-primary text-white",
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Supplier '{model.SupplierName}' registered successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: /Suppliers/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
            {
                return NotFound();
            }

            return View(supplier);
        }

        // POST: /Suppliers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Supplier model)
        {
            if (id != model.SupplierId)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Suppliers.FindAsync(id);
                    if (existing == null) return NotFound();

                    existing.SupplierName = model.SupplierName;
                    existing.SupplierCode = model.SupplierCode;
                    existing.ContactPerson = model.ContactPerson;
                    existing.Designation = model.Designation;
                    existing.Email = model.Email;
                    existing.Phone = model.Phone;
                    existing.Mobile = model.Mobile;
                    existing.AlternateMobile = model.AlternateMobile;
                    existing.GSTIN = model.GSTIN;
                    existing.PANNumber = model.PANNumber;
                    existing.Website = model.Website;
                    existing.AddressLine1 = model.AddressLine1;
                    existing.AddressLine2 = model.AddressLine2;
                    existing.City = model.City;
                    existing.State = model.State;
                    existing.Country = model.Country;
                    existing.PostalCode = model.PostalCode;
                    existing.BankName = model.BankName;
                    existing.AccountNumber = model.AccountNumber;
                    existing.IFSCCode = model.IFSCCode;
                    existing.UPIId = model.UPIId;
                    existing.CreditLimit = model.CreditLimit;
                    existing.OpeningBalance = model.OpeningBalance;
                    existing.PaymentTerms = model.PaymentTerms;
                    existing.WebsiteRemarks = model.WebsiteRemarks;
                    existing.IsActive = model.IsActive;
                    existing.UpdatedAt = DateTime.UtcNow;

                    _context.Suppliers.Update(existing);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Supplier '{model.SupplierName}' updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Suppliers.Any(s => s.SupplierId == id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // POST: /Suppliers/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
            {
                return NotFound();
            }

            supplier.IsActive = !supplier.IsActive;
            supplier.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Supplier status updated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
