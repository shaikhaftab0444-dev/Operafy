using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("LeaveTypes")]
    public class LeaveType
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        public string Code { get; set; } = string.Empty; // CL, SL, EL, ML

        public int YearlyLimit { get; set; } = 12;

        public int CarryForwardLimit { get; set; } = 0;

        public bool IsEncashable { get; set; } = false;

        public string? EligibilityCriteria { get; set; }

        public bool IsActive { get; set; } = true;
    }

    [Table("LeaveRequests")]
    public class LeaveRequest
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public int LeaveTypeId { get; set; }

        [ForeignKey("LeaveTypeId")]
        public virtual LeaveType? LeaveType { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int TotalDays { get; set; }

        [Required]
        public string Reason { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        [MaxLength(500)]
        public string? ApproverRemarks { get; set; }

        [MaxLength(100)]
        public string? ActionedByUserId { get; set; }

        [MaxLength(50)]
        public string? ManagerStatus { get; set; } = "Pending";

        [MaxLength(500)]
        public string? ManagerRemarks { get; set; }

        [MaxLength(100)]
        public string? ReviewedBy { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ActionedAt { get; set; }
    }

    [Table("CompanyHolidays")]
    public class CompanyHoliday
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public DateTime HolidayDate { get; set; }

        [MaxLength(50)]
        public string DayOfWeek { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Type { get; set; } = "Mandatory"; // Mandatory, Optional, Restricted

        [MaxLength(500)]
        public string? Description { get; set; }

        public int Year { get; set; } = 2026;
    }

    public class UserLeaveBalanceViewModel
    {
        public int UserId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string FinancialYear { get; set; } = "2026-2027";

        // Leave Balances
        public int CasualLeaveAllowed { get; set; } = 12;
        public int CasualLeaveTaken { get; set; } = 0;
        public int CasualLeaveBalance => Math.Max(0, CasualLeaveAllowed - CasualLeaveTaken);

        public int SickLeaveAllowed { get; set; } = 8;
        public int SickLeaveTaken { get; set; } = 0;
        public int SickLeaveBalance => Math.Max(0, SickLeaveAllowed - SickLeaveTaken);

        public int EarnedLeaveAllowed { get; set; } = 15;
        public int EarnedLeaveTaken { get; set; } = 0;
        public int EarnedLeaveBalance => Math.Max(0, EarnedLeaveAllowed - EarnedLeaveTaken);

        public int TotalEntitled => CasualLeaveAllowed + SickLeaveAllowed + EarnedLeaveAllowed;
        public int TotalTaken => CasualLeaveTaken + SickLeaveTaken + EarnedLeaveTaken;
        public int TotalBalance => CasualLeaveBalance + SickLeaveBalance + EarnedLeaveBalance;
    }
}
