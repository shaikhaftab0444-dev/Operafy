using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Data;
using ERP_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ERP_System.Controllers
{
    [Authorize]
    public class ESSPurchaseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ESSPurchaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /ESSPurchase (My PR History - strictly filtered to logged-in user)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentUserId = GetCurrentUserId();

            // Strictly query ONLY the logged-in user's requisitions
            var myRequisitions = await _context.PurchaseRequisitions
                .Include(p => p.Department)
                .Include(p => p.CatalogItem)
                .Where(p => p.UserId == currentUserId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(myRequisitions);
        }

        // GET: /ESSPurchase/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var currentUserId = GetCurrentUserId();
            var userWithDept = await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.UserId == currentUserId);

            var deptName = userWithDept?.DepartmentName ?? userWithDept?.Department?.DepartmentName ?? "General";
            var deptCode = (deptName.Length >= 3 ? deptName.Substring(0, 3) : deptName).ToUpper().Replace("&", "").Trim();
            var autoCostCenter = $"CC-{deptCode}-{DateTime.Today:yyyy-MM}";

            int userDeptId = userWithDept?.DepartmentId ?? 1;

            var catalogItems = await _context.ProcurementCatalogItems
                .Where(i => i.DepartmentId == userDeptId && i.IsActive)
                .ToListAsync();

            if (!catalogItems.Any())
            {
                catalogItems = await _context.ProcurementCatalogItems
                    .Where(i => i.IsActive)
                    .ToListAsync();
            }

            ViewBag.EmployeeName = userWithDept?.FullName ?? User.Identity?.Name ?? "Employee";
            ViewBag.DepartmentName = deptName;
            ViewBag.DepartmentId = userDeptId;
            ViewBag.AutoCostCenter = autoCostCenter;
            ViewBag.CatalogItems = catalogItems;

            return View();
        }

        // POST: /ESSPurchase/SubmitPR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitPR(int catalogItemId, int quantity, string urgencyLevel, string? justification, string? costCenterCode)
        {
            var currentUserId = GetCurrentUserId();
            var currentUser = await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.UserId == currentUserId);

            var catalogItem = await _context.ProcurementCatalogItems.FindAsync(catalogItemId);

            if (catalogItem == null || quantity < 1)
            {
                return Json(new { success = false, message = "Invalid catalog item or quantity." });
            }

            int deptId = currentUser?.DepartmentId ?? catalogItem.DepartmentId;
            decimal totalCost = catalogItem.UnitPrice * quantity;
            string urgency = !string.IsNullOrWhiteSpace(urgencyLevel) ? urgencyLevel : "Medium Priority";

            var pr = new PurchaseRequisition
            {
                UserId = currentUserId,
                DepartmentId = deptId,
                CatalogItemId = catalogItem.Id,
                Quantity = quantity,
                UnitPrice = catalogItem.UnitPrice,
                EstimatedTotalCost = totalCost,
                UrgencyLevel = urgency,
                BusinessJustification = justification,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.PurchaseRequisitions.Add(pr);
            await _context.SaveChangesAsync();

            // Sync with PurchaseDataStore for Purchase Manager console visibility
            var deptName = currentUser?.DepartmentName ?? currentUser?.Department?.DepartmentName ?? "General Operations";
            var dsItem = new RequisitionItem
            {
                Id = pr.Id,
                Department = deptName,
                RequestedBy = currentUser?.FullName ?? User.Identity?.Name ?? "Employee",
                ItemSummary = $"{quantity}x {catalogItem.ItemName} ({catalogItem.UnitOfMeasure})",
                EstimatedCost = "₹ " + totalCost.ToString("N2"),
                Urgency = urgency.Contains("Urgent") ? "Urgent" : (urgency.Contains("High") ? "High" : "Medium"),
                RequestedOn = DateTime.Now.ToString("dd MMM yyyy"),
                Status = "Pending"
            };
            PurchaseDataStore.AddRequisition(dsItem);

            return Json(new
            {
                success = true,
                id = pr.Id,
                message = "Purchase Requisition routed to Purchase Manager."
            });
        }

        // POST: /ESSPurchase/CancelPR (Employee withdrawal of pending request)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelPR(int prId)
        {
            var currentUserId = GetCurrentUserId();
            var pr = await _context.PurchaseRequisitions
                .FirstOrDefaultAsync(p => p.Id == prId && p.UserId == currentUserId);

            if (pr == null)
            {
                return Json(new { success = false, message = "Requisition not found." });
            }

            if (pr.Status != "Pending")
            {
                return Json(new { success = false, message = "Only pending requisitions can be withdrawn." });
            }

            pr.Status = "Cancelled";

            // Sync with PurchaseDataStore
            var dsItem = PurchaseDataStore.GetRequisitions().FirstOrDefault(r => r.Id == prId);
            if (dsItem != null)
            {
                dsItem.Status = "Cancelled";
                PurchaseDataStore.Save();
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Purchase requisition withdrawn successfully." });
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(claim, out int id))
            {
                return id;
            }
            return 1;
        }
    }
}
