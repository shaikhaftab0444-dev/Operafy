using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,HR,Accountant")]
    public class PayrollController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PayrollController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Payroll
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("PayrollProcessing", "HRPayroll");
        }

        // GET: /Payroll/ManageSalary/5
        [HttpGet]
        public async Task<IActionResult> ManageSalary(int id)
        {
            var employee = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (employee == null)
            {
                return NotFound();
            }

            var structure = await _context.SalaryStructures
                .FirstOrDefaultAsync(s => s.UserId == id);

            if (structure == null)
            {
                // Instantiate default structure
                structure = new SalaryStructure
                {
                    UserId = id,
                    BasicSalary = 30000,
                    HRA = 12000,
                    TransportAllowance = 3000,
                    MedicalAllowance = 2000,
                    ProvidentFund = 3600,
                    ProfessionalTax = 200,
                    NetSalary = 43200 // (30000 + 12000 + 3000 + 2000) - (3600 + 200)
                };
            }

            ViewBag.Employee = employee;
            return View(structure);
        }

        // POST: /Payroll/ManageSalary
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageSalary(SalaryStructure model)
        {
            if (ModelState.IsValid)
            {
                // Dynamic calculations of net salary
                model.NetSalary = model.BasicSalary + model.HRA + model.TransportAllowance + model.MedicalAllowance 
                                - model.ProvidentFund - model.ProfessionalTax;

                var existing = await _context.SalaryStructures
                    .FirstOrDefaultAsync(s => s.UserId == model.UserId);

                if (existing == null)
                {
                    model.CreatedAt = DateTime.UtcNow;
                    _context.SalaryStructures.Add(model);
                }
                else
                {
                    existing.BasicSalary = model.BasicSalary;
                    existing.HRA = model.HRA;
                    existing.TransportAllowance = model.TransportAllowance;
                    existing.MedicalAllowance = model.MedicalAllowance;
                    existing.ProvidentFund = model.ProvidentFund;
                    existing.ProfessionalTax = model.ProfessionalTax;
                    existing.NetSalary = model.NetSalary;
                    existing.UpdatedAt = DateTime.UtcNow;
                    _context.SalaryStructures.Update(existing);
                }

                await _context.SaveChangesAsync();

                // Log payroll update operation
                var user = await _context.Users.FindAsync(model.UserId);
                _context.ActivityLogs.Add(new ActivityLog
                {
                    Title = "Salary Configured",
                    Description = $"Salary structure updated for {user?.FullName}.",
                    IconClass = "fa-money-check-dollar",
                    ColorClass = "bg-primary text-white",
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Salary structure for '{user?.FullName}' saved successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Employee = await _context.Users.FindAsync(model.UserId);
            return View(model);
        }

        // POST: /Payroll/RunMonthlyPayroll
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RunMonthlyPayroll(string payPeriod)
        {
            if (string.IsNullOrEmpty(payPeriod))
            {
                TempData["ErrorMessage"] = "Please select a valid pay period.";
                return RedirectToAction(nameof(Index));
            }

            var activeEmployees = await _context.Users
                .Where(u => u.IsActive)
                .ToListAsync();

            var structures = await _context.SalaryStructures
                .ToDictionaryAsync(s => s.UserId);

            int payslipsGenerated = 0;

            foreach (var emp in activeEmployees)
            {
                if (structures.TryGetValue(emp.UserId, out var sal))
                {
                    // Check if payslip already exists for this user & period
                    var exists = await _context.Payslips
                        .AnyAsync(p => p.UserId == emp.UserId && p.PayPeriod == payPeriod);

                    if (!exists)
                    {
                        var slip = new Payslip
                        {
                            UserId = emp.UserId,
                            PayPeriod = payPeriod,
                            BasicSalary = sal.BasicSalary,
                            HRA = sal.HRA,
                            TransportAllowance = sal.TransportAllowance,
                            MedicalAllowance = sal.MedicalAllowance,
                            ProvidentFund = sal.ProvidentFund,
                            ProfessionalTax = sal.ProfessionalTax,
                            NetSalary = sal.NetSalary,
                            PaidDays = 30,
                            PaymentDate = DateTime.UtcNow,
                            Status = "Paid"
                        };

                        _context.Payslips.Add(slip);
                        payslipsGenerated++;
                    }
                }
            }

            if (payslipsGenerated > 0)
            {
                await _context.SaveChangesAsync();

                // Add log entry
                _context.ActivityLogs.Add(new ActivityLog
                {
                    Title = "Payroll Processed",
                    Description = $"Executed pay run for period {payPeriod}. {payslipsGenerated} payslips generated.",
                    IconClass = "fa-coins",
                    ColorClass = "bg-success text-white",
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Successfully processed payroll for period '{payPeriod}'. {payslipsGenerated} salary slips generated.";
            }
            else
            {
                TempData["ErrorMessage"] = $"No new payslips generated. All active employees already have payslips generated for '{payPeriod}', or no salary structures are configured.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Payroll/ViewPayslip/5
        [HttpGet]
        [AllowAnonymous] // Allow viewing slip easily (though it can be locked down or validated)
        public async Task<IActionResult> ViewPayslip(int id)
        {
            var payslip = await _context.Payslips
                .Include(p => p.User)
                .Include(p => p.User.Role)
                .FirstOrDefaultAsync(p => p.PayslipId == id);

            if (payslip == null)
            {
                return NotFound();
            }

            // Load company info to display on slip header
            var company = await _context.Companies.FirstOrDefaultAsync() ?? new Company
            {
                CompanyName = "AIT Technologies Pvt Ltd",
                CompanyCode = "AIT001",
                AddressLine1 = "AIT Campus, Tech Zone",
                City = "Delhi",
                State = "Delhi",
                Country = "India"
            };

            ViewBag.Company = company;
            return View(payslip);
        }
    }
}
