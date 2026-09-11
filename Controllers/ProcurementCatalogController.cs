using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Data;
using ERP_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,Purchase Manager")]
    [Route("[controller]/[action]")]
    [Route("Admin/[controller]/[action]")]
    [Route("[controller]")]
    [Route("Admin/[controller]")]
    public class ProcurementCatalogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProcurementCatalogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /ProcurementCatalog or /Admin/ProcurementCatalog
        [HttpGet]
        public async Task<IActionResult> Index(int? departmentId, string? search)
        {
            var query = _context.ProcurementCatalogItems
                .Include(p => p.Department)
                .AsQueryable();

            if (departmentId.HasValue && departmentId.Value > 0)
            {
                query = query.Where(p => p.DepartmentId == departmentId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(p => p.ItemName.ToLower().Contains(s) || 
                                         (p.Department != null && p.Department.DepartmentName.ToLower().Contains(s)));
            }

            var items = await query.OrderBy(p => p.DepartmentId).ThenBy(p => p.ItemName).ToListAsync();
            var departments = await _context.Departments.OrderBy(d => d.DepartmentName).ToListAsync();

            ViewBag.Departments = departments;
            ViewBag.SelectedDepartmentId = departmentId;
            ViewBag.SearchTerm = search;

            return View(items);
        }

        // POST: /ProcurementCatalog/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProcurementCatalogItem item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.ItemName) || item.DepartmentId <= 0)
            {
                TempData["ErrorMessage"] = "Item name and department are required.";
                return RedirectToAction(nameof(Index));
            }

            item.IsActive = true;
            _context.ProcurementCatalogItems.Add(item);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Catalog item '{item.ItemName}' created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /ProcurementCatalog/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProcurementCatalogItem item)
        {
            var existing = await _context.ProcurementCatalogItems.FindAsync(item.Id);
            if (existing == null)
            {
                TempData["ErrorMessage"] = "Item not found.";
                return RedirectToAction(nameof(Index));
            }

            existing.ItemName = item.ItemName;
            existing.DepartmentId = item.DepartmentId;
            existing.UnitPrice = item.UnitPrice;
            existing.UnitOfMeasure = !string.IsNullOrWhiteSpace(item.UnitOfMeasure) ? item.UnitOfMeasure : "Unit";
            existing.IsActive = item.IsActive;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Catalog item '{existing.ItemName}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /ProcurementCatalog/ToggleStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var existing = await _context.ProcurementCatalogItems.FindAsync(id);
            if (existing != null)
            {
                existing.IsActive = !existing.IsActive;
                await _context.SaveChangesAsync();
                return Json(new { success = true, isActive = existing.IsActive, message = $"Status changed to {(existing.IsActive ? "Active" : "Inactive")}." });
            }
            return Json(new { success = false, message = "Item not found." });
        }

        // POST: /ProcurementCatalog/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _context.ProcurementCatalogItems.FindAsync(id);
            if (existing != null)
            {
                _context.ProcurementCatalogItems.Remove(existing);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Catalog item '{existing.ItemName}' removed.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
