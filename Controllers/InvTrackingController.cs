using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System.Threading.Tasks;
using System.Linq;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,Inventory Manager")]
    public class InvTrackingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvTrackingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /InvTracking/LiveStock
        [HttpGet]
        public async Task<IActionResult> LiveStock()
        {
            var products = await _context.Products.Include(p => p.Branch).ToListAsync();
            return View(products);
        }

        // GET: /InvTracking/Alerts
        [HttpGet]
        public async Task<IActionResult> Alerts()
        {
            var lowStockProducts = await _context.Products
                .Where(p => p.StockQty <= 30 || p.Status == "Low Stock" || p.Status == "Out of Stock")
                .ToListAsync();
            return View(lowStockProducts);
        }

        // GET: /InvTracking/ExpiredSlow
        [HttpGet]
        public async Task<IActionResult> ExpiredSlow()
        {
            var slowProducts = await _context.Products
                .Where(p => p.SoldQty < 5)
                .ToListAsync();
            return View(slowProducts);
        }

        // GET: /InvTracking/BlockedReserved
        [HttpGet]
        public IActionResult BlockedReserved()
        {
            return View();
        }
    }
}
