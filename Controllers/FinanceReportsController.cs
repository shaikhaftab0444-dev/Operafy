using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,Finance Manager,Auditor,Accountant")]
    public class FinanceReportsController : Controller
    {
        // GET: /FinanceReports
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("Reports", "Finance");
        }
    }
}
