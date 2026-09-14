using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Data;
using ERP_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ERP_System.Controllers
{
    [Authorize]
    public class ESSPayDocumentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ESSPayDocumentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(claim, out int id))
            {
                return id;
            }
            return 1;
        }

        // GET: /ESSPayDocuments or /ESSPayDocuments/Index
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("Payslips");
        }

        // GET: /ESSPayDocuments/Payslips
        [HttpGet]
        public async Task<IActionResult> Payslips()
        {
            var currentUserId = GetCurrentUserId();

            var payslips = await _context.Payslips
                .Include(p => p.User)
                .Where(p => p.UserId == currentUserId)
                .OrderByDescending(p => p.PayslipId)
                .ToListAsync();

            // Auto-populate initial payslips if current employee has none in database
            if (!payslips.Any())
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(u => u.UserId == currentUserId);

                if (user != null)
                {
                    var baseSalary = 60000m;
                    var slipCurrent = new Payslip
                    {
                        UserId = currentUserId,
                        PayPeriod = DateTime.UtcNow.ToString("MMMM yyyy"),
                        PayslipNumber = $"PS-{DateTime.UtcNow:yyyyMM}-{currentUserId:D4}",
                        BasicSalary = baseSalary * 0.50m,
                        HRA = baseSalary * 0.25m,
                        TransportAllowance = 3000m,
                        MedicalAllowance = 2500m,
                        SpecialAllowance = baseSalary * 0.15m,
                        BonusIncentives = 0m,
                        OvertimePay = 0m,
                        GrossSalary = baseSalary,
                        ProvidentFund = baseSalary * 0.50m * 0.12m,
                        ProfessionalTax = 200m,
                        TDS = 1800m,
                        TotalDeductions = (baseSalary * 0.50m * 0.12m) + 200m + 1800m,
                        NetSalary = baseSalary - ((baseSalary * 0.50m * 0.12m) + 200m + 1800m),
                        TotalWorkingDays = 30,
                        PresentDays = 30,
                        PaidDays = 30,
                        UnpaidLeaveDays = 0,
                        Status = "Paid",
                        PaymentDate = DateTime.UtcNow
                    };

                    var lastMonth = DateTime.UtcNow.AddMonths(-1);
                    var slipPrevious = new Payslip
                    {
                        UserId = currentUserId,
                        PayPeriod = lastMonth.ToString("MMMM yyyy"),
                        PayslipNumber = $"PS-{lastMonth:yyyyMM}-{currentUserId:D4}",
                        BasicSalary = baseSalary * 0.50m,
                        HRA = baseSalary * 0.25m,
                        TransportAllowance = 3000m,
                        MedicalAllowance = 2500m,
                        SpecialAllowance = baseSalary * 0.15m,
                        BonusIncentives = 0m,
                        OvertimePay = 0m,
                        GrossSalary = baseSalary,
                        ProvidentFund = baseSalary * 0.50m * 0.12m,
                        ProfessionalTax = 200m,
                        TDS = 1800m,
                        TotalDeductions = (baseSalary * 0.50m * 0.12m) + 200m + 1800m,
                        NetSalary = baseSalary - ((baseSalary * 0.50m * 0.12m) + 200m + 1800m),
                        TotalWorkingDays = 30,
                        PresentDays = 30,
                        PaidDays = 30,
                        UnpaidLeaveDays = 0,
                        Status = "Paid",
                        PaymentDate = lastMonth
                    };

                    _context.Payslips.AddRange(slipCurrent, slipPrevious);
                    await _context.SaveChangesAsync();

                    payslips = await _context.Payslips
                        .Include(p => p.User)
                        .Where(p => p.UserId == currentUserId)
                        .OrderByDescending(p => p.PayslipId)
                        .ToListAsync();
                }
            }

            return View(payslips);
        }

        // GET: /ESSPayDocuments/DownloadPayslip/{id}
        [HttpGet]
        public async Task<IActionResult> DownloadPayslip(int id)
        {
            var currentUserId = GetCurrentUserId();

            var payslip = await _context.Payslips
                .Include(p => p.User)
                    .ThenInclude(u => u!.Role)
                .Include(p => p.User)
                    .ThenInclude(u => u!.Department)
                .Include(p => p.User)
                    .ThenInclude(u => u!.Branch)
                .FirstOrDefaultAsync(p => p.PayslipId == id);

            if (payslip == null)
            {
                return NotFound();
            }

            if (payslip.UserId != currentUserId && !User.IsInRole("HR") && !User.IsInRole("Admin") && !User.IsInRole("Super Admin"))
            {
                return Forbid();
            }

            var company = await _context.Companies.FirstOrDefaultAsync() ?? new Company
            {
                CompanyName = "Operafy ERP Systems Pvt Ltd",
                CompanyCode = "OPR001",
                AddressLine1 = "Tech Zone, Phase 2, Business District",
                City = "New Delhi",
                State = "Delhi",
                Country = "India"
            };

            ViewBag.Company = company;
            return View("PayslipPrintView", payslip);
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
