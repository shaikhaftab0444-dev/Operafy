using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,HR")]
    public class HRLeaveController : Controller
    {
        // GET: /HRLeave/Policy
        [HttpGet]
        public IActionResult Policy()
        {
            return View();
        }

        // GET: /HRLeave/Applications
        [HttpGet]
        public IActionResult Applications()
        {
            return View();
        }

        // GET: /HRLeave/BalanceLedger
        [HttpGet]
        public IActionResult BalanceLedger()
        {
            return View();
        }

        // GET: /HRLeave/Approvals
        [HttpGet]
        public IActionResult Approvals()
        {
            return View();
        }

        // GET: /HRLeave/HolidayList
        [HttpGet]
        public IActionResult HolidayList()
        {
            return View();
        }
    }
}
