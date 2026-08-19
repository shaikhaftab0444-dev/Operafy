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
    [Authorize(Roles = "Super Admin,Admin,Sales Executive,Sales Manager")]
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Customers
        public async Task<IActionResult> Index(string? searchTerm, string? statusFilter)
        {
            var query = _context.Customers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(c =>
                    c.FirstName.ToLower().Contains(term) ||
                    c.LastName.ToLower().Contains(term) ||
                    c.Email.ToLower().Contains(term) ||
                    c.PhoneNumber.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
            {
                bool activeVal = statusFilter == "Active";
                query = query.Where(c => c.IsActive == activeVal);
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.StatusFilter = statusFilter ?? "All";
            ViewBag.TotalCustomers = await _context.Customers.CountAsync();
            ViewBag.ActiveCustomers = await _context.Customers.CountAsync(c => c.IsActive);
            ViewBag.BusinessCustomers = 0; // Mocked as CustomerType is not in db

            var customers = await query.OrderByDescending(c => c.JoinedDate).ToListAsync();
            return View(customers);
        }

        // GET: /Customers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
            if (customer == null) return NotFound();

            return View(customer);
        }

        // GET: /Customers/Create
        public IActionResult Create()
        {
            return View(new Customer());
        }

        // POST: /Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (await _context.Customers.AnyAsync(c => c.Email == customer.Email))
            {
                ModelState.AddModelError(nameof(customer.Email), "A customer with this email already exists.");
            }

            if (ModelState.IsValid)
            {
                customer.JoinedDate = DateTime.UtcNow;
                _context.Add(customer);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Customer created successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(customer);
        }

        // GET: /Customers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            return View(customer);
        }

        // POST: /Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            if (id != customer.Id) return NotFound();

            if (await _context.Customers.AnyAsync(c => c.Email == customer.Email && c.Id != id))
            {
                ModelState.AddModelError(nameof(customer.Email), "Another customer already uses this email.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Customers.FindAsync(id);
                    if (existing == null) return NotFound();

                    existing.FirstName = customer.FirstName;
                    existing.LastName = customer.LastName;
                    existing.Email = customer.Email;
                    existing.PhoneNumber = customer.PhoneNumber;
                    existing.DateOfBirth = customer.DateOfBirth;
                    existing.IsActive = customer.IsActive;
                    
                    if (!string.IsNullOrEmpty(customer.Password) && customer.Password != "Default@123")
                    {
                        existing.Password = customer.Password;
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Customer updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Customers.AnyAsync(c => c.Id == id)) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(customer);
        }

        // GET: /Customers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
            if (customer == null) return NotFound();

            return View(customer);
        }

        // POST: /Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Customer deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}