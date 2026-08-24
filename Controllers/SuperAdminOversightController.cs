using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System.Threading.Tasks;
using System.Linq;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin")]
    public class SuperAdminOversightController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SuperAdminOversightController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /SuperAdminOversight/HrPayroll
        [HttpGet]
        public async Task<IActionResult> HrPayroll()
        {
            var employees = await _context.Users.Include(u => u.Role).Include(u => u.Branch).ToListAsync();
            return View(employees);
        }

        // GET: /SuperAdminOversight/SupplyChain
        [HttpGet]
        public async Task<IActionResult> SupplyChain()
        {
            var overrides = await _context.SuperAdminPriceOverrides.Include(o => o.Product).ToListAsync();
            ViewBag.Products = await _context.Products.ToListAsync();
            return View(overrides);
        }

        // POST: /SuperAdminOversight/CreateOverride
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOverride(SuperAdminPriceOverride model)
        {
            if (ModelState.IsValid || model.VendorName != null)
            {
                model.ApprovedBy = "Super Admin";
                _context.SuperAdminPriceOverrides.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(SupplyChain));
            }
            var overrides = await _context.SuperAdminPriceOverrides.Include(o => o.Product).ToListAsync();
            ViewBag.Products = await _context.Products.ToListAsync();
            return View(nameof(SupplyChain), overrides);
        }

        // GET: /SuperAdminOversight/Inventory
        [HttpGet]
        public async Task<IActionResult> Inventory()
        {
            var products = await _context.Products.ToListAsync();
            return View(products);
        }

        // GET: /SuperAdminOversight/SalesCrm
        [HttpGet]
        public async Task<IActionResult> SalesCrm()
        {
            var leads = await _context.Leads.ToListAsync();
            return View(leads);
        }

        // GET: /SuperAdminOversight/Finance
        [HttpGet]
        public IActionResult Finance()
        {
            return View();
        }
    }
}
