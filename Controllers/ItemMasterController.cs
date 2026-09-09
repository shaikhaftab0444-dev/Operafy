using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,Inventory Manager,Purchase Manager")]
    public class ItemMasterController : Controller
    {
        // GET: /ItemMaster or /ItemMaster/Index
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("Items", "InvCatalog");
        }

        // GET: /ItemMaster/Categories
        [HttpGet]
        public IActionResult Categories()
        {
            return RedirectToAction("Categories", "InvCatalog");
        }

        // GET: /ItemMaster/UOM
        [HttpGet]
        public IActionResult UOM()
        {
            return RedirectToAction("Uom", "InvCatalog");
        }
    }
}
