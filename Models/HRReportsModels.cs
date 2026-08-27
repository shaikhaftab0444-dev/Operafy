using System;
using System.Collections.Generic;

namespace ERP_System.Models
{
    // ==========================================
    // 1. ATTENDANCE & LATE REPORT VIEW MODEL
    // ==========================================
    public class AttendanceLateReportViewModel
    {
        public List<HRAttendanceLog> Logs { get; set; } = new List<HRAttendanceLog>();
        public List<User> Employees { get; set; } = new List<User>();
        public List<Department> Departments { get; set; } = new List<Department>();

        // Filter Inputs
        public string SearchTerm { get; set; } = string.Empty;
        public int? SelectedDepartmentId { get; set; }
        public string SelectedStatus { get; set; } = "All";
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime EndDate { get; set; } = DateTime.Today;

        // Metric Summary Totals
        public int TotalWorkingDays { get; set; } = 30;
        public int TotalPresent { get; set; } = 0;
        public int TotalAbsent { get; set; } = 0;
        public int TotalLateInstances { get; set; } = 0;
        public int TotalLateMinutes { get; set; } = 0;
        public decimal AvgDailyWorkHours { get; set; } = 8.5m;
        public int OvertimeHours { get; set; } = 0;
        public string AvgCheckInTime { get; set; } = "09:15 AM";
    }

    // ==========================================
    // 2. PAYROLL SUMMARY REPORT VIEW MODEL
    // ==========================================
    public class PayrollSummaryReportViewModel
    {
        public List<Payslip> Payslips { get; set; } = new List<Payslip>();
        public List<PayrollRun> PayrollRuns { get; set; } = new List<PayrollRun>();
        public List<Department> Departments { get; set; } = new List<Department>();

        // Filter Inputs
        public string SelectedPayPeriod { get; set; } = "August 2026";
        public int? SelectedDepartmentId { get; set; }
        public string SelectedStatus { get; set; } = "All";
        public string SearchTerm { get; set; } = string.Empty;

        // Metric Summary Totals
        public int TotalEmployeesPaid { get; set; } = 0;
        public decimal TotalGrossSalary { get; set; } = 0.00m;
        public decimal TotalAllowances { get; set; } = 0.00m;
        public decimal TotalDeductions { get; set; } = 0.00m;
        public decimal TotalEmployerPF { get; set; } = 0.00m;
        public decimal TotalEmployerESI { get; set; } = 0.00m;
        public decimal TotalTDS { get; set; } = 0.00m;
        public decimal TotalNetSalary { get; set; } = 0.00m;
    }

    // ==========================================
    // 3. LEAVE BALANCE REPORT VIEW MODEL
    // ==========================================
    public class EmployeeLeaveBalanceItem
    {
        public int UserId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string DesignationTitle { get; set; } = string.Empty;

        public int CasualLeaveAllocated { get; set; } = 12;
        public int CasualLeaveUsed { get; set; } = 0;
        public int CasualLeavePending { get; set; } = 0;
        public int CasualLeaveBalance => CasualLeaveAllocated - CasualLeaveUsed;

        public int SickLeaveAllocated { get; set; } = 12;
        public int SickLeaveUsed { get; set; } = 0;
        public int SickLeavePending { get; set; } = 0;
        public int SickLeaveBalance => SickLeaveAllocated - SickLeaveUsed;

        public int EarnedLeaveAllocated { get; set; } = 15;
        public int EarnedLeaveUsed { get; set; } = 0;
        public int EarnedLeavePending { get; set; } = 0;
        public int EarnedLeaveBalance => EarnedLeaveAllocated - EarnedLeaveUsed;

        public int TotalAllocated => CasualLeaveAllocated + SickLeaveAllocated + EarnedLeaveAllocated;
        public int TotalUsed => CasualLeaveUsed + SickLeaveUsed + EarnedLeaveUsed;
        public int TotalRemainingBalance => TotalAllocated - TotalUsed;
    }

