<<<<<<< HEAD
using Microsoft.AspNetCore.Authorization;
=======
﻿using Microsoft.AspNetCore.Authorization;
>>>>>>> f33209e (updated)
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP_System.Data;
using ERP_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_System.Controllers
{
    [Authorize]
    public class InboxController : Controller
    {
        private readonly ApplicationDbContext _context;

        // In-memory static store for demonstration & user interaction state across requests
        private static List<MessageItem>? _messageStore;
        private static readonly object _lock = new object();

        public InboxController(ApplicationDbContext context)
        {
            _context = context;
            EnsureMessagesInitialized();
        }

        // GET: /Inbox
        [HttpGet]
        public async Task<IActionResult> Index(string? folder, int? id, string? search)
        {
            string currentFolder = string.IsNullOrWhiteSpace(folder) ? "inbox" : folder.ToLower();

            List<MessageItem> all;
            lock (_lock)
            {
                all = _messageStore!.ToList();
            }

            // Filter by folder
            IEnumerable<MessageItem> filtered = currentFolder switch
            {
                "starred" => all.Where(m => m.IsStarred && !m.IsTrash),
                "sent" => all.Where(m => m.RecipientName != "Admin User" && !m.IsTrash),
                "trash" => all.Where(m => m.IsTrash),
                _ => all.Where(m => !m.IsTrash && m.RecipientName == "Admin User") // default inbox
            };

            if (!string.IsNullOrWhiteSpace(search))
            {
                string query = search.Trim().ToLower();
                filtered = filtered.Where(m => m.Subject.ToLower().Contains(query) ||
                                               m.SenderName.ToLower().Contains(query) ||
                                               m.BodyContent.ToLower().Contains(query));
            }

            var messageList = filtered.OrderByDescending(m => m.SentAt).ToList();

            // Auto-select first message or specified message id
            MessageItem? selectedMsg = null;
            if (id.HasValue)
            {
                selectedMsg = messageList.FirstOrDefault(m => m.MessageId == id.Value);
            }
            if (selectedMsg == null && messageList.Any())
            {
                selectedMsg = messageList.First();
            }

            if (selectedMsg != null)
            {
                lock (_lock)
                {
                    var storeMsg = _messageStore?.FirstOrDefault(m => m.MessageId == selectedMsg.MessageId);
                    if (storeMsg != null)
                    {
                        storeMsg.IsRead = true;
                        selectedMsg.IsRead = true;
                    }
                }
            }

            // Counters
            int unreadCount;
            int starredCount;
            int sentCount;
            int trashCount;

            lock (_lock)
            {
                unreadCount = _messageStore!.Count(m => !m.IsTrash && !m.IsRead && m.RecipientName == "Admin User");
                starredCount = _messageStore!.Count(m => !m.IsTrash && m.IsStarred);
                sentCount = _messageStore!.Count(m => !m.IsTrash && m.RecipientName != "Admin User");
                trashCount = _messageStore!.Count(m => m.IsTrash);
            }

            var availableUsers = await _context.Users.Where(u => u.IsActive).ToListAsync();

            var viewModel = new InboxViewModel
            {
                Messages = messageList,
                SelectedMessage = selectedMsg,
                SelectedFolder = currentFolder,
                UnreadCount = unreadCount,
                StarredCount = starredCount,
                SentCount = sentCount,
                TrashCount = trashCount,
                SearchTerm = search ?? string.Empty,
                AvailableUsers = availableUsers
            };

            return View(viewModel);
        }

        // POST: /Inbox/SendMessage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(string recipientEmail, string subject, string category, string priority, string body)
        {
            if (string.IsNullOrWhiteSpace(recipientEmail) || string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(body))
            {
                TempData["ErrorMessage"] = "Recipient, Subject, and Message Body are required.";
                return RedirectToAction(nameof(Index));
            }

            var recipientUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == recipientEmail || u.UserName == recipientEmail);
            string recipientName = recipientUser?.FullName ?? recipientEmail;

            lock (_lock)
            {
                int maxId = _messageStore!.Any() ? _messageStore!.Max(m => m.MessageId) + 1 : 1;
                var newMsg = new MessageItem
                {
                    MessageId = maxId,
                    SenderName = User.Identity?.Name ?? "Admin User",
                    SenderEmail = "admin@erp.com",
                    SenderRole = "Super Admin",
                    SenderAvatar = "/profile_images/admin-avatar.jpg",
                    RecipientName = recipientName,
                    RecipientEmail = recipientEmail,
                    Subject = subject.Trim(),
                    BodyContent = body.Trim(),
                    Category = string.IsNullOrWhiteSpace(category) ? "General" : category,
                    Priority = string.IsNullOrWhiteSpace(priority) ? "Normal" : priority,
                    SentAt = DateTime.Now,
                    IsRead = true,
                    IsStarred = false,
                    IsTrash = false
                };

                _messageStore!.Add(newMsg);
            }

            // Log activity
            _context.ActivityLogs.Add(new ActivityLog
            {
                Title = "Internal Message Sent",
                Description = $"Message '{subject}' sent to {recipientName} ({recipientEmail}).",
                IconClass = "fa-paper-plane",
                ColorClass = "text-primary",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Message sent successfully to {recipientName}.";
            return RedirectToAction(nameof(Index), new { folder = "sent" });
        }

        // POST: /Inbox/ToggleStar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStar(int id, string? folder)
        {
            lock (_lock)
            {
                var msg = _messageStore?.FirstOrDefault(m => m.MessageId == id);
                if (msg != null)
                {
                    msg.IsStarred = !msg.IsStarred;
                }
            }

            return RedirectToAction(nameof(Index), new { folder = folder ?? "inbox", id });
        }

        // POST: /Inbox/DeleteMessage/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteMessage(int id, string? folder)
        {
            lock (_lock)
            {
                var msg = _messageStore?.FirstOrDefault(m => m.MessageId == id);
                if (msg != null)
                {
                    if (msg.IsTrash)
                    {
                        _messageStore?.Remove(msg); // Permanent delete
                        TempData["SuccessMessage"] = "Message permanently deleted.";
                    }
                    else
                    {
                        msg.IsTrash = true; // Move to trash
                        TempData["SuccessMessage"] = "Message moved to Trash.";
                    }
                }
            }

            return RedirectToAction(nameof(Index), new { folder = folder ?? "inbox" });
        }

        private void EnsureMessagesInitialized()
        {
            lock (_lock)
            {
                if (_messageStore == null)
                {
                    _messageStore = new List<MessageItem>
                    {
                        new MessageItem
                        {
                            MessageId = 1,
                            SenderName = "Sarah Jenkins",
                            SenderEmail = "s.jenkins@erp.com",
                            SenderRole = "Finance Manager",
                            SenderAvatar = "/profile_images/admin-avatar.jpg",
                            RecipientName = "Admin User",
                            RecipientEmail = "admin@erp.com",
                            Subject = "Monthly Financial Audit Report - Q2 2026",
                            BodyContent = "Hello Admin,\n\nI have finalized the draft for the Q2 2026 financial audit and revenue reconciliation. All expense vouchers and accounts receivable figures match our ledgers. Please review the attached summary at your earliest convenience so we can archive this quarter.\n\nBest regards,\nSarah Jenkins",
                            Category = "Payroll",
                            SentAt = DateTime.Now.AddMinutes(-35),
                            IsRead = false,
                            IsStarred = true,
                            IsTrash = false,
                            Priority = "High"
                        },
                        new MessageItem
                        {
                            MessageId = 2,
                            SenderName = "Michael Scott",
                            SenderEmail = "m.scott@erp.com",
                            SenderRole = "Sales Director",
                            SenderAvatar = "/profile_images/admin-avatar.jpg",
                            RecipientName = "Admin User",
                            RecipientEmail = "admin@erp.com",
                            Subject = "New Enterprise Account Signed - Rahul Enterprises",
                            BodyContent = "Hi Team,\n\nWe successfully closed the deal with Rahul Enterprises today! The total contract value is ₹450,000 covering hardware inventory and software maintenance for 12 months. Sales invoice INV-10045 has been generated.\n\nCheers,\nMichael",
                            Category = "General",
                            SentAt = DateTime.Now.AddHours(-2),
                            IsRead = false,
                            IsStarred = false,
                            IsTrash = false,
                            Priority = "Normal"
                        },
                        new MessageItem
                        {
                            MessageId = 3,
                            SenderName = "System Operations Bot",
                            SenderEmail = "sysadmin@erp.com",
                            SenderRole = "Automated System",
                            SenderAvatar = "/profile_images/admin-avatar.jpg",
                            RecipientName = "Admin User",
                            RecipientEmail = "admin@erp.com",
                            Subject = "Scheduled Database Backup Completed Successfully",
                            BodyContent = "Automated System Notification:\n\nNightly database backup job completed on 17-Aug-2026 at 03:00:00 UTC. Total compressed archive size: 48.2 MB. No errors encountered during validation check.",
                            Category = "System",
                            SentAt = DateTime.Now.AddHours(-5),
                            IsRead = false,
                            IsStarred = false,
                            IsTrash = false,
                            Priority = "Low"
                        },
                        new MessageItem
                        {
                            MessageId = 4,
                            SenderName = "David Miller",
                            SenderEmail = "d.miller@erp.com",
                            SenderRole = "HR Manager",
                            SenderAvatar = "/profile_images/admin-avatar.jpg",
                            RecipientName = "Admin User",
                            RecipientEmail = "admin@erp.com",
                            Subject = "New Staff Onboarding Checklist for August",
                            BodyContent = "Good afternoon,\n\n3 new team members have completed their onboarding forms and system credential provisioning. Please ensure their role permissions are updated in the Page Permissions matrix.\n\nThanks,\nDavid",
                            Category = "Task",
                            SentAt = DateTime.Now.AddDays(-1),
                            IsRead = true,
                            IsStarred = true,
                            IsTrash = false,
                            Priority = "Normal"
                        }
                    };
                }
            }
        }
    }
}
