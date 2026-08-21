using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,Inventory Manager")]
    public class InvReportsController : Controller
    {
        // GET: /InvReports/Valuation
        [HttpGet]
        public IActionResult Valuation()
        {
            return View();
        }

        // GET: /InvReports/MovementLedger
        [HttpGet]
        public IActionResult MovementLedger()
        {
            return View();
        }

        // GET: /InvReports/ReorderReport
        [HttpGet]
        public IActionResult ReorderReport()
        {
            return View();
        }

        // GET: /InvReports/TransferSummary
        [HttpGet]
        public IActionResult TransferSummary()
        {
            return View();
        }

        // GET: /InvReports/ScrapReport
        [HttpGet]
        public IActionResult ScrapReport()
        {
            return View();
        }
    }
}
