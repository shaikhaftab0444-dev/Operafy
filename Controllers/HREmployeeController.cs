using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,HR")]
    public class HREmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HREmployeeController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: /HREmployee/Directory
        [HttpGet]
        public async Task<IActionResult> Directory(int? departmentId = null)
        {
            // Ensure core departments exist
            var existingDepts = await _context.Departments.ToListAsync();
            var targetDeptNames = new[] { "IT & Software", "Sales & Marketing", "Human Resources", "Finance & Accounts", "Operations & Logistics" };
            bool anyNew = false;
            foreach (var dName in targetDeptNames)
            {
                if (!existingDepts.Any(d => d.DepartmentName.Equals(dName, StringComparison.OrdinalIgnoreCase)))
                {
                    string code = dName switch
                    {
                        "IT & Software" => "IT-DEPT",
                        "Sales & Marketing" => "SALES-DEPT",
                        "Human Resources" => "HR-DEPT",
                        "Finance & Accounts" => "FIN-DEPT",
                        "Operations & Logistics" => "OPS-DEPT",
                        _ => "GEN-DEPT"
                    };
                    _context.Departments.Add(new Department
                    {
                        DepartmentCode = code,
                        DepartmentName = dName,
                        BranchId = 3,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    });
                    anyNew = true;
                }
            }
            if (anyNew)
            {
                await _context.SaveChangesAsync();
            }

            // Do not show Super Admin and Admin to HR
            var query = _context.Users
                .Include(u => u.Role)
                .Include(u => u.Department)
                .Where(u => u.Role != null && u.Role.RoleName != "Super Admin" && u.Role.RoleName != "Admin" && u.Role.RoleName != "System Admin");

            if (departmentId.HasValue && departmentId.Value > 0)
            {
                query = query.Where(u => u.DepartmentId == departmentId.Value);
            }

            var employees = await query.OrderBy(u => u.UserId).ToListAsync();
            
            ViewBag.Roles = await _context.Roles
                .Where(r => r.RoleName != "Super Admin" && r.RoleName != "Admin" && r.RoleName != "System Admin")
                .ToListAsync();

            ViewBag.Departments = await _context.Departments
                .Where(d => d.IsActive)
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();

            // Populate active managers in the company
            var managers = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.IsActive && u.Role != null && (u.Role.RoleName == "Manager" || u.Role.RoleName.Contains("Manager") || u.Role.RoleName == "Admin" || u.Role.RoleName == "Super Admin" || u.Role.RoleName.Contains("Lead") || u.Role.RoleName.Contains("Head")))
                .OrderBy(u => u.FullName)
                .ToListAsync();

            if (!managers.Any())
            {
                managers = await _context.Users.Where(u => u.IsActive).OrderBy(u => u.FullName).Take(5).ToListAsync();
            }

            ViewBag.Managers = managers;

            ViewBag.Branches = await _context.Branches.Where(b => b.IsActive).ToListAsync();
            ViewBag.SelectedDepartmentId = departmentId;

            return View(employees);
        }

        // POST: /HREmployee/CreateEmployee
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEmployee(CreateEmployeeInputModel model)
        {
            if (ModelState.IsValid || (!string.IsNullOrEmpty(model.UserName) && !string.IsNullOrEmpty(model.Email)))
            {
                var employee = new User
                {
                    FullName = model.FullName,
                    UserName = model.UserName,
                    Email = model.Email,
                    MobileNumber = model.MobileNumber,
                    CompanyId = 1,
                    BranchId = 3,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    RoleId = model.SelectedRoleId
                };

                var hasher = new PasswordHasher<User>();
                employee.PasswordHash = hasher.HashPassword(employee, string.IsNullOrEmpty(model.Password) ? "Monitor@2026" : model.Password);

                // Department mapping
                if (model.DepartmentId.HasValue && model.DepartmentId.Value > 0)
                {
                    employee.DepartmentId = model.DepartmentId.Value;
                    var dept = await _context.Departments.FindAsync(model.DepartmentId.Value);
                    employee.DepartmentName = dept?.DepartmentName ?? model.DepartmentName;
                }
                else if (!string.IsNullOrWhiteSpace(model.DepartmentName))
                {
                    employee.DepartmentName = model.DepartmentName;
                    var dept = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentName == model.DepartmentName);
                    if (dept != null) employee.DepartmentId = dept.DepartmentId;
                }

                // Reporting Manager mapping
                if (!string.IsNullOrWhiteSpace(model.ReportingManagerId))
                {
                    employee.ReportingManagerId = model.ReportingManagerId;
                    if (int.TryParse(model.ReportingManagerId, out int mgrId))
                    {
                        var mgr = await _context.Users.FindAsync(mgrId);
                        employee.ReportingManagerName = mgr?.FullName ?? mgr?.UserName ?? model.ReportingManagerName;
                    }
                    else
                    {
                        employee.ReportingManagerName = model.ReportingManagerName;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(model.ReportingManagerName))
                {
                    employee.ReportingManagerName = model.ReportingManagerName;
                }

                // Branch mapping
                if (!string.IsNullOrWhiteSpace(model.BranchName))
                {
                    employee.BranchName = model.BranchName;
                    var branch = await _context.Branches.FirstOrDefaultAsync(b => b.BranchName == model.BranchName);
                    if (branch != null) employee.BranchId = branch.BranchId;
                }
                else
                {
                    employee.BranchName = "Head Office";
                }

                // Handle file upload
                if (model.ProfilePhotoFile != null && model.ProfilePhotoFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profile_pics");
                    if (!System.IO.Directory.Exists(uploadsFolder))
                    {
                        System.IO.Directory.CreateDirectory(uploadsFolder);
                    }
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ProfilePhotoFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfilePhotoFile.CopyToAsync(fileStream);
                    }
                    employee.ProfilePhoto = "/uploads/profile_pics/" + uniqueFileName;
                }

                // Generate User Code
                int count = await _context.Users.CountAsync();
                employee.UserCode = $"EMP-{(count + 1).ToString("D3")}";

                _context.Users.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction("Directory");
            }

            var employees = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Department)
                .Where(u => u.Role != null && u.Role.RoleName != "Super Admin" && u.Role.RoleName != "Admin" && u.Role.RoleName != "System Admin")
                .OrderBy(u => u.UserId)
                .ToListAsync();
            
            ViewBag.Roles = await _context.Roles
                .Where(r => r.RoleName != "Super Admin" && r.RoleName != "Admin" && r.RoleName != "System Admin")
                .ToListAsync();

            ViewBag.Departments = await _context.Departments.Where(d => d.IsActive).OrderBy(d => d.DepartmentName).ToListAsync();
            ViewBag.Managers = await _context.Users.Include(u => u.Role).Where(u => u.IsActive && u.Role != null && u.Role.RoleName.Contains("Manager")).ToListAsync();
            ViewBag.Branches = await _context.Branches.Where(b => b.IsActive).ToListAsync();

            return View("Directory", employees);
        }

        // POST: /HREmployee/EditEmployee
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEmployee(int UserId, string FullName, string UserName, string Email, string? MobileNumber, int SelectedRoleId, int? DepartmentId, string? DepartmentName, string? ReportingManagerId, string? ReportingManagerName, string? BranchName, IFormFile? NewProfilePhotoFile)
        {
            var emp = await _context.Users.FindAsync(UserId);
            if (emp != null)
            {
                emp.FullName = FullName;
                emp.UserName = UserName;
                emp.Email = Email;
                emp.MobileNumber = MobileNumber;
                emp.RoleId = SelectedRoleId;
                emp.UpdatedAt = DateTime.Now;

                // Department
                if (DepartmentId.HasValue && DepartmentId.Value > 0)
                {
                    emp.DepartmentId = DepartmentId.Value;
                    var dept = await _context.Departments.FindAsync(DepartmentId.Value);
                    emp.DepartmentName = dept?.DepartmentName ?? DepartmentName;
                }
                else if (!string.IsNullOrWhiteSpace(DepartmentName))
                {
                    emp.DepartmentName = DepartmentName;
                    var dept = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentName == DepartmentName);
                    if (dept != null) emp.DepartmentId = dept.DepartmentId;
                }

                // Reporting Manager
                if (!string.IsNullOrWhiteSpace(ReportingManagerId))
                {
                    emp.ReportingManagerId = ReportingManagerId;
                    if (int.TryParse(ReportingManagerId, out int mgrId))
                    {
                        var mgr = await _context.Users.FindAsync(mgrId);
                        emp.ReportingManagerName = mgr?.FullName ?? mgr?.UserName ?? ReportingManagerName;
                    }
                    else
                    {
                        emp.ReportingManagerName = ReportingManagerName;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(ReportingManagerName))
                {
                    emp.ReportingManagerName = ReportingManagerName;
                }

                // Branch
                if (!string.IsNullOrWhiteSpace(BranchName))
                {
                    emp.BranchName = BranchName;
                    var branch = await _context.Branches.FirstOrDefaultAsync(b => b.BranchName == BranchName);
                    if (branch != null) emp.BranchId = branch.BranchId;
                }

                // Handle file upload
                if (NewProfilePhotoFile != null && NewProfilePhotoFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profile_pics");
                    if (!System.IO.Directory.Exists(uploadsFolder))
                    {
                        System.IO.Directory.CreateDirectory(uploadsFolder);
                    }
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(NewProfilePhotoFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await NewProfilePhotoFile.CopyToAsync(fileStream);
                    }
                    emp.ProfilePhoto = "/uploads/profile_pics/" + uniqueFileName;
                }

                _context.Users.Update(emp);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Directory");
        }

        // POST: /HREmployee/ToggleStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int UserId)
        {
            var emp = await _context.Users.FindAsync(UserId);
            if (emp != null)
            {
                emp.IsActive = !emp.IsActive;
                emp.UpdatedAt = DateTime.Now;
                _context.Users.Update(emp);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Directory");
        }

        // POST: /HREmployee/DeleteEmployee
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEmployee(int UserId)
        {
            var emp = await _context.Users.FindAsync(UserId);
            if (emp != null)
            {
                _context.Users.Remove(emp);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Directory");
        }

        // GET: /HREmployee/Onboarding
        [HttpGet]
        public async Task<IActionResult> Onboarding()
        {
            var onboardings = await _context.Onboardings.OrderByDescending(o => o.OnboardingId).ToListAsync();
            return View(onboardings);
        }

        // POST: /HREmployee/CreateOnboarding
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOnboarding(HROnboarding onboarding)
        {
            if (ModelState.IsValid)
            {
                _context.Onboardings.Add(onboarding);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Onboarding));
            }
            var onboardings = await _context.Onboardings.OrderByDescending(o => o.OnboardingId).ToListAsync();
            return View(nameof(Onboarding), onboardings);
        }

        // GET: /HREmployee/Contracts
        [HttpGet]
        public async Task<IActionResult> Contracts()
        {
            var contracts = await _context.Contracts.OrderByDescending(c => c.ContractId).ToListAsync();
            return View(contracts);
        }

        // POST: /HREmployee/CreateContract
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateContract(HRContract contract)
        {
            if (ModelState.IsValid)
            {
                _context.Contracts.Add(contract);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Contracts));
            }
            var contracts = await _context.Contracts.OrderByDescending(c => c.ContractId).ToListAsync();
            return View(nameof(Contracts), contracts);
        }

        // GET: /HREmployee/Transfers
        [HttpGet]
        public async Task<IActionResult> Transfers()
        {
            var transfers = await _context.Transfers.OrderByDescending(t => t.TransferId).ToListAsync();
            return View(transfers);
        }

        // POST: /HREmployee/CreateTransfer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTransfer(HRTransfer transfer)
        {
            if (ModelState.IsValid)
            {
                _context.Transfers.Add(transfer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Transfers));
            }
            var transfers = await _context.Transfers.OrderByDescending(t => t.TransferId).ToListAsync();
            return View(nameof(Transfers), transfers);
        }

        // GET: /HREmployee/Offboarding
        [HttpGet]
        public async Task<IActionResult> Offboarding()
        {
            var offboardings = await _context.Offboardings.OrderByDescending(o => o.OffboardingId).ToListAsync();
            return View(offboardings);
        }

        // POST: /HREmployee/CreateOffboarding
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOffboarding(HROffboarding offboarding)
        {
            if (ModelState.IsValid)
            {
                _context.Offboardings.Add(offboarding);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Offboarding));
            }
            var offboardings = await _context.Offboardings.OrderByDescending(o => o.OffboardingId).ToListAsync();
            return View(nameof(Offboarding), offboardings);
        }
    }
}
