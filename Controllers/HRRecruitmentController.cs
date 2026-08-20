using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,HR")]
    public class HRRecruitmentController : Controller
    {
        // GET: /HRRecruitment/JobOpenings
        [HttpGet]
        public IActionResult JobOpenings()
        {
            return View();
        }

        // GET: /HRRecruitment/CandidatePipeline
        [HttpGet]
        public IActionResult CandidatePipeline()
        {
            return View();
        }

        // GET: /HRRecruitment/InterviewSchedules
        [HttpGet]
        public IActionResult InterviewSchedules()
        {
            return View();
        }

        // GET: /HRRecruitment/OfferLetters
        [HttpGet]
        public IActionResult OfferLetters()
        {
            return View();
        }
    }
}
