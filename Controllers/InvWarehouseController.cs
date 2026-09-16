using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;

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

        #region Warehouses & Locations

        // GET: /InvWarehouse/Locations
        [HttpGet]
        public async Task<IActionResult> Locations()
        {
            var warehouses = await _context.WarehouseLocations
                .Include(w => w.Supervisor)
                .Include(w => w.StoragePositions)
                .OrderBy(w => w.WarehouseCode)
                .ToListAsync();

            ViewBag.Supervisors = await _context.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.UserName)
                .ToListAsync();

            return View(warehouses);
        }

        // POST: /InvWarehouse/SaveWarehouse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveWarehouse(WarehouseLocation model)
        {
            if (string.IsNullOrWhiteSpace(model.WarehouseCode) || string.IsNullOrWhiteSpace(model.WarehouseName))
            {
                TempData["ErrorMessage"] = "Warehouse Code and Name are required.";
                return RedirectToAction(nameof(Locations));
            }

            if (model.Id == 0)
            {
                // Check code uniqueness
                bool exists = await _context.WarehouseLocations.AnyAsync(w => w.WarehouseCode.ToUpper() == model.WarehouseCode.ToUpper());
                if (exists)
                {
                    TempData["ErrorMessage"] = $"Warehouse code '{model.WarehouseCode}' already exists.";
                    return RedirectToAction(nameof(Locations));
                }

                if (model.SupervisorId.HasValue)
                {
                    var sup = await _context.Users.FindAsync(model.SupervisorId.Value);
                    model.SupervisorUserId = sup?.UserName ?? sup?.FullName;
                }

                model.IsActive = true;
                _context.WarehouseLocations.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Warehouse '{model.WarehouseName}' registered successfully.";
            }
            else
            {
                var existing = await _context.WarehouseLocations.FindAsync(model.Id);
                if (existing != null)
                {
                    existing.WarehouseName = model.WarehouseName;
                    existing.WarehouseType = model.WarehouseType ?? "Finished Goods";
                    existing.LocationAddress = model.LocationAddress;
                    existing.MaxCapacityUnits = model.MaxCapacityUnits > 0 ? model.MaxCapacityUnits : 5000;
                    existing.IsActive = model.IsActive;
                    existing.SupervisorId = model.SupervisorId;

                    if (model.SupervisorId.HasValue)
                    {
                        var sup = await _context.Users.FindAsync(model.SupervisorId.Value);
                        existing.SupervisorUserId = sup?.UserName ?? sup?.FullName;
                    }
                    else
                    {
                        existing.SupervisorUserId = null;
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Warehouse '{existing.WarehouseName}' updated successfully.";
                }
            }

            return RedirectToAction(nameof(Locations));
        }

        // Legacy compatibility alias
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLocation(InvWarehouse warehouse)
        {
            if (warehouse != null && !string.IsNullOrEmpty(warehouse.Code) && !string.IsNullOrEmpty(warehouse.Name))
            {
                warehouse.IsActive = true;
                _context.InvWarehouses.Add(warehouse);

                // Also sync to modern WarehouseLocation
                var modernWh = new WarehouseLocation
                {
                    WarehouseCode = warehouse.Code,
                    WarehouseName = warehouse.Name,
                    LocationAddress = warehouse.Location ?? "Main Warehouse Site",
                    WarehouseType = "Finished Goods",
                    MaxCapacityUnits = 5000,
                    IsActive = true
                };
                _context.WarehouseLocations.Add(modernWh);

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Locations));
        }

        #endregion

        #region Bin & Rack Master

        // GET: /InvWarehouse/Bins
        [HttpGet]
        public async Task<IActionResult> Bins(int? warehouseId, string? status)
        {
            var query = _context.BinRackMasters
                .Include(b => b.Warehouse)
                .Include(b => b.AssignedItem)
                .AsQueryable();

            if (warehouseId.HasValue && warehouseId.Value > 0)
            {
                query = query.Where(b => b.WarehouseId == warehouseId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(b => b.Status == status);
            }

            var bins = await query.OrderBy(b => b.RackCode).ThenBy(b => b.BinLevel).ToListAsync();

            ViewBag.Warehouses = await _context.WarehouseLocations
                .Where(w => w.IsActive)
                .OrderBy(w => w.WarehouseName)
                .ToListAsync();

            ViewBag.CatalogItems = await _context.CatalogItems
                .Where(c => c.IsActive)
                .OrderBy(c => c.ItemName)
                .ToListAsync();

            ViewBag.SelectedWarehouseId = warehouseId;
            ViewBag.SelectedStatus = status;

            // Summary metrics
            ViewBag.TotalBins = await _context.BinRackMasters.CountAsync();
            ViewBag.AvailableBins = await _context.BinRackMasters.CountAsync(b => b.Status == "Available");
            ViewBag.FullBins = await _context.BinRackMasters.CountAsync(b => b.Status == "Full");
            ViewBag.MaintenanceBins = await _context.BinRackMasters.CountAsync(b => b.Status == "Maintenance");
            ViewBag.TotalVolumeCapacity = await _context.BinRackMasters.SumAsync(b => b.MaxCapacityVolume);
            ViewBag.TotalVolumeOccupied = await _context.BinRackMasters.SumAsync(b => b.CurrentOccupancyVolume);

            return View(bins);
        }

        // POST: /InvWarehouse/SaveBin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveBin(BinRackMaster model)
        {
            if (model.WarehouseId <= 0 || string.IsNullOrWhiteSpace(model.RackCode) || string.IsNullOrWhiteSpace(model.BinLevel))
            {
                TempData["ErrorMessage"] = "Warehouse, Rack Code, and Bin Level are required.";
                return RedirectToAction(nameof(Bins));
            }

            if (model.MaxCapacityVolume <= 0) model.MaxCapacityVolume = 500;

            if (model.Id == 0)
            {
                if (model.CurrentOccupancyVolume >= model.MaxCapacityVolume)
                {
                    model.Status = "Full";
                }
                else if (string.IsNullOrWhiteSpace(model.Status))
                {
                    model.Status = "Available";
                }

                _context.BinRackMasters.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Bin position '{model.RackCode} / {model.BinLevel}' created successfully.";
            }
            else
            {
                var existing = await _context.BinRackMasters.FindAsync(model.Id);
                if (existing != null)
                {
                    existing.WarehouseId = model.WarehouseId;
                    existing.RackCode = model.RackCode;
                    existing.BinLevel = model.BinLevel;
                    existing.AssignedCatalogItemId = model.AssignedCatalogItemId > 0 ? model.AssignedCatalogItemId : null;
                    existing.MaxCapacityVolume = model.MaxCapacityVolume;
                    existing.CurrentOccupancyVolume = model.CurrentOccupancyVolume;
                    existing.Status = model.Status ?? (existing.CurrentOccupancyVolume >= existing.MaxCapacityVolume ? "Full" : "Available");

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Bin position '{existing.RackCode} / {existing.BinLevel}' updated successfully.";
                }
            }

            return RedirectToAction(nameof(Bins), new { warehouseId = model.WarehouseId });
        }

        // GET: /InvWarehouse/PrintBinBarcode
        [HttpGet]
        public async Task<IActionResult> PrintBinBarcode(int id)
        {
            var bin = await _context.BinRackMasters
                .Include(b => b.Warehouse)
                .Include(b => b.AssignedItem)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bin == null)
            {
                return NotFound();
            }

            return View("PrintBinBarcode", bin);
        }

        #endregion

        #region Goods Receipt Note (GRN - Inward)

        // GET: /InvWarehouse/Grn
        [HttpGet]
        public async Task<IActionResult> Grn(int? warehouseId, string? status)
        {
            var query = _context.GoodsReceiptNotes
                .Include(g => g.DestinationWarehouse)
                .Include(g => g.ReceivedByUser)
                .Include(g => g.LineItems)
                    .ThenInclude(li => li.CatalogItem)
                .Include(g => g.LineItems)
                    .ThenInclude(li => li.TargetBin)
                .AsQueryable();

            if (warehouseId.HasValue && warehouseId.Value > 0)
            {
                query = query.Where(g => g.DestinationWarehouseId == warehouseId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(g => g.Status == status);
            }

            var grns = await query.OrderByDescending(g => g.ReceivedDate).ToListAsync();

            ViewBag.Warehouses = await _context.WarehouseLocations
                .Where(w => w.IsActive)
                .OrderBy(w => w.WarehouseName)
                .ToListAsync();

            ViewBag.CatalogItems = await _context.CatalogItems
                .Where(c => c.IsActive)
                .OrderBy(c => c.ItemName)
                .ToListAsync();

            ViewBag.Bins = await _context.BinRackMasters
                .Include(b => b.Warehouse)
                .Where(b => b.Status != "Maintenance")
                .OrderBy(b => b.RackCode)
                .ToListAsync();

            ViewBag.SelectedWarehouseId = warehouseId;
            ViewBag.SelectedStatus = status;

            return View(grns);
        }

        // POST: /InvWarehouse/CreateGrn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGrn(GoodsReceiptNote grn, List<GrnLineItem> lineItems)
        {
            if (string.IsNullOrWhiteSpace(grn.SupplierName) || grn.DestinationWarehouseId <= 0)
            {
                TempData["ErrorMessage"] = "Supplier Name and Destination Warehouse are required.";
                return RedirectToAction(nameof(Grn));
            }

            // Generate GRN Number
            int totalGrns = await _context.GoodsReceiptNotes.CountAsync();
            grn.GrnNumber = $"GRN-{DateTime.UtcNow.Year}-{(totalGrns + 1).ToString("D4")}";
            grn.ReceivedDate = DateTime.UtcNow;
            grn.Status = "Pending verification";

            var currentUserName = User.Identity?.Name ?? "Admin";
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == currentUserName);
            grn.ReceivedById = currentUser?.UserId;
            grn.ReceivedByUserId = currentUserName;

            // Process line items
            if (lineItems != null && lineItems.Any())
            {
                foreach (var item in lineItems)
                {
                    if (item.CatalogItemId > 0 && item.ReceivedQuantity > 0)
                    {
                        if (item.OrderedQuantity <= 0) item.OrderedQuantity = item.ReceivedQuantity;
                        grn.LineItems.Add(new GrnLineItem
                        {
                            CatalogItemId = item.CatalogItemId,
                            OrderedQuantity = item.OrderedQuantity,
                            ReceivedQuantity = item.ReceivedQuantity,
                            RejectedQuantity = item.RejectedQuantity,
                            TargetBinId = item.TargetBinId > 0 ? item.TargetBinId : null,
                            BatchNumber = string.IsNullOrWhiteSpace(item.BatchNumber) ? $"BAT-{DateTime.UtcNow:yyyyMMdd}-{item.CatalogItemId}" : item.BatchNumber
                        });
                    }
                }
            }

            _context.GoodsReceiptNotes.Add(grn);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Goods Receipt Note '{grn.GrnNumber}' logged successfully. Awaiting verification.";
            return RedirectToAction(nameof(Grn));
        }

        // POST: /InvWarehouse/VerifyAndAcceptGrn
        [HttpPost]
        public async Task<IActionResult> VerifyAndAcceptGrn(int id)
        {
            var grn = await _context.GoodsReceiptNotes
                .Include(g => g.LineItems)
                .Include(g => g.DestinationWarehouse)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (grn == null)
            {
                return Json(new { success = false, message = "GRN record not found." });
            }

            if (grn.Status == "Completed")
            {
                return Json(new { success = false, message = "This GRN has already been verified and accepted." });
            }

            var currentUserName = User.Identity?.Name ?? "Admin";

            // Mark GRN as Completed
            grn.Status = "Completed";

            // Automated Stock Adjustments & Bin Occupancy Update
            foreach (var line in grn.LineItems)
            {
                if (line.ReceivedQuantity > 0)
                {
                    // 1. Credit CatalogItem stock
                    var item = await _context.CatalogItems.FindAsync(line.CatalogItemId);
                    if (item != null)
                    {
                        item.CurrentStock += line.ReceivedQuantity;
                    }

                    // 2. Adjust target bin occupancy
                    if (line.TargetBinId.HasValue)
                    {
                        var bin = await _context.BinRackMasters.FindAsync(line.TargetBinId.Value);
                        if (bin != null)
                        {
                            bin.CurrentOccupancyVolume += line.ReceivedQuantity;
                            if (bin.CurrentOccupancyVolume >= bin.MaxCapacityVolume)
                            {
                                bin.Status = "Full";
                            }
                        }
                    }

                    // 3. Log inward movement in StockMovementLog
                    _context.StockMovementLogs.Add(new StockMovementLog
                    {
                        InventoryItemId = line.CatalogItemId,
                        MovementType = "INWARD",
                        Quantity = line.ReceivedQuantity,
                        ReferenceDocument = grn.GrnNumber,
                        HandledByUserId = currentUserName,
                        Timestamp = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = $"GRN {grn.GrnNumber} verified and accepted. Inventory quantities successfully credited." });
            }

            TempData["SuccessMessage"] = $"GRN {grn.GrnNumber} verified and accepted. Inventory quantities successfully credited.";
            return RedirectToAction(nameof(Grn));
        }

        // GET: /InvWarehouse/PrintGrn
        [HttpGet]
        public async Task<IActionResult> PrintGrn(int id)
        {
            var grn = await _context.GoodsReceiptNotes
                .Include(g => g.DestinationWarehouse)
                .Include(g => g.ReceivedByUser)
                .Include(g => g.LineItems)
                    .ThenInclude(li => li.CatalogItem)
                .Include(g => g.LineItems)
                    .ThenInclude(li => li.TargetBin)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (grn == null)
            {
                return NotFound();
            }

            return View("PrintGrn", grn);
        }

        #endregion

        #region Material Dispatch (Outward)

        // GET: /InvWarehouse/Dispatch
        [HttpGet]
        public async Task<IActionResult> Dispatch(int? warehouseId, string? status)
        {
            var query = _context.MaterialDispatches
                .Include(d => d.SourceWarehouse)
                .Include(d => d.LineItems)
                    .ThenInclude(li => li.CatalogItem)
                .Include(d => d.LineItems)
                    .ThenInclude(li => li.PickedFromBin)
                .AsQueryable();

            if (warehouseId.HasValue && warehouseId.Value > 0)
            {
                query = query.Where(d => d.SourceWarehouseId == warehouseId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(d => d.Status == status);
            }

            var dispatches = await query.OrderByDescending(d => d.DispatchDate).ToListAsync();

            ViewBag.Warehouses = await _context.WarehouseLocations
                .Where(w => w.IsActive)
                .OrderBy(w => w.WarehouseName)
                .ToListAsync();

            ViewBag.CatalogItems = await _context.CatalogItems
                .Where(c => c.IsActive)
                .OrderBy(c => c.ItemName)
                .ToListAsync();

            ViewBag.Bins = await _context.BinRackMasters
                .Include(b => b.Warehouse)
                .Where(b => b.Status != "Maintenance")
                .OrderBy(b => b.RackCode)
                .ToListAsync();

            ViewBag.SelectedWarehouseId = warehouseId;
            ViewBag.SelectedStatus = status;

            return View(dispatches);
        }

        // POST: /InvWarehouse/CreateDispatch
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDispatch(MaterialDispatch dispatch, List<DispatchLineItem> lineItems)
        {
            if (string.IsNullOrWhiteSpace(dispatch.DestinationParty) || dispatch.SourceWarehouseId <= 0 || string.IsNullOrWhiteSpace(dispatch.CarrierName))
            {
                TempData["ErrorMessage"] = "Destination Party, Source Warehouse, and Carrier Name are required.";
                return RedirectToAction(nameof(Dispatch));
            }

            int count = await _context.MaterialDispatches.CountAsync();
            dispatch.DispatchSlipNo = $"DSP-{DateTime.UtcNow.Year}-{(count + 9001).ToString("D4")}";
            dispatch.DispatchDate = DateTime.UtcNow;
            dispatch.Status = "Packing";

            if (lineItems != null && lineItems.Any())
            {
                foreach (var item in lineItems)
                {
                    if (item.CatalogItemId > 0 && item.Quantity > 0)
                    {
                        dispatch.LineItems.Add(new DispatchLineItem
                        {
                            CatalogItemId = item.CatalogItemId,
                            Quantity = item.Quantity,
                            PickedFromBinId = item.PickedFromBinId > 0 ? item.PickedFromBinId : null
                        });
                    }
                }
            }

            _context.MaterialDispatches.Add(dispatch);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Outward dispatch slip '{dispatch.DispatchSlipNo}' registered. Ready for execution.";
            return RedirectToAction(nameof(Dispatch));
        }

        // POST: /InvWarehouse/ExecuteDispatch
        [HttpPost]
        public async Task<IActionResult> ExecuteDispatch(int id)
        {
            var dispatch = await _context.MaterialDispatches
                .Include(d => d.LineItems)
                    .ThenInclude(li => li.CatalogItem)
                .Include(d => d.SourceWarehouse)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dispatch == null)
            {
                return Json(new { success = false, message = "Dispatch slip not found." });
            }

            if (dispatch.Status == "Dispatched" || dispatch.Status == "Delivered")
            {
                return Json(new { success = false, message = "This order has already been dispatched." });
            }

            // 1. Stock Sufficiency Check
            foreach (var line in dispatch.LineItems)
            {
                var item = await _context.CatalogItems.FindAsync(line.CatalogItemId);
                if (item == null || item.CurrentStock < line.Quantity)
                {
                    var itemName = item?.ItemName ?? $"Item #{line.CatalogItemId}";
                    var available = item?.CurrentStock ?? 0;
                    return Json(new { 
                        success = false, 
                        message = $"Insufficient stock for '{itemName}'. Required: {line.Quantity}, Available: {available}." 
                    });
                }
            }

            var currentUserName = User.Identity?.Name ?? "Admin";

            // 2. Perform Stock Deductions, UnitsSold increments, and Bin releases
            foreach (var line in dispatch.LineItems)
            {
                var item = await _context.CatalogItems.FindAsync(line.CatalogItemId);
                if (item != null)
                {
                    item.CurrentStock -= line.Quantity;
                    item.UnitsSold += line.Quantity;
                }

                if (line.PickedFromBinId.HasValue)
                {
                    var bin = await _context.BinRackMasters.FindAsync(line.PickedFromBinId.Value);
                    if (bin != null)
                    {
                        bin.CurrentOccupancyVolume = Math.Max(0, bin.CurrentOccupancyVolume - line.Quantity);
                        if (bin.Status == "Full" && bin.CurrentOccupancyVolume < bin.MaxCapacityVolume)
                        {
                            bin.Status = "Available";
                        }
                    }
                }

                // Log Outward Movement
                _context.StockMovementLogs.Add(new StockMovementLog
                {
                    InventoryItemId = line.CatalogItemId,
                    MovementType = "OUTWARD",
                    Quantity = line.Quantity,
                    ReferenceDocument = dispatch.DispatchSlipNo,
                    HandledByUserId = currentUserName,
                    Timestamp = DateTime.UtcNow
                });
            }

            dispatch.Status = "Dispatched";
            await _context.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = $"Dispatch slip {dispatch.DispatchSlipNo} executed successfully. Stock deducted and movement logged." });
            }

            TempData["SuccessMessage"] = $"Dispatch slip {dispatch.DispatchSlipNo} executed successfully. Stock deducted and movement logged.";
            return RedirectToAction(nameof(Dispatch));
        }

        // GET: /InvWarehouse/PrintChallan
        [HttpGet]
        public async Task<IActionResult> PrintChallan(int id)
        {
            var dispatch = await _context.MaterialDispatches
                .Include(d => d.SourceWarehouse)
                .Include(d => d.LineItems)
                    .ThenInclude(li => li.CatalogItem)
                .Include(d => d.LineItems)
                    .ThenInclude(li => li.PickedFromBin)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dispatch == null)
            {
                return NotFound();
            }

            return View("PrintChallan", dispatch);
        }

        #endregion

        #region Legacy Transfers & STO Compatibility

        // GET: /InvWarehouse/Transfers
        [HttpGet]
        public async Task<IActionResult> Transfers()
        {
            var transfers = await _context.InvTransfers.OrderByDescending(t => t.TransferId).ToListAsync();
            ViewBag.Warehouses = await _context.WarehouseLocations.Where(w => w.IsActive).ToListAsync();
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
                int count = await _context.InvTransfers.CountAsync();
                transfer.TransferNo = $"TR-900{(count + 81).ToString("D2")}";

                _context.InvTransfers.Add(transfer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Transfers));
            }
            var transfers = await _context.InvTransfers.OrderByDescending(t => t.TransferId).ToListAsync();
            ViewBag.Warehouses = await _context.WarehouseLocations.Where(w => w.IsActive).ToListAsync();
            return View(nameof(Transfers), transfers);
        }

        // GET: /InvWarehouse/Sto
        [HttpGet]
        public IActionResult Sto()
        {
            return View();
        }

        #endregion
    }
}
