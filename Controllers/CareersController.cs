using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ERP_System.Data;
using ERP_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

namespace ERP_System.Controllers
{
    [AllowAnonymous]
    public class CareersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CareersController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: /Careers
        [HttpGet]
        public async Task<IActionResult> Index(string? search, int? departmentId, string? workMode)
        {
            var query = _context.JobOpenings
                .Include(j => j.Department)
                .Include(j => j.Designation)
                .Where(j => j.Status == "Open")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var clean = search.Trim().ToLower();
                query = query.Where(j => j.JobTitle.ToLower().Contains(clean) ||
                                         j.JobCode.ToLower().Contains(clean) ||
                                         (j.RequiredSkills != null && j.RequiredSkills.ToLower().Contains(clean)) ||
                                         (j.JobLocation != null && j.JobLocation.ToLower().Contains(clean)));
            }

            if (departmentId.HasValue && departmentId.Value > 0)
            {
                query = query.Where(j => j.DepartmentId == departmentId.Value);
            }

            if (!string.IsNullOrWhiteSpace(workMode) && workMode != "All")
            {
                query = query.Where(j => j.WorkMode == workMode);
            }

            var openJobs = await query.OrderByDescending(j => j.PostingDate).ThenByDescending(j => j.JobId).ToListAsync();

            ViewBag.Departments = await _context.Departments.Where(d => d.IsActive).OrderBy(d => d.DepartmentName).ToListAsync();
            ViewBag.Search = search;
            ViewBag.SelectedDepartmentId = departmentId;
            ViewBag.SelectedWorkMode = workMode;

            var company = await _context.Companies.FirstOrDefaultAsync() ?? new Company
            {
                CompanyName = "Operafy ERP Systems",
                City = "New Delhi",
                Country = "India"
            };
            ViewBag.Company = company;

