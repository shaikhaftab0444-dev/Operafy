    using Microsoft.AspNetCore.Authorization;
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
        public class NotificationsController : Controller
        {
            private readonly ApplicationDbContext _context;

            // In-memory static store for demonstration & user interaction state across requests
            private static List<NotificationItem>? _notificationStore;
            private static readonly object _lock = new object();

            public NotificationsController(ApplicationDbContext context)
            {
                _context = context;
                EnsureNotificationsInitialized();
            }

            // GET: /Notifications
            [HttpGet]
            public async Task<IActionResult> Index(string? category, string? status)
            {
                // Sync with latest ActivityLogs from DB
                await SyncWithActivityLogsAsync();

                List<NotificationItem> list;
                lock (_lock)
                {
                    list = _notificationStore!.ToList();
                }

                if (!string.IsNullOrWhiteSpace(category))
                {
                    list = list.Where(n => n.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (status == "unread")
                {
                    list = list.Where(n => !n.IsRead).ToList();
                }

                int unreadCount;
                int totalCount;
                lock (_lock)
                {
                    unreadCount = _notificationStore!.Count(n => !n.IsRead);
                    totalCount = _notificationStore!.Count;
                }

                var viewModel = new NotificationViewModel
                {
                    Notifications = list.OrderByDescending(n => n.CreatedAt).ToList(),
                    UnreadCount = unreadCount,
                    TotalCount = totalCount,
                    SelectedCategory = category ?? string.Empty,
                    FilterStatus = status ?? "all"
                };

                return View(viewModel);
            }

            // POST: /Notifications/MarkAsRead/5
            [HttpPost]
            [ValidateAntiForgeryToken]
            public IActionResult MarkAsRead(int id)
            {
                lock (_lock)
                {
                    var item = _notificationStore?.FirstOrDefault(n => n.NotificationId == id);
                    if (item != null)
                    {
                        item.IsRead = true;
                    }
                }

                TempData["SuccessMessage"] = "Notification marked as read.";
                return RedirectToAction(nameof(Index));
            }

            // POST: /Notifications/MarkAllAsRead
            [HttpPost]
            [ValidateAntiForgeryToken]
            public IActionResult MarkAllAsRead()
            {
                lock (_lock)
                {
                    if (_notificationStore != null)
                    {
                        foreach (var item in _notificationStore)
                        {
                            item.IsRead = true;
                        }
                    }
                }

                TempData["SuccessMessage"] = "All notifications marked as read.";
                return RedirectToAction(nameof(Index));
            }

            // POST: /Notifications/ClearNotification/5
            [HttpPost]
            [ValidateAntiForgeryToken]
            public IActionResult ClearNotification(int id)
            {
                lock (_lock)
                {
                    if (_notificationStore != null)
                    {
                        _notificationStore.RemoveAll(n => n.NotificationId == id);
                    }
                }

                TempData["SuccessMessage"] = "Notification cleared.";
                return RedirectToAction(nameof(Index));
            }

            private void EnsureNotificationsInitialized()
            {
                lock (_lock)
                {
                    if (_notificationStore == null)
                    {
                        _notificationStore = new List<NotificationItem>
                        {
                            new NotificationItem
                            {
                                NotificationId = 1,
                                Title = "New Sales Invoice Generated",
                                Description = "Invoice INV-10045 created for Rahul Enterprises (₹25,000).",
                                Category = "Sales",
                                CreatedAt = DateTime.Now.AddMinutes(-12),
                                IsRead = false,
                                IconClass = "fa-file-invoice-dollar",
                                ColorClass = "text-primary",
                                BgColorClass = "bg-primary-subtle",
                                TargetUrl = "/Sales"
                            },
                            new NotificationItem
                            {
                                NotificationId = 2,
                                Title = "Low Stock Alert: Smartphone",
                                Description = "Product Smartphone stock quantity fell below reorder threshold (60 units remaining).",
                                Category = "Inventory",
                                CreatedAt = DateTime.Now.AddHours(-1),
                                IsRead = false,
                                IconClass = "fa-triangle-exclamation",
                                ColorClass = "text-warning",
                                BgColorClass = "bg-warning-subtle",
                                TargetUrl = "/Inventory"
                            },
                            new NotificationItem
                            {
                                NotificationId = 3,
                                Title = "New Purchase Order Created",
                                Description = "Purchase Order PO-10023 issued to Sharma Suppliers for ₹18,500.",
                                Category = "Purchase",
                                CreatedAt = DateTime.Now.AddHours(-2),
                                IsRead = false,
                                IconClass = "fa-cart-shopping",
                                ColorClass = "text-success",
                                BgColorClass = "bg-success-subtle",
                                TargetUrl = "/Purchase"
                            },
                            new NotificationItem
                            {
                                NotificationId = 4,
                                Title = "New Employee Registered",
                                Description = "John Doe joined as Senior Software Engineer under Engineering Branch.",
                                Category = "HR",
                                CreatedAt = DateTime.Now.AddHours(-4),
                                IsRead = false,
                                IconClass = "fa-user-plus",
                                ColorClass = "text-info",
                                BgColorClass = "bg-info-subtle",
                                TargetUrl = "/EmployeeManagement"
                            },
                            new NotificationItem
                            {
                                NotificationId = 5,
                                Title = "Expense Voucher Approval Needed",
                                Description = "Voucher EXP-10012 for Office Supplies (₹2,500) requires manager approval.",
                                Category = "Finance",
                                CreatedAt = DateTime.Now.AddDays(-1),
                                IsRead = false,
                                IconClass = "fa-receipt",
                                ColorClass = "text-danger",
                                BgColorClass = "bg-danger-subtle",
                                TargetUrl = "/Expense"
                            }
                        };
                    }
                }
            }

            private async Task SyncWithActivityLogsAsync()
            {
                try
                {
                    var logs = await _context.ActivityLogs.OrderByDescending(a => a.CreatedAt).Take(5).ToListAsync();
                    lock (_lock)
                    {
                        if (_notificationStore != null)
                        {
                            foreach (var log in logs)
                            {
                                if (!_notificationStore.Any(n => n.Title == log.Title && n.Description == log.Description))
                                {
                                    int maxId = _notificationStore.Any() ? _notificationStore.Max(n => n.NotificationId) + 1 : 1;
                                    _notificationStore.Add(new NotificationItem
                                    {
                                        NotificationId = maxId,
                                        Title = log.Title,
                                        Description = log.Description,
                                        Category = "System",
                                        CreatedAt = log.CreatedAt,
                                        IsRead = false,
                                        IconClass = string.IsNullOrEmpty(log.IconClass) ? "fa-info-circle" : log.IconClass,
                                        ColorClass = string.IsNullOrEmpty(log.ColorClass) ? "text-primary" : log.ColorClass,
                                        BgColorClass = "bg-primary-subtle",
                                        TargetUrl = "/Dashboard"
                                    });
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Fallback gracefully if database table query encounters non-critical issue
                }
            }
        }
    }
