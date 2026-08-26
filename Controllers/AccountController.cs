using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ERP_System.Models;
using ERP_System.Data;

namespace ERP_System.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user != null)
                {
                    if (!user.IsActive)
                    {
                        ModelState.AddModelError(string.Empty, "Your account has been locked. Please contact the administrator.");
                        return View(model);
                    }

                    var hasher = new PasswordHasher<User>();
                    var verificationResult = hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);

                    if (verificationResult == PasswordVerificationResult.Success)
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                            new Claim(ClaimTypes.Name, user.FullName),
                            new Claim(ClaimTypes.Email, user.Email),
                            new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "Employee"),
                            new Claim("Company", user.CompanyId.ToString())
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = model.RememberMe,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                        };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        return RedirectToAction("Index", "Dashboard");
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid email or password attempt.");
            }

            return View(model);
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return RedirectToAction("Login");
        }

        // POST: /Account/Register
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            return RedirectToAction("Login");
        }

        // GET: /Account/Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: /Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string emailOrUsername)
        {
            if (string.IsNullOrEmpty(emailOrUsername))
            {
                ModelState.AddModelError(string.Empty, "Please enter your registered email or username.");
                return View();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == emailOrUsername || u.UserName == emailOrUsername);

            if (user == null)
            {
                // To prevent user enumeration, we can say "If active user exists, a reset link has been dispatched."
                // But for ERP workflow simulation, let's explicitly validate or show success link.
                ModelState.AddModelError(string.Empty, "Account not found with the specified email or username.");
                return View();
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Your account is locked/inactive. Please contact the administrator.");
                return View();
            }

            var token = Guid.NewGuid().ToString("N");
            var resetRequest = new AdminPasswordReset
            {
                Username = user.UserName,
                Email = user.Email,
                RequestDate = DateTime.UtcNow,
                Status = "Pending",
                Token = token,
                ExpiryDate = DateTime.UtcNow.AddMinutes(15),
                RequestType = "Automated Self-Service",
                DeliveryMethod = "Email Magic Link"
            };

            _context.AdminPasswordResets.Add(resetRequest);
            await _context.SaveChangesAsync();

            // Simulate email dispatch by generating SSPR URL link
            var resetLink = Url.Action("ResetPassword", "Account", new { token }, Request.Scheme);
            TempData["ResetLink"] = resetLink;
            TempData["SuccessMessage"] = "A secure reset link has been generated. Use the link below to update your password (valid for 15 mins).";

            return View();
        }

        // GET: /Account/ResetPassword
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Invalid or missing password reset token.";
                return RedirectToAction(nameof(ForgotPassword));
            }

            var reset = await _context.AdminPasswordResets
                .FirstOrDefaultAsync(r => r.Token == token && r.Status == "Pending" && r.ExpiryDate > DateTime.UtcNow);

            if (reset == null)
            {
                TempData["ErrorMessage"] = "Password reset link is invalid, expired, or has already been used.";
                return RedirectToAction(nameof(ForgotPassword));
            }

            ViewBag.Token = token;
            ViewBag.Email = reset.Email;
            return View();
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string token, string password, string confirmPassword)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Invalid password reset attempt.";
                return RedirectToAction(nameof(ForgotPassword));
            }

            var reset = await _context.AdminPasswordResets
                .FirstOrDefaultAsync(r => r.Token == token && r.Status == "Pending" && r.ExpiryDate > DateTime.UtcNow);

            if (reset == null)
            {
                TempData["ErrorMessage"] = "Password reset link is invalid, expired, or has already been used.";
                return RedirectToAction(nameof(ForgotPassword));
            }

            ViewBag.Token = token;
            ViewBag.Email = reset.Email;

            if (string.IsNullOrEmpty(password) || password.Length < 8)
            {
                ModelState.AddModelError(string.Empty, "Password must be at least 8 characters long.");
                return View();
            }

            // Password Policy check: 1 uppercase, 1 digit, 1 special char
            bool hasUpper = password.Any(char.IsUpper);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

            if (!hasUpper || !hasDigit || !hasSpecial)
            {
                ModelState.AddModelError(string.Empty, "Password must contain at least 1 uppercase letter, 1 digit, and 1 special character.");
                return View();
            }

            if (password != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Passwords do not match.");
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == reset.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Associated user account could not be found.");
                return View();
            }

            var hasher = new PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, password);
            
            // Mark token as Auto-Completed (Self-Service)
            reset.Status = "Auto-Completed (Self-Service)";
            
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your password has been reset successfully! You can now log in.";
            return RedirectToAction(nameof(Login));
        }
    }
}
