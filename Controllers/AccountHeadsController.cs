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
    [Authorize(Roles = "Super Admin,Admin")]
    public class AccountHeadsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountHeadsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /AccountHeads
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var heads = await _context.AccountHeads
                .OrderBy(a => a.AccountType)
                .ThenBy(a => a.HeadCode)
                .ToListAsync();

            return View(heads);
        }

        // GET: /AccountHeads/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new AccountHead());
        }

        // POST: /AccountHeads/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AccountHead model)
        {
            if (ModelState.IsValid)
            {
                // Verify if code or name already exists to prevent duplicate entries
                var codeExists = await _context.AccountHeads.AnyAsync(a => a.HeadCode == model.HeadCode);
                if (codeExists)
                {
                    ModelState.AddModelError("HeadCode", "This Account Head Code is already registered.");
                    return View(model);
                }

                model.CreatedAt = DateTime.UtcNow;
                model.IsActive = true;
                _context.AccountHeads.Add(model);
                await _context.SaveChangesAsync();

                // Log activity
                _context.ActivityLogs.Add(new ActivityLog
                {
                    Title = "Account Head Created",
                    Description = $"New ledger account head '{model.HeadName}' ({model.HeadCode}) was registered under {model.AccountType}.",
                    IconClass = "fa-coins",
                    ColorClass = "bg-info text-white",
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Account Head '{model.HeadName}' created successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // POST: /AccountHeads/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var head = await _context.AccountHeads.FindAsync(id);
            if (head == null)
            {
                return NotFound();
            }

            head.IsActive = !head.IsActive;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Ledger Account Head status toggled successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
