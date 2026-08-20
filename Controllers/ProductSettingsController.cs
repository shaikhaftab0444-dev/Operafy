using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,Sales Manager,Inventory Manager,Manager")]
    public class ProductSettingsController : Controller
    {
        // GET: /ProductSettings/Categories
        [HttpGet]
        public IActionResult Categories()
        {
            return View();
        }

        // GET: /ProductSettings/Brands
        [HttpGet]
        public IActionResult Brands()
        {
            return View();
        }

        // GET: /ProductSettings/Units
        [HttpGet]
        public IActionResult Units()
        {
            return View();
        }

        // GET: /ProductSettings/Variants
        [HttpGet]
        public IActionResult Variants()
        {
            return View();
        }

        // GET: /ProductSettings/PriceLists
        [HttpGet]
        public IActionResult PriceLists()
        {
            return View();
        }

        // GET: /ProductSettings/Barcodes
        [HttpGet]
        public IActionResult Barcodes()
        {
            return View();
        }
    }
}
