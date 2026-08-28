using System;
using System.Collections.Generic;

namespace ERP_System.Models
{
    public class TaskDelegationViewModel
    {
        public List<DepartmentTask> Tasks { get; set; } = new();
        public int TotalTasksCount { get; set; }
        public int InProgressCount { get; set; }
        public int InReviewCount { get; set; }
        public int DelayedCount { get; set; }
        public int CompletedCount { get; set; }
        public List<TeamMemberDropdownItem> TeamMembers { get; set; } = new();
    }

    public class CreateTaskInputModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AssignedToName { get; set; } = string.Empty;
        public string AssignedToEmail { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium";
        public DateTime DueDate { get; set; }
    }

    public class TeamMemberDropdownItem
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
