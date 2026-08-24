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
    [Authorize(Roles = "Super Admin")]
    public class SuperAdminBroadcastsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SuperAdminBroadcastsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /SuperAdminBroadcasts/Announcements
        [HttpGet]
        public async Task<IActionResult> Announcements()
        {
            var announcements = await _context.AdminAnnouncements.OrderByDescending(a => a.AnnouncementId).ToListAsync();
            return View(announcements);
        }

        // POST: /SuperAdminBroadcasts/CreateAnnouncement
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAnnouncement(AdminAnnouncement announcement)
        {
            if (ModelState.IsValid || (announcement.Title != null && announcement.Content != null))
            {
                announcement.CreatedAt = DateTime.Now;
                announcement.PostedBy = "Super Admin";

                _context.AdminAnnouncements.Add(announcement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Announcements));
            }
            var announcements = await _context.AdminAnnouncements.OrderByDescending(a => a.AnnouncementId).ToListAsync();
            return View(nameof(Announcements), announcements);
        }

        // GET: /SuperAdminBroadcasts/BusinessReports
        [HttpGet]
        public IActionResult BusinessReports()
        {
            return View();
        }

        // GET: /SuperAdminBroadcasts/TaxPackages
        [HttpGet]
        public IActionResult TaxPackages()
        {
            return View();
        }
    }
}
