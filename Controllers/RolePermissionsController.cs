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
    public class RolePermissionUpdateDto
    {
        public string? RoleName { get; set; }
        public int? RoleId { get; set; }
        public string? ModuleKey { get; set; }
        public string? ModuleName { get; set; }
        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanApprove { get; set; }
    }

    [Authorize(Roles = "Super Admin,Admin")]
    public class RolePermissionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RolePermissionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(int? roleId, int? userId, string mode = "Role")
        {
            return RedirectToAction("Index", "PagePermissions", new { roleId, userId, mode });
        }

        [HttpPost]
        public async Task<IActionResult> SaveRolePermissions([FromBody] List<RolePermissionUpdateDto> updates)
        {
            if (updates == null || !updates.Any())
            {
                return Json(new { success = false, message = "No permissions provided." });
            }

            var roles = await _context.Roles.ToListAsync();

            foreach (var item in updates)
            {
                var modKey = !string.IsNullOrWhiteSpace(item.ModuleKey) ? item.ModuleKey : item.ModuleName;
                if (string.IsNullOrWhiteSpace(modKey)) continue;

                int targetRoleId = 0;
                if (item.RoleId.HasValue && item.RoleId.Value > 0)
                {
                    targetRoleId = item.RoleId.Value;
                }
                else if (!string.IsNullOrWhiteSpace(item.RoleName))
                {
                    var role = roles.FirstOrDefault(r => r.RoleName.Equals(item.RoleName, StringComparison.OrdinalIgnoreCase));
                    if (role != null) targetRoleId = role.RoleId;
                }

                if (targetRoleId <= 0) continue;

                var perm = await _context.RolePermissions
                    .FirstOrDefaultAsync(p => p.RoleId == targetRoleId && p.ModuleName == modKey);

                if (perm == null)
                {
                    perm = new RolePermission
                    {
                        RoleId = targetRoleId,
                        ModuleName = modKey,
                        CanView = item.CanView,
                        CanCreate = item.CanCreate,
                        CanEdit = item.CanEdit,
                        CanDelete = item.CanDelete,
                        CanApprove = item.CanApprove,
                        IsAllowed = item.CanView
                    };
                    _context.RolePermissions.Add(perm);
                }
                else
                {
                    perm.CanView = item.CanView;
                    perm.CanCreate = item.CanCreate;
                    perm.CanEdit = item.CanEdit;
                    perm.CanDelete = item.CanDelete;
                    perm.CanApprove = item.CanApprove;
                    perm.IsAllowed = item.CanView;
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Permissions synced with database." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SavePermissions(RolePermissionMatrixViewModel model)
        {
            return RedirectToAction("SavePermissions", "PagePermissions", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveUserPermissions(RolePermissionMatrixViewModel model)
        {
            return RedirectToAction("SaveUserPermissions", "PagePermissions", model);
        }
    }
}
