using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System.Threading.Tasks;
using System.Linq;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin")]
    public class SuperAdminSecurityController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SuperAdminSecurityController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /SuperAdminSecurity/AuditTrail
        [HttpGet]
        public async Task<IActionResult> AuditTrail()
        {
            var logs = await _context.ActivityLogs.OrderByDescending(a => a.ActivityLogId).ToListAsync();
            return View(logs);
        }

        // GET: /SuperAdminSecurity/ErrorLogs
        [HttpGet]
        public async Task<IActionResult> ErrorLogs()
        {
            var errors = await _context.SuperAdminErrorLogs.OrderByDescending(e => e.ErrorLogId).ToListAsync();
            return View(errors);
        }

        // GET: /SuperAdminSecurity/LoginFailures
        [HttpGet]
        public async Task<IActionResult> LoginFailures()
        {
            var failures = await _context.AdminLoginAudits
                .Where(l => l.Status == "Failed" || l.Username.Contains("invalid"))
                .OrderByDescending(l => l.AuditId)
                .ToListAsync();
            return View(failures);
        }

        // GET: /SuperAdminSecurity/TwoFactor
        [HttpGet]
        public IActionResult TwoFactor()
        {
            return View();
        }
    }
}
