using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,Finance Manager,Accountant")]
    public class AccountingController : Controller
    {
        // GET: /Accounting
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Accounts");
        }

        // GET: /Accounting/Ledger
        [HttpGet]
        public IActionResult Ledger()
        {
            return RedirectToAction("Ledger", "Finance");
        }
    }
}
