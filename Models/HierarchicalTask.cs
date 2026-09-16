using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_HierarchicalTasks")]
    public class HierarchicalTask
    {
        [Key]
        public int Id { get; set; }
        public string? Title { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
        public string? TaskType { get; set; } = "MANAGER_TO_EMPLOYEE";
        public string? AssignedByUserId { get; set; }
        public string? AssignedToUserId { get; set; } // Nullable when task is a General Team Task

        public bool IsGeneralTask { get; set; } = false; // True if visible to all team members
        public string? TargetWarehouseLocation { get; set; }

        public string? Priority { get; set; } = "Medium";
        public string? Status { get; set; } = "Pending";
        public int ProgressPercentage { get; set; } = 0;
        public DateTime? DueDate { get; set; } = DateTime.Today.AddDays(7);
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public virtual ApplicationUser? AssignedToUser { get; set; }

        [NotMapped]
        public virtual ApplicationUser? AssignedByUser { get; set; }
    }
}
