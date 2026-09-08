using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ERP_System.Data;

namespace ERP_System.Controllers
{
    [Authorize]
    public class EmployeeDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /EmployeeDashboard
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = 1;
            if (int.TryParse(userIdClaim, out int id))
            {
                userId = id;
            }

            var currentUser = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Branch)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            var activityLogs = await _context.ActivityLogs
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync() ?? new List<ActivityLog>();

            var transactions = await _context.Transactions
                .OrderByDescending(t => t.Date)
                .Take(5)
                .ToListAsync() ?? new List<Transaction>();

            var myPayslips = await _context.Payslips
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PayslipId)
                .Take(5)
                .ToListAsync() ?? new List<Payslip>();

            var today = DateTime.Today;
            var todayAttendance = await _context.HRAttendanceLogs
                .FirstOrDefaultAsync(l => l.UserId == userId && l.Date.Date == today);

            var recentAttendanceLogs = await _context.HRAttendanceLogs
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.Date)
                .Take(7)
                .ToListAsync() ?? new List<HRAttendanceLog>();

            var viewModel = new EmployeeDashboardViewModel
            {
                CurrentUser = currentUser,
                RecentPayslips = myPayslips,
                ActivityLogs = activityLogs,
                Transactions = transactions,
                TodayAttendance = todayAttendance,
                RecentAttendanceLogs = recentAttendanceLogs
            };

            return View(viewModel);
        }
    }
}