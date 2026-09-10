using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Data;
using ERP_System.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin")]
    public class BankMasterController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BankMasterController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /BankMaster
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var accounts = await _context.CompanyBankAccounts
                .AsNoTracking()
                .OrderByDescending(b => b.IsPrimaryAccount)
                .ThenByDescending(b => b.Id)
                .ToListAsync();

            return View(accounts);
        }

        // GET: /BankMaster/GetAccount/5
        [HttpGet]
        public async Task<IActionResult> GetAccount(int id)
        {
            var account = await _context.CompanyBankAccounts.FindAsync(id);
            if (account == null)
            {
                return Json(new { success = false, message = "Bank account not found." });
            }

            return Json(new
            {
                success = true,
                data = new
                {
                    id = account.Id,
                    bankName = account.BankName,
                    accountNumber = account.AccountNumber,
                    ifscCode = account.IFSCCode,
                    branchName = account.BranchName ?? string.Empty,
                    accountType = account.AccountType,
                    openingBalance = account.OpeningBalance,
                    currentBalance = account.CurrentBalance,
                    isPrimaryAccount = account.IsPrimaryAccount,
                    isActive = account.IsActive
                }
            });
        }

        // POST: /BankMaster/CreateOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(CompanyBankAccount model)
        {
            if (string.IsNullOrWhiteSpace(model.BankName) || string.IsNullOrWhiteSpace(model.AccountNumber) || string.IsNullOrWhiteSpace(model.IFSCCode))
            {
                TempData["ErrorMessage"] = "Bank Name, Account Number, and IFSC Code are required.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (model.IsPrimaryAccount)
                {
                    // Reset previous primary
                    var existingPrimary = await _context.CompanyBankAccounts.Where(b => b.IsPrimaryAccount && b.Id != model.Id).ToListAsync();
                    existingPrimary.ForEach(b => b.IsPrimaryAccount = false);
                }

                if (model.Id == 0)
                {
                    model.CurrentBalance = model.OpeningBalance;
                    model.CreatedAt = DateTime.UtcNow;
                    _context.CompanyBankAccounts.Add(model);
                    TempData["SuccessMessage"] = $"Company Bank Account '{model.BankName}' added successfully.";
                }
                else
                {
                    var existing = await _context.CompanyBankAccounts.FindAsync(model.Id);
                    if (existing == null) return NotFound();

                    existing.BankName = model.BankName;
                    existing.AccountNumber = model.AccountNumber;
                    existing.IFSCCode = model.IFSCCode;
                    existing.BranchName = model.BranchName ?? string.Empty;
                    existing.AccountType = model.AccountType;
                    existing.IsPrimaryAccount = model.IsPrimaryAccount;
                    existing.IsActive = model.IsActive;

                    TempData["SuccessMessage"] = $"Company Bank Account '{model.BankName}' updated successfully.";
                }

                await _context.SaveChangesAsync();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Saved successfully." });
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Error: " + ex.Message });
                }

                TempData["ErrorMessage"] = "Error saving bank account: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /BankMaster/ToggleStatus
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var account = await _context.CompanyBankAccounts.FindAsync(id);
            if (account == null) return Json(new { success = false, message = "Account not found." });

            account.IsActive = !account.IsActive;
            await _context.SaveChangesAsync();
            return Json(new { success = true, isActive = account.IsActive, message = $"Account status set to {(account.IsActive ? "Active" : "Inactive")}." });
        }

        // POST: /BankMaster/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var account = await _context.CompanyBankAccounts.FindAsync(id);
            if (account == null)
            {
                TempData["ErrorMessage"] = "Bank account not found.";
                return RedirectToAction(nameof(Index));
            }

            if (account.IsPrimaryAccount)
            {
                TempData["ErrorMessage"] = "Cannot delete the primary bank account. Assign another primary account first.";
                return RedirectToAction(nameof(Index));
            }

            _context.CompanyBankAccounts.Remove(account);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Bank account '{account.BankName}' removed successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
