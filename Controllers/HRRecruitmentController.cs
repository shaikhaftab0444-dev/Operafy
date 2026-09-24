using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using ERP_System.Data;
using ERP_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP_System.Controllers
{
    [Authorize(Roles = "Super Admin,Admin,HR,Recruiter,Hiring Manager")]
    public class HRRecruitmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ERP_System.Services.IEmailSenderService _emailSender;

        public HRRecruitmentController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, ERP_System.Services.IEmailSenderService emailSender)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(claim, out int userId)) return userId;
            return 1; // Default to Admin
        }

        // ==========================================
        // 1. JOB OPENINGS PAGE & ACTIONS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> JobOpenings(string? search, int? departmentId, string? status, string? employmentType)
        {
            var query = _context.JobOpenings
                .Include(j => j.Department)
                .Include(j => j.Designation)
                .Include(j => j.HiringManager)
                .Include(j => j.Recruiter)
                .Include(j => j.Applications)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(j => j.JobTitle.ToLower().Contains(search) || j.JobCode.ToLower().Contains(search));
            }

            if (departmentId.HasValue && departmentId.Value > 0)
            {
                query = query.Where(j => j.DepartmentId == departmentId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(j => j.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(employmentType) && employmentType != "All")
            {
                query = query.Where(j => j.EmploymentType == employmentType);
            }

            var jobs = await query.OrderByDescending(j => j.JobId).ToListAsync();

            ViewBag.Departments = await _context.Departments.Where(d => d.IsActive).ToListAsync();
            ViewBag.Designations = await _context.Designations.Where(d => d.IsActive).ToListAsync();
            ViewBag.Users = await _context.Users.Where(u => u.IsActive).ToListAsync();
            ViewBag.Search = search;
            ViewBag.SelectedDepartmentId = departmentId;
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedEmploymentType = employmentType;

            return View(jobs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateJobOpening(JobOpening job)
        {
            if (string.IsNullOrWhiteSpace(job.JobCode))
            {
                int count = await _context.JobOpenings.CountAsync();
                job.JobCode = $"JOB-{(1021 + count)}";
            }

            // Check duplicate job code
            if (await _context.JobOpenings.AnyAsync(j => j.JobCode == job.JobCode))
            {
                TempData["ErrorMessage"] = $"Job Code '{job.JobCode}' already exists. Please use a unique Job Code.";
                return RedirectToAction(nameof(JobOpenings));
            }

            job.CreatedAt = DateTime.Now;
            job.CreatedBy = GetCurrentUserId();
            job.PostingDate = job.PostingDate == default ? DateTime.Now : job.PostingDate;

            _context.JobOpenings.Add(job);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Job Opening '{job.JobTitle}' ({job.JobCode}) created successfully!";
            return RedirectToAction(nameof(JobOpenings));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditJobOpening(JobOpening job)
        {
            var existing = await _context.JobOpenings.FindAsync(job.JobId);
            if (existing != null)
            {
                existing.JobTitle = job.JobTitle;
                existing.DepartmentId = job.DepartmentId;
                existing.DesignationId = job.DesignationId;
                existing.HiringManagerId = job.HiringManagerId;
                existing.RecruiterId = job.RecruiterId;
                existing.EmploymentType = job.EmploymentType;
                existing.Vacancies = job.Vacancies;
                existing.JobLocation = job.JobLocation;
                existing.WorkMode = job.WorkMode;
                existing.ExperienceRequired = job.ExperienceRequired;
                existing.MinimumEducation = job.MinimumEducation;
                existing.RequiredSkills = job.RequiredSkills;
                existing.JobDescription = job.JobDescription;
                existing.Responsibilities = job.Responsibilities;
                existing.Requirements = job.Requirements;
                existing.MinimumSalary = job.MinimumSalary;
                existing.MaximumSalary = job.MaximumSalary;
                existing.Currency = job.Currency;
                existing.ClosingDate = job.ClosingDate;
                existing.Status = job.Status;
                existing.UpdatedAt = DateTime.Now;

                _context.JobOpenings.Update(existing);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Job Opening '{existing.JobTitle}' updated successfully!";
            }
            return RedirectToAction(nameof(JobOpenings));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DuplicateJobOpening(int jobId)
        {
            var existing = await _context.JobOpenings.FindAsync(jobId);
            if (existing != null)
            {
                int count = await _context.JobOpenings.CountAsync();
                var duplicate = new JobOpening
                {
                    JobCode = $"JOB-{(1021 + count)}",
                    JobTitle = $"{existing.JobTitle} (Copy)",
                    DepartmentId = existing.DepartmentId,
                    DesignationId = existing.DesignationId,
                    HiringManagerId = existing.HiringManagerId,
                    RecruiterId = existing.RecruiterId,
                    EmploymentType = existing.EmploymentType,
                    Vacancies = existing.Vacancies,
                    JobLocation = existing.JobLocation,
                    WorkMode = existing.WorkMode,
                    ExperienceRequired = existing.ExperienceRequired,
                    MinimumEducation = existing.MinimumEducation,
                    RequiredSkills = existing.RequiredSkills,
                    JobDescription = existing.JobDescription,
                    Responsibilities = existing.Responsibilities,
                    Requirements = existing.Requirements,
                    MinimumSalary = existing.MinimumSalary,
                    MaximumSalary = existing.MaximumSalary,
                    Currency = existing.Currency,
                    PostingDate = DateTime.Now,
                    Status = "Draft",
                    CreatedAt = DateTime.Now,
                    CreatedBy = GetCurrentUserId()
                };

                _context.JobOpenings.Add(duplicate);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Job Opening duplicated as '{duplicate.JobTitle}' ({duplicate.JobCode})!";
            }
            return RedirectToAction(nameof(JobOpenings));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeJobStatus(int jobId, string status)
        {
            var job = await _context.JobOpenings.FindAsync(jobId);
            if (job != null)
            {
                job.Status = status;
                job.UpdatedAt = DateTime.Now;
                _context.JobOpenings.Update(job);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Job Opening status changed to '{status}'.";
            }
            return RedirectToAction(nameof(JobOpenings));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteJobOpening(int jobId)
        {
            var job = await _context.JobOpenings.Include(j => j.Applications).FirstOrDefaultAsync(j => j.JobId == jobId);
            if (job != null)
            {
                if (job.Applications.Any())
                {
                    TempData["ErrorMessage"] = $"Cannot delete Job Opening '{job.JobTitle}' because it has {job.Applications.Count} attached applicant(s). You can archive it instead.";
                    return RedirectToAction(nameof(JobOpenings));
                }

                _context.JobOpenings.Remove(job);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Job Opening deleted successfully!";
            }
            return RedirectToAction(nameof(JobOpenings));
        }

        // ==========================================
        // 2. CANDIDATE PIPELINE PAGE & ACTIONS
        // ==========================================

        [HttpGet]
        [Authorize(Roles = "HR,Super Admin,Admin")]
        public async Task<IActionResult> CandidatePipeline(string search = "", int? jobOpeningId = null, int? jobId = null, string stage = "All", string status = "All")
        {
            ViewBag.JobOpenings = await _context.JobOpenings.Where(j => j.Status == "Open" || j.Status == "On Hold").ToListAsync();
            ViewBag.AllJobs = await _context.JobOpenings.ToListAsync();

            var query = _context.CandidateApplications
                .Include(a => a.Candidate)
                .Include(a => a.JobOpening)
                    .ThenInclude(j => j!.Department)
                .Include(a => a.StageHistories)
                    .ThenInclude(h => h.ChangedByUser)
                .Include(a => a.Interviews)
                .Include(a => a.Offers)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(a => (a.Candidate != null && (a.Candidate.FullName.ToLower().Contains(term) ||
                                                                   a.Candidate.Email.ToLower().Contains(term) ||
                                                                   a.Candidate.Phone.Contains(term))) ||
                                         (a.JobOpening != null && a.JobOpening.JobTitle.ToLower().Contains(term)));
            }

            int targetJobId = jobOpeningId ?? jobId ?? 0;
            if (targetJobId > 0)
            {
                query = query.Where(a => a.JobId == targetJobId);
            }

            if (!string.IsNullOrEmpty(stage) && stage != "All" && stage != "All Pipeline Stages")
            {
                query = query.Where(a => a.Stage == stage);
            }

            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                query = query.Where(a => a.Status == status);
            }

            var candidates = await query.OrderByDescending(a => a.ApplicationDate).ThenByDescending(a => a.ApplicationId).ToListAsync();

            ViewBag.SelectedJobOpeningId = targetJobId;
            ViewBag.SelectedJobId = targetJobId;
            ViewBag.SelectedStage = stage;
            ViewBag.SelectedStatus = status;
            ViewBag.Search = search;

            // Return Partial View if called via live AJAX filter
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CandidateTableRowsPartial", candidates);
            }

            return View(candidates);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HR,Super Admin,Admin")]
        public async Task<IActionResult> AddCandidate(
            int? jobOpeningId,
            int? jobId,
            string? candidateName,
            string? email,
            string? phone,
            string? experience,
            string? educationQualification,
            string? education,
            string? currentCompany,
            string? initialStage,
            Candidate? candidate,
            IFormFile? resume,
            IFormFile? resumeFile)
        {
            int targetJobId = (jobOpeningId.HasValue && jobOpeningId.Value > 0) ? jobOpeningId.Value : (jobId ?? 0);
            if (targetJobId <= 0)
            {
                TempData["ErrorMessage"] = "Please select a valid Job Opening.";
                return RedirectToAction(nameof(CandidatePipeline));
            }

            string name = !string.IsNullOrWhiteSpace(candidateName) ? candidateName.Trim() : (candidate?.FullName ?? "Unknown Candidate");
            string mail = !string.IsNullOrWhiteSpace(email) ? email.Trim() : (candidate?.Email ?? "");
            string ph = !string.IsNullOrWhiteSpace(phone) ? phone.Trim() : (candidate?.Phone ?? "");
            string exp = !string.IsNullOrWhiteSpace(experience) ? experience.Trim() : (!string.IsNullOrWhiteSpace(candidate?.Experience) ? candidate.Experience : "Fresher / Entry Level");
            string edu = !string.IsNullOrWhiteSpace(educationQualification) ? educationQualification.Trim() : (!string.IsNullOrWhiteSpace(education) ? education.Trim() : (candidate?.Education ?? "Bachelor's Degree"));
            string comp = !string.IsNullOrWhiteSpace(currentCompany) ? currentCompany.Trim() : (candidate?.CurrentCompany ?? "");

            var upload = resume ?? resumeFile;
            string resumePath = "/uploads/resumes/default_resume.pdf";

            if (upload != null && upload.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "resumes");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(upload.FileName)}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await upload.CopyToAsync(stream);
                }
                resumePath = $"/uploads/resumes/{uniqueFileName}";
            }

            var cand = await _context.Candidates.FirstOrDefaultAsync(c => c.Email.ToLower() == mail.ToLower());
            if (cand == null)
            {
                cand = new Candidate
                {
                    FullName = name,
                    Email = mail,
                    Phone = ph,
                    Experience = exp,
                    Education = edu,
                    CurrentCompany = comp,
                    ResumePath = resumePath,
                    ApplicationSource = "Internal / HR Recruiter",
                    CreatedAt = DateTime.Now
                };
                _context.Candidates.Add(cand);
                await _context.SaveChangesAsync();
            }
            else
            {
                cand.FullName = name;
                cand.Phone = ph;
                cand.Experience = exp;
                cand.Education = edu;
                cand.CurrentCompany = comp;
                if (upload != null && upload.Length > 0)
                {
                    cand.ResumePath = resumePath;
                }
                _context.Candidates.Update(cand);
                await _context.SaveChangesAsync();
            }

            string stage = string.IsNullOrWhiteSpace(initialStage) ? "Applied" : initialStage;
            var app = new CandidateApplication
            {
                CandidateId = cand.CandidateId,
                JobId = targetJobId,
                ApplicationDate = DateTime.Now,
                Stage = stage,
                Status = "Active",
                MatchScore = 85,
                Notes = "Candidate logged into the talent pipeline by HR.",
                CreatedAt = DateTime.Now
            };

            _context.CandidateApplications.Add(app);
            await _context.SaveChangesAsync();

            _context.CandidateStageHistories.Add(new CandidateStageHistory
            {
                ApplicationId = app.ApplicationId,
                PreviousStage = "None",
                NewStage = stage,
                ChangedByUserId = GetCurrentUserId(),
                ChangeDate = DateTime.Now,
                ReasonNotes = "Initial candidate application registered in talent pipeline"
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Candidate {name} successfully logged into the talent pipeline.";
            return RedirectToAction(nameof(CandidatePipeline));
        }

        [HttpPost]
        [Authorize(Roles = "HR,Super Admin,Admin")]
        public async Task<IActionResult> UpdateCandidateStage(int? candidateId, int? applicationId, string stage)
        {
            int targetId = applicationId ?? candidateId ?? 0;
            var app = await _context.CandidateApplications
                .Include(a => a.Candidate)
                .FirstOrDefaultAsync(a => a.ApplicationId == targetId || a.CandidateId == targetId);

            if (app == null) return Json(new { success = false, message = "Candidate not found." });

            string oldStage = app.Stage;
            app.Stage = stage;
            if (stage == "Rejected")
            {
                app.Status = "Rejected";
            }
            else if (stage == "On Hold")
            {
                app.Status = "On Hold";
            }
            else
            {
                app.Status = "Active";
            }
            app.UpdatedAt = DateTime.Now;

            _context.CandidateApplications.Update(app);
            _context.CandidateStageHistories.Add(new CandidateStageHistory
            {
                ApplicationId = app.ApplicationId,
                PreviousStage = oldStage,
                NewStage = stage,
                ChangedByUserId = GetCurrentUserId(),
                ChangeDate = DateTime.Now,
                ReasonNotes = $"Candidate stage updated to {stage} via Talent Pipeline."
            });

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"Candidate stage updated to {stage}." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HR,Super Admin,Admin")]
        public async Task<IActionResult> ChangeCandidateStage(int applicationId, string newStage, string? reasonNotes)
        {
            var app = await _context.CandidateApplications.Include(a => a.Candidate).FirstOrDefaultAsync(a => a.ApplicationId == applicationId);
            if (app != null && app.Stage != newStage)
            {
                string oldStage = app.Stage;
                app.Stage = newStage;
                app.UpdatedAt = DateTime.Now;

                _context.CandidateApplications.Update(app);
                _context.CandidateStageHistories.Add(new CandidateStageHistory
                {
                    ApplicationId = app.ApplicationId,
                    PreviousStage = oldStage,
                    NewStage = newStage,
                    ChangedByUserId = GetCurrentUserId(),
                    ChangeDate = DateTime.Now,
                    ReasonNotes = reasonNotes ?? $"Stage moved from {oldStage} to {newStage}"
                });

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Candidate '{app.Candidate?.FullName}' moved to stage '{newStage}'.";
            }
            return RedirectToAction(nameof(CandidatePipeline));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HR,Super Admin,Admin")]
        public async Task<IActionResult> ChangeCandidateStatus(int applicationId, string status, string? reasonNotes)
        {
            var app = await _context.CandidateApplications.Include(a => a.Candidate).FirstOrDefaultAsync(a => a.ApplicationId == applicationId);
            if (app != null)
            {
                app.Status = status;
                app.UpdatedAt = DateTime.Now;

                _context.CandidateApplications.Update(app);
                _context.CandidateStageHistories.Add(new CandidateStageHistory
                {
                    ApplicationId = app.ApplicationId,
                    PreviousStage = app.Stage,
                    NewStage = $"{app.Stage} ({status})",
                    ChangedByUserId = GetCurrentUserId(),
                    ChangeDate = DateTime.Now,
                    ReasonNotes = reasonNotes ?? $"Status updated to {status}"
                });

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Candidate status updated to '{status}'.";
            }
            return RedirectToAction(nameof(CandidatePipeline));
        }

        // ==========================================
        // 3. INTERVIEW SCHEDULES PAGE & ACTIONS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> InterviewSchedules(int? jobId, string? status, string? search, int? candidateId)
        {
            var query = _context.InterviewSchedules
                .Include(i => i.Candidate)
                .Include(i => i.JobOpening)
                .Include(i => i.Application)
                .Include(i => i.Interviewer)
                .Include(i => i.Feedbacks)
                .ThenInclude(f => f.Interviewer)
                .AsQueryable();

            if (jobId.HasValue && jobId.Value > 0)
            {
                query = query.Where(i => i.JobId == jobId.Value);
            }

            if (candidateId.HasValue && candidateId.Value > 0)
            {
                query = query.Where(i => i.CandidateId == candidateId.Value || i.ApplicationId == candidateId.Value);
                ViewBag.SelectedCandidateId = candidateId.Value;
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(i => i.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(i => i.Candidate!.FullName.ToLower().Contains(search) ||
                                         i.JobOpening!.JobTitle.ToLower().Contains(search) ||
                                         (i.InterviewerNames != null && i.InterviewerNames.ToLower().Contains(search)));
            }

            var interviews = await query.OrderByDescending(i => i.InterviewId).ToListAsync();

            ViewBag.ActiveApplications = await _context.CandidateApplications
                .Include(a => a.Candidate)
                .Include(a => a.JobOpening)
                .Where(a => a.Status == "Active")
                .ToListAsync();

            ViewBag.Interviewers = await _context.Users
                .Where(u => u.IsActive && u.Role != null && (u.Role.RoleName == "HR" || u.Role.RoleName == "Admin" || u.Role.RoleName == "Super Admin" || u.Role.RoleName.Contains("Manager") || u.Role.RoleName.Contains("Lead")))
                .OrderBy(u => u.FullName)
                .Select(u => new { Id = u.UserId, DisplayName = $"{u.FullName} ({(string.IsNullOrEmpty(u.DepartmentName) ? "Staff" : u.DepartmentName)})" })
                .ToListAsync();
            ViewBag.JobOpenings = await _context.JobOpenings.ToListAsync();
            ViewBag.SelectedJobId = jobId;
            ViewBag.SelectedStatus = status;
            ViewBag.Search = search;

            return View(interviews);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HR,Super Admin,Admin")]
        public async Task<IActionResult> CreateInterview(int applicationId, string? interviewRound, string? interviewType, string? interviewMode, DateTime scheduledDate, string? startTime, string? endTime, int? interviewerId, string? meetingLink, string? location, string? notes)
        {
            var app = await _context.CandidateApplications.Include(a => a.Candidate).FirstOrDefaultAsync(a => a.ApplicationId == applicationId);
            if (app == null)
            {
                TempData["ErrorMessage"] = "Candidate application not found.";
                return RedirectToAction(nameof(InterviewSchedules));
            }

            string interviewerName = "";
            if (interviewerId.HasValue && interviewerId.Value > 0)
            {
                var interviewerUser = await _context.Users.FindAsync(interviewerId.Value);
                if (interviewerUser != null) interviewerName = interviewerUser.FullName;
            }

            string safeInterviewType = interviewType ?? interviewRound ?? "General";
            string safeInterviewRound = interviewRound ?? "First Round";

            // Parse HTML5 time input (24-hour format like "14:30") to display format ("02:30 PM")
            string formattedStartTime = "10:00 AM";
            if (!string.IsNullOrWhiteSpace(startTime) && DateTime.TryParse(startTime, out DateTime st))
            {
                formattedStartTime = st.ToString("hh:mm tt");
            }
            
            string formattedEndTime = "11:00 AM";
            if (!string.IsNullOrWhiteSpace(endTime) && DateTime.TryParse(endTime, out DateTime et))
            {
                formattedEndTime = et.ToString("hh:mm tt");
            }

            var schedule = new InterviewSchedule
            {
                ApplicationId = app.ApplicationId,
                CandidateId = app.CandidateId,
                JobId = app.JobId,
                InterviewRound = safeInterviewRound,
                InterviewType = safeInterviewType,
                InterviewMode = interviewMode ?? "Virtual",
                ScheduledDate = scheduledDate,
                StartTime = formattedStartTime,
                EndTime = formattedEndTime,
                InterviewerId = interviewerId,
                InterviewerNames = interviewerName,
                MeetingLink = meetingLink,
                Location = location,
                Notes = notes,
                Status = "Scheduled",
                CreatedAt = DateTime.Now
            };

            _context.InterviewSchedules.Add(schedule);

            // Auto-update candidate pipeline status
            app.Stage = "Interview Scheduled";
            _context.CandidateApplications.Update(app);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Interview for '{app.Candidate?.FullName}' scheduled successfully!";
            
            // 1. Send Email to Candidate Automatically
            if (!string.IsNullOrEmpty(app.Email) && !string.IsNullOrEmpty(meetingLink))
            {
                var result = await _emailSender.SendInterviewInvitationAsync(
                    app.Email,
                    app.CandidateName,
                    app.JobOpening?.JobTitle ?? "Open Role",
                    safeInterviewRound,
                    scheduledDate.ToString("dd MMM yyyy"),
                    $"{schedule.StartTime} - {schedule.EndTime}",
                    meetingLink,
                    schedule.InterviewerNames ?? "HR Team"
                );
                
                if (result.Success)
                {
                    TempData["SuccessMessage"] = $"Interview scheduled for '{app.CandidateName}' and invite dispatched via Gmail SMTP!";
                }
                else
                {
                    TempData["SuccessMessage"] = $"Interview scheduled for '{app.CandidateName}' but invite failed: {result.Message}";
                }
            }

            return RedirectToAction(nameof(InterviewSchedules));
        }

        [HttpPost]
        [Authorize(Roles = "HR,Super Admin,Admin")]
        public async Task<IActionResult> ResendInterviewEmail(int scheduleId)
        {
            var s = await _context.InterviewSchedules
                .Include(i => i.Application)
                    .ThenInclude(a => a.JobOpening)
                .Include(i => i.Application)
                    .ThenInclude(a => a.Candidate)
                .FirstOrDefaultAsync(i => i.InterviewId == scheduleId);

            if (s == null) return Json(new { success = false, message = "Schedule not found." });

            if (string.IsNullOrWhiteSpace(s.Application?.Email))
            {
                return Json(new { success = false, message = "Candidate has no valid email address." });
            }

            var result = await _emailSender.SendInterviewInvitationAsync(
                s.Application?.Email ?? "",
                s.Application?.CandidateName ?? "",
                s.Application?.JobOpening?.JobTitle ?? "Open Role",
                s.InterviewRound,
                s.ScheduledDate.ToString("dd MMM yyyy"),
                $"{s.StartTime} - {s.EndTime}",
                s.MeetingLink ?? "#",
                s.InterviewerNames ?? "HR Team"
            );

            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        [Authorize(Roles = "HR,Super Admin,Admin")]
        public async Task<IActionResult> SendStageUpdateEmail(int scheduleId)
        {
            var s = await _context.InterviewSchedules
                .Include(i => i.Application)
                    .ThenInclude(a => a.JobOpening)
                .Include(i => i.Application)
                    .ThenInclude(a => a.Candidate)
                .FirstOrDefaultAsync(i => i.InterviewId == scheduleId);

            if (s == null) return Json(new { success = false, message = "Schedule not found." });

            if (string.IsNullOrWhiteSpace(s.Application?.Email))
            {
                return Json(new { success = false, message = "Candidate has no valid email address." });
            }

            var result = await _emailSender.SendStageUpdateEmailAsync(
                s.Application?.Email ?? "",
                s.Application?.CandidateName ?? "",
                s.Application?.JobOpening?.JobTitle ?? "Open Role",
                s.Application?.Stage ?? "Review"
            );

            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HR,Super Admin,Admin,Interviewer")]
        public async Task<IActionResult> SubmitInterviewFeedback(
            int scheduleId,
            int technicalSkills,
            int communication,
            int problemSolving,
            int culturalFit,
            string overallRecommendation,
            string? keyStrengths,
            string? weaknesses,
            string? detailedRemarks)
        {
            var schedule = await _context.InterviewSchedules
                .Include(s => s.Application)
                    .ThenInclude(a => a.JobOpening)
                .FirstOrDefaultAsync(s => s.InterviewId == scheduleId);

            if (schedule == null) return NotFound();

            // Calculate score
            decimal avgRating = (technicalSkills + communication + problemSolving + culturalFit) / 4.0m;

            var feedback = new InterviewFeedback
            {
                InterviewId = schedule.InterviewId,
                CandidateId = schedule.CandidateId,
                JobId = schedule.JobId,
                InterviewerId = schedule.InterviewerId,
                TechnicalRating = technicalSkills,
                CommunicationRating = communication,
                ProblemSolvingRating = problemSolving,
                CulturalFitRating = culturalFit,
                OverallRating = avgRating,
                Strengths = keyStrengths,
                Weaknesses = weaknesses,
                Comments = detailedRemarks,
                Recommendation = overallRecommendation,
                SubmittedAt = DateTime.Now
            };

            _context.InterviewFeedbacks.Add(feedback);

            schedule.Status = "Completed";

            // Advance Candidate Pipeline based on Recommendation
            if (schedule.Application != null)
            {
                if (overallRecommendation.Contains("Recommend", StringComparison.OrdinalIgnoreCase) && 
                    !overallRecommendation.Contains("Not", StringComparison.OrdinalIgnoreCase))
                {
                    schedule.Application.Stage = "Shortlisted for Offer";
                }
                else
                {
                    schedule.Application.Stage = "Interview Rejected";
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Feedback recorded for {schedule.Application?.CandidateName}. Pipeline stage updated.";
            return RedirectToAction("InterviewSchedules");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RescheduleInterview(int interviewId, DateTime newDate, string startTime, string endTime)
        {
            var interview = await _context.InterviewSchedules.FindAsync(interviewId);
            if (interview != null)
            {
                interview.ScheduledDate = newDate;
                interview.StartTime = startTime;
                interview.EndTime = endTime;
                interview.Status = "Rescheduled";
                interview.UpdatedAt = DateTime.Now;

                _context.InterviewSchedules.Update(interview);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Interview rescheduled successfully!";
            }
            return RedirectToAction(nameof(InterviewSchedules));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeInterviewStatus(int interviewId, string status)
        {
            var interview = await _context.InterviewSchedules.FindAsync(interviewId);
            if (interview != null)
            {
                interview.Status = status;
                interview.UpdatedAt = DateTime.Now;

                _context.InterviewSchedules.Update(interview);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Interview status updated to '{status}'.";
            }
            return RedirectToAction(nameof(InterviewSchedules));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitInterviewFeedback(int interviewId, int technicalRating, int communicationRating, int experienceRating, int problemSolvingRating, int culturalFitRating, string recommendation, string? strengths, string? weaknesses, string? comments)
        {
            var interview = await _context.InterviewSchedules.Include(i => i.Application).FirstOrDefaultAsync(i => i.InterviewId == interviewId);
            if (interview == null)
            {
                TempData["ErrorMessage"] = "Interview record not found.";
                return RedirectToAction(nameof(InterviewSchedules));
            }

            decimal overall = (decimal)(technicalRating + communicationRating + experienceRating + problemSolvingRating + culturalFitRating) / 5.0m;

            var feedback = new InterviewFeedback
            {
                InterviewId = interview.InterviewId,
                CandidateId = interview.CandidateId,
                JobId = interview.JobId,
                InterviewerId = GetCurrentUserId(),
                TechnicalRating = technicalRating,
                CommunicationRating = communicationRating,
                ExperienceRating = experienceRating,
                ProblemSolvingRating = problemSolvingRating,
                CulturalFitRating = culturalFitRating,
                OverallRating = Math.Round(overall, 2),
                Recommendation = recommendation,
                Strengths = strengths,
                Weaknesses = weaknesses,
                Comments = comments,
                IsFinalized = true,
                SubmittedAt = DateTime.Now
            };

            _context.InterviewFeedbacks.Add(feedback);
            interview.Status = "Completed";
            interview.UpdatedAt = DateTime.Now;
            _context.InterviewSchedules.Update(interview);

            // Auto advance candidate if recommendation is strongly recommend / recommend
            if (recommendation.Contains("Recommend") && interview.Application != null)
            {
                if (recommendation == "Strongly Recommend" || recommendation == "Recommend")
                {
                    if (interview.InterviewRound.Contains("Final"))
                    {
                        interview.Application.Stage = "Selected";
                    }
                    _context.CandidateApplications.Update(interview.Application);
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Feedback submitted successfully! Overall Rating: {Math.Round(overall, 2)} / 5.0";
            return RedirectToAction(nameof(InterviewSchedules));
        }

        // ==========================================
        // 4. OFFER LETTERS PAGE & ACTIONS
        // ==========================================

        [HttpGet("HRRecruitment/RunTempSql")]
        [AllowAnonymous]
        public async Task<IActionResult> RunTempSql()
        {
            try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE AITStudent.erp_OfferLetters ADD SignaturePath nvarchar(500) NULL"); } catch {}
            try { await _context.Database.ExecuteSqlRawAsync("ALTER TABLE AITStudent.erp_OfferLetters ADD AcceptedDate datetime2 NULL"); } catch {}
            return Content("Success");
        }

        [HttpGet]
        [Authorize(Roles = "HR,Super Admin,Admin")]
        public async Task<IActionResult> OfferLetters(string search = "", int? candidateId = null, string status = "All", DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.OfferLetters
                .Include(o => o.Application)
                    .ThenInclude(a => a.Candidate)
                .Include(o => o.Application)
                    .ThenInclude(a => a.JobOpening)
                .Include(o => o.ReportingManager)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(o => o.OfferCode.ToLower().Contains(term) ||
                                         (o.Application != null && o.Application.Candidate != null && o.Application.Candidate.FullName.ToLower().Contains(term)) ||
                                         (o.Application != null && o.Application.Candidate != null && o.Application.Candidate.Email.ToLower().Contains(term)) ||
                                         (o.Application != null && o.Application.JobOpening != null && o.Application.JobOpening.JobTitle.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "All" && status != "All Offer Statuses")
            {
                query = query.Where(o => o.Status == status);
            }

            // Calendar Date Filters
            if (fromDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt.Date >= fromDate.Value.Date);
            }
            if (toDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt.Date <= toDate.Value.Date);
            }

            var offers = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_OfferLettersTablePartial", offers);
            }

            // 1. Only consider candidates who have ACTIVE or ACCEPTED offers as blocked.
            // If an offer is 'Withdrawn', 'Rejected', or 'Declined', candidate can receive a new offer.
            var activeOfferedCandidateIds = await _context.OfferLetters
                .Where(o => o.Status == "Pending" || o.Status == "Sent" || o.Status == "Approved" || o.Status == "Draft" || o.Status == "Accepted" || o.Status == "Converted to Employee")
                .Select(o => o.ApplicationId)
                .ToListAsync();

            var rawApplications = await _context.CandidateApplications
                .Include(c => c.Candidate)
                .Include(c => c.JobOpening)
                    .ThenInclude(j => j.Department)
                .Where(c => !activeOfferedCandidateIds.Contains(c.ApplicationId) && c.Stage != "Rejected" && c.Stage != "Interview Rejected")
                .OrderByDescending(c => c.ApplicationId)
                .ToListAsync();

            var eligibleCandidates = rawApplications
                .Select(c => new
                {
                    Id = c.ApplicationId,
                    DisplayName = $"{(c.Candidate != null ? c.Candidate.FullName : "Candidate")} - {(c.JobOpening != null ? c.JobOpening.JobTitle : "Role")} (Stage: {c.Stage})",
                    CandidateName = c.Candidate != null ? c.Candidate.FullName : string.Empty,
                    Email = c.Candidate != null ? c.Candidate.Email : string.Empty,
                    JobTitle = c.JobOpening != null ? c.JobOpening.JobTitle : string.Empty,
                    Department = c.JobOpening?.Department != null ? c.JobOpening.Department.DepartmentName : string.Empty,
                    MinSalary = c.JobOpening != null ? c.JobOpening.MinimumSalary : 0,
                    MaxSalary = c.JobOpening != null ? c.JobOpening.MaximumSalary : 0
                })
                .ToList();

            ViewBag.EligibleCandidates = eligibleCandidates;
            ViewBag.SelectedCandidateId = candidateId;

            ViewBag.Managers = await _context.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.FullName)
                .Select(u => new { Id = u.UserId, FullName = u.FullName })
                .ToListAsync();

            ViewBag.JobOpenings = await _context.JobOpenings.Where(j => j.Status == "Open" || j.Status == "Draft").ToListAsync();

            return View(offers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HR,Super Admin,Admin")]
        public async Task<IActionResult> GenerateOfferLetter(
            int jobApplicationId,
            decimal annualCtc,
            DateTime joiningDate,
            DateTime offerExpiryDate,
            int reportingManagerId,
            string? termsAndConditions,
            IFormFile? signatureImage)
        {
            var app = await _context.CandidateApplications
                .Include(a => a.Candidate)
                .Include(a => a.JobOpening)
                    .ThenInclude(j => j.Department)
                .FirstOrDefaultAsync(a => a.ApplicationId == jobApplicationId);

            if (app == null) return NotFound();

            int nextCount = await _context.OfferLetters.CountAsync() + 1;
            string offerCode = $"OFF-2026-{nextCount:D3}";

            // Handle Signature Image Upload
            string? signaturePath = null;
            if (signatureImage != null && signatureImage.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "signatures");
                Directory.CreateDirectory(uploadsFolder);
                string uniqueName = $"{Guid.NewGuid()}_{Path.GetFileName(signatureImage.FileName)}";
                string fullPath = Path.Combine(uploadsFolder, uniqueName);
                using (var fs = new FileStream(fullPath, FileMode.Create))
                {
                    await signatureImage.CopyToAsync(fs);
                }
                signaturePath = $"/uploads/signatures/{uniqueName}";
            }

            decimal monthlyGross = Math.Round(annualCtc / 12m, 2);
            decimal basicSalary = Math.Round(monthlyGross * 0.50m, 2);
            decimal hra = Math.Round(monthlyGross * 0.25m, 2);
            decimal specialAllowance = monthlyGross - (basicSalary + hra);
            string salaryStructure = $"Basic Salary: ₹{basicSalary:N2}, HRA: ₹{hra:N2}, Special Allowance: ₹{specialAllowance:N2}";

            var offer = new OfferLetter
            {
                ApplicationId = app.ApplicationId,
                CandidateId = app.CandidateId,
                JobId = app.JobId,
                DesignationId = app.JobOpening?.DesignationId,
                DepartmentId = app.JobOpening?.DepartmentId,
                OfferCode = offerCode,
                OfferedCTC = annualCtc,
                SalaryStructure = salaryStructure,
                ProposedJoiningDate = joiningDate,
                OfferExpiryDate = offerExpiryDate,
                ReportingManagerId = reportingManagerId,
                SignaturePath = signaturePath,
                TermsAndConditions = termsAndConditions ?? "Standard employment contract terms, probation period of 3 months applies.",
                Status = "Pending", // Candidate will review and accept
                CreatedAt = DateTime.UtcNow
            };

            _context.OfferLetters.Add(offer);
            app.Stage = "Offered";
            await _context.SaveChangesAsync();

            // Generate Candidate Direct Review & Acceptance Link
            string reviewLink = $"{Request.Scheme}://{Request.Host}/Careers/ReviewOffer?offerCode={offerCode}";

            // Dispatch Offer Email via Gmail SMTP
            string emailBody = $@"
<div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.05);"">
    <div style=""background: linear-gradient(135deg, #1e3a8a, #3b82f6); padding: 25px; text-align: center; color: white;"">
        <h2 style=""margin: 0; font-size: 24px; font-weight: bold;"">Official Employment Offer</h2>
        <p style=""margin: 5px 0 0; opacity: 0.9;"">Wainfo Pvt Ltd • Human Resources</p>
    </div>
    <div style=""padding: 30px; background-color: #ffffff; color: #334155;"">
        <p style=""font-size: 16px; margin-top: 0;"">Dear <strong>{app.Candidate?.FullName}</strong>,</p>
        <p style=""line-height: 1.6;"">We are delighted to extend a formal job offer for the position of <strong>{app.JobOpening?.JobTitle}</strong> at Wainfo Pvt Ltd. Our panel was thoroughly impressed by your credentials.</p>
        
        <table style=""width: 100%; border-collapse: collapse; margin: 25px 0; background-color: #f8fafc; border-radius: 8px;"">
            <tr><td style=""padding: 12px 15px; border-bottom: 1px solid #e2e8f0; color: #64748b; width: 40%;"">Reference Code:</td><td style=""padding: 12px 15px; border-bottom: 1px solid #e2e8f0; font-weight: bold;"">{offerCode}</td></tr>
            <tr><td style=""padding: 12px 15px; border-bottom: 1px solid #e2e8f0; color: #64748b;"">Offered CTC:</td><td style=""padding: 12px 15px; border-bottom: 1px solid #e2e8f0; font-weight: bold; color: #16a34a;"">₹{annualCtc:N0} / Annum</td></tr>
            <tr><td style=""padding: 12px 15px; border-bottom: 1px solid #e2e8f0; color: #64748b;"">Proposed Joining Date:</td><td style=""padding: 12px 15px; border-bottom: 1px solid #e2e8f0; font-weight: bold;"">{joiningDate:dd MMMM yyyy}</td></tr>
            <tr><td style=""padding: 12px 15px; color: #64748b;"">Offer Expiry Date:</td><td style=""padding: 12px 15px; font-weight: bold; color: #dc2626;"">{offerExpiryDate:dd MMMM yyyy}</td></tr>
        </table>

        <div style=""text-align: center; margin: 35px 0;"">
            <a href=""{reviewLink}"" style=""background-color: #2563eb; color: white; padding: 14px 28px; text-decoration: none; border-radius: 8px; font-weight: bold; font-size: 16px; display: inline-block;"">Review & Accept Official Offer</a>
        </div>
        
        <p style=""font-size: 13px; color: #64748b; text-align: center;"">Direct Portal Link:<br/><a href=""{reviewLink}"" style=""color: #3b82f6;"">{reviewLink}</a></p>
    </div>
    <div style=""background-color: #f1f5f9; padding: 15px; text-align: center; font-size: 12px; color: #94a3b8;"">
        Wainfo Pvt Ltd • Automated Recruitment Management System • Confidential
    </div>
</div>";

            try
            {
                using var mail = new MailMessage("affuxx00@gmail.com", app.Candidate?.Email ?? "")
                {
                    Subject = $"Job Offer Letter: {app.JobOpening?.JobTitle} - Wainfo Pvt Ltd",
                    Body = emailBody,
                    IsBodyHtml = true
                };
                using var smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("affuxx00@gmail.com", "jblkicpealbwskrk"),
                    EnableSsl = true
                };
                await smtp.SendMailAsync(mail);
            }
            catch { /* Log failure gracefully */ }

            TempData["SuccessMessage"] = $"Offer letter {offerCode} generated for {app.Candidate?.FullName} and dispatched with Review & Acceptance portal link!";
            return RedirectToAction(nameof(OfferLetters));
        }

        [HttpPost]
        [Authorize(Roles = "HR,Super Admin,Admin")]
        public async Task<IActionResult> UpdateOfferStatus(int offerId, string status)
        {
            var offer = await _context.OfferLetters
                .Include(o => o.Application)
                .FirstOrDefaultAsync(o => o.OfferId == offerId);

            if (offer == null) return Json(new { success = false, message = "Offer not found." });

            offer.Status = status;
            if (offer.Application != null)
            {
                offer.Application.Stage = status == "Accepted" ? "Offer Accepted" : "Offer Rejected";
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"Offer status updated to {status}." });
        }

        // 1-Click Convert Accepted Candidate to Real Employee
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "HR,Super Admin,Admin")]
        public async Task<IActionResult> ConvertToEmployee(int offerId)
        {
            var offer = await _context.OfferLetters
                .Include(o => o.Application)
                    .ThenInclude(a => a.JobOpening)
                .Include(o => o.Candidate)
                .FirstOrDefaultAsync(o => o.OfferId == offerId);

            if (offer == null || offer.Status != "Accepted")
            {
                TempData["ErrorMessage"] = "Only accepted offers can be converted to an active employee.";
                return RedirectToAction("OfferLetters");
            }

            if (offer.ConvertedToEmployeeId.HasValue)
            {
                TempData["ErrorMessage"] = "Candidate has already been converted to an Employee!";
                return RedirectToAction(nameof(OfferLetters));
            }

            string empCode = $"USR{new Random().Next(100, 999)}";
            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
            
            var user = new User
            {
                UserName = empCode, // Emp code as username
                UserCode = empCode,
                Email = offer.Candidate!.Email,
                FullName = offer.Candidate.FullName,
                MobileNumber = offer.Candidate.Phone,
                DepartmentId = offer.Application?.JobOpening?.DepartmentId ?? 1,
                CompanyId = 1,
                BranchId = 1,
                RoleId = 5, // Default Employee
                JoiningDate = offer.ProposedJoiningDate,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            user.PasswordHash = hasher.HashPassword(user, "Employee@123");

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            offer.ConvertedToEmployeeId = user.UserId;
            offer.Status = "Converted to Employee";
            if (offer.Application != null) offer.Application.Stage = "Hired";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Successfully converted {user.FullName} into employee ({user.UserName})!";
            return RedirectToAction(nameof(OfferLetters));
        }

        // Action to Withdraw / Revoke Offer
        [HttpPost]
        [Authorize(Roles = "HR,Super Admin,Admin")]
        public async Task<IActionResult> RevokeOffer(int offerId)
        {
            var offer = await _context.OfferLetters
                .Include(o => o.Application)
                .FirstOrDefaultAsync(o => o.OfferId == offerId);

            if (offer == null) return Json(new { success = false, message = "Offer not found." });

            offer.Status = "Withdrawn";
            if (offer.Application != null)
            {
                offer.Application.Stage = "Offer Withdrawn";
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"Offer {offer.OfferCode} has been withdrawn. Candidate is now eligible for a revised offer." });
        }

        // Action to Resend Offer Email via Gmail SMTP
        [HttpPost]
        [Authorize(Roles = "HR,Super Admin,Admin")]
        public async Task<IActionResult> ResendOfferEmail(int offerId)
        {
            var offer = await _context.OfferLetters
                .Include(o => o.Application)
                    .ThenInclude(a => a.Candidate)
                .Include(o => o.Application)
                    .ThenInclude(a => a.JobOpening)
                .FirstOrDefaultAsync(o => o.OfferId == offerId);

            if (offer == null || offer.Application == null || offer.Application.Candidate == null)
            {
                return Json(new { success = false, message = "Offer record not found." });
            }

            string reviewLink = $"{Request.Scheme}://{Request.Host}/Careers/ReviewOffer?offerCode={offer.OfferCode}";

            string emailHtml = $@"
<div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.05);"">
    <div style=""background: linear-gradient(135deg, #1e3a8a, #3b82f6); padding: 25px; text-align: center; color: white;"">
        <h2 style=""margin: 0; font-size: 24px; font-weight: bold;"">Official Employment Offer</h2>
        <p style=""margin: 5px 0 0; opacity: 0.9;"">Wainfo Pvt Ltd • Human Resources</p>
    </div>
    <div style=""padding: 30px; background-color: #ffffff; color: #334155;"">
        <p style=""font-size: 16px; margin-top: 0;"">Dear <strong>{offer.Application.Candidate.FullName}</strong>,</p>
        <p style=""line-height: 1.6;"">We are delighted to extend a formal job offer for the position of <strong>{offer.Application.JobOpening?.JobTitle}</strong> at Wainfo Pvt Ltd. Our panel was thoroughly impressed by your credentials.</p>
        
        <table style=""width: 100%; border-collapse: collapse; margin: 25px 0; background-color: #f8fafc; border-radius: 8px;"">
            <tr><td style=""padding: 12px 15px; border-bottom: 1px solid #e2e8f0; color: #64748b; width: 40%;"">Reference Code:</td><td style=""padding: 12px 15px; border-bottom: 1px solid #e2e8f0; font-weight: bold;"">{offer.OfferCode}</td></tr>
            <tr><td style=""padding: 12px 15px; border-bottom: 1px solid #e2e8f0; color: #64748b;"">Offered CTC:</td><td style=""padding: 12px 15px; border-bottom: 1px solid #e2e8f0; font-weight: bold; color: #16a34a;"">₹{offer.OfferedCTC:N0} / Annum</td></tr>
            <tr><td style=""padding: 12px 15px; border-bottom: 1px solid #e2e8f0; color: #64748b;"">Proposed Joining Date:</td><td style=""padding: 12px 15px; border-bottom: 1px solid #e2e8f0; font-weight: bold;"">{offer.ProposedJoiningDate:dd MMMM yyyy}</td></tr>
            <tr><td style=""padding: 12px 15px; color: #64748b;"">Offer Expiry Date:</td><td style=""padding: 12px 15px; font-weight: bold; color: #dc2626;"">{offer.OfferExpiryDate:dd MMMM yyyy}</td></tr>
        </table>

        <div style=""text-align: center; margin: 35px 0;"">
            <a href=""{reviewLink}"" style=""background-color: #2563eb; color: white; padding: 14px 28px; text-decoration: none; border-radius: 8px; font-weight: bold; font-size: 16px; display: inline-block;"">Review & Accept Official Offer</a>
        </div>
        
        <p style=""font-size: 13px; color: #64748b; text-align: center;"">Direct Portal Link:<br/><a href=""{reviewLink}"" style=""color: #3b82f6;"">{reviewLink}</a></p>
    </div>
    <div style=""background-color: #f1f5f9; padding: 15px; text-align: center; font-size: 12px; color: #94a3b8;"">
        Wainfo Pvt Ltd • Automated Recruitment Management System • Confidential
    </div>
</div>";

            try
            {
                using var mail = new System.Net.Mail.MailMessage("affuxx00@gmail.com", offer.Application.Candidate.Email)
                {
                    Subject = $"Reminder: Job Offer Letter ({offer.OfferCode}) - Wainfo Pvt Ltd",
                    Body = emailHtml,
                    IsBodyHtml = true
                };
                using var smtp = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new System.Net.NetworkCredential("affuxx00@gmail.com", "jblkicpealbwskrk"),
                    EnableSsl = true
                };
                await smtp.SendMailAsync(mail);
                return Json(new { success = true, message = $"Offer email resent successfully to {offer.Application.Candidate.Email}!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Failed to send email: {ex.Message}" });
            }
        }

        [HttpGet]
        [Authorize(Roles = "HR,Super Admin,Admin")]
        public async Task<IActionResult> PrintOffer(int offerId)
        {
            var offer = await _context.OfferLetters
                .Include(o => o.Candidate)
                .Include(o => o.JobOpening)
                .Include(o => o.Department)
                .Include(o => o.Designation)
                .Include(o => o.ReportingManager)
                .FirstOrDefaultAsync(o => o.OfferId == offerId);

            if (offer == null) return NotFound();

            string? userEmail = User.FindFirstValue(ClaimTypes.Email);
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            var company = await _context.Companies
                .FirstOrDefaultAsync(c => currentUser != null && c.CompanyId == currentUser.CompanyId)
                ?? await _context.Companies.FirstOrDefaultAsync();

            var branch = await _context.Branches
                .FirstOrDefaultAsync(b => currentUser != null && b.BranchId == currentUser.BranchId)
                ?? await _context.Branches.FirstOrDefaultAsync(b => company != null && b.CompanyId == company.CompanyId);

            ViewBag.Company = company;
            ViewBag.Branch = branch;

            return View("PrintOffer", offer);
        }
    }
}
