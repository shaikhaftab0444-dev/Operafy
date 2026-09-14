using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System.Security.Claims;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "HR,Admin,Super Admin,IT Support,IT,Manager")]
    public class HelpdeskResolutionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HelpdeskResolutionController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out int id) ? id : 1;
        }

        // GET: /HelpdeskResolution or /HelpdeskResolution/Index
        [HttpGet]
        public async Task<IActionResult> Index(string statusFilter = "All")
        {
            return await DepartmentQueue(statusFilter);
        }

        // GET: /HelpdeskResolution/DepartmentQueue
        [HttpGet]
        public async Task<IActionResult> DepartmentQueue(string statusFilter = "All")
        {
            var query = _context.ESSSupportTickets
                .Include(t => t.User)
                .ThenInclude(u => u.Department)
                .AsQueryable();

            // Role-based Department Queue Filtering
            bool isSuperAdmin = User.IsInRole("Super Admin");
            bool isAdmin = User.IsInRole("Admin");
            bool isHR = User.IsInRole("HR");
            bool isIT = User.IsInRole("IT Support") || User.IsInRole("IT");

            string currentDeptScope = "Enterprise Wide";

            if (isSuperAdmin || isAdmin)
            {
                currentDeptScope = "All Departments (Enterprise)";
            }
            else if (isHR && !isIT)
            {
                query = query.Where(t => t.Department == "Human Resources" || t.Department == "HR");
                currentDeptScope = "Human Resources";
            }
            else if (isIT && !isHR)
            {
                query = query.Where(t => t.Department == "Information Technology" || t.Department == "IT Support" || t.Department == "IT");
                currentDeptScope = "Information Technology";
            }
            else if (User.IsInRole("Manager"))
            {
                currentDeptScope = "Manager Queue";
            }

            ViewBag.DepartmentScope = currentDeptScope;
            ViewBag.CurrentFilter = statusFilter;

            var allDepartmentTickets = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();

            ViewBag.TotalCount = allDepartmentTickets.Count;
            ViewBag.OpenCount = allDepartmentTickets.Count(t => t.Status == "Open");
            ViewBag.InProgressCount = allDepartmentTickets.Count(t => t.Status == "In Progress");
            ViewBag.ResolvedCount = allDepartmentTickets.Count(t => t.Status == "Resolved" || t.Status == "Closed");

            var filteredTickets = allDepartmentTickets;
            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
            {
                if (statusFilter == "Resolved")
                {
                    filteredTickets = allDepartmentTickets.Where(t => t.Status == "Resolved" || t.Status == "Closed").ToList();
                }
                else
                {
                    filteredTickets = allDepartmentTickets.Where(t => t.Status.Equals(statusFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                }
            }

            return View("DepartmentQueue", filteredTickets);
        }

        // POST: /HelpdeskResolution/UpdateTicketStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTicketStatus(int ticketId, string status, string resolutionRemarks)
        {
            var ticket = await _context.ESSSupportTickets
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TicketId == ticketId);

            if (ticket == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Ticket not found." });
                }
                TempData["ErrorMessage"] = "Ticket not found.";
                return RedirectToAction(nameof(DepartmentQueue));
            }

            int currentUserId = GetCurrentUserId();
            var resolver = await _context.Users.FindAsync(currentUserId);
            string resolverName = resolver?.FullName ?? User.Identity?.Name ?? "Department Head";

            ticket.Status = status;
            ticket.ResolutionRemarks = resolutionRemarks;
            ticket.ResolvedBy = resolverName;

            if (status == "Resolved" || status == "Closed")
            {
                ticket.ResolvedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = $"Ticket #{ticket.TicketNumber ?? ticket.TicketId.ToString()} has been updated to {status}." });
            }

            TempData["SuccessMessage"] = $"Ticket #{ticket.TicketNumber ?? ticket.TicketId.ToString()} marked as {status} successfully.";
            return RedirectToAction(nameof(DepartmentQueue));
        }
    }
}
