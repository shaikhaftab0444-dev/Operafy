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
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentUserId = int.TryParse(userIdClaim, out int id) ? id : 1;
            var currentUser = await _context.Users.FindAsync(currentUserId);
            var currentUserIdStr = currentUserId.ToString();

            var tasks = await _context.HierarchicalTasks
                .Where(t => t.AssignedToUserId == currentUserIdStr || 
                           (currentUser != null && t.AssignedToUserId == currentUser.UserName) || 
                           (t.IsGeneralTask && currentUser != null && t.DepartmentId == currentUser.DepartmentId))
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            if (!tasks.Any())
            {
                var sampleTask = new HierarchicalTask
                {
                    Title = "Warehouse General Safety & Daily Inventory Audit",
                    Description = "Conduct physical count across main warehouse aisles and ensure QA staging compliance.",
                    DepartmentId = currentUser?.DepartmentId ?? 1,
                    TaskType = "WAREHOUSE_OPERATIONS",
                    AssignedByUserId = "1",
                    AssignedToUserId = currentUserIdStr,
                    IsGeneralTask = false,
                    Priority = "High",
                    Status = "In Progress",
                    DueDate = DateTime.Today.AddDays(2),
                    CreatedAt = DateTime.UtcNow
                };
                _context.HierarchicalTasks.Add(sampleTask);
                await _context.SaveChangesAsync();
                tasks.Add(sampleTask);
            }

            // Populate navigation properties for display
            var userIds = tasks
                .SelectMany(t => new[] { t.AssignedToUserId, t.AssignedByUserId })
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct()
                .ToList();

            var users = await _context.Users
                .Include(u => u.Role)
                .Where(u => userIds.Contains(u.UserId.ToString()) || userIds.Contains(u.UserName))
                .ToListAsync();

            foreach (var t in tasks)
            {
                var toUser = users.FirstOrDefault(u => u.UserId.ToString() == t.AssignedToUserId || u.UserName == t.AssignedToUserId);
                if (toUser != null)
                {
                    t.AssignedToUser = new ApplicationUser
                    {
                        UserId = toUser.UserId,
                        FullName = toUser.FullName,
                        UserName = toUser.UserName,
                        Email = toUser.Email,
                        RoleId = toUser.RoleId,
                        Role = toUser.Role,
                        DepartmentId = toUser.DepartmentId,
                        Department = toUser.Department
                    };
                }
                var byUser = users.FirstOrDefault(u => u.UserId.ToString() == t.AssignedByUserId || u.UserName == t.AssignedByUserId);
                if (byUser != null)
                {
                    t.AssignedByUser = new ApplicationUser
                    {
                        UserId = byUser.UserId,
                        FullName = byUser.FullName,
                        UserName = byUser.UserName,
                        Email = byUser.Email,
                        RoleId = byUser.RoleId,
                        Role = byUser.Role,
                        DepartmentId = byUser.DepartmentId,
                        Department = byUser.Department
                    };
                }
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
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentUserId = int.TryParse(userIdClaim, out int id) ? id : 1;
            var currentUser = await _context.Users.FindAsync(currentUserId);
            var currentUserIdStr = currentUserId.ToString();

            // 1. Update HierarchicalTask if found
            var hierTask = await _context.HierarchicalTasks.FindAsync(TaskId);
            if (hierTask != null && (hierTask.AssignedToUserId == currentUserIdStr || 
                                     (currentUser != null && hierTask.AssignedToUserId == currentUser.UserName) || 
                                     (hierTask.IsGeneralTask && currentUser != null && hierTask.DepartmentId == currentUser.DepartmentId)))
            {
                hierTask.Status = Status;
                hierTask.ProgressPercentage = Status == "Completed" ? 100 : (Status == "In Progress" ? 50 : 0);
                _context.HierarchicalTasks.Update(hierTask);
            }

            // 2. Update ESSTask if found
            var task = await _context.ESSTasks.FindAsync(TaskId);
            if (task != null && task.UserId == currentUserId)
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
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Assigned));
        }
    }
}
