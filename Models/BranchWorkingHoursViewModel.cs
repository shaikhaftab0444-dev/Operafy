using System.Collections.Generic;

namespace ERP_System.Models
{
    public class BranchWorkingHoursViewModel
    {
        public AdminBranchHour Hour { get; set; } = new AdminBranchHour();
        public List<AdminBranchHour> Hours { get; set; } = new List<AdminBranchHour>();
        public List<Branch> Branches { get; set; } = new List<Branch>();
        
        public string SearchTerm { get; set; } = string.Empty;
    }
}
