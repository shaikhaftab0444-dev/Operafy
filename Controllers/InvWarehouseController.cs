using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,Inventory Manager")]
    public class InvWarehouseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvWarehouseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /InvWarehouse/Locations
        [HttpGet]
        public async Task<IActionResult> Locations()
        {
            var warehouses = await _context.InvWarehouses.ToListAsync();
            return View(warehouses);
        }

        // POST: /InvWarehouse/CreateLocation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLocation(InvWarehouse warehouse)
        {
            if (ModelState.IsValid || (warehouse.Code != null && warehouse.Name != null))
            {
                warehouse.IsActive = true;
                _context.InvWarehouses.Add(warehouse);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Locations));
            }
            var warehouses = await _context.InvWarehouses.ToListAsync();
            return View(nameof(Locations), warehouses);
        }

        // GET: /InvWarehouse/Bins
        [HttpGet]
        public IActionResult Bins()
        {
            return View();
        }

        // GET: /InvWarehouse/Grn
        [HttpGet]
        public async Task<IActionResult> Grn()
        {
            var grns = await _context.InvGrns.OrderByDescending(g => g.GrnId).ToListAsync();
            return View(grns);
        }

        // POST: /InvWarehouse/CreateGrn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGrn(InvGrn grn)
        {
            if (ModelState.IsValid || grn.SupplierName != null)
            {
                grn.ReceivedDate = DateTime.Now;
                grn.Status = "Completed";
                // Generate GRN No
                int count = await _context.InvGrns.CountAsync();
                grn.GrnNo = $"GRN-2026-{(count + 1).ToString("D4")}";

                _context.InvGrns.Add(grn);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Grn));
            }
            var grns = await _context.InvGrns.OrderByDescending(g => g.GrnId).ToListAsync();
            return View(nameof(Grn), grns);
        }

        // GET: /InvWarehouse/Dispatch
        [HttpGet]
        public IActionResult Dispatch()
        {
            return View();
        }

        // GET: /InvWarehouse/Transfers
        [HttpGet]
        public async Task<IActionResult> Transfers()
        {
            var transfers = await _context.InvTransfers.OrderByDescending(t => t.TransferId).ToListAsync();
            ViewBag.Warehouses = await _context.InvWarehouses.Where(w => w.IsActive).ToListAsync();
            return View(transfers);
        }

        // POST: /InvWarehouse/CreateTransfer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTransfer(InvTransfer transfer)
        {
            if (ModelState.IsValid || (transfer.FromWarehouse != null && transfer.ToWarehouse != null))
            {
                transfer.TransferDate = DateTime.Now;
                transfer.Status = "Transferred";
                // Generate Transfer No
                int count = await _context.InvTransfers.CountAsync();
                transfer.TransferNo = $"TR-900{(count + 81).ToString("D2")}";

                _context.InvTransfers.Add(transfer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Transfers));
            }
            var transfers = await _context.InvTransfers.OrderByDescending(t => t.TransferId).ToListAsync();
            ViewBag.Warehouses = await _context.InvWarehouses.Where(w => w.IsActive).ToListAsync();
            return View(nameof(Transfers), transfers);
        }

        // GET: /InvWarehouse/Sto
        [HttpGet]
        public IActionResult Sto()
        {
            return View();
        }
    }
}
