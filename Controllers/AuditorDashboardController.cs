using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,Auditor")]
    public class AuditorDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditorDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /AuditorDashboard
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            User? currentUser = null;

            if (int.TryParse(userIdClaim, out int parsedId))
            {
                currentUser = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.UserId == parsedId);
            }

            int companyId = currentUser?.CompanyId ?? 1;
            int branchId = currentUser?.BranchId ?? 3;

            var companyRecord = await _context.Companies.FindAsync(companyId);
            var branchRecord = await _context.Branches.FindAsync(branchId);
            var activeCompanyName = companyRecord?.CompanyName ?? "ERP Solutions Ltd";
            var activeBranchName = branchRecord?.BranchName ?? "Head Office";

            int totalCompanies = await _context.Companies.CountAsync();
            int totalUsers = await _context.Users.CountAsync();
            int totalRoles = await _context.Roles.CountAsync();
            int totalAccountHeads = await _context.AccountHeads.CountAsync();
            int totalTransactions = await _context.Transactions.CountAsync();

            var recentActivities = await _context.ActivityLogs
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();

            var auditTransactions = await _context.Transactions
                .OrderByDescending(t => t.Date)
                .Take(5)
                .ToListAsync();

            var viewModel = new AuditorDashboardViewModel
            {
                CurrentUserFullName = currentUser?.FullName ?? "System Auditor",
                CurrentUserRole = currentUser?.Role?.RoleName ?? "Auditor",
                CurrentCompany = $"{activeCompanyName} ({activeBranchName})",
                RegisteredCompaniesCount = totalCompanies,
                RegisteredUsersCount = totalUsers,
                TotalRolesCount = totalRoles,
                AccountHeadsCount = totalAccountHeads,
                TotalTransactionsCount = totalTransactions,
                RecentActivities = recentActivities,
                AuditTransactions = auditTransactions
            };

            return View(viewModel);
        }
    }
}
