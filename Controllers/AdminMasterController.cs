using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System.Threading.Tasks;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin")]
    public class AdminMasterController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminMasterController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /AdminMaster/Currency
        [HttpGet]
        public IActionResult Currency()
        {
            return View();
        }

        // GET: /AdminMaster/Tax
        [HttpGet]
        public IActionResult Tax()
        {
            return View();
        }

        // GET: /AdminMaster/Departments
        [HttpGet]
        public IActionResult Departments()
        {
            return View();
        }

        // GET: /AdminMaster/Designations
        [HttpGet]
        public IActionResult Designations()
        {
            return View();
        }

        // GET: /AdminMaster/Holidays
        [HttpGet]
        public async Task<IActionResult> Holidays()
        {
            var holidays = await _context.Holidays.ToListAsync();
            return View(holidays);
        }
    }
}
