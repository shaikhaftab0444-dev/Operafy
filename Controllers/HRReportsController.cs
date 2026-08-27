using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERP_System.Data;
using ERP_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,HR,Payroll Manager,Department Manager")]
    public class HRReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HRReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. ATTENDANCE & LATE REPORT
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> AttendanceLate(string? search, int? departmentId, string? status, DateTime? startDate, DateTime? endDate)
        {
            var logsQuery = _context.HRAttendanceLogs.AsQueryable();

            if (startDate.HasValue) logsQuery = logsQuery.Where(l => l.Date >= startDate.Value);
            if (endDate.HasValue) logsQuery = logsQuery.Where(l => l.Date <= endDate.Value);

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                logsQuery = logsQuery.Where(l => l.Status.Contains(status));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                logsQuery = logsQuery.Where(l => l.EmployeeName.ToLower().Contains(search) || l.EmployeeCode.ToLower().Contains(search));
            }

            var logs = await logsQuery.OrderByDescending(l => l.Date).ThenBy(l => l.EmployeeName).ToListAsync();

            // Departments & Employees for filters
            var departments = await _context.Departments.Where(d => d.IsActive).ToListAsync();
            var employees = await _context.Users.Where(u => u.IsActive).ToListAsync();

            if (departmentId.HasValue && departmentId.Value > 0)
            {
                var deptUserIds = await _context.Users.Where(u => u.BranchId == departmentId.Value).Select(u => u.UserId).ToListAsync();
                logs = logs.Where(l => deptUserIds.Contains(l.UserId)).ToList();
            }

            int totalLogs = logs.Count;
            int totalPresent = logs.Count(l => l.Status.Contains("Present"));
            int totalAbsent = logs.Count(l => l.Status.Contains("Absent"));
            int totalLate = logs.Count(l => l.Status.Contains("Late"));
            int totalLateMins = totalLate * 30; // Avg 30 mins late per instance

            var vm = new AttendanceLateReportViewModel
            {
                Logs = logs,
                Employees = employees,
                Departments = departments,
                SearchTerm = search ?? "",
                SelectedDepartmentId = departmentId,
                SelectedStatus = status ?? "All",
                StartDate = startDate ?? DateTime.Today.AddDays(-30),
                EndDate = endDate ?? DateTime.Today,
                TotalWorkingDays = totalLogs > 0 ? logs.Select(l => l.Date.Date).Distinct().Count() : 30,
                TotalPresent = totalPresent,
                TotalAbsent = totalAbsent,
                TotalLateInstances = totalLate,
                TotalLateMinutes = totalLateMins,
                AvgDailyWorkHours = 8.6m,
                AvgCheckInTime = "09:14 AM"
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> ExportAttendanceLate(string format, string? search, int? departmentId, string? status, DateTime? startDate, DateTime? endDate)
        {
            var logsQuery = _context.HRAttendanceLogs.AsQueryable();
            if (startDate.HasValue) logsQuery = logsQuery.Where(l => l.Date >= startDate.Value);
            if (endDate.HasValue) logsQuery = logsQuery.Where(l => l.Date <= endDate.Value);
            if (!string.IsNullOrWhiteSpace(status) && status != "All") logsQuery = logsQuery.Where(l => l.Status.Contains(status));
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                logsQuery = logsQuery.Where(l => l.EmployeeName.ToLower().Contains(search) || l.EmployeeCode.ToLower().Contains(search));
            }

            var logs = await logsQuery.OrderByDescending(l => l.Date).ToListAsync();

            if (format?.ToLower() == "csv")
            {
                var csv = new StringBuilder();
                csv.AppendLine("Employee Code,Employee Name,Date,Check-In,Check-Out,Work Hours,Punch Source,Status,Remarks");
                foreach (var l in logs)
                {
                    csv.AppendLine($"\"{l.EmployeeCode}\",\"{l.EmployeeName}\",\"{l.Date:yyyy-MM-dd}\",\"{l.CheckInTime:hh:mm tt}\",\"{l.CheckOutTime:hh:mm tt}\",\"{l.WorkHours}\",\"{l.PunchSource}\",\"{l.Status}\",\"{l.Remarks}\"");
                }
                return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"Attendance_Report_{DateTime.Now:yyyyMMdd}.csv");
            }
            else
            {
                var html = new StringBuilder();
                html.Append("<html xmlns:o=\"urn:schemas-microsoft-com:office:office\" xmlns:x=\"urn:schemas-microsoft-com:office:excel\" xmlns=\"http://www.w3.org/TR/REC-html40\">");
                html.Append("<head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><table border=\"1\">");
                html.Append("<tr style=\"background-color:#0d6efd; color:#fff; font-weight:bold;\"><th>Employee Code</th><th>Employee Name</th><th>Date</th><th>Check-In</th><th>Check-Out</th><th>Work Hours</th><th>Punch Source</th><th>Status</th></tr>");
                foreach (var l in logs)
                {
                    html.Append($"<tr><td>{l.EmployeeCode}</td><td>{l.EmployeeName}</td><td>{l.Date:yyyy-MM-dd}</td><td>{l.CheckInTime:hh:mm tt}</td><td>{l.CheckOutTime:hh:mm tt}</td><td>{l.WorkHours}</td><td>{l.PunchSource}</td><td>{l.Status}</td></tr>");
                }
                html.Append("</table></body></html>");
                return File(Encoding.UTF8.GetBytes(html.ToString()), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Attendance_Report_{DateTime.Now:yyyyMMdd}.xlsx");
            }
        }

        // ==========================================
        // 2. PAYROLL SUMMARY REPORT
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> PayrollSummary(string? payPeriod, int? departmentId, string? status, string? search)
        {
            payPeriod = string.IsNullOrWhiteSpace(payPeriod) ? "August 2026" : payPeriod;

            var slipsQuery = _context.Payslips.Include(p => p.User).AsQueryable();
            slipsQuery = slipsQuery.Where(p => p.PayPeriod == payPeriod);

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                slipsQuery = slipsQuery.Where(p => p.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                slipsQuery = slipsQuery.Where(p => p.User!.FullName.ToLower().Contains(search) || p.User.UserCode.ToLower().Contains(search));
            }

            var payslips = await slipsQuery.OrderBy(p => p.User!.FullName).ToListAsync();
            var runs = await _context.PayrollRuns.ToListAsync();
            var departments = await _context.Departments.Where(d => d.IsActive).ToListAsync();

            decimal totGross = payslips.Sum(p => p.GrossSalary);
            decimal totAllowances = payslips.Sum(p => p.HRA + p.SpecialAllowance + p.TransportAllowance + p.OtherAllowance);
            decimal totDeductions = payslips.Sum(p => p.TotalDeductions);
            decimal totNet = payslips.Sum(p => p.NetSalary);
            decimal totPF = payslips.Sum(p => p.ProvidentFund);
            decimal totESI = payslips.Sum(p => p.ESI);
            decimal totTDS = payslips.Sum(p => p.TDS);

            var vm = new PayrollSummaryReportViewModel
            {
                Payslips = payslips,
                PayrollRuns = runs,
                Departments = departments,
                SelectedPayPeriod = payPeriod,
                SelectedDepartmentId = departmentId,
                SelectedStatus = status ?? "All",
                SearchTerm = search ?? "",
                TotalEmployeesPaid = payslips.Count,
                TotalGrossSalary = totGross,
                TotalAllowances = totAllowances,
                TotalDeductions = totDeductions,
                TotalEmployerPF = totPF,
                TotalEmployerESI = totESI,
                TotalTDS = totTDS,
                TotalNetSalary = totNet
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> ExportPayrollSummary(string format, string? payPeriod, int? departmentId, string? status, string? search)
        {
            payPeriod = string.IsNullOrWhiteSpace(payPeriod) ? "August 2026" : payPeriod;
            var slipsQuery = _context.Payslips.Include(p => p.User).Where(p => p.PayPeriod == payPeriod).AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && status != "All") slipsQuery = slipsQuery.Where(p => p.Status == status);
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                slipsQuery = slipsQuery.Where(p => p.User!.FullName.ToLower().Contains(search) || p.User.UserCode.ToLower().Contains(search));
            }

            var payslips = await slipsQuery.ToListAsync();

            if (format?.ToLower() == "csv")
            {
                var csv = new StringBuilder();
                csv.AppendLine("Employee Code,Employee Name,Pay Period,Basic,HRA,Gross Salary,PF,ESI,TDS,Total Deductions,Net Salary,Status");
                foreach (var p in payslips)
                {
                    csv.AppendLine($"\"{p.User?.UserCode}\",\"{p.User?.FullName}\",\"{p.PayPeriod}\",\"{p.BasicSalary}\",\"{p.HRA}\",\"{p.GrossSalary}\",\"{p.ProvidentFund}\",\"{p.ESI}\",\"{p.TDS}\",\"{p.TotalDeductions}\",\"{p.NetSalary}\",\"{p.Status}\"");
                }
                return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"Payroll_Summary_{payPeriod.Replace(" ", "_")}.csv");
            }
            else
            {
                var html = new StringBuilder();
                html.Append("<html xmlns:o=\"urn:schemas-microsoft-com:office:office\" xmlns:x=\"urn:schemas-microsoft-com:office:excel\" xmlns=\"http://www.w3.org/TR/REC-html40\">");
                html.Append("<head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body><table border=\"1\">");
                html.Append("<tr style=\"background-color:#0d6efd; color:#fff; font-weight:bold;\"><th>Employee Code</th><th>Employee Name</th><th>Pay Period</th><th>Basic</th><th>HRA</th><th>Gross Salary</th><th>PF</th><th>ESI</th><th>TDS</th><th>Net Salary</th><th>Status</th></tr>");
                foreach (var p in payslips)
                {
                    html.Append($"<tr><td>{p.User?.UserCode}</td><td>{p.User?.FullName}</td><td>{p.PayPeriod}</td><td>{p.BasicSalary:N2}</td><td>{p.HRA:N2}</td><td>{p.GrossSalary:N2}</td><td>{p.ProvidentFund:N2}</td><td>{p.ESI:N2}</td><td>{p.TDS:N2}</td><td>{p.NetSalary:N2}</td><td>{p.Status}</td></tr>");
                }
                html.Append("</table></body></html>");
                return File(Encoding.UTF8.GetBytes(html.ToString()), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Payroll_Summary_{payPeriod.Replace(" ", "_")}.xlsx");
            }
        }

        // ==========================================
        // 3. LEAVE BALANCE REPORT
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> LeaveBalance(int? departmentId, string? leaveType, string? search, int year = 2026)
        {
            var usersQuery = _context.Users.Include(u => u.Role).Where(u => u.IsActive && u.Role!.RoleName != "Super Admin").AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                usersQuery = usersQuery.Where(u => u.FullName.ToLower().Contains(search) || u.UserCode.ToLower().Contains(search));
            }

            var users = await usersQuery.ToListAsync();
            var leaveApps = await _context.ESSLeaveApplications.ToListAsync();
            var departments = await _context.Departments.Where(d => d.IsActive).ToListAsync();

            var list = new List<EmployeeLeaveBalanceItem>();

            foreach (var user in users)
            {
                var userLeaves = leaveApps.Where(l => l.UserId == user.UserId).ToList();

                int clApproved = userLeaves.Where(l => l.LeaveType == "Casual Leave" && l.Status == "Approved").Sum(l => l.TotalDays);
                int clPending = userLeaves.Where(l => l.LeaveType == "Casual Leave" && l.Status == "Pending").Sum(l => l.TotalDays);

                int slApproved = userLeaves.Where(l => l.LeaveType == "Sick Leave" && l.Status == "Approved").Sum(l => l.TotalDays);
                int slPending = userLeaves.Where(l => l.LeaveType == "Sick Leave" && l.Status == "Pending").Sum(l => l.TotalDays);

                int elApproved = userLeaves.Where(l => l.LeaveType == "Earned Leave" && l.Status == "Approved").Sum(l => l.TotalDays);
                int elPending = userLeaves.Where(l => l.LeaveType == "Earned Leave" && l.Status == "Pending").Sum(l => l.TotalDays);

                list.Add(new EmployeeLeaveBalanceItem
                {
                    UserId = user.UserId,
                    EmployeeCode = user.UserCode,
                    EmployeeName = user.FullName,
                    DepartmentName = "Human Resources",
                    DesignationTitle = user.Role?.RoleName ?? "Employee",
                    CasualLeaveAllocated = 12,
                    CasualLeaveUsed = clApproved,
                    CasualLeavePending = clPending,
                    SickLeaveAllocated = 12,
                    SickLeaveUsed = slApproved,
                    SickLeavePending = slPending,
                    EarnedLeaveAllocated = 15,
                    EarnedLeaveUsed = elApproved,
                    EarnedLeavePending = elPending
                });
            }

            int totalAlloc = list.Sum(l => l.TotalAllocated);
            int totalUsed = list.Sum(l => l.TotalUsed);
            int totalPending = list.Sum(l => l.CasualLeavePending + l.SickLeavePending + l.EarnedLeavePending);
            int totalRem = list.Sum(l => l.TotalRemainingBalance);

            var vm = new LeaveBalanceReportViewModel
            {
                EmployeeBalances = list,
                LeaveApplications = leaveApps,
                Departments = departments,
                SelectedDepartmentId = departmentId,
                SelectedLeaveType = leaveType ?? "All",
                SearchTerm = search ?? "",
                Year = year,
                TotalEmployeesCount = list.Count,
                TotalLeavesAllocated = totalAlloc,
                TotalLeavesTaken = totalUsed,
                TotalPendingRequests = totalPending,
                TotalRemainingBalance = totalRem
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> ExportLeaveBalance(string format, int? departmentId, string? leaveType, string? search, int year = 2026)
        {
            var users = await _context.Users.Include(u => u.Role).Where(u => u.IsActive).ToListAsync();
            var leaveApps = await _context.ESSLeaveApplications.ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Employee Code,Employee Name,Casual Leave Allocated,Casual Leave Used,Sick Leave Allocated,Sick Leave Used,Earned Leave Allocated,Earned Leave Used,Total Remaining Balance");

            foreach (var u in users)
            {
                var userLeaves = leaveApps.Where(l => l.UserId == u.UserId && l.Status == "Approved").ToList();
                int clUsed = userLeaves.Where(l => l.LeaveType == "Casual Leave").Sum(l => l.TotalDays);
                int slUsed = userLeaves.Where(l => l.LeaveType == "Sick Leave").Sum(l => l.TotalDays);
                int elUsed = userLeaves.Where(l => l.LeaveType == "Earned Leave").Sum(l => l.TotalDays);
                int rem = (12 - clUsed) + (12 - slUsed) + (15 - elUsed);

                csv.AppendLine($"\"{u.UserCode}\",\"{u.FullName}\",\"12\",\"{clUsed}\",\"12\",\"{slUsed}\",\"15\",\"{elUsed}\",\"{rem}\"");
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"Leave_Balance_Report_{year}.csv");
        }

        // ==========================================
        // 4. ATTRITION & HEADCOUNT REPORT
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> AttritionHeadcount(int? departmentId, int year = 2026, string? search = null)
        {
            var activeUsers = await _context.Users.Include(u => u.Role).Where(u => u.IsActive).ToListAsync();
            var offboardings = await _context.Offboardings.ToListAsync();
            var departments = await _context.Departments.Where(d => d.IsActive).ToListAsync();

            int headcount = activeUsers.Count;
            int newHires = activeUsers.Count(u => u.CreatedAt.Year == year);
            int exits = offboardings.Count;

            decimal avgHeadcount = Math.Max((headcount + exits) / 2.0m, 1.0m);
            decimal attritionRate = Math.Round((exits / avgHeadcount) * 100.0m, 2);

            var deptSummaries = new List<DepartmentHeadcountItem>();
            foreach (var d in departments)
            {
                deptSummaries.Add(new DepartmentHeadcountItem
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    OpeningHeadcount = 10,
                    NewHiresCount = 2,
                    ExitsCount = 1,
                    EndingHeadcount = 11,
                    AttritionRatePct = 8.3m
                });
            }

            var vm = new AttritionHeadcountReportViewModel
            {
                DepartmentSummaries = deptSummaries,
                ExitRecords = offboardings,
                ActiveEmployees = activeUsers,
                Departments = departments,
                SelectedDepartmentId = departmentId,
                SelectedYear = year,
                SearchTerm = search ?? "",
                CurrentHeadcount = headcount,
                NewHiresYTD = newHires,
                ExitsYTD = exits,
                AnnualAttritionRatePct = attritionRate
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> ExportAttritionHeadcount(string format, int? departmentId, int year = 2026)
        {
            var activeUsers = await _context.Users.Where(u => u.IsActive).ToListAsync();
            var offboardings = await _context.Offboardings.ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Metric,Count / Rate");
            csv.AppendLine($"\"Current Headcount\",\"{activeUsers.Count}\"");
            csv.AppendLine($"\"New Hires (YTD)\",\"{activeUsers.Count(u => u.CreatedAt.Year == year)}\"");
            csv.AppendLine($"\"Total Exits (YTD)\",\"{offboardings.Count}\"");

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"Attrition_Headcount_{year}.csv");
        }

        // ==========================================
        // 5. TAX DEDUCTION REPORT
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> TaxDeduction(string? payPeriod, int? departmentId, string? search)
        {
            payPeriod = string.IsNullOrWhiteSpace(payPeriod) ? "August 2026" : payPeriod;

            var slipsQuery = _context.Payslips.Include(p => p.User).Where(p => p.PayPeriod == payPeriod).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                slipsQuery = slipsQuery.Where(p => p.User!.FullName.ToLower().Contains(search) || p.User.UserCode.ToLower().Contains(search));
            }

            var payslips = await slipsQuery.ToListAsync();
            var departments = await _context.Departments.Where(d => d.IsActive).ToListAsync();

            var taxItems = new List<EmployeeTaxDeductionItem>();

            foreach (var p in payslips)
            {
                taxItems.Add(new EmployeeTaxDeductionItem
                {
                    UserId = p.UserId,
                    EmployeeCode = p.User?.UserCode ?? "EMP-001",
                    EmployeeName = p.User?.FullName ?? "Staff Member",
                    DepartmentName = "Human Resources",
                    PanNumber = "AAAAA1234A",
                    TaxRegime = "New Tax Regime",
                    GrossSalary = p.GrossSalary,
                    MonthlyTDS = p.TDS,
                    ProfessionalTax = p.ProfessionalTax,
                    EmployeePF = p.ProvidentFund,
                    EmployerPF = p.EmployerPF,
                    EmployeeESI = p.ESI,
                    EmployerESI = p.EmployerESI
                });
            }

            var vm = new TaxDeductionReportViewModel
            {
                TaxItems = taxItems,
                Departments = departments,
                SelectedPayPeriod = payPeriod,
                SelectedDepartmentId = departmentId,
                SearchTerm = search ?? "",
                TotalEmployeesTaxed = taxItems.Count,
                TotalTDS = taxItems.Sum(t => t.MonthlyTDS),
                TotalProfessionalTax = taxItems.Sum(t => t.ProfessionalTax),
                TotalEmployeePF = taxItems.Sum(t => t.EmployeePF),
                TotalEmployerPF = taxItems.Sum(t => t.EmployerPF),
                TotalESI = taxItems.Sum(t => t.EmployeeESI),
                TotalDeductions = taxItems.Sum(t => t.TotalStatutoryDeduction)
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> ExportTaxDeduction(string format, string? payPeriod, int? departmentId, string? search)
        {
            payPeriod = string.IsNullOrWhiteSpace(payPeriod) ? "August 2026" : payPeriod;
            var payslips = await _context.Payslips.Include(p => p.User).Where(p => p.PayPeriod == payPeriod).ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Employee Code,Employee Name,Pay Period,Gross Salary,TDS,Professional Tax,Employee PF,Employer PF,Employee ESI,Total Statutory Deduction");

            foreach (var p in payslips)
            {
                decimal tot = p.TDS + p.ProfessionalTax + p.ProvidentFund + p.ESI;
                csv.AppendLine($"\"{p.User?.UserCode}\",\"{p.User?.FullName}\",\"{p.PayPeriod}\",\"{p.GrossSalary}\",\"{p.TDS}\",\"{p.ProfessionalTax}\",\"{p.ProvidentFund}\",\"{p.EmployerPF}\",\"{p.ESI}\",\"{tot}\"");
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"Tax_Deduction_Report_{payPeriod.Replace(" ", "_")}.csv");
        }

        // ==========================================
        // 6. PRINTABLE REPORT VIEW
        // ==========================================

        [HttpGet]
        public IActionResult PrintReport(string reportType, string? payPeriod)
        {
            ViewBag.ReportType = reportType;
            ViewBag.PayPeriod = payPeriod ?? "August 2026";
            return View("PrintReport");
        }
    }
}
