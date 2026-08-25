using System.Collections.Generic;

namespace ERP_System.Models
{
    public class RolePermissionMatrixViewModel
    {
        public int SelectedRoleId { get; set; }
        public string SelectedRoleName { get; set; } = string.Empty;
        public List<Role> Roles { get; set; } = new List<Role>();
        public List<ModulePermissionRow> ModulePermissions { get; set; } = new List<ModulePermissionRow>();
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
