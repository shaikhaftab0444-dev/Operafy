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
        public async Task<IActionResult> Directory()
        {
            // Do not show Super Admin and Admin to HR
            var employees = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.RoleName != "Super Admin" && u.Role.RoleName != "Admin" && u.Role.RoleName != "System Admin")
                .OrderBy(u => u.UserId)
                .ToListAsync();
            
            ViewBag.Roles = await _context.Roles
                .Where(r => r.RoleName != "Super Admin" && r.RoleName != "Admin" && r.RoleName != "System Admin")
                .ToListAsync();

            return View(employees);
        }

        // POST: /HREmployee/CreateEmployee
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEmployee(User employee, string Password, int SelectedRoleId, IFormFile? ProfilePhotoFile)
        {
            if (ModelState.IsValid || (employee.UserName != null && employee.Email != null))
            {
                var hasher = new PasswordHasher<User>();
                employee.PasswordHash = hasher.HashPassword(employee, string.IsNullOrEmpty(Password) ? "Monitor@2026" : Password);
                employee.CompanyId = 1;
                employee.BranchId = 3;
                employee.IsActive = true;
                employee.CreatedAt = DateTime.Now;
                employee.RoleId = SelectedRoleId;

                // Handle file upload
                if (ProfilePhotoFile != null && ProfilePhotoFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profile_pics");
                    if (!System.IO.Directory.Exists(uploadsFolder))
                    {
                        System.IO.Directory.CreateDirectory(uploadsFolder);
                    }
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ProfilePhotoFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ProfilePhotoFile.CopyToAsync(fileStream);
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
                .Where(u => u.Role.RoleName != "Super Admin" && u.Role.RoleName != "Admin" && u.Role.RoleName != "System Admin")
                .OrderBy(u => u.UserId)
                .ToListAsync();
            
            ViewBag.Roles = await _context.Roles
                .Where(r => r.RoleName != "Super Admin" && r.RoleName != "Admin" && r.RoleName != "System Admin")
                .ToListAsync();

            return View("Directory", employees);
        }

        // POST: /HREmployee/EditEmployee
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEmployee(int UserId, string FullName, string UserName, string Email, string? MobileNumber, int SelectedRoleId, IFormFile? NewProfilePhotoFile)
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