    public class LeaveBalanceReportViewModel
    {
        public List<EmployeeLeaveBalanceItem> EmployeeBalances { get; set; } = new List<EmployeeLeaveBalanceItem>();
        public List<ESSLeaveApplication> LeaveApplications { get; set; } = new List<ESSLeaveApplication>();
        public List<Department> Departments { get; set; } = new List<Department>();

        // Filter Inputs
        public int? SelectedDepartmentId { get; set; }
        public string SelectedLeaveType { get; set; } = "All";
        public string SearchTerm { get; set; } = string.Empty;
        public int Year { get; set; } = 2026;

        // Metric Summary Totals
        public int TotalEmployeesCount { get; set; } = 0;
        public int TotalLeavesAllocated { get; set; } = 0;
        public int TotalLeavesTaken { get; set; } = 0;
        public int TotalPendingRequests { get; set; } = 0;
        public int TotalRemainingBalance { get; set; } = 0;
    }

    // ==========================================
    // 4. ATTRITION & HEADCOUNT REPORT VIEW MODEL
    // ==========================================
    public class DepartmentHeadcountItem
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int OpeningHeadcount { get; set; } = 0;
        public int NewHiresCount { get; set; } = 0;
        public int ExitsCount { get; set; } = 0;
        public int EndingHeadcount { get; set; } = 0;
        public int NetGrowth => NewHiresCount - ExitsCount;
        public decimal AttritionRatePct { get; set; } = 0.0m;
    }

    public class AttritionHeadcountReportViewModel
    {
        public List<DepartmentHeadcountItem> DepartmentSummaries { get; set; } = new List<DepartmentHeadcountItem>();
        public List<HROffboarding> ExitRecords { get; set; } = new List<HROffboarding>();
        public List<User> ActiveEmployees { get; set; } = new List<User>();
        public List<Department> Departments { get; set; } = new List<Department>();

        // Filter Inputs
        public int? SelectedDepartmentId { get; set; }
        public int SelectedYear { get; set; } = 2026;
        public string SearchTerm { get; set; } = string.Empty;

        // Metric Summary Totals
        public int CurrentHeadcount { get; set; } = 0;
        public int NewHiresYTD { get; set; } = 0;
        public int ExitsYTD { get; set; } = 0;
        public int NetGrowthYTD => NewHiresYTD - ExitsYTD;
        public decimal AnnualAttritionRatePct { get; set; } = 0.0m;
    }

    // ==========================================
    // 5. TAX DEDUCTION REPORT VIEW MODEL
    // ==========================================
    public class EmployeeTaxDeductionItem
    {
        public int UserId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string PanNumber { get; set; } = "ABCDE1234F";
        public string TaxRegime { get; set; } = "New Tax Regime";

        public decimal GrossSalary { get; set; } = 0.00m;
        public decimal MonthlyTDS { get; set; } = 0.00m;
        public decimal ProfessionalTax { get; set; } = 0.00m;
        public decimal EmployeePF { get; set; } = 0.00m;
        public decimal EmployerPF { get; set; } = 0.00m;
        public decimal EmployeeESI { get; set; } = 0.00m;
        public decimal EmployerESI { get; set; } = 0.00m;
        public decimal TotalStatutoryDeduction => MonthlyTDS + ProfessionalTax + EmployeePF + EmployeeESI;
    }

    public class TaxDeductionReportViewModel
    {
        public List<EmployeeTaxDeductionItem> TaxItems { get; set; } = new List<EmployeeTaxDeductionItem>();
        public List<Department> Departments { get; set; } = new List<Department>();

        // Filter Inputs
        public string SelectedPayPeriod { get; set; } = "August 2026";
        public int? SelectedDepartmentId { get; set; }
        public string SearchTerm { get; set; } = string.Empty;

        // Metric Summary Totals
        public int TotalEmployeesTaxed { get; set; } = 0;
        public decimal TotalTDS { get; set; } = 0.00m;
        public decimal TotalProfessionalTax { get; set; } = 0.00m;
        public decimal TotalEmployeePF { get; set; } = 0.00m;
        public decimal TotalEmployerPF { get; set; } = 0.00m;
        public decimal TotalESI { get; set; } = 0.00m;
        public decimal TotalDeductions { get; set; } = 0.00m;
    }
}
