using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ERP_System.Data;
using ERP_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,HR,Manager,Hiring Manager,Employee")]
    public class HRPerformanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HRPerformanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(claim, out int userId)) return userId;
            return 1; // Default Admin
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "Admin";
        }

        // Helper method for safe percentage calculation
        private decimal CalculatePercentage(decimal current, decimal target)
        {
            if (target <= 0) return 0.0m;
            decimal pct = (current / target) * 100.0m;
            return Math.Round(Math.Min(pct, 200.0m), 2); // Cap display at 200% max safely
        }

        // ==========================================
        // 1. OKR & KPI SETUP PAGE & ACTIONS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> OkrKpi(int? departmentId, int? employeeId, string? status, string? search)
        {
            var okrQuery = _context.OkrObjectives
                .Include(o => o.Department)
                .Include(o => o.Designation)
                .Include(o => o.Employee)
                .Include(o => o.KeyResults)
                .AsQueryable();

            var kpiQuery = _context.Kpis
                .Include(k => k.Department)
                .Include(k => k.Designation)
                .Include(k => k.Employee)
                .AsQueryable();

            if (departmentId.HasValue && departmentId.Value > 0)
            {
                okrQuery = okrQuery.Where(o => o.DepartmentId == departmentId.Value);
                kpiQuery = kpiQuery.Where(k => k.DepartmentId == departmentId.Value);
            }

            if (employeeId.HasValue && employeeId.Value > 0)
            {
                okrQuery = okrQuery.Where(o => o.EmployeeId == employeeId.Value);
                kpiQuery = kpiQuery.Where(k => k.EmployeeId == employeeId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                okrQuery = okrQuery.Where(o => o.Status == status);
                kpiQuery = kpiQuery.Where(k => k.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                okrQuery = okrQuery.Where(o => o.ObjectiveTitle.ToLower().Contains(search) || (o.Description != null && o.Description.ToLower().Contains(search)));
                kpiQuery = kpiQuery.Where(k => k.KpiName.ToLower().Contains(search));
            }

            var okrs = await okrQuery.OrderByDescending(o => o.OkrId).ToListAsync();
            var kpis = await kpiQuery.OrderByDescending(k => k.KpiId).ToListAsync();

            ViewBag.Departments = await _context.Departments.Where(d => d.IsActive).ToListAsync();
            ViewBag.Designations = await _context.Designations.Where(d => d.IsActive).ToListAsync();
            ViewBag.Employees = await _context.Users.Where(u => u.IsActive).ToListAsync();
            ViewBag.Kpis = kpis;

            ViewBag.SelectedDepartmentId = departmentId;
            ViewBag.SelectedEmployeeId = employeeId;
            ViewBag.SelectedStatus = status;
            ViewBag.Search = search;

            return View(okrs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOkrGoal(OkrObjective okr, string keyResultNames, string keyResultTargets, string keyResultUnits)
        {
            okr.CreatedAt = DateTime.Now;
            _context.OkrObjectives.Add(okr);
            await _context.SaveChangesAsync();

            // Add Key Results if provided
            if (!string.IsNullOrWhiteSpace(keyResultNames))
            {
                var names = keyResultNames.Split(new[] { '\n', ';' }, StringSplitOptions.RemoveEmptyEntries);
                var targets = (keyResultTargets ?? "").Split(new[] { '\n', ';' }, StringSplitOptions.RemoveEmptyEntries);
                var units = (keyResultUnits ?? "").Split(new[] { '\n', ';' }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < names.Length; i++)
                {
                    string name = names[i].Trim();
                    if (string.IsNullOrWhiteSpace(name)) continue;

                    decimal target = 100.0m;
                    if (i < targets.Length && decimal.TryParse(targets[i].Trim(), out decimal tVal)) target = tVal;

                    string unit = "Percentage";
                    if (i < units.Length && !string.IsNullOrWhiteSpace(units[i].Trim())) unit = units[i].Trim();

                    _context.KeyResults.Add(new KeyResult
                    {
                        OkrId = okr.OkrId,
                        KeyResultName = name,
                        TargetValue = target,
                        CurrentValue = 0,
                        MeasurementUnit = unit,
                        ProgressPercentage = 0,
                        Status = "In Progress"
                    });
                }
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = $"OKR Objective '{okr.ObjectiveTitle}' created successfully!";
            return RedirectToAction(nameof(OkrKpi));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateKpi(KpiItem kpi)
        {
            kpi.AchievementPercentage = CalculatePercentage(kpi.ActualAchievement, kpi.TargetValue);
            kpi.CreatedAt = DateTime.Now;

            _context.Kpis.Add(kpi);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"KPI '{kpi.KpiName}' added successfully!";
            return RedirectToAction(nameof(OkrKpi));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateKeyResultProgress(int keyResultId, decimal currentValue)
        {
            var kr = await _context.KeyResults.Include(k => k.Okr).ThenInclude(o => o!.KeyResults).FirstOrDefaultAsync(k => k.KeyResultId == keyResultId);
            if (kr != null)
            {
                kr.CurrentValue = currentValue;
                kr.ProgressPercentage = CalculatePercentage(currentValue, kr.TargetValue);
                kr.Status = kr.ProgressPercentage >= 100.0m ? "Completed" : "In Progress";

                _context.KeyResults.Update(kr);
                await _context.SaveChangesAsync();

                // Recalculate parent OKR progress
                if (kr.Okr != null && kr.Okr.KeyResults.Any())
                {
                    decimal avgProgress = kr.Okr.KeyResults.Average(k => k.ProgressPercentage);
                    kr.Okr.OverallProgressPercentage = Math.Round(avgProgress, 2);
                    kr.Okr.Status = avgProgress >= 100.0m ? "Completed" : "In Progress";
                    _context.OkrObjectives.Update(kr.Okr);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = $"Key Result progress updated to {kr.ProgressPercentage}%!";
            }
            return RedirectToAction(nameof(OkrKpi));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateKpiProgress(int kpiId, decimal actualAchievement)
        {
            var kpi = await _context.Kpis.FindAsync(kpiId);
            if (kpi != null)
            {
                kpi.ActualAchievement = actualAchievement;
                kpi.AchievementPercentage = CalculatePercentage(actualAchievement, kpi.TargetValue);
                kpi.Status = kpi.AchievementPercentage >= 100.0m ? "Exceeded" : (kpi.AchievementPercentage >= 80.0m ? "Met" : "Active");

                _context.Kpis.Update(kpi);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"KPI achievement updated to {kpi.AchievementPercentage}%!";
            }
            return RedirectToAction(nameof(OkrKpi));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOkr(int okrId)
        {
            var okr = await _context.OkrObjectives.Include(o => o.KeyResults).FirstOrDefaultAsync(o => o.OkrId == okrId);
            if (okr != null)
            {
                _context.KeyResults.RemoveRange(okr.KeyResults);
                _context.OkrObjectives.Remove(okr);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "OKR Goal deleted successfully!";
            }
            return RedirectToAction(nameof(OkrKpi));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteKpi(int kpiId)
        {
            var kpi = await _context.Kpis.FindAsync(kpiId);
            if (kpi != null)
            {
                _context.Kpis.Remove(kpi);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "KPI deleted successfully!";
            }
            return RedirectToAction(nameof(OkrKpi));
        }

        // ==========================================
        // 2. APPRAISAL CYCLES PAGE & ACTIONS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> AppraisalCycles(string? status, string? search)
        {
            var query = _context.AppraisalCycles
                .Include(c => c.Appraisals)
                .ThenInclude(a => a.Employee)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(c => c.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(c => c.CycleName.ToLower().Contains(search) || (c.Description != null && c.Description.ToLower().Contains(search)));
            }

            var cycles = await query.OrderByDescending(c => c.CycleId).ToListAsync();

            ViewBag.ActiveEmployees = await _context.Users.Where(u => u.IsActive && u.Role!.RoleName != "Super Admin").ToListAsync();
            ViewBag.Departments = await _context.Departments.Where(d => d.IsActive).ToListAsync();
            ViewBag.SelectedStatus = status;
            ViewBag.Search = search;
            ViewBag.CurrentUserId = GetCurrentUserId();

            return View(cycles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCycle(AppraisalCycle cycle, bool autoLaunch)
        {
            cycle.CreatedAt = DateTime.Now;
            _context.AppraisalCycles.Add(cycle);
            await _context.SaveChangesAsync();

            if (autoLaunch)
            {
                await LaunchCycleInternal(cycle.CycleId);
                TempData["SuccessMessage"] = $"Appraisal Cycle '{cycle.CycleName}' created and launched for all employees!";
            }
            else
            {
                TempData["SuccessMessage"] = $"Appraisal Cycle '{cycle.CycleName}' created successfully!";
            }

            return RedirectToAction(nameof(AppraisalCycles));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LaunchCycle(int cycleId)
        {
            var cycle = await _context.AppraisalCycles.FindAsync(cycleId);
            if (cycle != null)
            {
                await LaunchCycleInternal(cycleId);
                cycle.Status = "Active";
                _context.AppraisalCycles.Update(cycle);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Appraisal Cycle '{cycle.CycleName}' is now Active!";
            }
            return RedirectToAction(nameof(AppraisalCycles));
        }

        private async Task LaunchCycleInternal(int cycleId)
        {
            var activeEmployees = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.IsActive && u.Role!.RoleName != "Super Admin" && u.Role!.RoleName != "Admin")
                .ToListAsync();

            var existingAppraisals = await _context.EmployeeAppraisals.Where(a => a.CycleId == cycleId).Select(a => a.EmployeeId).ToListAsync();

            foreach (var emp in activeEmployees)
            {
                if (!existingAppraisals.Contains(emp.UserId))
                {
                    _context.EmployeeAppraisals.Add(new EmployeeAppraisal
                    {
                        CycleId = cycleId,
                        EmployeeId = emp.UserId,
                        SelfReviewSubmitted = false,
                        ManagerReviewSubmitted = false,
                        Status = "Self Review Pending",
                        CreatedAt = DateTime.Now
                    });
                }
            }
            await _context.SaveChangesAsync();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitSelfReview(int appraisalId, decimal selfRating, string selfComments)
        {
            var appraisal = await _context.EmployeeAppraisals.FindAsync(appraisalId);
            if (appraisal != null)
            {
                appraisal.SelfRating = selfRating;
                appraisal.SelfComments = selfComments;
                appraisal.SelfReviewSubmitted = true;
                appraisal.SelfSubmittedAt = DateTime.Now;
                appraisal.Status = "Manager Review Pending";
                appraisal.UpdatedAt = DateTime.Now;

                _context.EmployeeAppraisals.Update(appraisal);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Your Self Review has been submitted successfully!";
            }
            return RedirectToAction(nameof(AppraisalCycles));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitManagerReview(int appraisalId, decimal managerRating, string managerComments, string? hrComments)
        {
            var appraisal = await _context.EmployeeAppraisals.FindAsync(appraisalId);
            if (appraisal != null)
            {
                appraisal.ManagerRating = managerRating;
                appraisal.ManagerComments = managerComments;
                appraisal.ManagerReviewSubmitted = true;
                appraisal.ManagerSubmittedAt = DateTime.Now;
                appraisal.ManagerId = GetCurrentUserId();
                if (!string.IsNullOrWhiteSpace(hrComments)) appraisal.HRComments = hrComments;

                // Calculate Goal and KPI scores from existing records if available
                appraisal.GoalScore = managerRating;
                appraisal.KpiScore = managerRating;

                // Compute weighted final score: 40% Manager Rating, 30% Goal Score, 30% Self Rating (or default)
                decimal selfPart = (appraisal.SelfRating ?? managerRating) * 0.30m;
                decimal mgrPart = managerRating * 0.40m;
                decimal goalPart = (appraisal.GoalScore ?? managerRating) * 0.30m;

                decimal final = Math.Round(selfPart + mgrPart + goalPart, 2);
                appraisal.FinalScore = final;

                // Assign Band
                if (final >= 4.8m) appraisal.FinalRatingBand = "Outstanding (A+)";
                else if (final >= 4.0m) appraisal.FinalRatingBand = "Exceeds Expectations (A)";
                else if (final >= 3.0m) appraisal.FinalRatingBand = "Meets Expectations (B)";
                else if (final >= 2.0m) appraisal.FinalRatingBand = "Needs Improvement (C)";
                else appraisal.FinalRatingBand = "Poor (D)";

                appraisal.Status = "Finalized";
                appraisal.UpdatedAt = DateTime.Now;

                _context.EmployeeAppraisals.Update(appraisal);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Manager Review completed for Employee! Final Score: {final} / 5.0 ({appraisal.FinalRatingBand})";
            }
            return RedirectToAction(nameof(AppraisalCycles));
        }

        // ==========================================
        // 3. EMPLOYEE RATING CARDS PAGE & ACTIONS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> RatingCards(int? cycleId, int? departmentId, string? ratingBand, string? search)
        {
            int currentUserId = GetCurrentUserId();
            string userRole = GetCurrentUserRole();

            var query = _context.EmployeeAppraisals
                .Include(a => a.AppraisalCycle)
                .Include(a => a.Employee)
                .ThenInclude(e => e!.Role)
                .Include(a => a.Manager)
                .AsQueryable();

            // Role Based Access Control (RBAC) filtering
            if (userRole == "Employee")
            {
                query = query.Where(a => a.EmployeeId == currentUserId);
            }
            else if (userRole == "Manager")
            {
                // Manager sees employees where they are manager or in department
                query = query.Where(a => a.ManagerId == currentUserId || a.EmployeeId == currentUserId);
            }

            if (cycleId.HasValue && cycleId.Value > 0)
            {
                query = query.Where(a => a.CycleId == cycleId.Value);
            }

            if (!string.IsNullOrWhiteSpace(ratingBand) && ratingBand != "All")
            {
                query = query.Where(a => a.FinalRatingBand.Contains(ratingBand));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(a => a.Employee!.FullName.ToLower().Contains(search) || a.Employee.UserCode.ToLower().Contains(search));
            }

            var appraisals = await query.OrderByDescending(a => a.AppraisalId).ToListAsync();

            ViewBag.Cycles = await _context.AppraisalCycles.ToListAsync();
            ViewBag.Departments = await _context.Departments.Where(d => d.IsActive).ToListAsync();
            ViewBag.SelectedCycleId = cycleId;
            ViewBag.SelectedRatingBand = ratingBand;
            ViewBag.Search = search;
            ViewBag.UserRole = userRole;

            return View(appraisals);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizeRatingCard(int appraisalId)
        {
            var appraisal = await _context.EmployeeAppraisals.FindAsync(appraisalId);
            if (appraisal != null)
            {
                appraisal.Status = "Completed";
                appraisal.UpdatedAt = DateTime.Now;
                _context.EmployeeAppraisals.Update(appraisal);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Employee Rating Card finalized successfully!";
            }
            return RedirectToAction(nameof(RatingCards));
        }

        [HttpGet]
        public async Task<IActionResult> PrintRatingCard(int appraisalId)
        {
            var appraisal = await _context.EmployeeAppraisals
                .Include(a => a.AppraisalCycle)
                .Include(a => a.Employee)
                .Include(a => a.Manager)
                .FirstOrDefaultAsync(a => a.AppraisalId == appraisalId);

            if (appraisal == null) return NotFound();

            return View("PrintRatingCard", appraisal);
        }
    }
}
