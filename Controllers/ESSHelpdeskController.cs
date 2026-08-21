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
                .Where(u => u.IsActive && u.Role.RoleName != "Super Admin")
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

        // POST: /ESSHelpdesk/SubmitTicket
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitTicket(ESSSupportTicket ticket)
        {
            if (ModelState.IsValid || (ticket.Subject != null && ticket.Description != null))
            {
                ticket.UserId = GetCurrentUserId();
                ticket.CreatedAt = DateTime.Now;
                ticket.Status = "Open";

                _context.ESSSupportTickets.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(RaiseTicket));
            }
            var tickets = await _context.ESSSupportTickets.Where(t => t.UserId == GetCurrentUserId()).ToListAsync();
            return View(nameof(RaiseTicket), tickets);
        }
    }
}
