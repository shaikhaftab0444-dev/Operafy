using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System.Threading.Tasks;
using System;
using System.IO;
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
            var announcements = await _context.AdminAnnouncements
                .OrderByDescending(a => a.IsPinned)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();

            ViewBag.Branches = await _context.Branches.ToListAsync();
            return View(announcements);
        }

        // POST: /AdminAnnouncements/CreateAnnouncement
        [HttpPost]
        public async Task<IActionResult> CreateAnnouncement([FromForm] AnnouncementViewModel model)
        {
            if (ModelState.IsValid)
            {
                string? filename = null;
                string? fileUrl = null;

                if (model.AttachmentFile != null && model.AttachmentFile.Length > 0)
                {
                    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);
                    
                    filename = Path.GetFileName(model.AttachmentFile.FileName);
                    var filePath = Path.Combine(uploadsDir, filename);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.AttachmentFile.CopyToAsync(stream);
                    }
                    fileUrl = "/uploads/" + filename;
                }

                var announcement = new AdminAnnouncement
                {
                    Title = model.Title,
                    Content = model.Content,
                    Priority = model.Priority,
                    Category = model.Category,
                    IsPinned = model.IsPinned,
                    AttachmentName = filename,
                    AttachmentUrl = fileUrl,
                    TargetAudience = model.TargetAudience,
                    TargetBranch = model.TargetBranch,
                    ExpiryDate = model.ExpiryDate,
                    CreatedAt = DateTime.UtcNow,
                    PostedBy = User.Identity?.Name ?? "System Admin"
                };

                _context.AdminAnnouncements.Add(announcement);
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "Announcement published successfully!",
                    announcementId = announcement.AnnouncementId,
                    title = announcement.Title,
                    content = announcement.Content,
                    priority = announcement.Priority,
                    category = announcement.Category,
                    isPinned = announcement.IsPinned,
                    attachmentName = announcement.AttachmentName,
                    attachmentUrl = announcement.AttachmentUrl,
                    postedBy = announcement.PostedBy,
                    createdAt = announcement.CreatedAt.ToLocalTime().ToString("dd MMM yyyy, hh:mm tt"),
                    targetAudience = announcement.TargetAudience,
                    targetBranch = announcement.TargetBranch
                });
            }

            var errors = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return Json(new { success = false, message = "Form validation failed: " + errors });
        }

        // POST: /AdminAnnouncements/EditAnnouncement
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAnnouncement(AnnouncementViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existing = await _context.AdminAnnouncements.FirstOrDefaultAsync(a => a.AnnouncementId == model.AnnouncementId);
                if (existing == null) return NotFound();

                existing.Title = model.Title;
                existing.Content = model.Content;
                existing.Priority = model.Priority;
                existing.Category = model.Category;
                existing.IsPinned = model.IsPinned;
                existing.TargetAudience = model.TargetAudience;
                existing.TargetBranch = model.TargetBranch;
                existing.ExpiryDate = model.ExpiryDate;

                if (model.AttachmentFile != null && model.AttachmentFile.Length > 0)
                {
                    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);
                    
                    var filename = Path.GetFileName(model.AttachmentFile.FileName);
                    var filePath = Path.Combine(uploadsDir, filename);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.AttachmentFile.CopyToAsync(stream);
                    }
                    existing.AttachmentName = filename;
                    existing.AttachmentUrl = "/uploads/" + filename;
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var announcements = await _context.AdminAnnouncements
                .OrderByDescending(a => a.IsPinned)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();

            ViewBag.Branches = await _context.Branches.ToListAsync();
            return View(nameof(Index), announcements);
        }

        // POST: /AdminAnnouncements/TogglePin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePin(int id)
        {
            var existing = await _context.AdminAnnouncements.FirstOrDefaultAsync(a => a.AnnouncementId == id);
            if (existing == null) return Json(new { success = false, message = "Announcement not found." });

            existing.IsPinned = !existing.IsPinned;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = existing.IsPinned ? "Announcement pinned to top." : "Announcement unpinned." });
        }

        // POST: /AdminAnnouncements/DeleteAnnouncement
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAnnouncement(int id)
        {
            var existing = await _context.AdminAnnouncements.FirstOrDefaultAsync(a => a.AnnouncementId == id);
            if (existing == null) return Json(new { success = false, message = "Announcement not found." });

            _context.AdminAnnouncements.Remove(existing);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Announcement deleted successfully." });
        }
    }
}
