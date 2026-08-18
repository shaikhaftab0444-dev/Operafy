using System.Collections.Generic;

namespace ERP_System.Models
{
    public class HRDashboardViewModel
    {
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public int InactiveEmployees { get; set; }
        public int LockedAccounts { get; set; }

        public List<User> EmployeesList { get; set; } = new List<User>();
        public List<User> RecentHires { get; set; } = new List<User>();
        public List<RoleDistributionItem> RoleDistribution { get; set; } = new List<RoleDistributionItem>();
    }

    public class RoleDistributionItem
    {
        public string RoleName { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
