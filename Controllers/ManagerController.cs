using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using ERP_System.Models;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,Manager")]
    public class ManagerController : Controller
    {
        // GET: /Manager
        [HttpGet]
        public IActionResult Index()
        {
            var vm = GetPopulatedManagerVM();
            return View(vm);
        }

        // GET: /Manager/Approvals
        [HttpGet]
        public IActionResult Approvals(string category = "All")
        {
            var model = GetPendingApprovalsList();
            if (!string.IsNullOrEmpty(category) && category != "All")
            {
                model = model.Where(x => x.CategoryKey == category).ToList();
            }
            ViewBag.ActiveCategory = category;
            return View(model);
        }

        // POST: /Manager/ProcessApproval
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult ProcessApproval(int id, string actionType, string remarks = "")
        {
            return Json(new { 
                success = true, 
                id = id, 
                action = actionType, 
                message = $"Request #{id} successfully {actionType.ToLower()}ed!" 
            });
        }

        // POST: /Manager/BulkApprove
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult BulkApprove([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return Json(new { success = false, message = "No requests selected." });
            }
            return Json(new { success = true, count = ids.Count, message = $"{ids.Count} requests approved successfully!" });
        }

        // GET: /Manager/DownloadReceipt
        [HttpGet]
        public IActionResult DownloadReceipt(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                filename = "Receipt.pdf";
            }

            // Simple valid PDF stream format
            string pdfContent = "%PDF-1.4\n" +
                               "1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n" +
                               "2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n" +
                               "3 0 obj\n<< /Type /Page /Parent 2 0 R /Resources << /Font << /F1 << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >> >> >> /MediaBox [0 0 612 792] /Contents 4 0 R >>\nendobj\n" +
                               "4 0 obj\n<< /Length 120 >>\nstream\nBT\n/F1 20 Tf\n70 700 Td\n(OPERAFY ERP SYSTEM) Tj\n/F1 12 Tf\n0 -30 Td\n(Receipt Attachment: " + filename + ") Tj\n0 -20 Td\n(Status: Verified & Audited) Tj\nET\nendstream\nendobj\n" +
                               "xref\n0 5\n0000000000 65535 f\n0000000009 00000 n\n0000000056 00000 n\n0000000111 00000 n\n0000000253 00000 n\n" +
                               "trailer\n<< /Size 5 >>\nstartxref\n424\n%%EOF";

            byte[] pdfBytes = System.Text.Encoding.ASCII.GetBytes(pdfContent);
            return File(pdfBytes, "application/pdf", filename);
        }

        // GET: /Manager/Tasks
        [HttpGet]
        public IActionResult Tasks()
        {
            var vm = GetPopulatedManagerVM();
            return View(vm);
        }

        // GET: /Manager/Team
        [HttpGet]
        public IActionResult Team()
        {
            var vm = GetPopulatedManagerVM();
            return View(vm);
        }

        // POST: /Manager/AssignTask
        [HttpPost]
        public IActionResult AssignTask(string title, string assignedTo, string priority, DateTime dueDate, string description)
        {
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(assignedTo))
            {
                return Json(new { success = false, message = "Deliverable title and assigned member are required." });
            }

            return Json(new { 
                success = true, 
                message = "Task assigned successfully to " + assignedTo,
                task = new {
                    title = title,
                    assignedTo = assignedTo,
                    priority = priority,
                    dueDate = dueDate.ToString("dd MMM yyyy")
                }
            });
        }

        // POST: /Manager/ApproveLeave
        [HttpPost]
        public IActionResult ApproveLeave(int id, string actionType)
        {
            string statusMessage = actionType == "Approve" ? "approved" : "rejected";
            return Json(new { success = true, message = $"Leave request #{id} marked as {statusMessage}." });
        }

        private ManagerDashboardViewModel GetPopulatedManagerVM()
        {
            return new ManagerDashboardViewModel
            {
                TotalTeamCount = 6,
                PresentTodayCount = 5,
                PendingApprovalsCount = 3,
                ActiveTasksCount = 8,
                DelayedTasksCount = 1,
                ProductivityRate = "94.2%",

                TeamAttendance = new List<TeamMemberStatus>
                {
                    new TeamMemberStatus { Name = "Numan Khan", Role = "Sales Executive", Status = "Present", ClockInTime = "08:58 AM", Avatar = "NK", StatusColor = "success" },
                    new TeamMemberStatus { Name = "Aftab Shaik", Role = "Senior Developer", Status = "Present", ClockInTime = "09:05 AM", Avatar = "AS", StatusColor = "success" },
                    new TeamMemberStatus { Name = "Sneha Patil", Role = "Operations Associate", Status = "On Leave", ClockInTime = "N/A", Avatar = "SP", StatusColor = "danger" },
                    new TeamMemberStatus { Name = "Rohan Sharma", Role = "Quality Analyst", Status = "Present", ClockInTime = "09:15 AM", Avatar = "RS", StatusColor = "success" },
                    new TeamMemberStatus { Name = "Zoya Malik", Role = "Backend Engineer", Status = "Late", ClockInTime = "09:42 AM", Avatar = "ZM", StatusColor = "warning" },
                    new TeamMemberStatus { Name = "Sameer Verma", Role = "UI/UX Designer", Status = "Present", ClockInTime = "09:00 AM", Avatar = "SV", StatusColor = "success" }
                },

                PendingApprovals = new List<ApprovalItem>
                {
                    new ApprovalItem { Id = 101, EmployeeName = "Sneha Patil", Type = "Casual Leave", Dates = "26 Aug - 27 Aug 2026 (2 Days)", Reason = "Family Medical Emergency", RequestedOn = "25 Aug 2026", Status = "Pending" },
                    new ApprovalItem { Id = 102, EmployeeName = "Zoya Malik", Type = "Attendance Regularization", Dates = "25 Aug 2026 (Morning)", Reason = "Biometric network sync timeout", RequestedOn = "26 Aug 2026", Status = "Pending" },
                    new ApprovalItem { Id = 103, EmployeeName = "Numan Khan", Type = "Travel Reimbursement", Dates = "Client Visit Pune", Reason = "Fuel & Toll Claim - INR 2,450", RequestedOn = "26 Aug 2026", Status = "Pending" }
                },

                TeamTasks = new List<ManagerTaskItem>
                {
                    new ManagerTaskItem { Id = 1, Title = "Finalize Q3 Client Billing Summary", AssignedTo = "Numan Khan", Priority = "Urgent", DueDate = "28 Aug 2026", Progress = 75, Status = "In Progress" },
                    new ManagerTaskItem { Id = 2, Title = "Resolve Payment Gateway Timeout Exception", AssignedTo = "Aftab Shaik", Priority = "High", DueDate = "27 Aug 2026", Progress = 90, Status = "Review" },
                    new ManagerTaskItem { Id = 3, Title = "Branch Inventory Stock Audit Reconciliation", AssignedTo = "Sneha Patil", Priority = "Medium", DueDate = "30 Aug 2026", Progress = 30, Status = "Delayed" },
                    new ManagerTaskItem { Id = 4, Title = "Prepare New Hire Onboarding Documentation", AssignedTo = "Rohan Sharma", Priority = "Low", DueDate = "31 Aug 2026", Progress = 50, Status = "In Progress" }
                }
            };
        }

        private List<ApprovalItemViewModel> GetPendingApprovalsList()
        {
            return new List<ApprovalItemViewModel>
            {
                new ApprovalItemViewModel {
                    Id = 101,
                    EmployeeName = "Sneha Patil",
                    Role = "Operations Associate",
                    Avatar = "SP",
                    ClaimCategory = "Casual Leave",
                    CategoryKey = "Leave",
                    Duration = "26 Aug – 27 Aug 2026 (2 Days)",
                    Reason = "Family Medical Emergency",
                    SubmittedDate = "25 Aug 2026, 04:30 PM",
                    HasAttachment = true,
                    AttachmentName = "Medical_Prescription.pdf",
                    Status = "Pending"
                },
                new ApprovalItemViewModel {
                    Id = 102,
                    EmployeeName = "Zoya Malik",
                    Role = "Backend Engineer",
                    Avatar = "ZM",
                    ClaimCategory = "Attendance Regularization",
                    CategoryKey = "Regularization",
                    Duration = "25 Aug 2026 (Morning Shift)",
                    Reason = "Biometric network sync timeout at entrance",
                    SubmittedDate = "26 Aug 2026, 09:30 AM",
                    HasAttachment = false,
                    Status = "Pending"
                },
                new ApprovalItemViewModel {
                    Id = 103,
                    EmployeeName = "Numan Khan",
                    Role = "Sales Executive",
                    Avatar = "NK",
                    ClaimCategory = "Travel Reimbursement",
                    CategoryKey = "Expense",
                    Duration = "Client Visit Pune",
                    Reason = "Fuel & Toll Claim - INR 2,450",
                    SubmittedDate = "26 Aug 2026, 11:15 AM",
                    HasAttachment = true,
                    AttachmentName = "Fuel_Toll_Receipts.pdf",
                    Status = "Pending"
                }
            };
        }
    }
}
