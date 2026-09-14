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
    public class ESSHelpdeskController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ESSHelpdeskController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out int id) ? id : 1;
        }

        // GET: /ESSHelpdesk/Directory
        [HttpGet]
        public async Task<IActionResult> Directory()
        {
            // Show all active employees
            var employees = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Department)
                .Include(u => u.Branch)
                .Where(u => u.IsActive && (u.Role == null || u.Role.RoleName != "Super Admin"))
                .OrderBy(u => u.FullName)
                .ToListAsync();
            return View(employees);
        }

        // GET: /ESSHelpdesk/RaiseTicket
        [HttpGet]
        public async Task<IActionResult> RaiseTicket()
        {
            int userId = GetCurrentUserId();
            var tickets = await _context.ESSSupportTickets
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
            return View(tickets);
        }

        // POST: /ESSHelpdesk/CreateTicket
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTicket(ESSSupportTicket ticket)
        {
            if (ModelState.IsValid || (!string.IsNullOrWhiteSpace(ticket.Subject) && !string.IsNullOrWhiteSpace(ticket.Description)))
            {
                ticket.UserId = GetCurrentUserId();
                ticket.TicketNumber = "TCK-" + DateTime.UtcNow.ToString("yyyyMM") + "-" + new Random().Next(100, 999);
                ticket.Status = "Open";
                if (string.IsNullOrWhiteSpace(ticket.Priority)) ticket.Priority = "Medium";
                if (string.IsNullOrWhiteSpace(ticket.Department)) ticket.Department = "Information Technology";
                ticket.CreatedAt = DateTime.UtcNow;

                _context.ESSSupportTickets.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(RaiseTicket));
            }
            var tickets = await _context.ESSSupportTickets.Where(t => t.UserId == GetCurrentUserId()).OrderByDescending(t => t.CreatedAt).ToListAsync();
            return View(nameof(RaiseTicket), tickets);
        }

        // POST: /ESSHelpdesk/SubmitTicket (Alias)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitTicket(ESSSupportTicket ticket)
        {
            return await CreateTicket(ticket);
        }
    }
}
