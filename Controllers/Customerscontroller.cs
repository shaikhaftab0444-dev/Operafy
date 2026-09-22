using ERP_System.Data;
using ERP_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,Sales Executive,Sales Manager,Employee")]
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
            var query = _context.Customers.Include(c => c.AssignedRepUser).AsQueryable();

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? currentUid = int.TryParse(currentUserId, out int uid) ? uid : (int?)null;
            bool isSalesExecutive = User.IsInRole("Sales Executive");

            // Strict Data Isolation for Sales Executive
            if (isSalesExecutive)
            {
                query = query.Where(c => c.AssignedRepId == currentUserId || (currentUid.HasValue && c.AssignedRepUserUserId == currentUid.Value));
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(c =>
                    (c.CustomerName != null && c.CustomerName.ToLower().Contains(term)) ||
                    (c.FirstName != null && c.FirstName.ToLower().Contains(term)) ||
                    (c.LastName != null && c.LastName.ToLower().Contains(term)) ||
                    (c.CompanyName != null && c.CompanyName.ToLower().Contains(term)) ||
                    (c.CustomerCode != null && c.CustomerCode.ToLower().Contains(term)) ||
                    (c.TaxIdOrGSTIN != null && c.TaxIdOrGSTIN.ToLower().Contains(term)) ||
                    (c.Email != null && c.Email.ToLower().Contains(term)) ||
                    (c.PhoneNumber != null && c.PhoneNumber.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
            {
                bool activeVal = statusFilter == "Active";
                query = query.Where(c => c.IsActive == activeVal);
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.StatusFilter = statusFilter ?? "All";

            if (isSalesExecutive)
            {
                ViewBag.TotalCustomers = await _context.Customers.CountAsync(c => c.AssignedRepId == currentUserId || (currentUid.HasValue && c.AssignedRepUserUserId == currentUid.Value));
                ViewBag.ActiveCustomers = await _context.Customers.CountAsync(c => c.IsActive && (c.AssignedRepId == currentUserId || (currentUid.HasValue && c.AssignedRepUserUserId == currentUid.Value)));
                ViewBag.BusinessCustomers = await _context.Customers.CountAsync(c => !string.IsNullOrEmpty(c.CompanyName) && (c.AssignedRepId == currentUserId || (currentUid.HasValue && c.AssignedRepUserUserId == currentUid.Value)));
            }
            else
            {
                ViewBag.TotalCustomers = await _context.Customers.CountAsync();
                ViewBag.ActiveCustomers = await _context.Customers.CountAsync(c => c.IsActive);
                ViewBag.BusinessCustomers = await _context.Customers.CountAsync(c => !string.IsNullOrEmpty(c.CompanyName));
            }

            ViewBag.Users = await _context.Users.OrderBy(u => u.FullName).ToListAsync();

            var customers = await query.OrderByDescending(c => c.Id).ToListAsync();
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
            if (string.IsNullOrWhiteSpace(customer.FirstName) && !string.IsNullOrWhiteSpace(customer.CompanyName))
            {
                customer.CustomerName = customer.CompanyName;
            }

            if (string.IsNullOrWhiteSpace(customer.CustomerCode))
            {
                var nextNum = (await _context.Customers.CountAsync()) + 1;
                customer.CustomerCode = $"CUST#{nextNum:D4}";
            }

            if (customer.CreditLimit <= 0)
            {
                customer.CreditLimit = 500000m;
            }

            if (await _context.Customers.AnyAsync(c => c.Email == customer.Email))
            {
                ModelState.AddModelError(nameof(customer.Email), "A customer with this email already exists.");
            }

            if (ModelState.IsValid)
            {
                customer.JoinedDate = DateTime.UtcNow;
                customer.CreatedAt = DateTime.UtcNow;
                _context.Add(customer);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Customer '{customer.CustomerName}' created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Users = await _context.Users.OrderBy(u => u.FullName).ToListAsync();
            return View(customer);
        }

        // GET: /Customers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            ViewBag.Users = await _context.Users.OrderBy(u => u.FullName).ToListAsync();
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

                    existing.CustomerCode = customer.CustomerCode;
                    existing.CustomerName = !string.IsNullOrWhiteSpace(customer.CustomerName) ? customer.CustomerName : $"{customer.FirstName} {customer.LastName}".Trim();
                    existing.CompanyName = customer.CompanyName;
                    existing.FirstName = customer.FirstName;
                    existing.LastName = customer.LastName;
                    existing.Email = customer.Email;
                    existing.PhoneNumber = customer.PhoneNumber;
                    existing.TaxIdOrGSTIN = customer.TaxIdOrGSTIN;
                    existing.CreditLimit = customer.CreditLimit;
                    existing.AssignedRepUserUserId = customer.AssignedRepUserUserId;
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

            ViewBag.Users = await _context.Users.OrderBy(u => u.FullName).ToListAsync();
            return View(customer);
        }

        // GET: /Customers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (User.IsInRole("Sales Executive")) return Forbid();
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
            if (User.IsInRole("Sales Executive")) return Forbid();

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