using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System.Threading.Tasks;
using System.Security.Claims;
using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace ERP_System.Controllers
{
    [Authorize]
    public class ESSClaimsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ESSClaimsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out int id) ? id : 1;
        }

        // GET: /ESSClaims/Submit
        [HttpGet]
        public IActionResult Submit()
        {
            return View();
        }

        // POST: /ESSClaims/SubmitClaim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitClaim(ESSExpenseClaim claim, IFormFile? ReceiptFile)
        {
            if (ModelState.IsValid || claim.Amount > 0)
            {
                claim.UserId = GetCurrentUserId();
                claim.EmployeeName = User.Identity?.Name ?? "Employee";
                claim.ClaimDate = DateTime.Now;
                claim.Status = "Pending";
                claim.ManagerStatus = "Pending";
                claim.CreatedAt = DateTime.UtcNow;

                // Handle file upload
                if (ReceiptFile != null && ReceiptFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "receipts");
                    if (!System.IO.Directory.Exists(uploadsFolder))
                    {
                        System.IO.Directory.CreateDirectory(uploadsFolder);
                    }
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ReceiptFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ReceiptFile.CopyToAsync(fileStream);
                    }
                    claim.ReceiptFileName = "/uploads/receipts/" + uniqueFileName;
                }

                _context.ESSExpenseClaims.Add(claim);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(StatusTracker));
            }
            return View(nameof(Submit), claim);
        }

        // GET: /ESSClaims/Receipts
        [HttpGet]
        public async Task<IActionResult> Receipts()
        {
            int userId = GetCurrentUserId();
            var claimsWithReceipts = await _context.ESSExpenseClaims
                .Where(c => c.UserId == userId && c.ReceiptFileName != null)
                .OrderByDescending(c => c.ClaimDate)
                .ToListAsync();
            return View(claimsWithReceipts);
        }

        // GET: /ESSClaims/StatusTracker
        [HttpGet]
        public async Task<IActionResult> StatusTracker()
        {
            int userId = GetCurrentUserId();
            var claims = await _context.ESSExpenseClaims
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.ClaimDate)
                .ToListAsync();
            return View(claims);
        }
    }
}
