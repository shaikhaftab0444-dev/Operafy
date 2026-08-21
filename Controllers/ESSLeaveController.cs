using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System.Threading.Tasks;
using System.Security.Claims;
using System;
using System.Linq;

namespace ERP_System.Controllers
{
    [Authorize]
    public class ESSLeaveController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ESSLeaveController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out int id) ? id : 1;
        }

        // GET: /ESSLeave/Apply
        [HttpGet]
        public IActionResult Apply()
        {
            return View();
        }

        // POST: /ESSLeave/SubmitLeave
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitLeave(ESSLeaveApplication leave)
        {
            if (ModelState.IsValid || (leave.LeaveType != null && leave.Reason != null))
            {
                leave.UserId = GetCurrentUserId();
                leave.Status = "Pending";
                
                // Calculate Total Days
                leave.TotalDays = (leave.EndDate - leave.StartDate).Days + 1;
                if (leave.TotalDays <= 0) leave.TotalDays = 1;

                _context.ESSLeaveApplications.Add(leave);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(History));
            }
            return View(nameof(Apply), leave);
        }

        // GET: /ESSLeave/BalanceSummary
        [HttpGet]
        public async Task<IActionResult> BalanceSummary()
        {
            int userId = GetCurrentUserId();
            var approvedLeaves = await _context.ESSLeaveApplications
                .Where(l => l.UserId == userId && l.Status == "Approved")
                .ToListAsync();

            // Calculate taken leave days by type
            ViewBag.CasualTaken = approvedLeaves.Where(l => l.LeaveType == "Casual Leave").Sum(l => l.TotalDays);
            ViewBag.SickTaken = approvedLeaves.Where(l => l.LeaveType == "Sick Leave").Sum(l => l.TotalDays);
            ViewBag.EarnedTaken = approvedLeaves.Where(l => l.LeaveType == "Earned Leave").Sum(l => l.TotalDays);

            return View();
        }

        // GET: /ESSLeave/History
        [HttpGet]
        public async Task<IActionResult> History()
        {
            int userId = GetCurrentUserId();
            var history = await _context.ESSLeaveApplications
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.StartDate)
                .ToListAsync();
            return View(history);
        }
    }
}
