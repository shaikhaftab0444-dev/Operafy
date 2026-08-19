using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using ERP_System.Data;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,Inventory Manager,Sales Manager,Manager")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Products
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Branch)
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            ViewBag.BranchesList = await _context.Branches.Where(b => b.IsActive).ToListAsync();
            
            // Generate list of distinct categories plus default ones
            var categories = new List<string> { "Electronics", "Office Supplies", "Hardware", "General" };
            var dbCategories = await _context.Products.Select(p => p.Category).Distinct().ToListAsync();
            foreach (var cat in dbCategories)
            {
                if (!categories.Contains(cat)) categories.Add(cat);
            }
            ViewBag.CategoriesList = categories;

            return View(products);
        }

        // GET: /Products/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.BranchesList = await _context.Branches.Where(b => b.IsActive).ToListAsync();
            return View(new Product());
        }

        // POST: /Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product model)
        {
            if (ModelState.IsValid)
            {
                // Dynamic product status based on stock level
                if (model.StockQty == 0)
                    model.Status = "Out of Stock";
                else if (model.StockQty <= 50)
                    model.Status = "Low Stock";
                else
                    model.Status = "In Stock";

                _context.Products.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Product '{model.ProductName}' created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.BranchesList = await _context.Branches.Where(b => b.IsActive).ToListAsync();
            return View(model);
        }

        // GET: /Products/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.BranchesList = await _context.Branches.Where(b => b.IsActive).ToListAsync();
            return View(product);
        }

        // POST: /Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product model)
        {
            if (id != model.ProductId)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                // Dynamic product status based on stock level
                if (model.StockQty == 0)
                    model.Status = "Out of Stock";
                else if (model.StockQty <= 50)
                    model.Status = "Low Stock";
                else
                    model.Status = "In Stock";

                _context.Entry(model).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Product '{model.ProductName}' updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.BranchesList = await _context.Branches.Where(b => b.IsActive).ToListAsync();
            return View(model);
        }

        // POST: /Products/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Product '{product.ProductName}' deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
