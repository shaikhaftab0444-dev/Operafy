using System;
using System.Collections.Generic;

namespace ERP_System.Models
{
    public class ManagerDashboardViewModel
    {
        public int TotalTeamCount { get; set; }
        public int PresentTodayCount { get; set; }
        public int PendingApprovalsCount { get; set; }
        public int ActiveTasksCount { get; set; }
        public int DelayedTasksCount { get; set; }
        public string ProductivityRate { get; set; } = "0.0%";

        public List<TeamMemberStatus> TeamAttendance { get; set; } = new();
        public List<ApprovalItem> PendingApprovals { get; set; } = new();
        public List<ManagerTaskItem> TeamTasks { get; set; } = new();
    }

    public class TeamMemberStatus
    {
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Present, On Leave, Late
        public string ClockInTime { get; set; } = "N/A";
        public string Avatar { get; set; } = string.Empty;
        public string StatusColor { get; set; } = "secondary"; // success, danger, warning
    }

    public class ApprovalItem
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Casual Leave, Attendance Regularization, Travel Reimbursement
        public string Dates { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string RequestedOn { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
    }

    public class ManagerTaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string AssignedTo { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium"; // Urgent, High, Medium, Low
        public string DueDate { get; set; } = string.Empty;
        public int Progress { get; set; } // Percentage (0-100)
        public string Status { get; set; } = "Pending"; // In Progress, Review, Delayed, Completed
    }
}
