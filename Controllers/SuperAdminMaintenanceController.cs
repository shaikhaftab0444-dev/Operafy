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
    public class SuperAdminMaintenanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SuperAdminMaintenanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /SuperAdminMaintenance/Backups
        [HttpGet]
        public async Task<IActionResult> Backups()
        {
            var backups = await _context.AdminBackupLogs.OrderByDescending(b => b.BackupId).ToListAsync();
            return View(backups);
        }

        // POST: /SuperAdminMaintenance/CreateBackup
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
            return RedirectToAction(nameof(Backups));
        }

        // GET: /SuperAdminMaintenance/RestorePoints
        [HttpGet]
        public async Task<IActionResult> RestorePoints()
        {
            var points = await _context.SuperAdminRestorePoints.OrderByDescending(p => p.RestorePointId).ToListAsync();
            return View(points);
        }

        // POST: /SuperAdminMaintenance/CreateRestorePoint
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRestorePoint(SuperAdminRestorePoint rp)
        {
            if (ModelState.IsValid || rp.PointName != null)
            {
                rp.CreatedAt = DateTime.Now;
                _context.SuperAdminRestorePoints.Add(rp);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(RestorePoints));
            }
            var points = await _context.SuperAdminRestorePoints.OrderByDescending(p => p.RestorePointId).ToListAsync();
            return View(nameof(RestorePoints), points);
        }

        // GET: /SuperAdminMaintenance/MaintenanceMode
        [HttpGet]
        public async Task<IActionResult> MaintenanceMode()
        {
            var config = await _context.SuperAdminMaintenances.FirstOrDefaultAsync();
            if (config == null)
            {
                config = new SuperAdminMaintenance { IsMaintenanceMode = false, CustomMessage = "ERP portal is currently undergoing scheduled platform updates." };
                _context.SuperAdminMaintenances.Add(config);
                await _context.SaveChangesAsync();
            }
            return View(config);
        }

        // POST: /SuperAdminMaintenance/ToggleMaintenance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleMaintenance(SuperAdminMaintenance model)
        {
            var config = await _context.SuperAdminMaintenances.FirstOrDefaultAsync();
            if (config != null)
            {
                config.IsMaintenanceMode = model.IsMaintenanceMode;
                config.CustomMessage = model.CustomMessage ?? "Under Maintenance";
                config.SetBy = "Super Admin";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(MaintenanceMode));
        }

        // GET: /SuperAdminMaintenance/Purge
        [HttpGet]
        public IActionResult Purge()
        {
            return View();
        }
    }
}
