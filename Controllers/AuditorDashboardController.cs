using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,Auditor")]
    public class AuditorDashboardController : Controller
    {
        // GET: /AuditorDashboard
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Auditor");
        }
    }
}