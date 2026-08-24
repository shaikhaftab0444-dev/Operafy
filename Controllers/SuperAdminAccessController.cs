using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Collections.Generic;
using System;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin")]
    public class SuperAdminAccessController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SuperAdminAccessController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /SuperAdminAccess/Roles
        [HttpGet]
        public async Task<IActionResult> Roles()
        {
            var roles = await _context.Roles.ToListAsync();
            return View(roles);
        }

        // POST: /SuperAdminAccess/CreateRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(Role role)
        {
            if (ModelState.IsValid || role.RoleName != null)
            {
                role.IsActive = true;
                _context.Roles.Add(role);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Roles));
            }
            var roles = await _context.Roles.ToListAsync();
            return View(nameof(Roles), roles);
        }

        // GET: /SuperAdminAccess/Permissions
        [HttpGet]
        public async Task<IActionResult> Permissions()
        {
            var perms = await _context.RolePermissions.Include(p => p.Role).ToListAsync();
            return View(perms);
        }

        // GET: /SuperAdminAccess/UserRoles
        [HttpGet]
        public async Task<IActionResult> UserRoles()
        {
            var users = await _context.Users.Include(u => u.Role).Include(u => u.Branch).ToListAsync();
            return View(users);
        }

        // GET: /SuperAdminAccess/Impersonation
        [HttpGet]
        public async Task<IActionResult> Impersonation()
        {
            var users = await _context.Users.Include(u => u.Role).ToListAsync();
            return View(users);
        }

        // POST: /SuperAdminAccess/ImpersonateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImpersonateUser(int id)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == id);
            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "Employee"),
                    new Claim("Company", user.CompanyId.ToString()),
                    new Claim("ImpersonatedBy", "Super Admin")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Dashboard");
            }
            return RedirectToAction(nameof(Impersonation));
        }
    }
}
