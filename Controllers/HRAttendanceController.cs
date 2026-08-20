using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,HR")]
    public class HRAttendanceController : Controller
    {
        // GET: /HRAttendance/DailyLogs
        [HttpGet]
        public IActionResult DailyLogs()
        {
            return View();
        }

        // GET: /HRAttendance/Biometric
        [HttpGet]
        public IActionResult Biometric()
        {
            return View();
        }

        // GET: /HRAttendance/ShiftScheduling
        [HttpGet]
        public IActionResult ShiftScheduling()
        {
            return View();
        }

        // GET: /HRAttendance/Overtime
        [HttpGet]
        public IActionResult Overtime()
        {
            return View();
        }

        // GET: /HRAttendance/Regularization
        [HttpGet]
        public IActionResult Regularization()
        {
            return View();
        }
    }
}
