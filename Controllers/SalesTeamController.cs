using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,Sales Manager,Sales Executive,Manager")]
    public class SalesTeamController : Controller
    {
        // GET: /SalesTeam/Executives
        [HttpGet]
        public IActionResult Executives()
        {
            return View();
        }

        // GET: /SalesTeam/Coordinators
        [HttpGet]
        public IActionResult Coordinators()
        {
            return View();
        }

        // GET: /SalesTeam/Targets
        [HttpGet]
        public IActionResult Targets()
        {
            return View();
        }

        // GET: /SalesTeam/Performance
        [HttpGet]
        public IActionResult Performance()
        {
            return View();
        }
    }
}
