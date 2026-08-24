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
    public class AdminBranchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminBranchController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /AdminBranch/Details
        [HttpGet]
        public async Task<IActionResult> Details()
        {
            var branches = await _context.Branches.ToListAsync();
            return View(branches);
        }

        // POST: /AdminBranch/CreateBranch
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBranch(Branch branch)
        {
            if (ModelState.IsValid || (branch.BranchName != null && branch.BranchCode != null))
            {
                branch.IsActive = true;
                _context.Branches.Add(branch);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details));
            }
            var branches = await _context.Branches.ToListAsync();
            return View(nameof(Details), branches);
        }

        // GET: /AdminBranch/Hours
        [HttpGet]
        public async Task<IActionResult> Hours()
        {
            var hours = await _context.AdminBranchHours.ToListAsync();
            return View(hours);
        }

        // POST: /AdminBranch/SaveHours
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveHours(AdminBranchHour hour)
        {
            if (ModelState.IsValid || hour.BranchName != null)
            {
                _context.AdminBranchHours.Add(hour);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Hours));
            }
            var hours = await _context.AdminBranchHours.ToListAsync();
            return View(nameof(Hours), hours);
        }

        // GET: /AdminBranch/Regional
        [HttpGet]
        public IActionResult Regional()
        {
            return View();
        }
    }
}
