using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_HRAttendanceLogs")]
    public class HRAttendanceLog
    {
        [Key]
        public int LogId { get; set; }

        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string EmployeeName { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public DateTime? CheckInTime { get; set; }

        public DateTime? CheckOutTime { get; set; }

        [StringLength(50)]
        public string WorkHours { get; set; } = "0h 0m";

        [Required]
        [StringLength(100)]
        public string PunchSource { get; set; } = "Web Clock";

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Present (On Time)"; // Present (On Time), Late Check-in, Early Departure, Half Day, Absent, On Leave

        [StringLength(255)]
        public string? Remarks { get; set; }
    }

    [Table("erp_HRBiometricDevices")]
    public class HRBiometricDevice
    {
        [Key]
        public int DeviceId { get; set; }

        [Required]
        [StringLength(150)]
        public string DeviceName { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string IpOrLocation { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string ConnectionStatus { get; set; } = "Connected"; // Connected, Offline, Syncing

        public DateTime LastSyncTime { get; set; } = DateTime.Now;

        public int TodaySyncCount { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }

    [Table("erp_HRShiftRosters")]
    public class HRShiftRoster
    {
        [Key]
        public int RosterId { get; set; }

        public int UserId { get; set; }

        [Required]
        [StringLength(150)]
        public string EmployeeName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ShiftName { get; set; } = "General Shift (Day)"; // General Shift (Day), Morning Shift, Evening Shift, Night Shift

        [Required]
        [StringLength(100)]
        public string Timings { get; set; } = "09:00 AM - 06:00 PM";

        [Required]
        [StringLength(100)]
        public string WeeklyOffs { get; set; } = "Sunday";

        public DateTime EffectiveDate { get; set; } = DateTime.Today;

        [StringLength(255)]
        public string? Notes { get; set; }
    }

    [Table("erp_HROvertimeRecords")]
    public class HROvertimeRecord
    {
        [Key]
        public int OvertimeId { get; set; }

        public int UserId { get; set; }

        [Required]
        [StringLength(150)]
        public string EmployeeName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string MonthYear { get; set; } = string.Empty; // e.g. "August 2026"

        public int StandardHours { get; set; } = 160;

        public int HoursLogged { get; set; } = 160;

        public int OvertimeHours { get; set; } = 0;

        [Required]
        [StringLength(20)]
        public string Multiplier { get; set; } = "1.5x";

        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; } = 250.00m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalOvertimePay { get; set; } = 0.00m;

        [Required]
        [StringLength(50)]
        public string PayoutStatus { get; set; } = "Pending Monthly Cycle"; // Pending Monthly Cycle, Approved for Payroll, Paid, Rejected
    }

    [Table("erp_HRAttendanceRegularizations")]
    public class HRAttendanceRegularization
    {
        [Key]
        public int RequestId { get; set; }

        public int UserId { get; set; }

        [Required]
        [StringLength(150)]
        public string EmployeeName { get; set; } = string.Empty;

        public DateTime CorrectionDate { get; set; }

        [Required]
        [StringLength(100)]
        public string IncorrectPunch { get; set; } = "Missing Check-out";

        [Required]
        [StringLength(100)]
        public string RequestedCorrectTime { get; set; } = "06:00 PM Check-out";

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        public DateTime RequestDate { get; set; } = DateTime.Today;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending Review"; // Pending Review, Approved, Rejected

        [StringLength(255)]
        public string? AdminRemarks { get; set; }

        [StringLength(50)]
        public string? ManagerStatus { get; set; } = "Pending";

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(255)]
        public string? ManagerRemarks { get; set; }

        [StringLength(150)]
        public string? ReviewedBy { get; set; }

        public DateTime? ReviewedAt { get; set; }
    }

    // View Models for Pages
    public class DailyLogsViewModel
    {
        public List<HRAttendanceLog> Logs { get; set; } = new();
        public List<User> Employees { get; set; } = new();
        public DateTime SelectedDate { get; set; } = DateTime.Today;
        public string SearchTerm { get; set; } = string.Empty;
        public string StatusFilter { get; set; } = "All";

        public int TotalPresent { get; set; }
        public int TotalLate { get; set; }
        public int TotalOnLeave { get; set; }
        public int TotalAbsent { get; set; }
    }

    public class BiometricViewModel
    {
        public List<HRBiometricDevice> Devices { get; set; } = new();
        public List<HRAttendanceLog> LivePunches { get; set; } = new();
        public int TotalPunchesToday { get; set; }
        public int ActiveDevicesCount { get; set; }
    }

    public class ShiftSchedulingViewModel
    {
        public List<HRShiftRoster> Rosters { get; set; } = new();
        public List<User> Employees { get; set; } = new();
    }

    public class OvertimeViewModel
    {
        public List<HROvertimeRecord> OvertimeRecords { get; set; } = new();
        public List<User> Employees { get; set; } = new();
        public string SelectedMonth { get; set; } = "August 2026";
        public decimal TotalOvertimePay { get; set; }
        public int PendingApprovalsCount { get; set; }
    }

    public class RegularizationViewModel
    {
        public List<HRAttendanceRegularization> Requests { get; set; } = new();
        public List<User> Employees { get; set; } = new();
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
    }

    public class HRLeaveApplicationViewModel
    {
        public int LeaveApplicationId { get; set; }
        public int UserId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string LeaveType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public string? AdminRemarks { get; set; }
    }
}
