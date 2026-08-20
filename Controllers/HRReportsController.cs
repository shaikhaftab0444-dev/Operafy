using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,HR")]
    public class HRReportsController : Controller
    {
        // GET: /HRReports/AttendanceLate
        [HttpGet]
        public IActionResult AttendanceLate()
        {
            return View();
        }

        // GET: /HRReports/PayrollSummary
        [HttpGet]
        public IActionResult PayrollSummary()
        {
            return View();
        }

        // GET: /HRReports/LeaveBalance
        [HttpGet]
        public IActionResult LeaveBalance()
        {
            return View();
        }

        // GET: /HRReports/AttritionHeadcount
        [HttpGet]
        public IActionResult AttritionHeadcount()
        {
            return View();
        }

        // GET: /HRReports/TaxDeduction
        [HttpGet]
        public IActionResult TaxDeduction()
        {
            return View();
        }
    }
}
