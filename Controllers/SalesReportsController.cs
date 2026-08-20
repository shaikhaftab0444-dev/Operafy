using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,Sales Manager,Sales Executive,Manager")]
    public class SalesReportsController : Controller
    {
        // GET: /SalesReports/Summary
        [HttpGet]
        public IActionResult Summary()
        {
            return View();
        }

        // GET: /SalesReports/ByCustomer
        [HttpGet]
        public IActionResult ByCustomer()
        {
            return View();
        }

        // GET: /SalesReports/ByProduct
        [HttpGet]
        public IActionResult ByProduct()
        {
            return View();
        }

        // GET: /SalesReports/BySalesperson
        [HttpGet]
        public IActionResult BySalesperson()
        {
            return View();
        }

        // GET: /SalesReports/TargetReport
        [HttpGet]
        public IActionResult TargetReport()
        {
            return View();
        }

        // GET: /SalesReports/ReceivablesReport
        [HttpGet]
        public IActionResult ReceivablesReport()
        {
            return View();
        }
    }
}
