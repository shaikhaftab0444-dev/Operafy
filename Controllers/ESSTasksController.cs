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
    public class ESSTasksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ESSTasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out int id) ? id : 1;
        }

        // GET: /ESSTasks/Assigned
        [HttpGet]
        public async Task<IActionResult> Assigned()
        {
            int userId = GetCurrentUserId();
            var tasks = await _context.ESSTasks
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.DueDate)
                .ToListAsync();
            return View(tasks);
        }

        // GET: /ESSTasks/Timesheet
        [HttpGet]
        public async Task<IActionResult> Timesheet()
        {
            int userId = GetCurrentUserId();
            var tasks = await _context.ESSTasks
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.TaskId)
                .ToListAsync();
            return View(tasks);
        }

        // POST: /ESSTasks/AddTimesheetTask
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTimesheetTask(ESSTask task)
        {
            if (ModelState.IsValid || (task.TaskTitle != null && task.Description != null))
            {
                task.UserId = GetCurrentUserId();
                task.Status = "Pending";
                _context.ESSTasks.Add(task);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Timesheet));
            }
            var tasks = await _context.ESSTasks.Where(t => t.UserId == GetCurrentUserId()).ToListAsync();
            return View(nameof(Timesheet), tasks);
        }

        // GET: /ESSTasks/StatusUpdate
        [HttpGet]
        public async Task<IActionResult> StatusUpdate()
        {
            int userId = GetCurrentUserId();
            var tasks = await _context.ESSTasks
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.TaskId)
                .ToListAsync();
            return View(tasks);
        }

        // POST: /ESSTasks/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int TaskId, string Status)
        {
            var task = await _context.ESSTasks.FindAsync(TaskId);
            if (task != null && task.UserId == GetCurrentUserId())
            {
                task.Status = Status;
                _context.ESSTasks.Update(task);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(StatusUpdate));
        }
    }
}
