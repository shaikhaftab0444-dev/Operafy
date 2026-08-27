using System.Collections.Generic;

namespace ERP_System.Models
{
    public class EmployeeDashboardViewModel
    {
        public User? CurrentUser { get; set; }
        public List<Payslip> RecentPayslips { get; set; } = new List<Payslip>();
        public List<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
