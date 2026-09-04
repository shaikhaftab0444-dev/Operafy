using System.Collections.Generic;

namespace ERP_System.Models
{
    public class RolePermissionMatrixViewModel
    {
        public string ActiveMode { get; set; } = "Role"; // "Role" or "User"

        // Role Context
        public int SelectedRoleId { get; set; }
        public string SelectedRoleName { get; set; } = string.Empty;
        public List<Role> Roles { get; set; } = new List<Role>();

        // User Context
        public int SelectedUserId { get; set; }
        public string SelectedUserName { get; set; } = string.Empty;
        public string SelectedUserRole { get; set; } = string.Empty;
        public List<UserItemViewModel> Users { get; set; } = new List<UserItemViewModel>();

        // Permissions Matrix Rows
        public List<ModulePermissionRow> ModulePermissions { get; set; } = new List<ModulePermissionRow>();
    }

    public class UserItemViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string? ProfilePhoto { get; set; }
    }

    public class ModulePermissionRow
    {
        public string ModuleName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        
        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanApprove { get; set; }
    }
}
