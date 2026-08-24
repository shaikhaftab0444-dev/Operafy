using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System.Threading.Tasks;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin")]
    public class SuperAdminSettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SuperAdminSettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /SuperAdminSettings/TaxSlabs
        [HttpGet]
        public IActionResult TaxSlabs()
        {
            return View();
        }

        // GET: /SuperAdminSettings/Uoms
        [HttpGet]
        public IActionResult Uoms()
        {
            return View();
        }

        // GET: /SuperAdminSettings/EmailSettings
        [HttpGet]
        public IActionResult EmailSettings()
        {
            return View();
        }

        // GET: /SuperAdminSettings/SmsSettings
        [HttpGet]
        public async Task<IActionResult> SmsSettings()
        {
            var integrations = await _context.SuperAdminIntegrations.ToListAsync();
            return View(integrations);
        }

        // POST: /SuperAdminSettings/SaveIntegration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveIntegration(SuperAdminIntegration model)
        {
            if (ModelState.IsValid || (model.ProviderName != null && model.ApiKey != null))
            {
                _context.SuperAdminIntegrations.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(SmsSettings));
            }
            var integrations = await _context.SuperAdminIntegrations.ToListAsync();
            return View(nameof(SmsSettings), integrations);
        }

        // GET: /SuperAdminSettings/PaymentSettings
        [HttpGet]
        public IActionResult PaymentSettings()
        {
            return View();
        }
    }
}
