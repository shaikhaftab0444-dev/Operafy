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

        // GET: /RolePermissions or /PagePermissions or /UserPermissions
        [HttpGet]
        [Route("RolePermissions")]
        [Route("PagePermissions")]
        [Route("UserPermissions")]
        public async Task<IActionResult> Index(int? roleId, int? userId, string mode = "Role")
        {
            var roles = await _context.Roles.Where(r => r.IsActive).ToListAsync();
            var dbUsers = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.IsActive)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var userViewModels = dbUsers.Select(u => new UserItemViewModel
            {
                UserId = u.UserId,
                FullName = u.FullName,
                UserName = u.UserName,
                Email = u.Email,
                RoleName = u.Role?.RoleName ?? "Employee",
                RoleId = u.RoleId ?? 0,
                ProfilePhoto = u.ProfilePhoto
            }).ToList();

            var allDbRolePermissions = await _context.RolePermissions.ToListAsync();
            var allDbUserPermissions = await _context.UserPermissions.ToListAsync();

            string activeMode = (mode.Equals("User", StringComparison.OrdinalIgnoreCase) || userId.HasValue) ? "User" : "Role";

            int selectedRoleId = roleId ?? (roles.FirstOrDefault(r => r.RoleName != "Super Admin" && r.RoleName != "Admin")?.RoleId 
                                            ?? (roles.FirstOrDefault()?.RoleId ?? 0));
            int selectedUserId = userId ?? (userViewModels.FirstOrDefault(u => u.RoleName != "Super Admin")?.UserId 
                                            ?? (userViewModels.FirstOrDefault()?.UserId ?? 0));

            var targetUser = userViewModels.FirstOrDefault(u => u.UserId == selectedUserId);
            int targetUserRoleId = targetUser?.RoleId ?? selectedRoleId;

            List<ModulePermissionRow> modulePermissionRows;

            if (activeMode == "User")
            {
                var userPerms = allDbUserPermissions.Where(p => p.UserId == selectedUserId).ToList();
                var fallbackRolePerms = allDbRolePermissions.Where(p => p.RoleId == targetUserRoleId).ToList();

                modulePermissionRows = DefinedModules.Select(dm =>
                {
                    var existingUserPerm = userPerms.FirstOrDefault(ep => ep.ModuleName == dm.ModuleName);
                    var fallbackRolePerm = fallbackRolePerms.FirstOrDefault(rp => rp.ModuleName == dm.ModuleName);

                    return new ModulePermissionRow
                    {
                        ModuleName = dm.ModuleName,
                        DisplayName = dm.DisplayName,
                        Category = dm.Category,
                        CanView = existingUserPerm != null ? existingUserPerm.CanView : (fallbackRolePerm?.CanView ?? false),
                        CanCreate = existingUserPerm != null ? existingUserPerm.CanCreate : (fallbackRolePerm?.CanCreate ?? false),
                        CanEdit = existingUserPerm != null ? existingUserPerm.CanEdit : (fallbackRolePerm?.CanEdit ?? false),
                        CanDelete = existingUserPerm != null ? existingUserPerm.CanDelete : (fallbackRolePerm?.CanDelete ?? false),
                        CanApprove = existingUserPerm != null ? existingUserPerm.CanApprove : (fallbackRolePerm?.CanApprove ?? false)
                    };
                }).ToList();
            }
            else
            {
                var rolePerms = allDbRolePermissions.Where(p => p.RoleId == selectedRoleId).ToList();

                modulePermissionRows = DefinedModules.Select(dm =>
                {
                    var existingRolePerm = rolePerms.FirstOrDefault(ep => ep.ModuleName == dm.ModuleName);
                    return new ModulePermissionRow
                    {
                        ModuleName = dm.ModuleName,
                        DisplayName = dm.DisplayName,
                        Category = dm.Category,
                        CanView = existingRolePerm?.CanView ?? false,
                        CanCreate = existingRolePerm?.CanCreate ?? false,
                        CanEdit = existingRolePerm?.CanEdit ?? false,
                        CanDelete = existingRolePerm?.CanDelete ?? false,
                        CanApprove = existingRolePerm?.CanApprove ?? false
                    };
                }).ToList();
            }

            var viewModel = new RolePermissionMatrixViewModel
            {
                ActiveMode = activeMode,
                SelectedRoleId = selectedRoleId,
                SelectedRoleName = roles.FirstOrDefault(r => r.RoleId == selectedRoleId)?.RoleName ?? "Role",
                Roles = roles,
                SelectedUserId = selectedUserId,
                SelectedUserName = targetUser?.FullName ?? "User",
                SelectedUserRole = targetUser?.RoleName ?? "Employee",
                Users = userViewModels,
                ModulePermissions = modulePermissionRows
            };

            // Pre-seed JSON caches for 0ms client-side switching
            ViewBag.AllRolePermissionsJson = System.Text.Json.JsonSerializer.Serialize(
                allDbRolePermissions.Select(p => new
                {
                    p.RoleId,
                    p.ModuleName,
                    p.CanView,
                    p.CanCreate,
                    p.CanEdit,
                    p.CanDelete,
                    p.CanApprove
                })
            );

            ViewBag.AllUserPermissionsJson = System.Text.Json.JsonSerializer.Serialize(
                allDbUserPermissions.Select(p => new
                {
                    p.UserId,
                    p.ModuleName,
                    p.CanView,
                    p.CanCreate,
                    p.CanEdit,
                    p.CanDelete,
                    p.CanApprove
                })
            );

            return View(viewModel);
        }

        // POST: /RolePermissions/SavePermissions or /PagePermissions/SavePermissions
        [HttpPost]
        [Route("PagePermissions/SavePermissions")]
        [Route("RolePermissions/SavePermissions")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePermissions(RolePermissionMatrixViewModel model)
        {
            if (model.SelectedRoleId <= 0)
            {
                TempData["ErrorMessage"] = "Invalid role selected.";
                return RedirectToAction(nameof(Index), new { mode = "Role" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingPermissions = await _context.RolePermissions
                    .Where(p => p.RoleId == model.SelectedRoleId)
                    .ToListAsync();

                if (model.ModulePermissions != null && model.ModulePermissions.Any())
                {
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
                            existing.IsAllowed = row.CanView;
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
                                IsAllowed = row.CanView
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var roleName = await _context.Roles
                    .Where(r => r.RoleId == model.SelectedRoleId)
                    .Select(r => r.RoleName)
                    .FirstOrDefaultAsync() ?? "Role";

                _context.ActivityLogs.Add(new ActivityLog
                {
                    Title = "RBAC Role Permissions Saved",
                    Description = $"Permissions matrix for role '{roleName}' updated in bulk by admin.",
                    IconClass = "fa-shield-halved",
                    ColorClass = "text-indigo",
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Role permissions for '{roleName}' saved successfully!";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = $"Failed to save role permissions: {ex.Message}";
            }

            return RedirectToAction(nameof(Index), new { roleId = model.SelectedRoleId, mode = "Role" });
        }

        // POST: /RolePermissions/SaveUserPermissions or /PagePermissions/SaveUserPermissions
        [HttpPost]
        [Route("PagePermissions/SaveUserPermissions")]
        [Route("RolePermissions/SaveUserPermissions")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveUserPermissions(RolePermissionMatrixViewModel model)
        {
            if (model.SelectedUserId <= 0)
            {
                TempData["ErrorMessage"] = "Invalid user selected.";
                return RedirectToAction(nameof(Index), new { mode = "User" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingUserPermissions = await _context.UserPermissions
                    .Where(p => p.UserId == model.SelectedUserId)
                    .ToListAsync();

                if (model.ModulePermissions != null && model.ModulePermissions.Any())
                {
                    foreach (var row in model.ModulePermissions)
                    {
                        var existing = existingUserPermissions.FirstOrDefault(p => p.ModuleName == row.ModuleName);
                        if (existing != null)
                        {
                            existing.CanView = row.CanView;
                            existing.CanCreate = row.CanCreate;
                            existing.CanEdit = row.CanEdit;
                            existing.CanDelete = row.CanDelete;
                            existing.CanApprove = row.CanApprove;
                            existing.IsAllowed = row.CanView;
                            existing.UpdatedAt = DateTime.UtcNow;
                        }
                        else
                        {
                            _context.UserPermissions.Add(new UserPermission
                            {
                                UserId = model.SelectedUserId,
                                ModuleName = row.ModuleName,
                                CanView = row.CanView,
                                CanCreate = row.CanCreate,
                                CanEdit = row.CanEdit,
                                CanDelete = row.CanDelete,
                                CanApprove = row.CanApprove,
                                IsAllowed = row.CanView,
                                UpdatedAt = DateTime.UtcNow
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == model.SelectedUserId);
                var userName = user?.FullName ?? user?.UserName ?? "User";

                _context.ActivityLogs.Add(new ActivityLog
                {
                    Title = "User Custom Permissions Saved",
                    Description = $"Direct user permissions for '{userName}' updated in bulk by admin.",
                    IconClass = "fa-user-gear",
                    ColorClass = "text-primary",
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"User-specific permissions for '{userName}' saved successfully!";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = $"Failed to save user permissions: {ex.Message}";
            }

            return RedirectToAction(nameof(Index), new { userId = model.SelectedUserId, mode = "User" });
        }

        // GET: /PagePermissions/GetRolePermissionsJson or /RolePermissions/GetRolePermissionsJson
        [HttpGet]
        [Route("PagePermissions/GetRolePermissionsJson")]
        [Route("RolePermissions/GetRolePermissionsJson")]
        public async Task<IActionResult> GetRolePermissionsJson(int roleId)
        {
            var permissions = await _context.RolePermissions
                .Where(p => p.RoleId == roleId)
                .ToListAsync();

            return Json(permissions.Select(p => new
            {
                p.RoleId,
                p.ModuleName,
                p.CanView,
                p.CanCreate,
                p.CanEdit,
                p.CanDelete,
                p.CanApprove
            }));
        }

        // GET: /PagePermissions/GetUserPermissionsJson or /RolePermissions/GetUserPermissionsJson
        [HttpGet]
        [Route("PagePermissions/GetUserPermissionsJson")]
        [Route("RolePermissions/GetUserPermissionsJson")]
        public async Task<IActionResult> GetUserPermissionsJson(int userId)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == userId);
            var userPermissions = await _context.UserPermissions
                .Where(p => p.UserId == userId)
                .ToListAsync();

            if (!userPermissions.Any() && user?.RoleId.HasValue == true)
            {
                // Fallback to user's assigned role defaults
                var rolePermissions = await _context.RolePermissions
                    .Where(p => p.RoleId == user.RoleId.Value)
                    .ToListAsync();

                return Json(rolePermissions.Select(p => new
                {
                    UserId = userId,
                    p.ModuleName,
                    p.CanView,
                    p.CanCreate,
                    p.CanEdit,
                    p.CanDelete,
                    p.CanApprove
                }));
            }

            return Json(userPermissions.Select(p => new
            {
                p.UserId,
                p.ModuleName,
                p.CanView,
                p.CanCreate,
                p.CanEdit,
                p.CanDelete,
                p.CanApprove
            }));
        }

        // POST: /RolePermissions/ResetUserPermissionsToRole
        [HttpPost]
        [Route("PagePermissions/ResetUserPermissionsToRole")]
        [Route("RolePermissions/ResetUserPermissionsToRole")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetUserPermissionsToRole(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index), new { mode = "User" });
            }

            var userPerms = await _context.UserPermissions.Where(up => up.UserId == userId).ToListAsync();
            if (userPerms.Any())
            {
                _context.UserPermissions.RemoveRange(userPerms);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = $"Permissions for '{user.FullName}' reset to Role defaults!";
            return RedirectToAction(nameof(Index), new { userId = userId, mode = "User" });
        }
    }
}
