using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,Inventory Manager,Purchase Manager")]
    public class GRNController : Controller
    {
        // GET: /GRN or /GRN/Index
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("Grn", "InvWarehouse");
        }
    }
}
