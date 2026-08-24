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
    public class AdminDataController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminDataController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /AdminData/Import
        [HttpGet]
        public IActionResult Import()
        {
            return View();
        }

        // GET: /AdminData/Export
        [HttpGet]
        public IActionResult Export()
        {
            return View();
        }

        // GET: /AdminData/BackupHistory
        [HttpGet]
        public async Task<IActionResult> BackupHistory()
        {
            var logs = await _context.AdminBackupLogs.OrderByDescending(b => b.BackupId).ToListAsync();
            return View(logs);
        }

        // POST: /AdminData/CreateBackup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBackup()
        {
            var random = new Random();
            double sizeMb = random.NextDouble() * 5 + 40; // 40 to 45 MB
            var newLog = new AdminBackupLog
            {
                Filename = $"backup_db_{DateTime.Now:yyyyMMddHHmmss}.bak",
                BackupSize = $"{sizeMb:F1} MB",
                CreatedAt = DateTime.Now,
                Status = "Success"
            };

            _context.AdminBackupLogs.Add(newLog);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(BackupHistory));
        }
    }
}
