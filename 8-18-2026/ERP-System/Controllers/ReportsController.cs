using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using System.Linq;
using System.Threading.Tasks;
using ERP_System.Data;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,HR,Manager,Accountant,Finance Manager,Sales Manager,Auditor")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Reports
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var salesTransactions = await _context.Transactions
                .Where(t => t.Type == "Sales Invoice")
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            var purchaseTransactions = await _context.Transactions
                .Where(t => t.Type == "Purchase Order")
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            var products = await _context.Products
                .OrderByDescending(p => p.SoldQty)
                .ToListAsync();

            var totalSales = salesTransactions.Sum(t => t.Amount);
            var totalPurchase = purchaseTransactions.Sum(t => t.Amount);
            var totalProducts = products.Count;
            var totalStockQty = products.Sum(p => p.StockQty);

            ViewBag.SalesTransactions = salesTransactions;
            ViewBag.PurchaseTransactions = purchaseTransactions;
            ViewBag.ProductsList = products;
            ViewBag.TotalSales = totalSales;
            ViewBag.TotalPurchase = totalPurchase;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalStockQty = totalStockQty;

            return View();
        }
    }
}
