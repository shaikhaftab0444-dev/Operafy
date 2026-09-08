using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_WorkShifts")]
    public class WorkShift
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string ShiftName { get; set; } = string.Empty;

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public int GracePeriodMinutes { get; set; } = 15;
    }
}
