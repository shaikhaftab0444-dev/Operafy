using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System.Threading.Tasks;
using System.Linq;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin")]
    public class AdminReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /AdminReports/UserActivity
        [HttpGet]
        public async Task<IActionResult> UserActivity()
        {
            var logs = await _context.ActivityLogs.OrderByDescending(a => a.ActivityLogId).ToListAsync();
            return View(logs);
        }

        // GET: /AdminReports/LoginAudit
        [HttpGet]
        public async Task<IActionResult> LoginAudit()
        {
            var audits = await _context.AdminLoginAudits.OrderByDescending(l => l.AuditId).ToListAsync();
            return View(audits);
        }

        // GET: /AdminReports/BranchSummary
        [HttpGet]
        public async Task<IActionResult> BranchSummary()
        {
            var branches = await _context.Branches.ToListAsync();
            return View(branches);
        }
    }
}