            return View(openJobs);
        }

        // GET: /Careers/Apply/{id}
        [HttpGet]
        public async Task<IActionResult> Apply(int id)
        {
            var job = await _context.JobOpenings
                .Include(j => j.Department)
                .Include(j => j.Designation)
                .FirstOrDefaultAsync(j => j.JobId == id && j.Status == "Open");

            if (job == null)
            {
                TempData["ErrorMessage"] = "The requested job opening is either closed or does not exist.";
                return RedirectToAction(nameof(Index));
            }

            var company = await _context.Companies.FirstOrDefaultAsync() ?? new Company
            {
                CompanyName = "Operafy ERP Systems",
                City = "New Delhi",
                Country = "India"
            };
            ViewBag.Company = company;

            return View(job);
        }

        // POST: /Careers/SubmitApplication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitApplication(
            int jobOpeningId,
            string candidateName,
            string email,
            string phone,
            string experience,
            string? address,
            string? education,
            string? skills,
            string? currentCompany,
            decimal? currentSalary,
            decimal? expectedSalary,
            string? noticePeriod,
            string? linkedIn,
            string? portfolio,
            IFormFile? resume)
        {
            var job = await _context.JobOpenings
                .Include(j => j.Department)
                .FirstOrDefaultAsync(j => j.JobId == jobOpeningId && j.Status == "Open");

            if (job == null)
            {
                TempData["ErrorMessage"] = "Job opening not found or is no longer accepting applications.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(candidateName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(phone))
            {
                TempData["ErrorMessage"] = "Please fill in all required personal information.";
                return RedirectToAction(nameof(Apply), new { id = jobOpeningId });
            }

            if (resume == null || resume.Length == 0)
            {
                TempData["ErrorMessage"] = "Please attach a valid PDF or Word (DOC/DOCX) resume.";
                return RedirectToAction(nameof(Apply), new { id = jobOpeningId });
            }

            // Allowed extensions
            var ext = Path.GetExtension(resume.FileName).ToLowerInvariant();
            var allowedExts = new[] { ".pdf", ".doc", ".docx", ".rtf" };
            if (!allowedExts.Contains(ext))
            {
                TempData["ErrorMessage"] = "Invalid resume format. Allowed formats: PDF, DOC, DOCX.";
                return RedirectToAction(nameof(Apply), new { id = jobOpeningId });
            }

            // 1. Save Resume File
            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "resumes");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString().Substring(0, 8)}_{Path.GetFileName(resume.FileName)}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await resume.CopyToAsync(fileStream);
            }

            string resumeRelativePath = $"/uploads/resumes/{uniqueFileName}";

            // 2. Find or Create Candidate
            var cleanEmail = email.Trim().ToLower();
            var candidate = await _context.Candidates.FirstOrDefaultAsync(c => c.Email.ToLower() == cleanEmail);
            if (candidate == null)
            {
                candidate = new Candidate
                {
                    FullName = candidateName.Trim(),
                    Email = cleanEmail,
                    Phone = phone.Trim(),
                    Address = address?.Trim(),
                    Education = education?.Trim() ?? "Bachelor's Degree",
                    Experience = experience?.Trim() ?? "1-3 Years",
                    Skills = skills?.Trim() ?? job.RequiredSkills,
                    CurrentCompany = currentCompany?.Trim(),
                    CurrentSalary = currentSalary,
                    ExpectedSalary = expectedSalary,
                    NoticePeriod = noticePeriod?.Trim() ?? "Immediate",
                    ResumePath = resumeRelativePath,
                    LinkedIn = linkedIn?.Trim(),
                    Portfolio = portfolio?.Trim(),
                    ApplicationSource = "Public Careers Portal",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Candidates.Add(candidate);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Update candidate info with latest application data
                candidate.FullName = candidateName.Trim();
                candidate.Phone = phone.Trim();
                if (!string.IsNullOrWhiteSpace(address)) candidate.Address = address.Trim();
                if (!string.IsNullOrWhiteSpace(education)) candidate.Education = education.Trim();
                if (!string.IsNullOrWhiteSpace(experience)) candidate.Experience = experience.Trim();
                if (!string.IsNullOrWhiteSpace(skills)) candidate.Skills = skills.Trim();
                if (!string.IsNullOrWhiteSpace(currentCompany)) candidate.CurrentCompany = currentCompany.Trim();
                if (currentSalary.HasValue) candidate.CurrentSalary = currentSalary;
                if (expectedSalary.HasValue) candidate.ExpectedSalary = expectedSalary;
                if (!string.IsNullOrWhiteSpace(noticePeriod)) candidate.NoticePeriod = noticePeriod.Trim();
                if (!string.IsNullOrWhiteSpace(linkedIn)) candidate.LinkedIn = linkedIn.Trim();
                if (!string.IsNullOrWhiteSpace(portfolio)) candidate.Portfolio = portfolio.Trim();
                candidate.ResumePath = resumeRelativePath;
                candidate.UpdatedAt = DateTime.UtcNow;
                _context.Candidates.Update(candidate);
                await _context.SaveChangesAsync();
            }

            // 3. Create Candidate Application
            var application = new CandidateApplication
            {
                CandidateId = candidate.CandidateId,
                JobId = jobOpeningId,
                ApplicationDate = DateTime.UtcNow,
                Stage = "Applied",
                Status = "Active",
                MatchScore = 85,
                Notes = $"Applied via Careers Portal on {DateTime.UtcNow:dd-MMM-yyyy HH:mm} UTC.",
                CreatedAt = DateTime.UtcNow
            };
            _context.CandidateApplications.Add(application);
            await _context.SaveChangesAsync();

            // 4. Record Stage History
            var stageHistory = new CandidateStageHistory
            {
                ApplicationId = application.ApplicationId,
                PreviousStage = "None",
                NewStage = "Applied",
                ChangeDate = DateTime.UtcNow,
                ReasonNotes = "Direct application received from public career portal."
            };
            _context.CandidateStageHistories.Add(stageHistory);

            // 5. System Notification & Activity Log
            var notif = new SystemNotification
            {
                UserId = 1, // Admin / HR
                Title = $"New Application: {candidate.FullName}",
                Message = $"{candidate.FullName} applied for '{job.JobTitle}' ({job.JobCode}).",
                Category = "HR",
                TargetUrl = $"/HRRecruitment/CandidatePipeline?jobId={job.JobId}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.SystemNotifications.Add(notif);

            _context.ActivityLogs.Add(new ActivityLog
            {
                Title = "New Job Application Received",
                Description = $"{candidate.FullName} ({candidate.Email}) applied for '{job.JobTitle}'.",
                IconClass = "fa-user-check",
                ColorClass = "text-success",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            ViewBag.CandidateName = candidate.FullName;
            ViewBag.JobTitle = job.JobTitle;
            ViewBag.JobCode = job.JobCode;
            ViewBag.CompanyName = (await _context.Companies.FirstOrDefaultAsync())?.CompanyName ?? "Operafy ERP Systems";

            return View("ApplicationSuccess", job);
        }

        // GET: /Careers/ReviewOffer
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ReviewOffer(string offerCode)
        {
            var offer = await _context.OfferLetters
                .Include(o => o.Application)
                    .ThenInclude(a => a.Candidate)
                .Include(o => o.Application)
                    .ThenInclude(a => a.JobOpening)
                        .ThenInclude(j => j.Department)
                .FirstOrDefaultAsync(o => o.OfferCode == offerCode);

            if (offer == null) return NotFound("Invalid or expired offer link.");
            return View(offer);
        }

        // POST: /Careers/AcceptOfferAction
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> AcceptOfferAction(
            string offerCode, 
            bool accept, 
            [FromServices] IHubContext<ERP_System.Hubs.ErpNotificationHub> hubContext)
        {
            var offer = await _context.OfferLetters
                .Include(o => o.Application)
                    .ThenInclude(a => a.Candidate)
                .FirstOrDefaultAsync(o => o.OfferCode == offerCode);

            if (offer == null) return Json(new { success = false, message = "Offer not found." });

            offer.Status = accept ? "Accepted" : "Declined";
            offer.AcceptedDate = DateTime.UtcNow;

            if (offer.Application != null)
            {
                offer.Application.Stage = accept ? "Offer Accepted" : "Offer Declined";
            }

            await _context.SaveChangesAsync();

            // PUSH REAL-TIME SIGNALR BROADCAST TO HR (NO PAGE REFRESH NEEDED)
            await hubContext.Clients.All.SendAsync("OfferStatusChanged", new
            {
                offerId = offer.OfferId,
                offerCode = offer.OfferCode,
                candidateName = offer.Application?.Candidate?.FullName,
                status = offer.Status
            });

            return Json(new { success = true, status = offer.Status, message = accept ? "Thank you! You have accepted the offer." : "You have declined the offer." });
        }
    }
}
