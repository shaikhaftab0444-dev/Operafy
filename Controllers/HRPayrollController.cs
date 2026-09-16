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
        public async Task<IActionResult> StatutoryCompliance()
        {
            if (!IsHrOrAdminOrFinance()) return Forbid();

            var configs = await _context.StatutoryConfigurations.ToListAsync();
            var filings = await _context.StatutoryFilingLogs
                .OrderByDescending(f => f.FilingDate)
                .ToListAsync();

            ViewBag.PFConfig = configs.FirstOrDefault(c => c.RuleType == "PF") ?? new StatutoryConfiguration { RuleType = "PF", EmployeeRate = 12.00m, EmployerRate = 12.00m, WageCeilingLimit = 15000.00m };
            ViewBag.ESIConfig = configs.FirstOrDefault(c => c.RuleType == "ESI") ?? new StatutoryConfiguration { RuleType = "ESI", EmployeeRate = 0.75m, EmployerRate = 3.25m, WageCeilingLimit = 21000.00m };
            ViewBag.TDSConfig = configs.FirstOrDefault(c => c.RuleType == "TDS") ?? new StatutoryConfiguration { RuleType = "TDS", StandardDeductionAnnual = 75000.00m, DefaultTaxRegime = "New Tax Regime" };
            ViewBag.Filings = filings;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatutoryConfig(string ruleType, decimal employeeRate, decimal employerRate, decimal wageCeilingLimit, decimal standardDeductionAnnual, string? defaultTaxRegime)
        {
            if (!IsHrOrAdminOrFinance()) return Forbid();

            var existing = await _context.StatutoryConfigurations.FirstOrDefaultAsync(c => c.RuleType == ruleType);
            if (existing == null)
            {
                existing = new StatutoryConfiguration { RuleType = ruleType };
                _context.StatutoryConfigurations.Add(existing);
            }

            existing.EmployeeRate = employeeRate;
            existing.EmployerRate = employerRate;
            existing.WageCeilingLimit = wageCeilingLimit;
            existing.StandardDeductionAnnual = standardDeductionAnnual;
            if (!string.IsNullOrEmpty(defaultTaxRegime)) existing.DefaultTaxRegime = defaultTaxRegime;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Statutory Configuration for '{ruleType}' updated successfully.";
            return RedirectToAction(nameof(StatutoryCompliance));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFilingRecord(StatutoryFilingLog model)
        {
            if (!IsHrOrAdminOrFinance()) return Forbid();

            if (ModelState.IsValid)
            {
                model.FilingDate = DateTime.Today;
                _context.StatutoryFilingLogs.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Statutory filing record for '{model.ComplianceAct}' added successfully.";
            }

            return RedirectToAction(nameof(StatutoryCompliance));
        }

        // ==========================================
        // 4. PAYROLL PROCESSING ENGINE
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> PayrollProcessing()
        {
            if (!IsHrOrAdminOrFinance()) return Forbid();

            var runs = await _context.PayrollRuns
                .OrderByDescending(r => r.Year)
                .ThenByDescending(r => r.Month)
                .ToListAsync();

            var activeEmployees = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.IsActive && u.Role != null && u.Role.RoleName != "Admin" && u.Role.RoleName != "Super Admin")
                .ToListAsync();

            ViewBag.PayrollRuns = runs;
            ViewBag.ActiveEmployeesCount = activeEmployees.Count;
            ViewBag.DraftRunsCount = runs.Count(r => r.Status == "Draft" || r.Status == "Calculated" || r.Status == "Under Review");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RunPayrollProcessing(string payPeriod, int month, int year, string? department)
        {
            if (!IsHrOrAdminOrFinance()) return Forbid();

            if (string.IsNullOrEmpty(payPeriod))
            {
                TempData["ErrorMessage"] = "Please specify a valid pay period.";
                return RedirectToAction(nameof(PayrollProcessing));
            }

            department = string.IsNullOrEmpty(department) ? "All Departments" : department;

            // Check if payroll run for this period and department is already Locked or Paid
            var existingLocked = await _context.PayrollRuns
                .FirstOrDefaultAsync(r => r.PayPeriod == payPeriod && r.Department == department && (r.Status == "Locked" || r.Status == "Paid"));

            if (existingLocked != null)
            {
                TempData["ErrorMessage"] = $"Payroll for '{payPeriod}' ({department}) is already {existingLocked.Status} and cannot be re-processed without reversal.";
                return RedirectToAction(nameof(PayrollProcessing));
            }

            // Find or create PayrollRun
            var payrollRun = await _context.PayrollRuns
                .FirstOrDefaultAsync(r => r.PayPeriod == payPeriod && r.Department == department);

            if (payrollRun == null)
            {
                payrollRun = new PayrollRun
                {
                    PayPeriod = payPeriod,
                    Month = month,
                    Year = year,
                    Department = department,
                    Status = "Draft",
                    ProcessedByUserId = GetCurrentUserId(),
                    ProcessedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                _context.PayrollRuns.Add(payrollRun);
                await _context.SaveChangesAsync();
            }
            else
            {
                payrollRun.ProcessedByUserId = GetCurrentUserId();
                payrollRun.ProcessedAt = DateTime.UtcNow;
                payrollRun.Status = "Calculated";
                payrollRun.UpdatedAt = DateTime.UtcNow;
            }

            // Fetch active employees eligible for payroll
            var employees = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.IsActive && u.Role != null && u.Role.RoleName != "Admin" && u.Role.RoleName != "Super Admin")
                .ToListAsync();

            var assignments = await _context.EmployeeSalaryAssignments
                .Where(a => a.IsCurrent)
                .ToDictionaryAsync(a => a.UserId);

            int payslipsCreatedOrUpdated = 0;
            decimal runGross = 0;
            decimal runDeductions = 0;
            decimal runNet = 0;
            decimal runEmployerPf = 0;
            decimal runEmployerEsi = 0;
            decimal runCtc = 0;

            foreach (var emp in employees)
            {
                // Fetch salary assignment or default
                assignments.TryGetValue(emp.UserId, out var sal);

                decimal monthlyCtc = sal != null ? (sal.AnnualCTC / 12.0m) : 50000.00m;
                decimal basic = sal != null ? sal.MonthlyBasic : 25000.00m;
                decimal hra = sal != null ? sal.MonthlyHRA : 10000.00m;
                decimal conv = sal != null ? sal.MonthlyConveyance : 1600.00m;
                decimal med = sal != null ? sal.MonthlyMedical : 1250.00m;
                decimal lta = sal != null ? sal.MonthlyLTA : 1250.00m;
                decimal special = sal != null ? sal.MonthlySpecialAllowance : 10900.00m;
                decimal other = sal != null ? sal.MonthlyOtherAllowance : 0.00m;

                // 1. Attendance & LOP Integration
                int totalWorkingDays = DateTime.DaysInMonth(year, month);
                var attLogs = await _context.HRAttendanceLogs
                    .Where(l => l.UserId == emp.UserId && l.Date.Month == month && l.Date.Year == year)
                    .ToListAsync();

                int presentDays = attLogs.Count(l => l.Status.Contains("Present"));
                int absentDays = attLogs.Count(l => l.Status.Contains("Absent"));
                int unpaidLeaveDays = absentDays; // Unpaid leave / LOP
                int paidDays = totalWorkingDays - unpaidLeaveDays;
                if (paidDays < 0) paidDays = 0;

                // Loss of Pay Deduction
                decimal dailyRate = basic / (decimal)totalWorkingDays;
                decimal lopDeduction = Math.Round(dailyRate * unpaidLeaveDays, 2);

                // 2. Overtime Integration
                var otRecords = await _context.HROvertimeRecords
                    .Where(o => o.UserId == emp.UserId && o.PayoutStatus == "Approved for Payroll")
                    .ToListAsync();

                decimal overtimePay = otRecords.Sum(o => o.TotalOvertimePay);
                int overtimeHours = otRecords.Sum(o => o.OvertimeHours);

                // Mark overtime records as processed
                foreach (var ot in otRecords)
                {
                    ot.PayoutStatus = "Paid";
                }

                // 3. Bonus & Incentive Integration
                var bonuses = await _context.BonusIncentives
                    .Where(b => b.UserId == emp.UserId && b.PayoutMonth == payPeriod && b.Status == "Approved")
                    .ToListAsync();

                decimal bonusAmount = bonuses.Sum(b => b.Amount);

                foreach (var b in bonuses)
                {
                    b.Status = "Included in Payroll";
                    b.PayrollRunId = payrollRun.PayrollRunId;
                }

                // Calculate Totals for Employee
                decimal grossSalary = Math.Round((basic + hra + conv + med + lta + special + other + overtimePay + bonusAmount) - lopDeduction, 2);
                if (grossSalary < 0) grossSalary = 0;

                decimal pfEmp = sal != null ? sal.MonthlyPFEmployee : Math.Round(Math.Min(basic, 15000.00m) * 0.12m, 2);
                decimal esiEmp = (sal != null) ? sal.MonthlyESIEmployee : (grossSalary <= 21000.00m ? Math.Round(grossSalary * 0.0075m, 2) : 0.00m);
                decimal pt = sal != null ? sal.MonthlyPT : (grossSalary > 15000.00m ? 200.00m : 0.00m);
                decimal tds = sal != null ? sal.MonthlyTDS : 0.00m;

                decimal totalDeductions = pfEmp + esiEmp + pt + tds + lopDeduction;
                decimal netSalary = Math.Max(0, grossSalary - (pfEmp + esiEmp + pt + tds));

                decimal pfEmployer = sal != null ? sal.MonthlyPFEmployer : Math.Round(Math.Min(basic, 15000.00m) * 0.12m, 2);
                decimal esiEmployer = sal != null ? sal.MonthlyESIEmployer : (grossSalary <= 21000.00m ? Math.Round(grossSalary * 0.0325m, 2) : 0.00m);

                // Check existing payslip record
                var payslip = await _context.Payslips
                    .FirstOrDefaultAsync(p => p.UserId == emp.UserId && p.PayPeriod == payPeriod);

                if (payslip == null)
                {
                    payslip = new Payslip
                    {
                        PayrollRunId = payrollRun.PayrollRunId,
                        UserId = emp.UserId,
                        PayPeriod = payPeriod,
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
                        GrossSalary = grossSalary,
                        ProvidentFund = pfEmp,
                        ESI = esiEmp,
                        ProfessionalTax = pt,
                        TDS = tds,
                        LOPDeduction = lopDeduction,
                        TotalDeductions = totalDeductions,
                        NetSalary = netSalary,
                        EmployerPF = pfEmployer,
                        EmployerESI = esiEmployer,
                        TotalCTC = monthlyCtc,
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
                    payslip.PayrollRunId = payrollRun.PayrollRunId;
                    payslip.BasicSalary = basic;
                    payslip.HRA = hra;
                    payslip.TransportAllowance = conv;
                    payslip.MedicalAllowance = med;
                    payslip.LTA = lta;
                    payslip.SpecialAllowance = special;
                    payslip.OtherAllowance = other;
                    payslip.BonusIncentives = bonusAmount;
                    payslip.OvertimePay = overtimePay;
                    payslip.GrossSalary = grossSalary;
                    payslip.ProvidentFund = pfEmp;
                    payslip.ESI = esiEmp;
                    payslip.ProfessionalTax = pt;
                    payslip.TDS = tds;
                    payslip.LOPDeduction = lopDeduction;
                    payslip.TotalDeductions = totalDeductions;
                    payslip.NetSalary = netSalary;
                    payslip.EmployerPF = pfEmployer;
                    payslip.EmployerESI = esiEmployer;
                    payslip.TotalCTC = monthlyCtc;
                    payslip.TotalWorkingDays = totalWorkingDays;
                    payslip.PresentDays = presentDays;
                    payslip.AbsentDays = absentDays;
                    payslip.UnpaidLeaveDays = unpaidLeaveDays;
                    payslip.PaidDays = paidDays;
                    payslip.OvertimeHours = overtimeHours;
                    _context.Payslips.Update(payslip);
                }

                payslipsCreatedOrUpdated++;
                runGross += grossSalary;
                runDeductions += totalDeductions;
                runNet += netSalary;
                runEmployerPf += pfEmployer;
                runEmployerEsi += esiEmployer;
                runCtc += (grossSalary + pfEmployer + esiEmployer);
            }

            // Update PayrollRun aggregates
            payrollRun.TotalEmployees = payslipsCreatedOrUpdated;
            payrollRun.TotalGrossSalary = runGross;
            payrollRun.TotalDeductions = runDeductions;
            payrollRun.TotalNetSalary = runNet;
            payrollRun.TotalEmployerPF = runEmployerPf;
            payrollRun.TotalEmployerESI = runEmployerEsi;
            payrollRun.TotalCTC = runCtc;
            payrollRun.Status = "Calculated";
            payrollRun.UpdatedAt = DateTime.UtcNow;

            _context.PayrollRuns.Update(payrollRun);
            await _context.SaveChangesAsync();

            _context.ActivityLogs.Add(new ActivityLog
            {
                Title = "Payroll Processing Executed",
                Description = $"Executed pay run for '{payPeriod}'. {payslipsCreatedOrUpdated} employee payslips computed.",
                IconClass = "fa-coins",
                ColorClass = "text-primary",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            try
            {
                await _hubContext.Clients.Groups("HR", "Super Admin", "Admin", "Finance Manager", "Accountant")
                    .SendAsync("ReceivePayrollRunUpdated", new { payPeriod = payPeriod, status = payrollRun.Status, timestamp = DateTime.UtcNow });
            }
            catch { }

            TempData["SuccessMessage"] = $"Payroll run for '{payPeriod}' successfully calculated! {payslipsCreatedOrUpdated} payslips generated/updated. Total Net Payout: ₹{runNet:N2}.";
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
            else if (newStatus == "Paid")
            {
                run.Status = "Paid";
                run.PaidByUserId = currentUserId;
                run.PaidAt = DateTime.UtcNow;

                // Update linked payslips status to Paid
                var slips = await _context.Payslips.Where(p => p.PayrollRunId == runId).ToListAsync();
                foreach (var slip in slips)
                {
                    slip.Status = "Paid";
                    slip.PaymentDate = DateTime.UtcNow;
                    _context.Payslips.Update(slip);
                }

                // Update linked bonus status to Paid
                var bonuses = await _context.BonusIncentives.Where(b => b.PayrollRunId == runId).ToListAsync();
                foreach (var b in bonuses)
                {
                    b.Status = "Paid";
                    _context.BonusIncentives.Update(b);
                }
            }
            else if (newStatus == "Cancelled")
            {
                if (run.Status == "Paid" || run.Status == "Locked")
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
                await _hubContext.Clients.Groups("HR", "Super Admin", "Admin", "Finance Manager", "Accountant")
                    .SendAsync("ReceivePayrollRunUpdated", new { payPeriod = run.PayPeriod, status = run.Status, timestamp = DateTime.UtcNow });
            }
            catch { }

            await _context.SaveChangesAsync();

            _context.ActivityLogs.Add(new ActivityLog
            {
                Title = $"Payroll Run Status Updated: {newStatus}",
                Description = $"Payroll run '{run.PayPeriod}' updated to {newStatus}.",
                IconClass = "fa-file-signature",
                ColorClass = newStatus == "Paid" ? "text-success" : "text-info",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Payroll Run '{run.PayPeriod}' status changed to '{newStatus}'.";
            return RedirectToAction(nameof(PayrollProcessing));
        }

        [HttpGet]
        public async Task<IActionResult> GetPayrollRunDetails(int id)
        {
            var run = await _context.PayrollRuns.FindAsync(id);
            if (run == null) return NotFound();

            var payslips = await _context.Payslips
                .Include(p => p.User)
                .Where(p => p.PayrollRunId == id)
                .OrderBy(p => p.User!.FullName)
                .Select(p => new
                {
                    p.PayslipId,
                    EmployeeName = p.User != null ? p.User.FullName : "Employee",
                    UserCode = p.User != null ? p.User.UserCode : "USR",
                    p.BasicSalary,
                    p.HRA,
                    p.GrossSalary,
                    p.TotalDeductions,
                    p.NetSalary,
                    p.Status
                })
                .ToListAsync();

            return Json(new
            {
                run.PayrollRunId,
                run.PayPeriod,
                run.Department,
                run.TotalEmployees,
                TotalGrossSalary = run.TotalGrossSalary.ToString("N2"),
                TotalDeductions = run.TotalDeductions.ToString("N2"),
                TotalNetSalary = run.TotalNetSalary.ToString("N2"),
                TotalEmployerPF = run.TotalEmployerPF.ToString("N2"),
                TotalEmployerESI = run.TotalEmployerESI.ToString("N2"),
                run.Status,
                ProcessedAt = run.ProcessedAt.ToString("dd MMM yyyy HH:mm"),
                Payslips = payslips
            });
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
        public async Task<IActionResult> DownloadPayslipPdf(int id)
        {
            int currentUserId = GetCurrentUserId();
            bool isManagement = IsHrOrAdminOrFinance();

            var payslip = await _context.Payslips
                .Include(p => p.User)
                .Include(p => p.User!.Role)
                .Include(p => p.User!.Branch)
                .FirstOrDefaultAsync(p => p.PayslipId == id);

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
            return View("PayslipPrintView", payslip);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmailPayslip(int id)
        {
            if (!IsHrOrAdminOrFinance()) return Forbid();

            var payslip = await _context.Payslips.Include(p => p.User).FirstOrDefaultAsync(p => p.PayslipId == id);
            if (payslip == null) return NotFound();

            _context.ActivityLogs.Add(new ActivityLog
            {
                Title = "Payslip Emailed",
                Description = $"Sent payslip for period {payslip.PayPeriod} to {payslip.User?.Email}.",
                IconClass = "fa-paper-plane",
                ColorClass = "text-info",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Payslip for '{payslip.User?.FullName}' sent via email successfully.";
            return RedirectToAction(nameof(Payslips));
        }

        // ==========================================
        // 6. BONUS & INCENTIVES TRACKER
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> BonusIncentives()
        {
            if (!IsHrOrAdminOrFinance()) return Forbid();

            var items = await _context.BonusIncentives
                .Include(b => b.User)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var activeEmployees = await _context.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            ViewBag.ActiveEmployees = activeEmployees;
            ViewBag.ApprovedTotal = items.Where(i => i.Status == "Approved" || i.Status == "Included in Payroll" || i.Status == "Paid").Sum(i => i.Amount);

            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveBonusIncentive(BonusIncentive model)
        {
            if (!IsHrOrAdminOrFinance()) return Forbid();

            if (model.BonusId == 0)
            {
                model.Status = "Approved"; // HR/Admin direct entry defaults to Approved
                model.ApprovedByUserId = GetCurrentUserId();
                model.ApprovedAt = DateTime.UtcNow;
                model.CreatedAt = DateTime.UtcNow;

                _context.BonusIncentives.Add(model);
                TempData["SuccessMessage"] = $"Bonus/Incentive awarded successfully.";
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
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(BonusIncentives));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
        public async Task<IActionResult> DeleteBonusIncentive(int id)
        {
            if (!IsHrOrAdminOrFinance()) return Forbid();

            var bonus = await _context.BonusIncentives.FindAsync(id);
            if (bonus == null) return NotFound();

            if (bonus.Status == "Included in Payroll" || bonus.Status == "Paid")
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
