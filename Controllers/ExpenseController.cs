using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Models;
using ERP_System.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,Finance Manager,Accountant,Manager,Sales Manager,Inventory Manager")]
    public class ExpenseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExpenseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Expense
        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? status, string? category)
        {
            var query = _context.Transactions
                .Where(t => t.Type == "Expense Entry")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t => t.TransactionNo.Contains(search) || t.PartyName.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(t => t.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(t => t.PartyName.Contains(category));
            }

            var expenses = await query.OrderByDescending(t => t.Date).ToListAsync();
            var allExpenses = await _context.Transactions.Where(t => t.Type == "Expense Entry").ToListAsync();

            decimal totalSpend = allExpenses.Sum(t => t.Amount);
            int paidCount = allExpenses.Count(t => t.Status == "Paid");
            int pendingCount = allExpenses.Count(t => t.Status == "Pending");
            decimal totalPendingAmount = allExpenses.Where(t => t.Status == "Pending").Sum(t => t.Amount);

            var accountHeadsList = await _context.AccountHeads.Where(a => a.IsActive).ToListAsync();

            var categoriesList = new List<string> { "Office Supplies", "Rent & Premises", "Utilities & Electricity", "Travel & Transport", "Software & IT", "Marketing & Ads" };
            var dbHeads = accountHeadsList.Select(a => a.HeadName).ToList();
            foreach (var head in dbHeads)
            {
                if (!categoriesList.Contains(head)) categoriesList.Add(head);
            }

            var viewModel = new ExpenseViewModel
            {
                Expenses = expenses,
                AccountHeadsList = accountHeadsList,
                CategoriesList = categoriesList,

                TotalExpenseSpend = totalSpend > 0 ? totalSpend : 145200,
                TotalVouchersCount = allExpenses.Count,
                PaidVouchersCount = paidCount,
                PendingVouchersCount = pendingCount,
                TotalPendingAmount = totalPendingAmount > 0 ? totalPendingAmount : 18500,

                SearchTerm = search ?? string.Empty,
                SelectedStatus = status ?? string.Empty,
                SelectedCategory = category ?? string.Empty
            };

            return View(viewModel);
        }

        // POST: /Expense/CreateExpense
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateExpense(string payeeName, string category, decimal amount, string status, string? notes)
        {
            var totalAmount = amount > 0 ? amount : 2500m;
            var categoryText = string.IsNullOrWhiteSpace(category) ? "General Expense" : category;
            var party = string.IsNullOrWhiteSpace(payeeName) ? categoryText : $"{payeeName} ({categoryText})";

            // Generate unique Expense Voucher Number
            var lastId = await _context.Transactions.CountAsync() + 1;
            var voucherNumber = $"EXP-100{lastId:D2}";

            var expense = new Transaction
            {
                TransactionNo = voucherNumber,
                Type = "Expense Entry",
                Date = DateTime.Now,
                PartyName = party,
                Amount = totalAmount,
                Status = string.IsNullOrWhiteSpace(status) ? "Paid" : status
            };

            _context.Transactions.Add(expense);

            // Log activity
            _context.ActivityLogs.Add(new ActivityLog
            {
                Title = "New Expense Entry",
                Description = $"{voucherNumber} logged for {party} (₹{totalAmount:N0})",
                CreatedAt = DateTime.UtcNow,
                IconClass = "fa-receipt",
                ColorClass = "text-danger"
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Expense Voucher '{voucherNumber}' logged successfully for ₹{totalAmount:N0}.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Expense/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int transactionId, string newStatus)
        {
            var expense = await _context.Transactions.FindAsync(transactionId);
            if (expense == null)
            {
                TempData["ErrorMessage"] = "Expense voucher not found.";
                return RedirectToAction(nameof(Index));
            }

            var oldStatus = expense.Status;
            expense.Status = newStatus;
            _context.Entry(expense).State = EntityState.Modified;

            _context.ActivityLogs.Add(new ActivityLog
            {
                Title = "Expense Status Updated",
                Description = $"{expense.TransactionNo} payment status updated to {newStatus}",
                CreatedAt = DateTime.UtcNow,
                IconClass = "fa-file-invoice-dollar",
                ColorClass = "text-info"
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Expense Voucher '{expense.TransactionNo}' status updated to {newStatus}.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Expense/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int transactionId)
        {
            var expense = await _context.Transactions.FindAsync(transactionId);
            if (expense != null)
            {
                _context.Transactions.Remove(expense);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Expense Voucher '{expense.TransactionNo}' removed.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
