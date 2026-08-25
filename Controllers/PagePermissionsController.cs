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
    public class PagePermissionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PagePermissionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private static readonly List<(string ModuleName, string DisplayName, string Category)> DefinedModules = new()
        {
            // System Administration
            ("UserManagement", "User Accounts & Security", "Admin"),
            ("Company", "Company Settings", "Admin"),
            ("BranchManagement", "Branch Operations", "Admin"),
            ("Settings", "System Configuration", "Admin"),

            // HR Management
            ("HRManagement_DailyAttendance", "Daily Attendance Logs", "HRMS"),
            ("HRManagement_EmployeeDirectory", "Employee Directory & KYC", "HRMS"),
            ("HRManagement_LeaveRequests", "Leave Requests & Approvals", "HRMS"),
            ("HRManagement_MonthlyPayroll", "Monthly Payroll & Payslips", "HRMS"),

            // Supply Chain & Warehouse
            ("SupplyChain_InventoryStock", "Inventory & Stock Levels", "Warehouse"),
            ("SupplyChain_PurchaseOrders", "Purchase Orders & Vendors", "Warehouse"),
            ("SupplyChain_GoodsReceipt", "Goods Receipt Note (GRN)", "Warehouse"),

            // CRM & Sales
            ("SalesCRM_CustomerDirectory", "Customer Directory", "CRM"),
            ("SalesCRM_QuotationsEstimates", "Quotations & Estimates", "CRM"),
            ("SalesCRM_OrdersInvoicing", "Sales Orders & Invoicing", "CRM"),

            // Finance & Accounts
            ("FinanceAccounts_GeneralLedger", "General Ledger & Daybook", "Finance"),
            ("FinanceAccounts_ReportsTax", "Financial Reports & Tax", "Finance")
        };

        // GET: /RolePermissions or /PagePermissions
        [HttpGet]
        [Route("RolePermissions")]
        [Route("PagePermissions")]
        public async Task<IActionResult> Index(int? roleId)
        {
            var roles = await _context.Roles.Where(r => r.IsActive).ToListAsync();
            
            // Default to the first non-Admin role if none selected
            int selectedRoleId = roleId ?? (roles.FirstOrDefault(r => r.RoleName != "Super Admin" && r.RoleName != "Admin")?.RoleId 
                                            ?? (roles.FirstOrDefault()?.RoleId ?? 0));

            var existingPermissions = await _context.RolePermissions
                .Where(p => p.RoleId == selectedRoleId)
                .ToListAsync();

            var viewModel = new RolePermissionMatrixViewModel
            {
                SelectedRoleId = selectedRoleId,
                SelectedRoleName = roles.FirstOrDefault(r => r.RoleId == selectedRoleId)?.RoleName ?? "Role",
                Roles = roles,
                ModulePermissions = DefinedModules.Select(dm => {
                    var existing = existingPermissions.FirstOrDefault(ep => ep.ModuleName == dm.ModuleName);
                    return new ModulePermissionRow
                    {
                        ModuleName = dm.ModuleName,
                        DisplayName = dm.DisplayName,
                        Category = dm.Category,
                        CanView = existing?.CanView ?? false,
                        CanCreate = existing?.CanCreate ?? false,
                        CanEdit = existing?.CanEdit ?? false,
                        CanDelete = existing?.CanDelete ?? false,
                        CanApprove = existing?.CanApprove ?? false
                    };
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: /PagePermissions/SavePermissions
        [HttpPost]
        [Route("PagePermissions/SavePermissions")]
        [Route("RolePermissions/SavePermissions")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePermissions(RolePermissionMatrixViewModel model)
        {
            if (model.SelectedRoleId <= 0)
            {
                TempData["ErrorMessage"] = "Invalid role selected.";
                return RedirectToAction(nameof(Index));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingPermissions = await _context.RolePermissions
                    .Where(p => p.RoleId == model.SelectedRoleId)
                    .ToListAsync();

                foreach (var row in model.ModulePermissions)
                {
                    var existing = existingPermissions.FirstOrDefault(p => p.ModuleName == row.ModuleName);
                    if (existing != null)
                    {
                        existing.CanView = row.CanView;
                        existing.CanCreate = row.CanCreate;
                        existing.CanEdit = row.CanEdit;
                        existing.CanDelete = row.CanDelete;
                        existing.CanApprove = row.CanApprove;
                        existing.IsAllowed = row.CanView; // Map to legacy IsAllowed check
                    }
                    else
                    {
                        _context.RolePermissions.Add(new RolePermission
                        {
                            RoleId = model.SelectedRoleId,
                            ModuleName = row.ModuleName,
                            CanView = row.CanView,
                            CanCreate = row.CanCreate,
                            CanEdit = row.CanEdit,
                            CanDelete = row.CanDelete,
                            CanApprove = row.CanApprove,
                            IsAllowed = row.CanView // Map to legacy IsAllowed check
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Save system audit logs
                var roleName = await _context.Roles
                    .Where(r => r.RoleId == model.SelectedRoleId)
                    .Select(r => r.RoleName)
                    .FirstOrDefaultAsync() ?? "Role";

                _context.ActivityLogs.Add(new ActivityLog
                {
                    Title = "RBAC Matrix Saved",
                    Description = $"Permissions matrix for role '{roleName}' updated in bulk by admin.",
                    IconClass = "fa-shield-halved",
                    ColorClass = "text-indigo",
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Permissions matrix for role '{roleName}' saved successfully!";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = $"Failed to save permissions: {ex.Message}";
            }

            return RedirectToAction(nameof(Index), new { roleId = model.SelectedRoleId });
        }

        // GET: /PagePermissions/GetRolePermissionsJson
        [HttpGet]
        [Route("PagePermissions/GetRolePermissionsJson")]
        public async Task<IActionResult> GetRolePermissionsJson(int roleId)
        {
            var permissions = await _context.RolePermissions
                .Where(p => p.RoleId == roleId)
                .ToListAsync();

            return Json(permissions.Select(p => new
            {
                p.ModuleName,
                p.CanView,
                p.CanCreate,
                p.CanEdit,
                p.CanDelete,
                p.CanApprove
            }));
        }
    }
}
