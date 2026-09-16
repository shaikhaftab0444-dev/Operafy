using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Data;
using ERP_System.Models;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,Inventory Manager,Purchase Manager")]
    public class InvCatalogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvCatalogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================================
        // PAGE 2: ITEMS & SKUs MASTER (/InvCatalog/Items)
        // =========================================================================

        // GET: /InvCatalog/Items
        [HttpGet]
        public async Task<IActionResult> Items(string? search, int? categoryId, string? status)
        {
            var query = _context.CatalogItems
                .Include(i => i.Category)
                .Include(i => i.SubCategory)
                .Include(i => i.Uom)
                .AsQueryable();

            // All items for metric calculation before filters
            var allItems = await _context.CatalogItems.ToListAsync();
            int totalCount = allItems.Count;
            int inStockCount = allItems.Count(i => i.CurrentStock > i.MinimumReorderLevel);
            int lowStockCount = allItems.Count(i => i.CurrentStock > 0 && i.CurrentStock <= i.MinimumReorderLevel);
            int outOfStockCount = allItems.Count(i => i.CurrentStock <= 0);

            // Real-time search filter (Item Name, SKU, Barcode)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(i => i.ItemName.ToLower().Contains(term)
                                      || i.SKU.ToLower().Contains(term)
                                      || i.Barcode.ToLower().Contains(term));
            }

            // Category filter
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(i => i.CategoryId == categoryId.Value);
            }

            // Stock Status filter
            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                if (status.Equals("In Stock", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(i => i.CurrentStock > i.MinimumReorderLevel);
                }
                else if (status.Equals("Low Stock", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(i => i.CurrentStock > 0 && i.CurrentStock <= i.MinimumReorderLevel);
                }
                else if (status.Equals("Out of Stock", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(i => i.CurrentStock <= 0);
                }
            }

            var items = await query.OrderByDescending(i => i.CreatedAt).ToListAsync();

            var categories = await _context.ProductCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var subCategories = await _context.ProductSubCategories
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            var uoms = await _context.UnitsOfMeasure
                .Where(u => u.IsActive)
                .OrderBy(u => u.Code)
                .ToListAsync();

            var viewModel = new ItemsPageViewModel
            {
                Items = items,
                Categories = categories,
                SubCategories = subCategories,
                UnitsOfMeasure = uoms,
                SearchTerm = search,
                SelectedCategoryId = categoryId,
                SelectedStatus = status ?? "All",
                TotalItemsCount = totalCount,
                InStockCount = inStockCount,
                LowStockCount = lowStockCount,
                OutOfStockCount = outOfStockCount
            };

            return View(viewModel);
        }

        // POST: /InvCatalog/SaveItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveItem([FromForm] CatalogItem model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.ItemName))
                {
                    return Json(new { success = false, message = "Product item name is required." });
                }

                if (model.CategoryId <= 0)
                {
                    return Json(new { success = false, message = "Please select a valid Category." });
                }

                if (model.UomId <= 0)
                {
                    return Json(new { success = false, message = "Please select a valid Base UOM." });
                }

                // Auto-generate SKU if empty
                if (string.IsNullOrWhiteSpace(model.SKU))
                {
                    var cat = await _context.ProductCategories.FindAsync(model.CategoryId);
                    var prefix = cat != null && cat.Name.Length >= 2
                        ? cat.Name.Substring(0, 2).ToUpper()
                        : "PR";
                    var randomCode = new Random().Next(1000, 9999);
                    model.SKU = $"SKU-{prefix}-{randomCode}";
                }

                // Auto-generate Barcode if empty
                if (string.IsNullOrWhiteSpace(model.Barcode))
                {
                    var randomDigits = new Random().Next(100000000, 999999999);
                    model.Barcode = $"890{randomDigits}";
                }

                if (model.Id == 0)
                {
                    // Create new
                    model.CreatedAt = DateTime.UtcNow;
                    model.IsActive = true;
                    _context.CatalogItems.Add(model);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true, message = $"Product '{model.ItemName}' ({model.SKU}) added successfully." });
                }
                else
                {
                    // Edit existing
                    var existing = await _context.CatalogItems.FindAsync(model.Id);
                    if (existing == null)
                    {
                        return Json(new { success = false, message = "Product item not found." });
                    }

                    existing.ItemName = model.ItemName;
                    existing.SKU = model.SKU;
                    existing.Barcode = model.Barcode;
                    existing.CategoryId = model.CategoryId;
                    existing.SubCategoryId = model.SubCategoryId > 0 ? model.SubCategoryId : null;
                    existing.UomId = model.UomId;
                    existing.PurchasePrice = model.PurchasePrice;
                    existing.SellingPrice = model.SellingPrice;
                    existing.CurrentStock = model.CurrentStock;
                    existing.MinimumReorderLevel = model.MinimumReorderLevel;
                    existing.BranchLocation = string.IsNullOrWhiteSpace(model.BranchLocation) ? "Head Office" : model.BranchLocation;
                    existing.BinLocation = model.BinLocation;
                    existing.IsActive = model.IsActive;

                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = $"Product '{model.ItemName}' updated successfully." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while saving: " + ex.Message });
            }
        }

        // POST: /InvCatalog/AdjustItemStock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdjustItemStock(int itemId, int adjustmentQty, string reason)
        {
            try
            {
                var item = await _context.CatalogItems.FindAsync(itemId);
                if (item == null)
                {
                    return Json(new { success = false, message = "Item not found in catalog." });
                }

                int prevStock = item.CurrentStock;
                int newStock = prevStock + adjustmentQty;
                if (newStock < 0)
                {
                    return Json(new { success = false, message = $"Cannot reduce stock below zero. Current stock is {prevStock}." });
                }

                item.CurrentStock = newStock;

                // Log adjustment audit entry
                _context.StockAdjustments.Add(new StockAdjustment
                {
                    ProductId = item.Id,
                    ProductName = $"{item.ItemName} ({item.SKU})",
                    AdjustmentType = adjustmentQty >= 0 ? "Add Audit" : "Deduct Audit",
                    PreviousQty = prevStock,
                    QuantityChange = adjustmentQty,
                    NewQty = newStock,
                    Reason = string.IsNullOrWhiteSpace(reason) ? "Manual Audit Adjustment" : reason,
                    PerformedBy = User.Identity?.Name ?? "Inventory Manager",
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    newStock = item.CurrentStock,
                    status = item.StockStatus,
                    message = $"Stock for '{item.ItemName}' adjusted by {(adjustmentQty >= 0 ? "+" : "")}{adjustmentQty}. New stock: {item.CurrentStock}."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Adjustment failed: " + ex.Message });
            }
        }

        // GET: /InvCatalog/GetSubCategories?categoryId=...
        [HttpGet]
        public async Task<IActionResult> GetSubCategories(int categoryId)
        {
            var subs = await _context.ProductSubCategories
                .Where(s => s.CategoryId == categoryId && s.IsActive)
                .OrderBy(s => s.Name)
                .Select(s => new { s.Id, s.Name })
                .ToListAsync();

            return Json(subs);
        }

        // GET: /InvCatalog/GetItemHistory?itemId=...
        [HttpGet]
        public async Task<IActionResult> GetItemHistory(int itemId)
        {
            var adjustments = await _context.StockAdjustments
                .Where(a => a.ProductId == itemId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(15)
                .Select(a => new
                {
                    AdjustmentId = a.StockAdjustmentId,
                    a.AdjustmentType,
                    a.PreviousQty,
                    a.QuantityChange,
                    a.NewQty,
                    a.Reason,
                    a.PerformedBy,
                    Date = a.CreatedAt.ToString("dd MMM yyyy, hh:mm tt")
                })
                .ToListAsync();

            return Json(adjustments);
        }

        // =========================================================================
        // PAGE 3: CATEGORIES & SUB-CATEGORIES (/InvCatalog/Categories)
        // =========================================================================

        // GET: /InvCatalog/Categories
        [HttpGet]
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.ProductCategories
                .Include(c => c.SubCategories)
                .Include(c => c.Items)
                .OrderBy(c => c.Name)
                .ToListAsync();

            int totalSubCount = categories.Sum(c => c.SubCategories.Count);
            int totalLinkedProducts = categories.Sum(c => c.Items.Count);

            var viewModel = new CategoriesPageViewModel
            {
                Categories = categories,
                TotalCategoriesCount = categories.Count,
                TotalSubCategoriesCount = totalSubCount,
                TotalLinkedProductsCount = totalLinkedProducts
            };

            return View(viewModel);
        }

        // POST: /InvCatalog/SaveCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCategory([FromForm] ProductCategory model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    return Json(new { success = false, message = "Category name is required." });
                }

                if (model.Id == 0)
                {
                    _context.ProductCategories.Add(model);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = $"Category '{model.Name}' created successfully." });
                }
                else
                {
                    var existing = await _context.ProductCategories.FindAsync(model.Id);
                    if (existing == null)
                    {
                        return Json(new { success = false, message = "Category not found." });
                    }

                    existing.Name = model.Name;
                    existing.HsnCode = model.HsnCode;
                    existing.DefaultGstRate = model.DefaultGstRate;
                    existing.IsActive = model.IsActive;

                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = $"Category '{model.Name}' updated successfully." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving category: " + ex.Message });
            }
        }

        // POST: /InvCatalog/SaveSubCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSubCategory([FromForm] ProductSubCategory model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    return Json(new { success = false, message = "Sub-Category name is required." });
                }

                if (model.CategoryId <= 0)
                {
                    return Json(new { success = false, message = "Parent Category is required." });
                }

                if (model.Id == 0)
                {
                    _context.ProductSubCategories.Add(model);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = $"Sub-Category '{model.Name}' added successfully." });
                }
                else
                {
                    var existing = await _context.ProductSubCategories.FindAsync(model.Id);
                    if (existing == null)
                    {
                        return Json(new { success = false, message = "Sub-Category not found." });
                    }

                    existing.Name = model.Name;
                    existing.CategoryId = model.CategoryId;
                    existing.Description = model.Description;
                    existing.IsActive = model.IsActive;

                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = $"Sub-Category '{model.Name}' updated successfully." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving sub-category: " + ex.Message });
            }
        }

        // POST: /InvCatalog/DeleteCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var category = await _context.ProductCategories
                    .Include(c => c.Items)
                    .Include(c => c.SubCategories)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    return Json(new { success = false, message = "Category not found." });
                }

                // SAFETY VALIDATION GUARD: Block category deletion if Linked Products count > 0
                if (category.Items != null && category.Items.Count > 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Cannot delete category containing active products. Reassign items first."
                    });
                }

                // Also verify sub-category items
                var subCategoryIds = category.SubCategories.Select(s => s.Id).ToList();
                if (await _context.CatalogItems.AnyAsync(i => i.SubCategoryId != null && subCategoryIds.Contains(i.SubCategoryId.Value)))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Cannot delete category containing active products. Reassign items first."
                    });
                }

                // Remove subcategories first
                if (category.SubCategories.Any())
                {
                    _context.ProductSubCategories.RemoveRange(category.SubCategories);
                }

                _context.ProductCategories.Remove(category);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Category '{category.Name}' and its sub-categories were deleted." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Delete failed: " + ex.Message });
            }
        }

        // POST: /InvCatalog/DeleteSubCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSubCategory(int id)
        {
            try
            {
                var sub = await _context.ProductSubCategories.FindAsync(id);
                if (sub == null)
                {
                    return Json(new { success = false, message = "Sub-Category not found." });
                }

                // Safety guard on sub-category deletion
                if (await _context.CatalogItems.AnyAsync(i => i.SubCategoryId == id))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Cannot delete sub-category containing active products. Reassign items first."
                    });
                }

                _context.ProductSubCategories.Remove(sub);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Sub-Category '{sub.Name}' deleted." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Delete failed: " + ex.Message });
            }
        }

        // =========================================================================
        // PAGE 4: UNITS OF MEASURE (UOM) (/InvCatalog/Uom)
        // =========================================================================

        // GET: /InvCatalog/Uom
        [HttpGet]
        public async Task<IActionResult> Uom()
        {
            var uoms = await _context.UnitsOfMeasure
                .OrderBy(u => u.Code)
                .ToListAsync();

            var conversions = await _context.UomConversions
                .Include(c => c.FromUom)
                .Include(c => c.ToUom)
                .ToListAsync();

            var viewModel = new UomPageViewModel
            {
                UnitsOfMeasure = uoms,
                Conversions = conversions,
                TotalUomCount = uoms.Count,
                BaseUnitsCount = uoms.Count(u => u.IsBaseUnit),
                ConversionUnitsCount = uoms.Count(u => !u.IsBaseUnit)
            };

            return View(viewModel);
        }

        // POST: /InvCatalog/SaveUom
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveUom([FromForm] UnitOfMeasure model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Code))
                {
                    return Json(new { success = false, message = "UOM Code (e.g. PCS, KG) is required." });
                }

                if (string.IsNullOrWhiteSpace(model.Description))
                {
                    return Json(new { success = false, message = "UOM Description is required." });
                }

                model.Code = model.Code.Trim().ToUpper();

                if (model.Id == 0)
                {
                    if (await _context.UnitsOfMeasure.AnyAsync(u => u.Code == model.Code))
                    {
                        return Json(new { success = false, message = $"UOM Code '{model.Code}' already exists." });
                    }

                    _context.UnitsOfMeasure.Add(model);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = $"UOM '{model.Code}' created successfully." });
                }
                else
                {
                    var existing = await _context.UnitsOfMeasure.FindAsync(model.Id);
                    if (existing == null)
                    {
                        return Json(new { success = false, message = "Unit of Measure not found." });
                    }

                    existing.Code = model.Code;
                    existing.Description = model.Description;
                    existing.IsBaseUnit = model.IsBaseUnit;
                    existing.AllowDecimals = model.AllowDecimals;
                    existing.OfficialGstUomCode = model.OfficialGstUomCode;
                    existing.IsActive = model.IsActive;

                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = $"UOM '{model.Code}' updated successfully." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving UOM: " + ex.Message });
            }
        }

        // POST: /InvCatalog/SaveConversion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveConversion([FromForm] UomConversion model)
        {
            try
            {
                if (model.FromUomId <= 0 || model.ToUomId <= 0)
                {
                    return Json(new { success = false, message = "Please select both Secondary and Base units." });
                }

                if (model.FromUomId == model.ToUomId)
                {
                    return Json(new { success = false, message = "From Unit and To Unit must be distinct." });
                }

                if (model.ConversionFactor <= 0)
                {
                    return Json(new { success = false, message = "Conversion factor must be greater than zero." });
                }

                var fromUnit = await _context.UnitsOfMeasure.FindAsync(model.FromUomId);
                var toUnit = await _context.UnitsOfMeasure.FindAsync(model.ToUomId);

                if (model.Id == 0)
                {
                    _context.UomConversions.Add(model);
                    await _context.SaveChangesAsync();

                    return Json(new
                    {
                        success = true,
                        message = $"Conversion saved: 1 {fromUnit?.Code ?? "Unit"} = {model.ConversionFactor} {toUnit?.Code ?? "Units"}."
                    });
                }
                else
                {
                    var existing = await _context.UomConversions.FindAsync(model.Id);
                    if (existing == null)
                    {
                        return Json(new { success = false, message = "Conversion rule not found." });
                    }

                    existing.FromUomId = model.FromUomId;
                    existing.ToUomId = model.ToUomId;
                    existing.ConversionFactor = model.ConversionFactor;

                    await _context.SaveChangesAsync();
                    return Json(new
                    {
                        success = true,
                        message = $"Conversion updated: 1 {fromUnit?.Code ?? "Unit"} = {model.ConversionFactor} {toUnit?.Code ?? "Units"}."
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving conversion: " + ex.Message });
            }
        }

        // POST: /InvCatalog/DeleteConversion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConversion(int id)
        {
            try
            {
                var conv = await _context.UomConversions.FindAsync(id);
                if (conv == null)
                {
                    return Json(new { success = false, message = "Conversion rule not found." });
                }

                _context.UomConversions.Remove(conv);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Conversion rule deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Delete failed: " + ex.Message });
            }
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
            var items = await _context.CatalogItems
                .Include(i => i.Category)
                .Include(i => i.Uom)
                .OrderBy(i => i.ItemName)
                .ToListAsync();

            return View(items);
        }

        // GET: /InvCatalog/Serials
        [HttpGet]
        public IActionResult Serials()
        {
            return View();
        }
    }
}
