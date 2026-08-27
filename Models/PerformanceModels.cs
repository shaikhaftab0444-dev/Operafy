using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_Okrs")]
    public class OkrObjective
    {
        [Key]
        public int OkrId { get; set; }

        [Required(ErrorMessage = "Objective Title is required")]
        [StringLength(200)]
        public string ObjectiveTitle { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public int? DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }

        public int? DesignationId { get; set; }

        [ForeignKey("DesignationId")]
        public Designation? Designation { get; set; }

        public int? EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public User? Employee { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Now;

        public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(3);

        [Required]
        [StringLength(50)]
        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical

        [Column(TypeName = "decimal(5,2)")]
        public decimal Weightage { get; set; } = 100.0m;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "In Progress"; // Not Started, In Progress, At Risk, Completed, Cancelled

        [Column(TypeName = "decimal(5,2)")]
        public decimal OverallProgressPercentage { get; set; } = 0.0m;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public List<KeyResult> KeyResults { get; set; } = new List<KeyResult>();
    }

    [Table("erp_KeyResults")]
    public class KeyResult
    {
        [Key]
        public int KeyResultId { get; set; }

        [Required]
        public int OkrId { get; set; }

        [ForeignKey("OkrId")]
        public OkrObjective? Okr { get; set; }

        [Required(ErrorMessage = "Key Result Name is required")]
        [StringLength(200)]
        public string KeyResultName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TargetValue { get; set; } = 100.0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentValue { get; set; } = 0.0m;

        [StringLength(50)]
        public string MeasurementUnit { get; set; } = "Percentage"; // Percentage, Amount, Number, Unit

        [Column(TypeName = "decimal(5,2)")]
        public decimal Weightage { get; set; } = 100.0m;

        [Column(TypeName = "decimal(5,2)")]
        public decimal ProgressPercentage { get; set; } = 0.0m;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "In Progress"; // Not Started, In Progress, At Risk, Completed, Cancelled
    }

    [Table("erp_Kpis")]
    public class KpiItem
    {
        [Key]
        public int KpiId { get; set; }

        [Required(ErrorMessage = "KPI Name is required")]
        [StringLength(150)]
        public string KpiName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public int? DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }

        public int? DesignationId { get; set; }

        [ForeignKey("DesignationId")]
        public Designation? Designation { get; set; }

        public int? EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public User? Employee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TargetValue { get; set; } = 100.0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal ActualAchievement { get; set; } = 0.0m;

        [Column(TypeName = "decimal(5,2)")]
        public decimal Weightage { get; set; } = 100.0m;

        [Required]
        [StringLength(50)]
        public string MeasurementType { get; set; } = "Percentage"; // Percentage, Number, Amount, Rating, Boolean

        [Required]
        [StringLength(50)]
        public string ReviewPeriod { get; set; } = "Quarterly"; // Monthly, Quarterly, Half-Yearly, Annual

        public DateTime StartDate { get; set; } = DateTime.Now;

        public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(3);

        [Column(TypeName = "decimal(5,2)")]
        public decimal AchievementPercentage { get; set; } = 0.0m;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Active"; // Active, Met, Exceeded, Needs Improvement, Inactive

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    [Table("erp_AppraisalCycles")]
    public class AppraisalCycle
    {
        [Key]
        public int CycleId { get; set; }

        [Required(ErrorMessage = "Cycle Name is required")]
        [StringLength(150)]
        public string CycleName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string ReviewType { get; set; } = "Annual"; // Monthly, Quarterly, Half-Yearly, Annual, Probation

        public DateTime StartDate { get; set; } = DateTime.Now;

        public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(1);

        public DateTime SelfReviewDeadline { get; set; } = DateTime.Now.AddDays(15);

        public DateTime ManagerReviewDeadline { get; set; } = DateTime.Now.AddDays(25);

        [StringLength(250)]
        public string ApplicableDepartmentIds { get; set; } = "All"; // Comma-separated IDs or "All"

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Active"; // Draft, Scheduled, Active, Under Review, Completed, Closed

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<EmployeeAppraisal> Appraisals { get; set; } = new List<EmployeeAppraisal>();
    }

    [Table("erp_EmployeeAppraisals")]
    public class EmployeeAppraisal
    {
        [Key]
        public int AppraisalId { get; set; }

        [Required]
        public int CycleId { get; set; }

        [ForeignKey("CycleId")]
        public AppraisalCycle? AppraisalCycle { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public User? Employee { get; set; }

        public int? ManagerId { get; set; }

        [ForeignKey("ManagerId")]
        public User? Manager { get; set; }

        public bool SelfReviewSubmitted { get; set; } = false;

        [Column(TypeName = "decimal(3,2)")]
        public decimal? SelfRating { get; set; } // 1.0 to 5.0

        [StringLength(1500)]
        public string? SelfComments { get; set; }

        public DateTime? SelfSubmittedAt { get; set; }

        public bool ManagerReviewSubmitted { get; set; } = false;

        [Column(TypeName = "decimal(3,2)")]
        public decimal? ManagerRating { get; set; } // 1.0 to 5.0

        [StringLength(1500)]
        public string? ManagerComments { get; set; }

        public DateTime? ManagerSubmittedAt { get; set; }

        [StringLength(1500)]
        public string? HRComments { get; set; }

        [Column(TypeName = "decimal(3,2)")]
        public decimal? GoalScore { get; set; } // 1.0 to 5.0 or calculated %

        [Column(TypeName = "decimal(3,2)")]
        public decimal? KpiScore { get; set; } // 1.0 to 5.0 or calculated %

        [Column(TypeName = "decimal(3,2)")]
        public decimal? FinalScore { get; set; } // Calculated 1.0 to 5.0

        [StringLength(100)]
        public string FinalRatingBand { get; set; } = "Pending Evaluation"; 
        // Outstanding (5.0), Exceeds Expectations (4.0 - 4.9), Meets Expectations (3.0 - 3.9), Needs Improvement (2.0 - 2.9), Poor (1.0 - 1.9)

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Self Review Pending"; 
        // Self Review Pending, Manager Review Pending, HR Review Pending, Finalized, Completed, Returned

        public bool EmployeeAcknowledged { get; set; } = false;

        public DateTime? AcknowledgedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }
}
