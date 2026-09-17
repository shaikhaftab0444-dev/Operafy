using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ERP_System.Data;
using ERP_System.Models;
using ERP_System.Hubs;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,HR,Finance Manager,Accountant,Employee")]
    public class HRPayrollController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ErpNotificationHub> _hubContext;

        public HRPayrollController(ApplicationDbContext context, IHubContext<ErpNotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }

            var username = User.Identity?.Name;
            if (!string.IsNullOrEmpty(username))
            {
                var user = _context.Users.FirstOrDefault(u => u.UserName == username || u.Email == username);
                if (user != null) return user.UserId;
            }

            return 1; // Fallback to Admin
        }

        private bool IsHrOrAdminOrFinance()
        {
            return User.IsInRole("Super Admin") || User.IsInRole("Admin") || User.IsInRole("HR") || User.IsInRole("Finance Manager") || User.IsInRole("Accountant");
        }

        // ==========================================
        // 1. SALARY STRUCTURES & ASSIGNMENTS
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> SalaryStructures(int? year, int? month, int? departmentId, string search = "", string activeTab = "templates")
        {
            if (!IsHrOrAdminOrFinance())
            {
                return RedirectToAction(nameof(Payslips));
            }

            int filterYear = year ?? DateTime.Today.Year;
            int filterMonth = month ?? DateTime.Today.Month;

            ViewBag.SelectedYear = filterYear;
            ViewBag.SelectedMonth = filterMonth;
            ViewBag.SelectedDepartmentId = departmentId;
            ViewBag.SearchTerm = search;
            ViewBag.ActiveTab = activeTab;

            // 1. Dynamic database-driven departments for dropdowns
            var departments = await _context.Departments
                .Where(d => d.IsActive)
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();

            // 2. Salary structure templates with linked department
            var structures = await _context.SalaryStructureMasters
                .Include(s => s.DepartmentObj)
                .OrderByDescending(s => s.IsActive)
                .ThenBy(s => s.StructureName)
                .ToListAsync();

            // 3. Current active salary assignments query with server-side date and scope filtering
            var query = _context.EmployeeSalaryAssignments
                .Include(a => a.User)
                    .ThenInclude(u => u!.Department)
                .Include(a => a.User)
                    .ThenInclude(u => u!.Role)
                .Include(a => a.Structure)
                .Where(a => a.IsCurrent)
                .AsQueryable();

            // Filter by effective period (records effective on or before the selected Month/Year)
            if (filterYear > 0)
            {
                if (filterMonth > 0)
                {
                    query = query.Where(a => a.EffectiveFrom.Year <= filterYear &&
                                             (a.EffectiveFrom.Year < filterYear || a.EffectiveFrom.Month <= filterMonth));
                }
                else
                {
                    query = query.Where(a => a.EffectiveFrom.Year <= filterYear);
                }
            }

            if (departmentId.HasValue && departmentId.Value > 0)
            {
                query = query.Where(a => a.User != null && (a.User.DepartmentId == departmentId.Value || (a.Structure != null && a.Structure.DepartmentId == departmentId.Value)));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var cleanSearch = search.Trim().ToLower();
                query = query.Where(a => a.User != null &&
                    (a.User.FullName.ToLower().Contains(cleanSearch) ||
                     a.User.UserName.ToLower().Contains(cleanSearch) ||
                     a.User.UserCode.ToLower().Contains(cleanSearch) ||
                     a.User.Email.ToLower().Contains(cleanSearch)));
            }

            var assignments = await query.OrderByDescending(a => a.EffectiveFrom).ToListAsync();

            var activeEmployees = await _context.Users
                .Include(u => u.Department)
                .Include(u => u.Role)
                .Where(u => u.IsActive && u.Role != null && u.Role.RoleName != "Admin" && u.Role.RoleName != "Super Admin")
                .OrderBy(u => u.FullName)
                .ToListAsync();

            ViewBag.Departments = departments;
            ViewBag.Structures = structures;
            ViewBag.Assignments = assignments;
            ViewBag.ActiveEmployees = activeEmployees;
            ViewBag.TotalAssigned = assignments.Count;
            ViewBag.TotalStructures = structures.Count;
            ViewBag.TotalTemplates = structures.Count;
            ViewBag.AssignedCount = assignments.Count;

            return View();
        }

        // Scalable 300ms debounced AJAX typeahead lookup to prevent client freezing with 2,000+ employees
        [HttpGet]
        public async Task<IActionResult> SearchEmployees(string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            {
                return Json(new List<object>());
            }

            q = q.Trim().ToLower();

            var employees = await _context.Users
                .Include(u => u.Department)
                .Include(u => u.Role)
                .Where(u => u.IsActive &&
                    (u.FullName.ToLower().Contains(q) ||
                     u.Email.ToLower().Contains(q) ||
                     u.UserCode.ToLower().Contains(q) ||
                     u.UserName.ToLower().Contains(q)))
                .OrderBy(u => u.FullName)
                .Take(25)
                .Select(u => new
                {
                    id = u.UserId,
                    name = u.FullName,
                    code = u.UserCode,
                    email = u.Email,
                    department = u.Department != null ? u.Department.DepartmentName : (!string.IsNullOrEmpty(u.DepartmentName) ? u.DepartmentName : "General"),
                    designation = u.Role != null ? u.Role.RoleName : "Staff",
                    initials = u.FullName.Length >= 2 ? u.FullName.Substring(0, 2).ToUpper() : "U",
                    avatar = u.ProfilePhoto
                })
                .ToListAsync();

            return Json(employees);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSalaryStructure(SalaryStructureMaster model)
        {
            if (!IsHrOrAdminOrFinance()) return Forbid();

            if (ModelState.IsValid)
            {
                if (model.DepartmentId.HasValue && model.DepartmentId.Value > 0)
                {
                    var dept = await _context.Departments.FindAsync(model.DepartmentId.Value);
                    if (dept != null)
                    {
                        model.Department = dept.DepartmentName;
                    }
                }

                model.CreatedAt = DateTime.UtcNow;
                _context.SalaryStructureMasters.Add(model);
                await _context.SaveChangesAsync();

                _context.ActivityLogs.Add(new ActivityLog
                {
                    Title = "Salary Structure Created",
                    Description = $"Created new structure template '{model.StructureName}'.",
                    IconClass = "fa-sitemap",
                    ColorClass = "text-primary",
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                // Broadcast real-time SignalR event
                try
                {
                    await _hubContext.Clients.Groups("HR", "Super Admin", "Admin", "Finance Manager")
                        .SendAsync("ReceiveSalaryStructureUpdated", new { structureName = model.StructureName, action = "Created", timestamp = DateTime.UtcNow });
                }
                catch { }

                TempData["SuccessMessage"] = $"Salary Structure '{model.StructureName}' created successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Validation failed. Please check form fields.";
            }

            return RedirectToAction(nameof(SalaryStructures));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSalaryStructure(SalaryStructureMaster model)
        {
            if (!IsHrOrAdminOrFinance()) return Forbid();

            var existing = await _context.SalaryStructureMasters.FindAsync(model.StructureId);
            if (existing == null) return NotFound();

            if (model.DepartmentId.HasValue && model.DepartmentId.Value > 0)
            {
                var dept = await _context.Departments.FindAsync(model.DepartmentId.Value);
                if (dept != null)
                {
                    existing.Department = dept.DepartmentName;
                    existing.DepartmentId = dept.DepartmentId;
                }
            }
            else
            {
                existing.Department = model.Department ?? "All Departments";
                existing.DepartmentId = null;
            }

            existing.StructureName = model.StructureName;
            existing.Description = model.Description;
            existing.Designation = model.Designation;
            existing.BasicPercent = model.BasicPercent;
            existing.HRAPercent = model.HRAPercent;
            existing.LTAPercent = model.LTAPercent;
            existing.ConveyanceAllowance = model.ConveyanceAllowance;
            existing.MedicalAllowance = model.MedicalAllowance;
            existing.OtherAllowance = model.OtherAllowance;
            existing.AutoCalculateSpecialAllowance = model.AutoCalculateSpecialAllowance;
            existing.IsPFEnabled = model.IsPFEnabled;
            existing.IsESIEnabled = model.IsESIEnabled;
            existing.IsPTEnabled = model.IsPTEnabled;
            existing.IsTDSEnabled = model.IsTDSEnabled;
            existing.IsActive = model.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.SalaryStructureMasters.Update(existing);
            await _context.SaveChangesAsync();

            // Broadcast real-time SignalR event
            try
            {
                await _hubContext.Clients.Groups("HR", "Super Admin", "Admin", "Finance Manager")
                    .SendAsync("ReceiveSalaryStructureUpdated", new { structureName = existing.StructureName, action = "Updated", timestamp = DateTime.UtcNow });
            }
            catch { }

            TempData["SuccessMessage"] = $"Salary Structure '{existing.StructureName}' updated successfully.";
            return RedirectToAction(nameof(SalaryStructures));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DuplicateSalaryStructure(int id)
        {
            if (!IsHrOrAdminOrFinance()) return Forbid();

            var original = await _context.SalaryStructureMasters.FindAsync(id);
            if (original == null) return NotFound();

            var copy = new SalaryStructureMaster
            {
                StructureName = $"Copy of {original.StructureName}",
                Description = original.Description,
                DepartmentId = original.DepartmentId,
                Department = original.Department,
                Designation = original.Designation,
                BasicPercent = original.BasicPercent,
                HRAPercent = original.HRAPercent,
                LTAPercent = original.LTAPercent,
                ConveyanceAllowance = original.ConveyanceAllowance,
                MedicalAllowance = original.MedicalAllowance,
                OtherAllowance = original.OtherAllowance,
                AutoCalculateSpecialAllowance = original.AutoCalculateSpecialAllowance,
                IsPFEnabled = original.IsPFEnabled,
                IsESIEnabled = original.IsESIEnabled,
                IsPTEnabled = original.IsPTEnabled,
                IsTDSEnabled = original.IsTDSEnabled,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.SalaryStructureMasters.Add(copy);
            await _context.SaveChangesAsync();

            try
            {
                await _hubContext.Clients.Groups("HR", "Super Admin", "Admin", "Finance Manager")
                    .SendAsync("ReceiveSalaryStructureUpdated", new { structureName = copy.StructureName, action = "Created", timestamp = DateTime.UtcNow });
            }
            catch { }

            TempData["SuccessMessage"] = $"Duplicated structure successfully as '{copy.StructureName}'.";
            return RedirectToAction(nameof(SalaryStructures));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleSalaryStructureStatus(int id)
        {
            if (!IsHrOrAdminOrFinance()) return Forbid();

            var structure = await _context.SalaryStructureMasters.FindAsync(id);
            if (structure == null) return NotFound();

            structure.IsActive = !structure.IsActive;
            structure.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            try
            {
                await _hubContext.Clients.Groups("HR", "Super Admin", "Admin", "Finance Manager")
                    .SendAsync("ReceiveSalaryStructureUpdated", new { structureName = structure.StructureName, action = structure.IsActive ? "Activated" : "Deactivated", timestamp = DateTime.UtcNow });
            }
            catch { }

            TempData["SuccessMessage"] = $"Structure '{structure.StructureName}' status set to {(structure.IsActive ? "Active" : "Inactive")}.";
            return RedirectToAction(nameof(SalaryStructures));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSalaryStructure(int id)
        {
            if (!IsHrOrAdminOrFinance()) return Forbid();

            var structure = await _context.SalaryStructureMasters.FindAsync(id);
            if (structure == null) return NotFound();

            // Safety check: Do not delete if assigned to employees or used in finalized payrolls
            bool isAssigned = await _context.EmployeeSalaryAssignments.AnyAsync(a => a.StructureId == id && a.IsCurrent);
            if (isAssigned)
            {
                TempData["ErrorMessage"] = $"Cannot delete structure '{structure.StructureName}' because it is currently assigned to one or more active employees.";
                return RedirectToAction(nameof(SalaryStructures));
            }

            _context.SalaryStructureMasters.Remove(structure);
            await _context.SaveChangesAsync();

            try
            {
                await _hubContext.Clients.Groups("HR", "Super Admin", "Admin", "Finance Manager")
                    .SendAsync("ReceiveSalaryStructureUpdated", new { structureName = structure.StructureName, action = "Deleted", timestamp = DateTime.UtcNow });
            }
            catch { }

            TempData["SuccessMessage"] = $"Salary Structure '{structure.StructureName}' deleted successfully.";
            return RedirectToAction(nameof(SalaryStructures));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignSalaryToEmployee(int userId, int? structureId, decimal annualCtc, string? remarks)
        {
            if (!IsHrOrAdminOrFinance()) return Forbid();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            SalaryStructureMaster? structConfig = null;
            if (structureId.HasValue && structureId.Value > 0)
            {
                structConfig = await _context.SalaryStructureMasters.FindAsync(structureId.Value);
            }

            decimal basicPct = structConfig?.BasicPercent ?? 50.00m;
            decimal hraPct = structConfig?.HRAPercent ?? 20.00m;
            decimal ltaPct = structConfig?.LTAPercent ?? 5.00m;
            decimal conv = structConfig?.ConveyanceAllowance ?? 1600.00m;
            decimal med = structConfig?.MedicalAllowance ?? 1250.00m;
            decimal other = structConfig?.OtherAllowance ?? 0.00m;

            decimal monthlyCtc = Math.Round(annualCtc / 12.0m, 2);
            decimal basic = Math.Round(monthlyCtc * (basicPct / 100.0m), 2);
            decimal hra = Math.Round(basic * (hraPct / 100.0m), 2);
            decimal lta = Math.Round(basic * (ltaPct / 100.0m), 2);
            decimal special = Math.Max(0, monthlyCtc - (basic + hra + lta + conv + med + other));
            decimal gross = basic + hra + lta + conv + med + other + special;

            bool isPf = structConfig?.IsPFEnabled ?? true;
            bool isEsi = structConfig?.IsESIEnabled ?? true;
            bool isPt = structConfig?.IsPTEnabled ?? true;
            bool isTds = structConfig?.IsTDSEnabled ?? true;

            decimal pfEmp = isPf ? Math.Round(Math.Min(basic, 15000.00m) * 0.12m, 2) : 0.00m;
            decimal esiEmp = (isEsi && gross <= 21000.00m) ? Math.Round(gross * 0.0075m, 2) : 0.00m;
            decimal pt = (isPt && gross > 15000.00m) ? 200.00m : 0.00m;
            decimal tds = (isTds && annualCtc > 700000.00m) ? Math.Round(monthlyCtc * 0.05m, 2) : 0.00m;
            decimal net = gross - (pfEmp + esiEmp + pt + tds);

            decimal pfEmployer = isPf ? Math.Round(Math.Min(basic, 15000.00m) * 0.12m, 2) : 0.00m;
            decimal esiEmployer = (isEsi && gross <= 21000.00m) ? Math.Round(gross * 0.0325m, 2) : 0.00m;

            // Deactivate old assignment for user
            var oldAssignments = await _context.EmployeeSalaryAssignments
                .Where(a => a.UserId == userId && a.IsCurrent)
                .ToListAsync();

            foreach (var old in oldAssignments)
            {
                old.IsCurrent = false;
                old.EffectiveTo = DateTime.Today;
                old.UpdatedAt = DateTime.UtcNow;
                _context.EmployeeSalaryAssignments.Update(old);
            }

            // Create new assignment
            var newAssignment = new EmployeeSalaryAssignment
            {
                UserId = userId,
                StructureId = structureId,
                AnnualCTC = annualCtc,
                MonthlyBasic = basic,
                MonthlyHRA = hra,
                MonthlyLTA = lta,
                MonthlySpecialAllowance = special,
                MonthlyConveyance = conv,
                MonthlyMedical = med,
                MonthlyOtherAllowance = other,
                MonthlyGrossSalary = gross,
                MonthlyPFEmployee = pfEmp,
                MonthlyESIEmployee = esiEmp,
                MonthlyPT = pt,
                MonthlyTDS = tds,
                MonthlyNetSalary = net,
                MonthlyPFEmployer = pfEmployer,
                MonthlyESIEmployer = esiEmployer,
                EffectiveFrom = DateTime.Today,
                IsCurrent = true,
                Remarks = remarks ?? "Salary assignment updated",
                CreatedAt = DateTime.UtcNow
            };

            _context.EmployeeSalaryAssignments.Add(newAssignment);
            await _context.SaveChangesAsync();

            // Also keep legacy SalaryStructure in sync for backward compatibility
            var legacy = await _context.SalaryStructures.FirstOrDefaultAsync(s => s.UserId == userId);
            if (legacy == null)
            {
                _context.SalaryStructures.Add(new SalaryStructure
                {
                    UserId = userId,
                    BasicSalary = basic,
                    HRA = hra,
                    TransportAllowance = conv,
                    MedicalAllowance = med,
                    ProvidentFund = pfEmp,
                    ProfessionalTax = pt,
                    NetSalary = net,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                legacy.BasicSalary = basic;
                legacy.HRA = hra;
                legacy.TransportAllowance = conv;
                legacy.MedicalAllowance = med;
                legacy.ProvidentFund = pfEmp;
                legacy.ProfessionalTax = pt;
                legacy.NetSalary = net;
                legacy.UpdatedAt = DateTime.UtcNow;
                _context.SalaryStructures.Update(legacy);
            }
            await _context.SaveChangesAsync();

            _context.ActivityLogs.Add(new ActivityLog
            {
                Title = "Employee CTC Configured",
                Description = $"Assigned CTC of ₹{annualCtc:N0} to {user.FullName}.",
                IconClass = "fa-money-check-dollar",
                ColorClass = "text-success",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            // Broadcast real-time SignalR notification
            try
            {
                await _hubContext.Clients.Groups("HR", "Super Admin", "Admin", "Finance Manager")
                    .SendAsync("ReceiveEmployeeSalaryAssigned", new { employeeId = userId, employeeName = user.FullName, ctc = annualCtc, timestamp = DateTime.UtcNow });
                await _hubContext.Clients.User(userId.ToString())
                    .SendAsync("ReceivePersonalSalaryUpdated", new { ctc = annualCtc, basic = basic, net = net });
            }
            catch { }

            TempData["SuccessMessage"] = $"Salary & CTC successfully assigned to '{user.FullName}'.";
            return RedirectToAction(nameof(SalaryStructures));
        }

        // ==========================================
        // 2. ALLOWANCES & DEDUCTIONS MASTER
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> AllowancesDeductions()
        {
            if (!IsHrOrAdminOrFinance()) return Forbid();

            // Fetch from PayrollComponents
            var components = await _context.PayrollComponents
                .OrderBy(c => c.Type)
                .ThenBy(c => c.ComponentName)
                .ToListAsync();

            // If empty, fallback or seed default standard components
            if (!components.Any())
            {
                var defaults = new List<PayrollComponent>
                {
                    new PayrollComponent { ComponentName = "House Rent Allowance", Code = "HRA", Type = "Allowance", Taxability = "Partially Exempt", CalculationBasis = "Percentage of Basic", DefaultValueOrRate = 40.00m, MaxCapLimit = "No Limit", PayFrequency = "Monthly", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new PayrollComponent { ComponentName = "Conveyance Allowance", Code = "CONV", Type = "Allowance", Taxability = "Tax Exempt", CalculationBasis = "Fixed Amount", DefaultValueOrRate = 1600.00m, MaxCapLimit = "No Limit", PayFrequency = "Monthly", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new PayrollComponent { ComponentName = "Medical Allowance", Code = "MED", Type = "Allowance", Taxability = "Tax Exempt", CalculationBasis = "Fixed Amount", DefaultValueOrRate = 1250.00m, MaxCapLimit = "No Limit", PayFrequency = "Monthly", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new PayrollComponent { ComponentName = "Special Allowance", Code = "SPEC", Type = "Allowance", Taxability = "Fully Taxable", CalculationBasis = "Fixed Amount", DefaultValueOrRate = 0.00m, MaxCapLimit = "No Limit", PayFrequency = "Monthly", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new PayrollComponent { ComponentName = "Provident Fund", Code = "PF", Type = "Deduction", Taxability = "Fully Deductible", CalculationBasis = "Percentage of Basic", DefaultValueOrRate = 12.00m, MaxCapLimit = "1800", PayFrequency = "Monthly", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new PayrollComponent { ComponentName = "Employee State Insurance", Code = "ESI", Type = "Deduction", Taxability = "Fully Deductible", CalculationBasis = "Percentage of Gross", DefaultValueOrRate = 0.75m, MaxCapLimit = "No Limit", PayFrequency = "Monthly", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new PayrollComponent { ComponentName = "Professional Tax", Code = "PT", Type = "Deduction", Taxability = "Fully Deductible", CalculationBasis = "Fixed Amount", DefaultValueOrRate = 200.00m, MaxCapLimit = "200", PayFrequency = "Monthly", IsActive = true, CreatedAt = DateTime.UtcNow }
                };
                _context.PayrollComponents.AddRange(defaults);
                await _context.SaveChangesAsync();
                components = defaults;
            }

            ViewBag.Allowances = components.Where(c => c.Type == "Allowance").ToList();
            ViewBag.Deductions = components.Where(c => c.Type == "Deduction").ToList();
            ViewBag.TotalComponents = components.Count;
            ViewBag.ActiveAllowancesCount = components.Count(c => c.Type == "Allowance" && c.IsActive);
            ViewBag.ActiveDeductionsCount = components.Count(c => c.Type == "Deduction" && c.IsActive);
            ViewBag.TotalEarningsCount = ViewBag.ActiveAllowancesCount;
            ViewBag.TotalDeductionsCount = ViewBag.ActiveDeductionsCount;

            return View(components);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateComponent(PayrollComponent model)
        {
            if (!IsHrOrAdminOrFinance()) return Forbid();

            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                {
                    model.CreatedAt = DateTime.UtcNow;
                    _context.PayrollComponents.Add(model);
                    await _context.SaveChangesAsync();

                    _context.ActivityLogs.Add(new ActivityLog
                    {
                        Title = "Payroll Component Created",
                        Description = $"Created {model.Type} '{model.ComponentName}' ({model.Code}).",
                        IconClass = model.Type == "Allowance" ? "fa-arrow-trend-up" : "fa-arrow-trend-down",
                        ColorClass = model.Type == "Allowance" ? "text-success" : "text-danger",
                        CreatedAt = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync();

                    try
                    {
                        await _hubContext.Clients.Groups("HR", "Super Admin", "Admin", "Finance Manager")
                            .SendAsync("ReceivePayrollComponentUpdated", new { id = model.Id, name = model.ComponentName, type = model.Type, action = "Created", timestamp = DateTime.UtcNow });
                    }
                    catch { }

                    TempData["SuccessMessage"] = $"{model.Type} '{model.ComponentName}' created successfully.";
                }
                else
                {
                    var existing = await _context.PayrollComponents.FindAsync(model.Id);
                    if (existing == null) return NotFound();

                    existing.ComponentName = model.ComponentName;
                    existing.Code = model.Code;
                    existing.Type = model.Type;
                    existing.Taxability = model.Taxability;
                    existing.CalculationBasis = model.CalculationBasis;
                    existing.DefaultValueOrRate = model.DefaultValueOrRate;
                    existing.MaxCapLimit = model.MaxCapLimit ?? "No Limit";
                    existing.PayFrequency = model.PayFrequency;
                    existing.IsActive = model.IsActive;
                    existing.UpdatedAt = DateTime.UtcNow;

                    _context.PayrollComponents.Update(existing);
                    await _context.SaveChangesAsync();

                    try
                    {
                        await _hubContext.Clients.Groups("HR", "Super Admin", "Admin", "Finance Manager")
                            .SendAsync("ReceivePayrollComponentUpdated", new { id = existing.Id, name = existing.ComponentName, type = existing.Type, action = "Updated", timestamp = DateTime.UtcNow });
                    }
                    catch { }

                    TempData["SuccessMessage"] = $"{existing.Type} '{existing.ComponentName}' updated successfully.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Validation failed. Please verify the component details.";
            }

            return RedirectToAction(nameof(AllowancesDeductions));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveComponent(PayrollComponent model)
        {
            return await CreateComponent(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleComponentStatus(int id)
        {
            if (!IsHrOrAdminOrFinance()) return Forbid();

            var item = await _context.PayrollComponents.FindAsync(id);
            if (item == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return Json(new { success = false, message = "Component not found." });
                }
                return NotFound();
            }

            item.IsActive = !item.IsActive;
            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            try
            {
                await _hubContext.Clients.Groups("HR", "Super Admin", "Admin", "Finance Manager")
                    .SendAsync("ReceivePayrollComponentUpdated", new { id = item.Id, name = item.ComponentName, type = item.Type, action = item.IsActive ? "Activated" : "Deactivated", timestamp = DateTime.UtcNow });
            }
            catch { }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers["Accept"].ToString().Contains("application/json"))
            {
                return Json(new { success = true, isActive = item.IsActive, message = $"Status of '{item.ComponentName}' set to {(item.IsActive ? "Active" : "Inactive")}." });
            }

            TempData["SuccessMessage"] = $"Status of '{item.ComponentName}' updated to {(item.IsActive ? "Active" : "Inactive")}.";
            return RedirectToAction(nameof(AllowancesDeductions));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComponent(int id)
        {
            if (!IsHrOrAdminOrFinance()) return Forbid();

            var item = await _context.PayrollComponents.FindAsync(id);
            if (item == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return Json(new { success = false, message = "Component not found." });
                }
                return NotFound();
            }

            var compName = item.ComponentName;
            var compType = item.Type;

            _context.PayrollComponents.Remove(item);
            await _context.SaveChangesAsync();

            _context.ActivityLogs.Add(new ActivityLog
            {
                Title = "Payroll Component Removed",
                Description = $"Deleted {compType} '{compName}'.",
                IconClass = "fa-trash-can",
                ColorClass = "text-danger",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            try
            {
                await _hubContext.Clients.Groups("HR", "Super Admin", "Admin", "Finance Manager")
                    .SendAsync("ReceivePayrollComponentUpdated", new { id = id, name = compName, type = compType, action = "Deleted", timestamp = DateTime.UtcNow });
            }
            catch { }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers["Accept"].ToString().Contains("application/json"))
            {
                return Json(new { success = true, message = $"Component '{compName}' removed successfully." });
            }

            TempData["SuccessMessage"] = $"Component '{compName}' deleted successfully.";
            return RedirectToAction(nameof(AllowancesDeductions));
        }

        // ==========================================
        // 3. STATUTORY COMPLIANCE (PF / ESI / TDS)
        // ==========================================
        [HttpGet]
        [Authorize(Roles = "HR,Super Admin,Admin,Finance Manager,Auditor,Accountant")]
        public async Task<IActionResult> StatutoryCompliance()
        {
            if (!IsHrOrAdminOrFinance() && !User.IsInRole("Auditor")) return Forbid();

            // Fetch all active company bank accounts created by Super Admin
            ViewBag.BankAccounts = await _context.CompanyBankAccounts
                .Where(b => b.IsActive)
                .OrderByDescending(b => b.IsPrimaryAccount)
                .ThenBy(b => b.BankName)
                .ToListAsync();

            var filings = await _context.StatutoryReturnFilings
                .Include(f => f.CompanyBankAccount)
                .OrderByDescending(f => f.FilingDate)
                .ThenByDescending(f => f.Id)
                .ToListAsync();

            var rules = await _context.StatutoryRuleConfigs.ToListAsync();
            var pfRule = rules.FirstOrDefault(r => r.RuleKey == "PF") ?? new StatutoryRuleConfig { RuleKey = "PF", EmployerRate = 12m, EmployeeRate = 12m, WageCeiling = 15000m };
            var esiRule = rules.FirstOrDefault(r => r.RuleKey == "ESI") ?? new StatutoryRuleConfig { RuleKey = "ESI", EmployerRate = 3.25m, EmployeeRate = 0.75m, WageCeiling = 21000m };
            var tdsRule = rules.FirstOrDefault(r => r.RuleKey == "TDS") ?? new StatutoryRuleConfig { RuleKey = "TDS", StandardDeduction = 75000m, ActiveRegime = "New Tax Regime" };

            ViewBag.PfRule = pfRule;
            ViewBag.EsiRule = esiRule;
            ViewBag.TdsRule = tdsRule;

            // Legacy model bags for fallback compatibility
            ViewBag.PFConfig = new StatutoryConfiguration { RuleType = "PF", EmployerRate = pfRule.EmployerRate, EmployeeRate = pfRule.EmployeeRate, WageCeilingLimit = pfRule.WageCeiling };
            ViewBag.ESIConfig = new StatutoryConfiguration { RuleType = "ESI", EmployerRate = esiRule.EmployerRate, EmployeeRate = esiRule.EmployeeRate, WageCeilingLimit = esiRule.WageCeiling };
            ViewBag.TDSConfig = new StatutoryConfiguration { RuleType = "TDS", StandardDeductionAnnual = tdsRule.StandardDeduction, DefaultTaxRegime = tdsRule.ActiveRegime };

            return View(filings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HR,Super Admin,Admin,Finance Manager,Accountant")]
        public async Task<IActionResult> LogFilingReceipt(
            string complianceReturn, 
            string frequency, 
            string filingPeriod, 
            DateTime dueDate, 
            string receiptOrChallanNumber, 
            decimal challanAmountPaid, 
            int? companyBankAccountId)
        {
            if (!IsHrOrAdminOrFinance()) return Forbid();

            int currentUserId = GetCurrentUserId();

            var filing = new StatutoryReturnFiling
            {
                ComplianceReturn = complianceReturn,
                Frequency = string.IsNullOrEmpty(frequency) ? "Monthly" : frequency,
                FilingPeriod = filingPeriod,
                DueDate = dueDate,
                FilingDate = DateTime.Today,
                ReceiptOrChallanNumber = receiptOrChallanNumber,
                ChallanAmountPaid = challanAmountPaid,
                CompanyBankAccountId = companyBankAccountId,
                Status = "Filed",
                LoggedByUserId = currentUserId.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            _context.StatutoryReturnFilings.Add(filing);

            // If a company bank account was selected, debit the bank account balance and create ledger expense
            if (companyBankAccountId.HasValue && companyBankAccountId.Value > 0 && challanAmountPaid > 0)
            {
                var bankAccount = await _context.CompanyBankAccounts.FindAsync(companyBankAccountId.Value);
                if (bankAccount != null)
                {
                    bankAccount.CurrentBalance -= challanAmountPaid;

                    // Record transaction in general ledger/bank statements
                    var txn = new BankTransaction
                    {
                        CompanyBankAccountId = bankAccount.Id,
                        TransactionDate = DateTime.Today,
                        Description = $"Statutory Challan: {complianceReturn} ({filingPeriod}) - Ref #{receiptOrChallanNumber}",
                        DebitAmount = challanAmountPaid,
                        CreditAmount = 0,
                        BalanceAfter = bankAccount.CurrentBalance,
                        Category = "Tax & Statutory Dues",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.BankTransactions.Add(txn);
                }
            }

            _context.ActivityLogs.Add(new ActivityLog
            {
                Title = "Statutory Return Filed",
                Description = $"Filed {complianceReturn} for {filingPeriod} (Challan: {receiptOrChallanNumber}, Amount: ₹{challanAmountPaid:N2}).",
                IconClass = "fa-file-invoice",
                ColorClass = "text-primary",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            try
            {
                await _hubContext.Clients.Groups("HR", "Super Admin", "Admin", "Finance Manager", "Auditor")
                    .SendAsync("ReceiveStatutoryReturnFiled", new { returnName = complianceReturn, period = filingPeriod, challan = receiptOrChallanNumber, amount = challanAmountPaid, timestamp = DateTime.UtcNow });
            }
            catch { }

            TempData["SuccessMessage"] = $"Challan #{receiptOrChallanNumber} recorded and debited from selected company account.";
            return RedirectToAction(nameof(StatutoryCompliance));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Super Admin,Admin,HR,Finance Manager")]
        public async Task<IActionResult> UpdateRuleConfig(string ruleKey, decimal employerRate, decimal employeeRate, decimal wageCeiling, decimal standardDeduction, string? activeRegime)
        {
            if (!IsHrOrAdminOrFinance()) return Forbid();

            var rule = await _context.StatutoryRuleConfigs.FirstOrDefaultAsync(r => r.RuleKey == ruleKey);
            if (rule == null)
            {
                rule = new StatutoryRuleConfig { RuleKey = ruleKey };
                _context.StatutoryRuleConfigs.Add(rule);
            }

            rule.EmployerRate = employerRate;
            rule.EmployeeRate = employeeRate;
            rule.WageCeiling = wageCeiling;
            rule.StandardDeduction = standardDeduction;
            rule.ActiveRegime = activeRegime ?? "New Tax Regime";
            rule.UpdatedAt = DateTime.UtcNow;

            // Sync with StatutoryConfigurations table as well
            var statConfig = await _context.StatutoryConfigurations.FirstOrDefaultAsync(c => c.RuleType == ruleKey);
            if (statConfig == null)
            {
                statConfig = new StatutoryConfiguration { RuleType = ruleKey };
                _context.StatutoryConfigurations.Add(statConfig);
            }
            statConfig.EmployeeRate = employeeRate;
            statConfig.EmployerRate = employerRate;
            statConfig.WageCeilingLimit = wageCeiling;
            statConfig.StandardDeductionAnnual = standardDeduction;
            if (!string.IsNullOrEmpty(activeRegime)) statConfig.DefaultTaxRegime = activeRegime;
            statConfig.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            try
            {
                await _hubContext.Clients.Groups("HR", "Super Admin", "Admin", "Finance Manager", "Auditor")
                    .SendAsync("ReceiveStatutoryRuleUpdated", new { ruleKey = ruleKey, timestamp = DateTime.UtcNow });
            }
            catch { }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers["Accept"].ToString().Contains("application/json"))
            {
                return Json(new { success = true, message = $"{ruleKey} Statutory rules updated successfully." });
            }

            TempData["SuccessMessage"] = $"{ruleKey} Statutory rules updated successfully.";
            return RedirectToAction(nameof(StatutoryCompliance));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatutoryConfig(string ruleType, decimal employeeRate, decimal employerRate, decimal wageCeilingLimit, decimal standardDeductionAnnual, string? defaultTaxRegime)
        {
            return await UpdateRuleConfig(ruleType, employerRate, employeeRate, wageCeilingLimit, standardDeductionAnnual, defaultTaxRegime);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFilingRecord(StatutoryFilingLog model)
        {
            return await LogFilingReceipt(model.ComplianceAct, model.Frequency, model.Period, model.DueDate, model.ReceiptNumber, 0, null);
        }

        // ==========================================
        // 4. PAYROLL PROCESSING ENGINE
        // ==========================================
        [HttpGet]
        [Authorize(Roles = "HR,Super Admin,Admin,Finance Manager,Auditor,Accountant")]
        public async Task<IActionResult> PayrollProcessing()
        {
            if (!IsHrOrAdminOrFinance() && !User.IsInRole("Auditor")) return Forbid();

            var runs = await _context.PayrollRuns
                .Include(r => r.CompanyBankAccount)
                .OrderByDescending(r => r.Year)
                .ThenByDescending(r => r.Month)
                .ToListAsync();

            var activeEmployees = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.IsActive && u.Role != null && u.Role.RoleName != "Admin" && u.Role.RoleName != "Super Admin")
                .ToListAsync();

            var companyBanks = await _context.CompanyBankAccounts
                .Where(b => b.IsActive)
                .OrderByDescending(b => b.IsPrimaryAccount)
                .ThenBy(b => b.BankName)
                .ToListAsync();

            ViewBag.PayrollRuns = runs;
            ViewBag.EligibleEmployees = activeEmployees.Count;
            ViewBag.ActiveEmployeesCount = activeEmployees.Count;
            ViewBag.TotalRuns = runs.Count;
            ViewBag.PendingApproval = runs.Count(r => r.Status == "Draft" || r.Status == "Calculated" || r.Status == "Under Review" || r.Status == "Pending Approval");
            ViewBag.DraftRunsCount = ViewBag.PendingApproval;
            ViewBag.TotalNetPaid = runs.Where(r => r.Status == "Paid & Closed" || r.Status == "Paid").Sum(r => r.TotalNetSalary);
            ViewBag.CompanyBanks = companyBanks;
            ViewBag.BankAccounts = companyBanks;

            return View(runs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HR,Super Admin,Admin")]
        public async Task<IActionResult> RunPayrollCalculation(int month, int year, int? departmentId, string? department, string? payPeriod)
        {
            if (!IsHrOrAdminOrFinance()) return Forbid();

            string monthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
            string calculatedPayPeriod = !string.IsNullOrEmpty(payPeriod) ? payPeriod : $"{monthName} {year}";
            string targetDept = !string.IsNullOrEmpty(department) ? department : "All Departments";

            var existingRun = await _context.PayrollRuns
                .FirstOrDefaultAsync(r => r.Month == month && r.Year == year && (targetDept == "All Departments" || r.Department == targetDept));

            if (existingRun != null && (existingRun.Status == "Paid & Closed" || existingRun.Status == "Paid"))
            {
                TempData["ErrorMessage"] = $"Payroll for {calculatedPayPeriod} is already finalized and paid.";
                return RedirectToAction(nameof(PayrollProcessing));
            }

            var activeEmployees = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Department)
                .Where(u => u.IsActive && u.Role != null && u.Role.RoleName != "Admin" && u.Role.RoleName != "Super Admin")
                .ToListAsync();

            if (departmentId.HasValue && departmentId.Value > 0)
            {
                activeEmployees = activeEmployees.Where(u => u.DepartmentId == departmentId.Value).ToList();
            }
            else if (!string.IsNullOrEmpty(targetDept) && targetDept != "All Departments")
            {
                activeEmployees = activeEmployees.Where(u => u.Department != null && u.Department.DepartmentName == targetDept).ToList();
            }

            if (!activeEmployees.Any())
            {
                TempData["ErrorMessage"] = "No active employees found for payroll execution.";
                return RedirectToAction(nameof(PayrollProcessing));
            }

            var assignments = await _context.EmployeeSalaryAssignments
                .Include(a => a.Structure)
                .Where(a => a.IsCurrent || a.Status == "Current")
                .ToDictionaryAsync(a => a.UserId);

            int currentUserId = GetCurrentUserId();

            var run = existingRun ?? new PayrollRun
            {
                PayPeriod = calculatedPayPeriod,
                Month = month,
                Year = year,
                Department = targetDept,
                Status = "Draft",
                ProcessedByUserId = currentUserId,
                ProcessedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            if (existingRun == null)
            {
                _context.PayrollRuns.Add(run);
                await _context.SaveChangesAsync();
            }
            else
            {
                run.ProcessedByUserId = currentUserId;
                run.ProcessedAt = DateTime.UtcNow;
                run.Status = "Calculated";
                run.UpdatedAt = DateTime.UtcNow;

                var oldSlips = await _context.SalarySlips.Where(s => s.PayrollRunId == run.PayrollRunId).ToListAsync();
                if (oldSlips.Any()) _context.SalarySlips.RemoveRange(oldSlips);
                await _context.SaveChangesAsync();
            }

            decimal runGross = 0;
            decimal runDeductions = 0;
            decimal runNet = 0;
            decimal runEmployerPf = 0;
            decimal runEmployerEsi = 0;
            decimal runCtc = 0;
            int totalWorkingDays = DateTime.DaysInMonth(year, month);

            foreach (var emp in activeEmployees)
            {
                assignments.TryGetValue(emp.UserId, out var sal);

                decimal basic = sal != null ? sal.MonthlyBasic : 25000m;
                decimal hra = sal != null ? sal.MonthlyHRA : Math.Round(basic * 0.40m, 2);
                decimal conv = sal != null ? sal.MonthlyConveyance : 1600m;
                decimal med = sal != null ? sal.MonthlyMedical : 1250m;
                decimal lta = sal != null ? sal.MonthlyLTA : 1250m;
                decimal special = sal != null ? sal.MonthlySpecialAllowance : 10900m;
                decimal other = sal != null ? sal.MonthlyOtherAllowance : 0m;
                decimal gross = sal != null ? sal.MonthlyGrossSalary : (basic + hra + conv + med + lta + special + other);

                // Attendance & Overtime
                var attLogs = await _context.HRAttendanceLogs
                    .Where(l => l.UserId == emp.UserId && l.Date.Month == month && l.Date.Year == year)
                    .ToListAsync();
                int presentDays = attLogs.Count(l => l.Status.Contains("Present"));
                int absentDays = attLogs.Count(l => l.Status.Contains("Absent"));
                int unpaidLeaveDays = absentDays;
                int paidDays = Math.Max(0, totalWorkingDays - unpaidLeaveDays);
                decimal dailyRate = basic / (decimal)totalWorkingDays;
                decimal lopDeduction = Math.Round(dailyRate * unpaidLeaveDays, 2);

                var otRecords = await _context.HROvertimeRecords
                    .Where(o => o.UserId == emp.UserId && o.PayoutStatus == "Approved for Payroll")
                    .ToListAsync();
                decimal overtimePay = otRecords.Sum(o => o.TotalOvertimePay);
                int overtimeHours = otRecords.Sum(o => o.OvertimeHours);

                var bonuses = await _context.BonusIncentives
                    .Where(b => b.UserId == emp.UserId && b.PayoutMonth == calculatedPayPeriod && b.Status == "Approved")
                    .ToListAsync();
                decimal bonusAmount = bonuses.Sum(b => b.Amount);

                decimal finalGross = Math.Max(0, Math.Round((gross + overtimePay + bonusAmount) - lopDeduction, 2));

                decimal pfEmp = sal != null ? sal.MonthlyPFEmployee : Math.Round(Math.Min(basic, 15000m) * 0.12m, 2);
                decimal esiEmp = sal != null ? sal.MonthlyESIEmployee : (finalGross <= 21000m ? Math.Round(finalGross * 0.0075m, 2) : 0m);
                decimal pt = sal != null ? sal.MonthlyPT : (finalGross > 15000m ? 200m : 0m);
                decimal tds = sal != null ? sal.MonthlyTDS : Math.Round(finalGross * 0.05m, 2);
                decimal totalDed = pfEmp + esiEmp + pt + tds + lopDeduction;
                decimal net = Math.Max(0, finalGross - (pfEmp + esiEmp + pt + tds));

                decimal pfEmployer = sal != null ? sal.MonthlyPFEmployer : Math.Round(Math.Min(basic, 15000m) * 0.12m, 2);
                decimal esiEmployer = sal != null ? sal.MonthlyESIEmployer : (finalGross <= 21000m ? Math.Round(finalGross * 0.0325m, 2) : 0m);

                // 1. Add SalarySlip
                var slip = new SalarySlip
                {
                    PayrollRunId = run.PayrollRunId,
                    UserId = emp.UserId,
                    Month = month,
                    Year = year,
                    MonthName = monthName,
                    WorkingDays = totalWorkingDays,
                    PaidDays = paidDays,
                    BasicSalary = basic,
                    Hra = hra,
                    Allowances = Math.Max(0, finalGross - (basic + hra)),
                    GrossSalary = finalGross,
                    PfDeduction = pfEmp,
                    PtDeduction = pt,
                    TdsDeduction = tds,
                    TotalDeductions = totalDed,
                    NetSalary = net,
                    Status = "Draft",
                    CreatedAt = DateTime.UtcNow
                };
                _context.SalarySlips.Add(slip);

                // 2. Synchronize Payslip
                var payslip = await _context.Payslips
                    .FirstOrDefaultAsync(p => p.UserId == emp.UserId && p.PayPeriod == calculatedPayPeriod);
                if (payslip == null)
                {
                    payslip = new Payslip
                    {
                        PayrollRunId = run.PayrollRunId,
                        UserId = emp.UserId,
                        PayPeriod = calculatedPayPeriod,
                        PayslipNumber = $"PAY-{year}{month:D2}-{emp.UserId:D4}",
                        BasicSalary = basic,
                        HRA = hra,
                        TransportAllowance = conv,
                        MedicalAllowance = med,
                        LTA = lta,
                        SpecialAllowance = special,
                        OtherAllowance = other,
                        BonusIncentives = bonusAmount,
                        OvertimePay = overtimePay,
                        GrossSalary = finalGross,
                        ProvidentFund = pfEmp,
                        ESI = esiEmp,
                        ProfessionalTax = pt,
                        TDS = tds,
                        LOPDeduction = lopDeduction,
                        TotalDeductions = totalDed,
                        NetSalary = net,
                        EmployerPF = pfEmployer,
                        EmployerESI = esiEmployer,
                        TotalCTC = (finalGross + pfEmployer + esiEmployer),
                        TotalWorkingDays = totalWorkingDays,
                        PresentDays = presentDays,
                        AbsentDays = absentDays,
                        UnpaidLeaveDays = unpaidLeaveDays,
                        PaidDays = paidDays,
                        OvertimeHours = overtimeHours,
                        PaymentDate = DateTime.UtcNow,
                        Status = "Draft"
                    };
                    _context.Payslips.Add(payslip);
                }
                else
                {
                    payslip.PayrollRunId = run.PayrollRunId;
                    payslip.BasicSalary = basic;
                    payslip.HRA = hra;
                    payslip.GrossSalary = finalGross;
                    payslip.ProvidentFund = pfEmp;
                    payslip.ESI = esiEmp;
                    payslip.ProfessionalTax = pt;
                    payslip.TDS = tds;
                    payslip.LOPDeduction = lopDeduction;
                    payslip.TotalDeductions = totalDed;
                    payslip.NetSalary = net;
                    payslip.TotalCTC = (finalGross + pfEmployer + esiEmployer);
                    payslip.PaidDays = paidDays;
                    payslip.Status = "Draft";
                    _context.Payslips.Update(payslip);
                }

                runGross += finalGross;
                runDeductions += totalDed;
                runNet += net;
                runEmployerPf += pfEmployer;
                runEmployerEsi += esiEmployer;
                runCtc += (finalGross + pfEmployer + esiEmployer);
            }

            run.TotalEmployees = activeEmployees.Count;
            run.TotalGrossSalary = runGross;
            run.TotalDeductions = runDeductions;
            run.TotalNetSalary = runNet;
            run.TotalEmployerPF = runEmployerPf;
            run.TotalEmployerESI = runEmployerEsi;
            run.TotalCTC = runCtc;
            run.Status = "Calculated";
            run.UpdatedAt = DateTime.UtcNow;

            _context.PayrollRuns.Update(run);
            await _context.SaveChangesAsync();

            try
            {
                await _hubContext.Clients.Groups("HR", "Super Admin", "Admin", "Finance Manager", "Auditor")
                    .SendAsync("ReceivePayrollRunUpdated", new { payPeriod = calculatedPayPeriod, status = run.Status, timestamp = DateTime.UtcNow });
            }
            catch { }

            TempData["SuccessMessage"] = $"Payroll calculation generated for {calculatedPayPeriod} across {activeEmployees.Count} staff. Total Net Payout: ₹{runNet:N2}.";
            return RedirectToAction(nameof(PayrollProcessing));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RunPayrollProcessing(string payPeriod, int month, int year, string? department)
        {
            return await RunPayrollCalculation(month, year, null, department, payPeriod);
        }

        // AJAX Endpoint to populate breakdown modal with zero NaN values
        [HttpGet]
        public async Task<IActionResult> GetPayrollBreakdown(int runId)
        {
            var run = await _context.PayrollRuns
                .Include(r => r.SalarySlips)
                    .ThenInclude(s => s.User)
                        .ThenInclude(u => u!.Department)
                .Include(r => r.Payslips)
                    .ThenInclude(p => p.User)
                        .ThenInclude(u => u!.Department)
                .FirstOrDefaultAsync(r => r.PayrollRunId == runId);

            if (run == null) return NotFound();

            var slipList = new List<object>();

            if (run.SalarySlips != null && run.SalarySlips.Any())
            {
                foreach (var s in run.SalarySlips.OrderBy(s => s.User != null ? s.User.FullName : ""))
                {
                    slipList.Add(new
                    {
                        employeeName = s.User?.FullName ?? "Staff",
                        employeeCode = s.User?.UserCode ?? s.User?.UserName ?? "EMP",
                        department = s.User?.Department?.DepartmentName ?? "General",
                        basic = s.BasicSalary.ToString("N2"),
                        hra = s.Hra.ToString("N2"),
                        gross = s.GrossSalary.ToString("N2"),
                        deductions = s.TotalDeductions.ToString("N2"),
                        net = s.NetSalary.ToString("N2"),
                        status = s.Status
                    });
                }
            }
            else if (run.Payslips != null && run.Payslips.Any())
            {
                foreach (var p in run.Payslips.OrderBy(p => p.User != null ? p.User.FullName : ""))
                {
                    slipList.Add(new
                    {
                        employeeName = p.User?.FullName ?? "Staff",
                        employeeCode = p.User?.UserCode ?? p.User?.UserName ?? "EMP",
                        department = p.User?.Department?.DepartmentName ?? "General",
                        basic = (p.BasicSalary ?? 0).ToString("N2"),
                        hra = (p.HRA ?? 0).ToString("N2"),
                        gross = (p.GrossSalary ?? 0).ToString("N2"),
                        deductions = (p.TotalDeductions ?? 0).ToString("N2"),
                        net = (p.NetSalary ?? 0).ToString("N2"),
                        status = p.Status ?? "Draft"
                    });
                }
            }

            return Json(new
            {
                payrollRunId = run.PayrollRunId,
                id = run.PayrollRunId,
                payPeriod = run.PayPeriod,
                department = run.Department,
                totalEmployees = run.TotalEmployees,
                grossSalary = run.TotalGrossSalary.ToString("N2"),
                netPayout = run.TotalNetSalary.ToString("N2"),
                totalGrossSalary = run.TotalGrossSalary.ToString("N2"),
                totalNetSalary = run.TotalNetSalary.ToString("N2"),
                totalDeductions = run.TotalDeductions.ToString("N2"),
                status = run.Status,
                slips = slipList,
                payslips = slipList
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetPayrollRunDetails(int id)
        {
            return await GetPayrollBreakdown(id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Finance Manager,Super Admin,Admin")]
        public async Task<IActionResult> DisbursePayroll(int runId, int companyBankAccountId)
        {
            if (!IsHrOrAdminOrFinance()) return Forbid();

            var run = await _context.PayrollRuns
                .Include(r => r.SalarySlips)
                .Include(r => r.Payslips)
                .FirstOrDefaultAsync(r => r.PayrollRunId == runId);

            if (run == null) return NotFound();

            if (run.Status == "Paid & Closed" || run.Status == "Paid")
            {
                TempData["ErrorMessage"] = $"Payroll run '{run.PayPeriod}' is already disbursed and closed.";
                return RedirectToAction(nameof(PayrollProcessing));
            }

            var bank = await _context.CompanyBankAccounts.FindAsync(companyBankAccountId);
            if (bank == null)
            {
                TempData["ErrorMessage"] = "Selected company bank account not found.";
                return RedirectToAction(nameof(PayrollProcessing));
            }

            if (bank.CurrentBalance < run.TotalNetSalary)
            {
                TempData["ErrorMessage"] = $"Selected bank account '{bank.BankName}' has insufficient balance (Available: ₹{bank.CurrentBalance:N2}, Required: ₹{run.TotalNetSalary:N2}).";
                return RedirectToAction(nameof(PayrollProcessing));
            }

            int currentUserId = GetCurrentUserId();

            // 1. Debit Bank Account
            bank.CurrentBalance -= run.TotalNetSalary;

            // 2. Record General Ledger Bank Transaction
            var txn = new BankTransaction
            {
                CompanyBankAccountId = bank.Id,
                TransactionDate = DateTime.Today,
                Description = $"Bulk Salary Payout - {run.PayPeriod} ({run.TotalEmployees} Employees)",
                DebitAmount = run.TotalNetSalary,
                CreditAmount = 0,
                BalanceAfter = bank.CurrentBalance,
                Category = "Payroll Disbursement",
                CreatedAt = DateTime.UtcNow
            };
            _context.BankTransactions.Add(txn);

            // 3. Mark Run as Paid & Closed
            run.Status = "Paid & Closed";
            run.CompanyBankAccountId = bank.Id;
            run.PaidByUserId = currentUserId;
            run.PaidAt = DateTime.UtcNow;
            run.DisbursedAt = DateTime.UtcNow;
            run.UpdatedAt = DateTime.UtcNow;

            // 4. Auto-publish all employee salary slips & payslips to Paid
            if (run.SalarySlips != null)
            {
                foreach (var slip in run.SalarySlips)
                {
                    slip.Status = "Paid";
                }
            }

            var slips = await _context.Payslips.Where(p => p.PayrollRunId == run.PayrollRunId || p.PayPeriod == run.PayPeriod).ToListAsync();
            foreach (var p in slips)
            {
                p.Status = "Paid";
                p.PaymentDate = DateTime.UtcNow;
                _context.Payslips.Update(p);
            }

            // 5. Update linked bonus incentives
            var bonuses = await _context.BonusIncentives.Where(b => b.PayrollRunId == run.PayrollRunId || b.PayoutMonth == run.PayPeriod).ToListAsync();
            foreach (var b in bonuses)
            {
                b.Status = "Paid";
                _context.BonusIncentives.Update(b);
            }

            // 6. Log activity
            _context.ActivityLogs.Add(new ActivityLog
            {
                Title = "Payroll Disbursed & Published",
                Description = $"Disbursed ₹{run.TotalNetSalary:N2} via {bank.BankName} for {run.PayPeriod}. Payslips published to Employee ESS Portals.",
                IconClass = "fa-money-bill-wave",
                ColorClass = "text-success",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            // 7. SignalR Live Broadcast
            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveSalaryDisbursed", new { period = run.PayPeriod, amount = run.TotalNetSalary, timestamp = DateTime.UtcNow });
                await _hubContext.Clients.Groups("HR", "Super Admin", "Admin", "Finance Manager", "Auditor")
                    .SendAsync("ReceivePayrollRunUpdated", new { payPeriod = run.PayPeriod, status = run.Status, timestamp = DateTime.UtcNow });
            }
            catch { }

            TempData["SuccessMessage"] = $"Payroll for {run.PayPeriod} successfully disbursed via {bank.BankName}. Payslips released to Employee ESS Portals immediately.";
            return RedirectToAction(nameof(PayrollProcessing));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePayrollRunStatus(int runId, string newStatus, string? remarks)
        {
            if (!IsHrOrAdminOrFinance()) return Forbid();

            var run = await _context.PayrollRuns.FindAsync(runId);
            if (run == null) return NotFound();

            int currentUserId = GetCurrentUserId();

            if (newStatus == "Under Review")
            {
                run.Status = "Under Review";
            }
            else if (newStatus == "Approved")
            {
                run.Status = "Approved";
                run.ApprovedByUserId = currentUserId;
                run.ApprovedAt = DateTime.UtcNow;
            }
            else if (newStatus == "Locked")
            {
                run.Status = "Locked";
            }
            else if (newStatus == "Paid" || newStatus == "Paid & Closed")
            {
                run.Status = "Paid & Closed";
                run.PaidByUserId = currentUserId;
                run.PaidAt = DateTime.UtcNow;
                run.DisbursedAt = DateTime.UtcNow;

                var slips = await _context.Payslips.Where(p => p.PayrollRunId == runId).ToListAsync();
                foreach (var slip in slips)
                {
                    slip.Status = "Paid";
                    slip.PaymentDate = DateTime.UtcNow;
                    _context.Payslips.Update(slip);
                }

                var salarySlips = await _context.SalarySlips.Where(s => s.PayrollRunId == runId).ToListAsync();
                foreach (var s in salarySlips)
                {
                    s.Status = "Paid";
                    _context.SalarySlips.Update(s);
                }

                var bonuses = await _context.BonusIncentives.Where(b => b.PayrollRunId == runId).ToListAsync();
                foreach (var b in bonuses)
                {
                    b.Status = "Paid";
                    _context.BonusIncentives.Update(b);
                }
            }
            else if (newStatus == "Cancelled")
            {
                if (run.Status == "Paid" || run.Status == "Paid & Closed" || run.Status == "Locked")
                {
                    TempData["ErrorMessage"] = "Cannot cancel a Paid or Locked payroll run.";
                    return RedirectToAction(nameof(PayrollProcessing));
                }
                run.Status = "Cancelled";
            }

            if (!string.IsNullOrEmpty(remarks)) run.Remarks = remarks;
            run.UpdatedAt = DateTime.UtcNow;

            _context.PayrollRuns.Update(run);
            await _context.SaveChangesAsync();

            try
            {
                await _hubContext.Clients.Groups("HR", "Super Admin", "Admin", "Finance Manager", "Auditor")
                    .SendAsync("ReceivePayrollRunUpdated", new { payPeriod = run.PayPeriod, status = run.Status, timestamp = DateTime.UtcNow });
            }
            catch { }

            TempData["SuccessMessage"] = $"Payroll Run '{run.PayPeriod}' status changed to '{newStatus}'.";
            return RedirectToAction(nameof(PayrollProcessing));
        }

        // ==========================================
        // 5. PAYSLIPS & DISTRIBUTION
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Payslips(string? payPeriod, int? userId)
        {
            int currentUserId = GetCurrentUserId();
            bool isManagement = IsHrOrAdminOrFinance();

            IQueryable<Payslip> query = _context.Payslips
                .Include(p => p.User)
                .Include(p => p.User!.Role)
                .OrderByDescending(p => p.PaymentDate);

            // Role Security: Non-management users can ONLY see their own payslips
            if (!isManagement)
            {
                query = query.Where(p => p.UserId == currentUserId);
            }
            else if (userId.HasValue && userId.Value > 0)
            {
                query = query.Where(p => p.UserId == userId.Value);
            }

            if (!string.IsNullOrEmpty(payPeriod))
            {
                query = query.Where(p => p.PayPeriod == payPeriod);
            }

            var payslips = await query.ToListAsync();

            var periods = await _context.Payslips
                .Select(p => p.PayPeriod)
                .Distinct()
                .ToListAsync();

            var employees = await _context.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            ViewBag.PayPeriods = periods;
            ViewBag.Employees = employees;
            ViewBag.IsManagement = isManagement;
            ViewBag.SelectedPeriod = payPeriod;
            ViewBag.SelectedUserId = userId;

            return View(payslips);
        }

        [HttpGet]
        public async Task<IActionResult> ViewPayslipModal(int id)
        {
            int currentUserId = GetCurrentUserId();
            bool isManagement = IsHrOrAdminOrFinance();

            var payslip = await _context.Payslips
                .Include(p => p.User)
                .Include(p => p.User!.Role)
                .Include(p => p.User!.Branch)
                .FirstOrDefaultAsync(p => p.PayslipId == id);

            if (payslip == null) return NotFound();

            // Security Check
            if (!isManagement && payslip.UserId != currentUserId)
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
            return PartialView("_PayslipModalPartial", payslip);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadPayslipPdf(int id, bool download = false)
        {
            int currentUserId = GetCurrentUserId();
            bool isManagement = IsHrOrAdminOrFinance();

            var payslip = await _context.Payslips
                .Include(p => p.User)
                .Include(p => p.User!.Role)
                .Include(p => p.User!.Branch)
                .FirstOrDefaultAsync(p => p.PayslipId == id);

            if (payslip == null)
            {
                var sslip = await _context.SalarySlips
                    .Include(s => s.User)
                    .Include(s => s.User!.Role)
                    .Include(s => s.User!.Branch)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (sslip != null)
                {
                    if (!isManagement && sslip.UserId != currentUserId) return Forbid();

                    payslip = new Payslip
                    {
                        PayslipId = sslip.Id,
                        UserId = sslip.UserId,
                        User = sslip.User,
                        PayPeriod = $"{sslip.MonthName} {sslip.Year}",
                        TotalWorkingDays = sslip.WorkingDays,
                        PaidDays = sslip.PaidDays,
                        BasicSalary = sslip.BasicSalary,
                        HRA = sslip.Hra,
                        GrossSalary = sslip.GrossSalary,
                        ProvidentFund = sslip.PfDeduction,
                        ProfessionalTax = sslip.PtDeduction,
                        TDS = sslip.TdsDeduction,
                        TotalDeductions = sslip.TotalDeductions,
                        NetSalary = sslip.NetSalary,
                        Status = sslip.Status,
                        PayslipNumber = $"PAY-{sslip.Year}{sslip.Month:D2}-{sslip.UserId:D4}"
                    };
                }
            }

            if (payslip == null) return NotFound();

            if (!isManagement && payslip.UserId != currentUserId)
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
            ViewBag.AutoDownload = download;
            return View("PayslipPrintView", payslip);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Super Admin,Admin,HR,Finance Manager")]
        public async Task<IActionResult> SendPayslipNotification(int slipId)
        {
            if (!IsHrOrAdminOrFinance()) return Json(new { success = false, message = "Access denied." });

            var payslip = await _context.Payslips
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PayslipId == slipId);

            int recipientId;
            string employeeName;
            string employeeEmail;
            string employeeUserName;
            string periodStr;
            decimal netSalary;
            int downloadId;

            if (payslip == null)
            {
                var sslip = await _context.SalarySlips
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.Id == slipId);

                if (sslip == null)
                {
                    return Json(new { success = false, message = "Payslip record not found." });
                }

                recipientId = sslip.UserId;
                var recipient = sslip.User ?? await _context.Users.FindAsync(recipientId);
                employeeName = recipient?.FullName ?? $"Employee #{recipientId}";
                employeeEmail = recipient?.Email ?? "";
                employeeUserName = recipient?.UserName ?? "";
                periodStr = $"{sslip.MonthName} {sslip.Year}";
                netSalary = sslip.NetSalary;
                downloadId = sslip.Id;
            }
            else
            {
                recipientId = payslip.UserId ?? 0;
                var emp = payslip.User ?? await _context.Users.FindAsync(recipientId);
                employeeName = emp?.FullName ?? $"Employee #{recipientId}";
                employeeEmail = emp?.Email ?? "";
                employeeUserName = emp?.UserName ?? "";
                periodStr = payslip.PayPeriod ?? "Current Period";
                netSalary = payslip.NetSalary ?? 0m;
                downloadId = payslip.PayslipId;
            }

            int currentSenderId = GetCurrentUserId();
            var currentSender = await _context.Users.FindAsync(currentSenderId);
            string senderName = currentSender?.FullName ?? currentSender?.UserName ?? "HR Department";
            string senderEmail = currentSender?.Email ?? "hr@erp.com";

            // 1. Recipient Inbox Entry
            var recipientMsg = new InternalMessage
            {
                SenderUserId = currentSenderId,
                RecipientUserId = recipientId,
                Subject = $"Official Salary Payslip Released - {periodStr}",
                Body = $"Hello {employeeName}, your salary slip for {periodStr} is now published. Net Disbursed: INR {netSalary:N0}. Click below to inspect your official payslip statement.",
                Folder = "Inbox",
                Category = "Payroll & Audit",
                AttachmentUrl = $"/HRPayroll/DownloadPayslipPdf/{downloadId}",
                IsRead = false,
                SentAt = DateTime.UtcNow
            };
            _context.InternalMessages.Add(recipientMsg);

            // 2. Sender (HR) Sent Items Copy (So HR can see it under localhost:7195/Inbox?folder=sent)
            var senderCopy = new InternalMessage
            {
                SenderUserId = currentSenderId,
                RecipientUserId = recipientId,
                Subject = $"[Sent] Payslip Dispatched to {employeeName} - {periodStr}",
                Body = $"Successfully dispatched {periodStr} salary payslip to {employeeName} ({employeeUserName}) for INR {netSalary:N0}.",
                Folder = "Sent",
                Category = "Payroll & Audit",
                AttachmentUrl = $"/HRPayroll/DownloadPayslipPdf/{downloadId}",
                IsRead = true,
                SentAt = DateTime.UtcNow
            };
            _context.InternalMessages.Add(senderCopy);

            // 3. Persistent System Notification (for Recipient's Bell Icon)
            var notification = new SystemNotification
            {
                UserId = recipientId,
                Title = $"Payslip Dispatched: {periodStr}",
                Message = $"Your monthly payslip for {periodStr} has been credited and is ready for download.",
                Category = "HR",
                TargetUrl = $"/HRPayroll/DownloadPayslipPdf/{downloadId}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.SystemNotifications.Add(notification);

            // Topbar Notification Sync
            NotificationsController.AddNotification(new NotificationItem
            {
                Title = $"Payslip Dispatched: {periodStr}",
                Description = $"Your monthly payslip for {periodStr} has been credited (Net: INR {netSalary:N0}).",
                Category = "HR",
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                IconClass = "fa-file-invoice-dollar",
                ColorClass = "text-success",
                BgColorClass = "bg-success-subtle",
                TargetUrl = $"/HRPayroll/DownloadPayslipPdf/{downloadId}",
                TargetEmail = employeeEmail
            });

            // 4. ActivityLog
            _context.ActivityLogs.Add(new ActivityLog
            {
                Title = "Payslip Dispatched",
                Description = $"Payslip notification for {periodStr} dispatched to {employeeName} ({employeeEmail}).",
                IconClass = "fa-paper-plane",
                ColorClass = "text-primary",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            // 5. SignalR Real-time Toast & Badge Push to recipient and broadcast
            try
            {
                await _hubContext.Clients.User(recipientId.ToString()).SendAsync("ReceivePayslipIssuedAlert", new
                {
                    payPeriod = periodStr,
                    netSalary = netSalary.ToString("N0"),
                    slipId = downloadId,
                    message = $"Official payslip for {periodStr} has been issued and dispatched to {employeeName}."
                });

                await _hubContext.Clients.All.SendAsync("ReceivePayslipIssuedAlert", new
                {
                    payPeriod = periodStr,
                    recipientUserId = recipientId,
                    employeeName = employeeName,
                    netSalary = netSalary.ToString("N0"),
                    slipId = downloadId,
                    message = $"Official payslip for {periodStr} has been issued and dispatched to {employeeName}."
                });
            }
            catch { }

            return Json(new { success = true, message = $"Official payslip for {periodStr} dispatched to {employeeName} successfully." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmailPayslip(int id)
        {
            if (!IsHrOrAdminOrFinance()) return Forbid();

            var result = await SendPayslipNotification(id);
            if (result is JsonResult jsonResult)
            {
                dynamic data = jsonResult.Value!;
                TempData["SuccessMessage"] = data.message;
            }
            else
            {
                TempData["SuccessMessage"] = "Payslip notification dispatched successfully.";
            }
            return RedirectToAction(nameof(Payslips));
        }

        // ==========================================
        // 6. BONUS & INCENTIVES TRACKER
        // ==========================================
        [HttpGet]
        [Authorize(Roles = "HR,Super Admin,Admin,Sales Manager,Finance Manager,Auditor")]
        public async Task<IActionResult> BonusIncentives(string search = "", string status = "All")
        {
            if (!IsHrOrAdminOrFinance() && !User.IsInRole("Sales Manager") && !User.IsInRole("Auditor")) return Forbid();

            var query = _context.BonusIncentives
                .Include(b => b.User)
                    .ThenInclude(u => u!.Department)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var clean = search.Trim().ToLower();
                query = query.Where(b => (b.User != null && (b.User.FullName.ToLower().Contains(clean) || 
                                                             b.User.UserName.ToLower().Contains(clean) || 
                                                             b.User.UserCode.ToLower().Contains(clean) ||
                                                             b.User.Email.ToLower().Contains(clean))) ||
                                         (b.Type != null && b.Type.ToLower().Contains(clean)) ||
                                         (b.PerformancePeriod != null && b.PerformancePeriod.ToLower().Contains(clean)) ||
                                         (b.PayoutMonth != null && b.PayoutMonth.ToLower().Contains(clean)));
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(b => b.Status == status);
            }

            var list = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();

            var activeEmployees = await _context.Users
                .Include(u => u.Department)
                .Where(u => u.IsActive)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            ViewBag.ActiveEmployees = activeEmployees;
            ViewBag.TotalBonusRecords = list.Count;
            ViewBag.TotalRecords = list.Count;
            ViewBag.ApprovedPayout = list.Where(b => b.Status == "Approved" || b.Status == "Included in Payroll" || b.Status == "Paid in Payroll" || b.Status == "Paid").Sum(b => b.Amount);
            ViewBag.ApprovedTotal = ViewBag.ApprovedPayout;
            ViewBag.PendingApprovals = list.Count(b => b.Status == "Pending" || b.Status == "Draft" || b.Status == "Submitted");
            ViewBag.SearchTerm = search;
            ViewBag.SelectedStatus = status;

            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HR,Super Admin,Admin,Sales Manager")]
        public async Task<IActionResult> AwardBonus(int userId, string incentiveType, decimal amount, string performancePeriod, string payoutMonth, string? reasonOrDescription)
        {
            if (!IsHrOrAdminOrFinance() && !User.IsInRole("Sales Manager")) return Forbid();

            if (userId <= 0 || amount <= 0)
            {
                TempData["ErrorMessage"] = "Select a valid employee and provide an amount greater than zero.";
                return RedirectToAction(nameof(BonusIncentives));
            }

            int currentUserId = GetCurrentUserId();

            var award = new BonusIncentive
            {
                UserId = userId,
                Type = !string.IsNullOrEmpty(incentiveType) ? incentiveType : "Performance Bonus",
                Amount = amount,
                PerformancePeriod = !string.IsNullOrEmpty(performancePeriod) ? performancePeriod : "September 2026",
                PayoutMonth = !string.IsNullOrEmpty(payoutMonth) ? payoutMonth : "September 2026",
                Reason = reasonOrDescription,
                Status = "Approved",
                ApprovedByUserId = currentUserId,
                ApprovedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.BonusIncentives.Add(award);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Incentive / Bonus awarded successfully and linked to payroll.";
            return RedirectToAction(nameof(BonusIncentives));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveBonusIncentive(BonusIncentive model)
        {
            if (!IsHrOrAdminOrFinance() && !User.IsInRole("Sales Manager")) return Forbid();

            if (model.BonusId == 0)
            {
                return await AwardBonus(model.UserId, model.Type, model.Amount, model.PerformancePeriod, model.PayoutMonth, model.Reason);
            }
            else
            {
                var existing = await _context.BonusIncentives.FindAsync(model.BonusId);
                if (existing == null) return NotFound();

                existing.UserId = model.UserId;
                existing.Type = model.Type;
                existing.Amount = model.Amount;
                existing.Reason = model.Reason;
                existing.PerformancePeriod = model.PerformancePeriod;
                existing.PayoutMonth = model.PayoutMonth;
                existing.UpdatedAt = DateTime.UtcNow;

                _context.BonusIncentives.Update(existing);
                TempData["SuccessMessage"] = $"Bonus record updated successfully.";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(BonusIncentives));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HR,Super Admin,Admin")]
        public async Task<IActionResult> ApproveBonusIncentive(int id)
        {
            if (!IsHrOrAdminOrFinance()) return Forbid();

            var bonus = await _context.BonusIncentives.FindAsync(id);
            if (bonus == null) return NotFound();

            bonus.Status = "Approved";
            bonus.ApprovedByUserId = GetCurrentUserId();
            bonus.ApprovedAt = DateTime.UtcNow;
            bonus.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Bonus record approved for payout.";
            return RedirectToAction(nameof(BonusIncentives));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HR,Super Admin,Admin")]
        public async Task<IActionResult> DeleteBonus(int id)
        {
            return await DeleteBonusIncentive(id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HR,Super Admin,Admin")]
        public async Task<IActionResult> DeleteBonusIncentive(int id)
        {
            if (!IsHrOrAdminOrFinance()) return Forbid();

            var bonus = await _context.BonusIncentives.FindAsync(id);
            if (bonus == null) return NotFound();

            if (bonus.Status == "Included in Payroll" || bonus.Status == "Paid" || bonus.Status == "Paid in Payroll")
            {
                TempData["ErrorMessage"] = "Cannot delete bonus that is already included in a finalized payroll run.";
                return RedirectToAction(nameof(BonusIncentives));
            }

            _context.BonusIncentives.Remove(bonus);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Bonus record deleted successfully.";
            return RedirectToAction(nameof(BonusIncentives));
        }
    }
}
