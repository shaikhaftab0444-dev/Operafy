using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,Sales Manager,Sales Executive,Manager")]
    public class SalesManagementController : Controller
    {
        // GET: /SalesManagement/Leads
        [HttpGet]
        public IActionResult Leads()
        {
            return View();
        }

        // GET: /SalesManagement/Quotations
        [HttpGet]
        public IActionResult Quotations()
        {
            return View();
        }

        // GET: /SalesManagement/Orders
        [HttpGet]
        public IActionResult Orders()
        {
            return View();
        }

        // GET: /SalesManagement/Returns
        [HttpGet]
        public IActionResult Returns()
        {
            return View();
        }

        // GET: /SalesManagement/Receivables
        [HttpGet]
        public IActionResult Receivables()
        {
            return View();
        }
    }
}
