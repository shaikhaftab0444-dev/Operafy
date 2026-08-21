using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System.Threading.Tasks;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,Inventory Manager")]
    public class InvCatalogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvCatalogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /InvCatalog/Items
        [HttpGet]
        public async Task<IActionResult> Items()
        {
            var products = await _context.Products.Include(p => p.Branch).ToListAsync();
            return View(products);
        }

        // GET: /InvCatalog/Categories
        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            var products = await _context.Products.ToListAsync();
            return View(products);
        }

        // GET: /InvCatalog/Uom
        [HttpGet]
        public IActionResult Uom()
        {
            return View();
        }

        // GET: /InvCatalog/Batches
        [HttpGet]
        public IActionResult Batches()
        {
            return View();
        }

        // GET: /InvCatalog/Barcodes
        [HttpGet]
        public async Task<IActionResult> Barcodes()
        {
            var products = await _context.Products.ToListAsync();
            return View(products);
        }

        // GET: /InvCatalog/Serials
        [HttpGet]
        public IActionResult Serials()
        {
            return View();
        }
    }
}
