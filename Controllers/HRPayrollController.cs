using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,HR")]
    public class HRPayrollController : Controller
    {
        // GET: /HRPayroll/SalaryStructures
        [HttpGet]
        public IActionResult SalaryStructures()
        {
            return View();
        }

        // GET: /HRPayroll/AllowancesDeductions
        [HttpGet]
        public IActionResult AllowancesDeductions()
        {
            return View();
        }

        // GET: /HRPayroll/StatutoryCompliance
        [HttpGet]
        public IActionResult StatutoryCompliance()
        {
            return View();
        }

        // GET: /HRPayroll/PayrollProcessing
        [HttpGet]
        public IActionResult PayrollProcessing()
        {
            return View();
        }

        // GET: /HRPayroll/Payslips
        [HttpGet]
        public IActionResult Payslips()
        {
            return View();
        }

        // GET: /HRPayroll/BonusIncentives
        [HttpGet]
        public IActionResult BonusIncentives()
        {
            return View();
        }
    }
}
