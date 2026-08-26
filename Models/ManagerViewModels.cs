using System;
using System.Collections.Generic;

namespace ERP_System.Models
{
    public class TeamMemberAttendanceStatus
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = "Absent"; // On Duty, Late, On Leave, On Break, Absent
        public string InTime { get; set; } = "--:--";
        public string AvatarInitials { get; set; } = string.Empty;
    }

    public class PendingLeaveRequest
    {
        public int LeaveApplicationId { get; set; }
        public int UserId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string LeaveType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
    }

    public class PendingRegularizationRequest
    {
        public int PunchId { get; set; }
        public int UserId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
    }

    public class PendingExpenseClaim
    {
        public int ExpenseClaimId { get; set; }
        public int UserId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string ExpenseType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime ClaimDate { get; set; }
        public string Status { get; set; } = "Pending";
    }

    public class TeamTaskItem
    {
        public int TaskId { get; set; }
        public int UserId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public string AssignedMember { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public string Priority { get; set; } = "Medium"; // Urgent, Medium, Low
        public int ProgressPercent { get; set; }
        public string Status { get; set; } = "Pending";
    }
}
