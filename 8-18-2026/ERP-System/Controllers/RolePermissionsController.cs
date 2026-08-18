using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Data;
using ERP_System.Models;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin")]
    public class RolePermissionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RolePermissionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /RolePermissions
        [HttpGet]
        public async Task<IActionResult> Index(int? roleId)
        {
            var roles = await _context.Roles.Where(r => r.IsActive).ToListAsync();
            
            // Default to the first non-Admin role if none selected (to make sure they configure employees first)
            int selectedRoleId = roleId ?? (roles.FirstOrDefault(r => r.RoleName != "Super Admin" && r.RoleName != "Admin")?.RoleId 
                                            ?? (roles.FirstOrDefault()?.RoleId ?? 0));

            var permissions = await _context.RolePermissions
                .Where(p => p.RoleId == selectedRoleId)
                .ToListAsync();

            ViewBag.Roles = roles;
            ViewBag.SelectedRoleId = selectedRoleId;
            ViewBag.SelectedRoleName = roles.FirstOrDefault(r => r.RoleId == selectedRoleId)?.RoleName ?? "Role";

            return View(permissions);
        }

        // POST: /RolePermissions/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(int roleId, Dictionary<string, string> modulePermissions)
        {
            var existingPermissions = await _context.RolePermissions
                .Where(p => p.RoleId == roleId)
                .ToListAsync();

            var modules = new[]
            {
                "UserManagement", "Company", "BranchManagement", "EmployeeManagement", "CustomerManagement",
                "SupplierManagement", "ProductManagement", "InventoryManagement", "PurchaseManagement", "SalesManagement",
                "ExpenseManagement", "Accounting", "HRManagement", "Reports", "Settings"
            };

            foreach (var mod in modules)
            {
                // Check if module is allowed (submitted from checkbox form)
                bool isAllowed = modulePermissions.ContainsKey(mod) && modulePermissions[mod] == "true";
                var perm = existingPermissions.FirstOrDefault(p => p.ModuleName == mod);
                if (perm != null)
                {
                    perm.IsAllowed = isAllowed;
                }
                else
                {
                    _context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = roleId,
                        ModuleName = mod,
                        IsAllowed = isAllowed
                    });
                }
            }

            await _context.SaveChangesAsync();

            // Record in administrative activity logs
            var roleName = await _context.Roles.Where(r => r.RoleId == roleId).Select(r => r.RoleName).FirstOrDefaultAsync() ?? "Role";
            _context.ActivityLogs.Add(new ActivityLog
            {
                Title = "Permissions Updated",
                Description = $"Page permissions for role '{roleName}' updated by admin.",
                IconClass = "fa-user-gear",
                ColorClass = "text-primary",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Permissions for role '{roleName}' updated successfully!";
            return RedirectToAction(nameof(Index), new { roleId = roleId });
        }
    }
}
