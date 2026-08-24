using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin")]
    public class AdminAnnouncementsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminAnnouncementsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /AdminAnnouncements/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var announcements = await _context.AdminAnnouncements.OrderByDescending(a => a.AnnouncementId).ToListAsync();
            return View(announcements);
        }

        // POST: /AdminAnnouncements/CreateAnnouncement
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAnnouncement(AdminAnnouncement announcement)
        {
            if (ModelState.IsValid || (announcement.Title != null && announcement.Content != null))
            {
                announcement.CreatedAt = DateTime.Now;
                announcement.PostedBy = User.Identity?.Name ?? "System Admin";

                _context.AdminAnnouncements.Add(announcement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var announcements = await _context.AdminAnnouncements.OrderByDescending(a => a.AnnouncementId).ToListAsync();
            return View(nameof(Index), announcements);
        }
    }
}
