using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_DepartmentTasks")]
    public class DepartmentTask
    {
        [Key]
        public int TaskId { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string AssignedToName { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        [EmailAddress]
        public string AssignedToEmail { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Priority { get; set; } = "Medium"; // Urgent, High, Medium, Low

        public DateTime DueDate { get; set; }

        public int ProgressPercentage { get; set; } = 0;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "In Progress";

        [Required]
        [StringLength(150)]
        public string AssignedBy { get; set; } = "Manager";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
