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
        public IActionResult Approvals()
        {
            var vm = GetPopulatedManagerVM();
            return View(vm);
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
    }
}
