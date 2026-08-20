using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,HR")]
    public class HRPerformanceController : Controller
    {
        // GET: /HRPerformance/OkrKpi
        [HttpGet]
        public IActionResult OkrKpi()
        {
            return View();
        }

        // GET: /HRPerformance/AppraisalCycles
        [HttpGet]
        public IActionResult AppraisalCycles()
        {
            return View();
        }

        // GET: /HRPerformance/RatingCards
        [HttpGet]
        public IActionResult RatingCards()
        {
            return View();
        }
    }
}
