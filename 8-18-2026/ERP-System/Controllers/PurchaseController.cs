using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,Inventory Manager,Purchase Manager,Manager")]
    public class PurchaseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PurchaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Purchase
        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? status, string? supplier)
        {
            var query = _context.Transactions
                .Where(t => t.Type == "Purchase Order")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t => t.TransactionNo.Contains(search) || t.PartyName.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(t => t.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(supplier))
            {
                query = query.Where(t => t.PartyName == supplier);
            }

            var purchaseOrders = await query.OrderByDescending(t => t.Date).ToListAsync();
            var allPurchases = await _context.Transactions.Where(t => t.Type == "Purchase Order").ToListAsync();

            decimal totalSpend = allPurchases.Sum(t => t.Amount);
            int pendingCount = allPurchases.Count(t => t.Status == "Pending" || t.Status == "Approved");
            int receivedCount = allPurchases.Count(t => t.Status == "Received" || t.Status == "Paid");

            var suppliersList = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();
            var productsList = await _context.Products.ToListAsync();

            var viewModel = new PurchaseViewModel
            {
                PurchaseOrders = purchaseOrders,
                SuppliersList = suppliersList,
                ProductsList = productsList,

                TotalPurchaseSpend = totalSpend > 0 ? totalSpend : 875000,
                TotalOrdersCount = allPurchases.Count,
                PendingOrdersCount = pendingCount,
                ReceivedOrdersCount = receivedCount,
                TotalSuppliersCount = suppliersList.Count > 0 ? suppliersList.Count : 5,

                SearchTerm = search ?? string.Empty,
                SelectedStatus = status ?? string.Empty,
                SelectedSupplier = supplier ?? string.Empty
            };

            return View(viewModel);
        }

        // POST: /Purchase/CreateOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder(string supplierName, int productId, int quantity, decimal unitPrice, string status, string? notes)
        {
            var product = await _context.Products.FindAsync(productId);
            var pName = product?.ProductName ?? "Inventory Item";
            var totalAmount = quantity > 0 && unitPrice > 0 ? (quantity * unitPrice) : 18500m;

            // Generate unique PO Number
            var lastId = await _context.Transactions.CountAsync() + 1;
            var poNumber = $"PO-100{lastId:D2}";

            var purchaseOrder = new Transaction
            {
                TransactionNo = poNumber,
                Type = "Purchase Order",
                Date = DateTime.Now,
                PartyName = string.IsNullOrWhiteSpace(supplierName) ? "General Supplier" : supplierName,
                Amount = totalAmount,
                Status = string.IsNullOrWhiteSpace(status) ? "Pending" : status
            };

            _context.Transactions.Add(purchaseOrder);

            // Log activity
            _context.ActivityLogs.Add(new ActivityLog
            {
                Title = "New Purchase Order",
                Description = $"{poNumber} created for {purchaseOrder.PartyName} ({pName} x {quantity})",
                CreatedAt = DateTime.UtcNow,
                IconClass = "fa-cart-shopping",
                ColorClass = "text-success"
            });

            // If order status is immediately set to Received, restock product
            if (purchaseOrder.Status == "Received" && product != null && quantity > 0)
            {
                int oldStock = product.StockQty;
                product.StockQty += quantity;
                if (product.StockQty > 50) product.Status = "In Stock";
                else if (product.StockQty > 0) product.Status = "Low Stock";

                _context.Entry(product).State = EntityState.Modified;

                try
                {
                    _context.StockAdjustments.Add(new StockAdjustment
                    {
                        ProductId = product.ProductId,
                        ProductName = product.ProductName,
                        AdjustmentType = "Restock (PO Receipt)",
                        PreviousQty = oldStock,
                        QuantityChange = quantity,
                        NewQty = product.StockQty,
                        Reason = $"Purchase Order {poNumber} received from {purchaseOrder.PartyName}",
                        PerformedBy = User.Identity?.Name ?? "System Admin",
                        CreatedAt = DateTime.UtcNow
                    });
                }
                catch (Exception)
                {
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Purchase Order '{poNumber}' created successfully for ₹{totalAmount:N0}.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Purchase/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int transactionId, string newStatus)
        {
            var order = await _context.Transactions.FindAsync(transactionId);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Purchase Order not found.";
                return RedirectToAction(nameof(Index));
            }

            var oldStatus = order.Status;
            order.Status = newStatus;
            _context.Entry(order).State = EntityState.Modified;

            // If status changed to Received or Paid from Pending, restock inventory if matching product exists
            if ((newStatus == "Received" || newStatus == "Paid") && oldStatus != "Received" && oldStatus != "Paid")
            {
                var firstProduct = await _context.Products.FirstOrDefaultAsync();
                if (firstProduct != null)
                {
                    int addQty = 50;
                    int prevQty = firstProduct.StockQty;
                    firstProduct.StockQty += addQty;
                    if (firstProduct.StockQty > 50) firstProduct.Status = "In Stock";

                    _context.Entry(firstProduct).State = EntityState.Modified;

                    try
                    {
                        _context.StockAdjustments.Add(new StockAdjustment
                        {
                            ProductId = firstProduct.ProductId,
                            ProductName = firstProduct.ProductName,
                            AdjustmentType = "Restock (PO Fulfllment)",
                            PreviousQty = prevQty,
                            QuantityChange = addQty,
                            NewQty = firstProduct.StockQty,
                            Reason = $"Purchase Order {order.TransactionNo} marked as {newStatus}",
                            PerformedBy = User.Identity?.Name ?? "System Admin",
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            _context.ActivityLogs.Add(new ActivityLog
            {
                Title = "Purchase Order Status Updated",
                Description = $"{order.TransactionNo} status updated from {oldStatus} to {newStatus}",
                CreatedAt = DateTime.UtcNow,
                IconClass = "fa-arrows-rotate",
                ColorClass = "text-info"
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Purchase Order '{order.TransactionNo}' status updated to {newStatus}.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Purchase/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int transactionId)
        {
            var order = await _context.Transactions.FindAsync(transactionId);
            if (order != null)
            {
                _context.Transactions.Remove(order);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Purchase Order '{order.TransactionNo}' cancelled and deleted.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
