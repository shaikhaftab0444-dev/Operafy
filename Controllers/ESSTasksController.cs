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

        // GET: /ESSTasks or /ESSTasks/Index
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Assigned));
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

            if (!tasks.Any())
            {
                tasks = await _context.ESSTasks
                    .OrderByDescending(t => t.DueDate)
                    .Take(10)
                    .ToListAsync();
            }

            if (!tasks.Any())
            {
                // Seed initial tasks for demo/testing
                var sampleTasks = new List<ESSTask>
                {
                    new ESSTask
                    {
                        UserId = userId,
                        TaskTitle = "Reconcile GSTR-2B with Purchase Register",
                        Description = "Cross-match supplier GST invoices against inward goods receipts for September closing.",
                        DueDate = DateTime.Today.AddDays(2),
                        Status = "In Progress"
                    },
                    new ESSTask
                    {
                        UserId = userId,
                        TaskTitle = "Monthly Bank Statement Reconciliation",
                        Description = "Clear 4 pending NEFT inbound entries and verify bank charges against HDFC statement.",
                        DueDate = DateTime.Today.AddDays(1),
                        Status = "In Progress"
                    },
                    new ESSTask
                    {
                        UserId = userId,
                        TaskTitle = "Vendor Bill Verification & 3-Way Match",
                        Description = "Verify invoice rates against approved PO #PO-2026-0041 and Warehouse GRN receipts.",
                        DueDate = DateTime.Today.AddDays(5),
                        Status = "Pending"
                    },
                    new ESSTask
                    {
                        UserId = userId,
                        TaskTitle = "Petty Cash Imprest Audit & Vouchers",
                        Description = "Review administrative petty cash receipts and replenishment vouchers for Head Office.",
                        DueDate = DateTime.Today.AddDays(7),
                        Status = "Completed"
                    }
                };
                await _context.ESSTasks.AddRangeAsync(sampleTasks);
                await _context.SaveChangesAsync();
                tasks = sampleTasks;
            }

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

                if (task.DepartmentTaskId.HasValue)
                {
                    var depTask = await _context.DepartmentTasks.FindAsync(task.DepartmentTaskId.Value);
                    if (depTask != null)
                    {
                        depTask.Status = Status == "Pending" ? "In Progress" : Status;
                        depTask.ProgressPercentage = Status == "Completed" ? 100 : (Status == "In Progress" ? 50 : 0);
                        _context.DepartmentTasks.Update(depTask);
                    }
                }

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(StatusUpdate));
        }
    }
}
