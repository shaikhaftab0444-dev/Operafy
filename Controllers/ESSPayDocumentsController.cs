using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ERP_System.Controllers
{
    [Authorize]
    public class ESSPayDocumentsController : Controller
    {
        // GET: /ESSPayDocuments/Payslips
        [HttpGet]
        public IActionResult Payslips()
        {
            return View();
        }

        // GET: /ESSPayDocuments/TaxDeduction
        [HttpGet]
        public IActionResult TaxDeduction()
        {
            return View();
        }

        // GET: /ESSPayDocuments/CompanyPolicies
        [HttpGet]
        public IActionResult CompanyPolicies()
        {
            return View();
        }
    }
}
