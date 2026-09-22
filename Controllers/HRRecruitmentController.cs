using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            ViewBag.Interviewers = await _context.Users.Where(u => u.IsActive).ToListAsync();
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

            var schedule = new InterviewSchedule
            {
                ApplicationId = app.ApplicationId,
                CandidateId = app.CandidateId,
                JobId = app.JobId,
                InterviewRound = safeInterviewRound,
                InterviewType = safeInterviewType,
                InterviewMode = interviewMode ?? "Virtual",
                ScheduledDate = scheduledDate,
                StartTime = string.IsNullOrWhiteSpace(startTime) ? "10:00 AM" : startTime,
                EndTime = string.IsNullOrWhiteSpace(endTime) ? "11:00 AM" : endTime,
                InterviewerId = interviewerId,
                InterviewerNames = interviewerName,
                MeetingLink = meetingLink,
                Location = location,
                Notes = notes,
                Status = "Scheduled",
                CreatedAt = DateTime.Now
            };

            _context.InterviewSchedules.Add(schedule);

            // Move candidate stage if applicable
            if (safeInterviewType.Contains("Technical", StringComparison.OrdinalIgnoreCase) || safeInterviewRound.Contains("Technical", StringComparison.OrdinalIgnoreCase))
            {
                app.Stage = "Technical Interview";
            }
            else if (safeInterviewType.Contains("Manager", StringComparison.OrdinalIgnoreCase) || safeInterviewRound.Contains("Manager", StringComparison.OrdinalIgnoreCase))
            {
                app.Stage = "Manager Interview";
            }
            else if (safeInterviewType.Contains("Final", StringComparison.OrdinalIgnoreCase))
            {
                app.Stage = "Final Interview";
            }
            else
            {
                app.Stage = "HR Interview";
            }
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
                    TempData["SuccessMessage"] = $"Interview scheduled for '{app.CandidateName}' and invite dispatched via Mailtrap!";
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

        [HttpGet]
        public async Task<IActionResult> OfferLetters(int? jobId, string? status, string? search, int? candidateId)
        {
            var query = _context.OfferLetters
                .Include(o => o.Candidate)
                .Include(o => o.JobOpening)
                .Include(o => o.Department)
                .Include(o => o.Designation)
                .Include(o => o.ReportingManager)
                .Include(o => o.ConvertedEmployee)
                .AsQueryable();

            if (jobId.HasValue && jobId.Value > 0)
            {
                query = query.Where(o => o.JobId == jobId.Value);
            }

            if (candidateId.HasValue && candidateId.Value > 0)
            {
                query = query.Where(o => o.CandidateId == candidateId.Value || o.ApplicationId == candidateId.Value);
                ViewBag.SelectedCandidateId = candidateId.Value;
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(o => o.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(o => o.OfferCode.ToLower().Contains(search) ||
                                         o.Candidate!.FullName.ToLower().Contains(search) ||
                                         o.JobOpening!.JobTitle.ToLower().Contains(search));
            }

            var offers = await query.OrderByDescending(o => o.OfferId).ToListAsync();

            ViewBag.SelectedCandidates = await _context.CandidateApplications
                .Include(a => a.Candidate)
                .Include(a => a.JobOpening)
                .Where(a => a.Stage == "Selected" || a.Stage == "Final Interview" || a.Stage == "Offer Sent" || a.Stage == "Offer Accepted")
                .ToListAsync();

            ViewBag.JobOpenings = await _context.JobOpenings.ToListAsync();
            ViewBag.Designations = await _context.Designations.Where(d => d.IsActive).ToListAsync();
            ViewBag.Departments = await _context.Departments.Where(d => d.IsActive).ToListAsync();
            ViewBag.Managers = await _context.Users.Where(u => u.IsActive).ToListAsync();
            ViewBag.Roles = await _context.Roles.Where(r => r.RoleName != "Super Admin").ToListAsync();

            ViewBag.SelectedJobId = jobId;
            ViewBag.SelectedStatus = status;
            ViewBag.Search = search;

            return View(offers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOffer(int applicationId, decimal offeredCTC, DateTime proposedJoiningDate, DateTime offerExpiryDate, int? designationId, int? departmentId, int? reportingManagerId, string? employmentType, string? salaryStructure, string? termsAndConditions, string? additionalNotes)
        {
            var app = await _context.CandidateApplications.Include(a => a.Candidate).Include(a => a.JobOpening).FirstOrDefaultAsync(a => a.ApplicationId == applicationId);
            if (app == null)
            {
                TempData["ErrorMessage"] = "Candidate application not found.";
                return RedirectToAction(nameof(OfferLetters));
            }

            int count = await _context.OfferLetters.CountAsync();
            string offerCode = $"OFF-2026-{(count + 1).ToString("D3")}";

            var offer = new OfferLetter
            {
                OfferCode = offerCode,
                ApplicationId = app.ApplicationId,
                CandidateId = app.CandidateId,
                JobId = app.JobId,
                DesignationId = designationId ?? app.JobOpening?.DesignationId,
                DepartmentId = departmentId ?? app.JobOpening?.DepartmentId,
                EmploymentType = string.IsNullOrWhiteSpace(employmentType) ? "Full-Time" : employmentType,
                ProposedJoiningDate = proposedJoiningDate == default ? DateTime.Now.AddDays(15) : proposedJoiningDate,
                ReportingManagerId = reportingManagerId,
                OfferedCTC = offeredCTC,
                SalaryStructure = string.IsNullOrWhiteSpace(salaryStructure) ? $"Basic Salary: ₹{offeredCTC * 0.50m:N0}, HRA: ₹{offeredCTC * 0.25m:N0}, Special Allowance: ₹{offeredCTC * 0.25m:N0}" : salaryStructure,
                OfferExpiryDate = offerExpiryDate == default ? DateTime.Now.AddDays(7) : offerExpiryDate,
                TermsAndConditions = string.IsNullOrWhiteSpace(termsAndConditions) ? "1. Probation period: 90 days.\n2. Confidentiality agreement applies.\n3. Background check verification required." : termsAndConditions,
                AdditionalNotes = additionalNotes,
                Status = "Approved",
                CreatedAt = DateTime.Now,
                CreatedBy = GetCurrentUserId()
            };

            _context.OfferLetters.Add(offer);

            // Update Application stage to Offer Sent
            app.Stage = "Offer Sent";
            _context.CandidateApplications.Update(app);

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Offer Letter '{offerCode}' generated successfully for '{app.Candidate?.FullName}'!";
            return RedirectToAction(nameof(OfferLetters));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeOfferStatus(int offerId, string status)
        {
            var offer = await _context.OfferLetters.Include(o => o.Application).FirstOrDefaultAsync(o => o.OfferId == offerId);
            if (offer != null)
            {
                offer.Status = status;
                if (status == "Sent") offer.SentAt = DateTime.Now;
                if (status == "Accepted" || status == "Rejected") offer.RespondedAt = DateTime.Now;

                _context.OfferLetters.Update(offer);

                if (offer.Application != null)
                {
                    if (status == "Accepted") offer.Application.Stage = "Offer Accepted";
                    if (status == "Rejected") offer.Application.Status = "Rejected";
                    _context.CandidateApplications.Update(offer.Application);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Offer status changed to '{status}'.";
            }
            return RedirectToAction(nameof(OfferLetters));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConvertCandidateToEmployee(int offerId, int roleId)
        {
            var offer = await _context.OfferLetters
                .Include(o => o.Candidate)
                .Include(o => o.Application)
                .Include(o => o.JobOpening)
                .FirstOrDefaultAsync(o => o.OfferId == offerId);

            if (offer == null || offer.Candidate == null)
            {
                TempData["ErrorMessage"] = "Offer Letter or Candidate record not found.";
                return RedirectToAction(nameof(OfferLetters));
            }

            if (offer.ConvertedToEmployeeId.HasValue)
            {
                TempData["ErrorMessage"] = "Candidate has already been converted to an Employee!";
                return RedirectToAction(nameof(OfferLetters));
            }

            // Check if user already exists with candidate email
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == offer.Candidate.Email.ToLower());
            if (existingUser != null)
            {
                offer.ConvertedToEmployeeId = existingUser.UserId;
                offer.Status = "Accepted";
                if (offer.Application != null) offer.Application.Stage = "Hired";

                _context.OfferLetters.Update(offer);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Candidate '{offer.Candidate.FullName}' linked to existing Employee record ({existingUser.UserCode}) successfully!";
                return RedirectToAction(nameof(OfferLetters));
            }

            // Generate New Employee Code
            int empCount = await _context.Users.CountAsync();
            string userCode = $"EMP-{(empCount + 1).ToString("D3")}";

            var hasher = new PasswordHasher<User>();
            var newEmployee = new User
            {
                CompanyId = 1,
                BranchId = 3,
                UserCode = userCode,
                UserName = offer.Candidate.Email.Split('@')[0],
                FullName = offer.Candidate.FullName,
                Email = offer.Candidate.Email,
                MobileNumber = offer.Candidate.Phone,
                RoleId = roleId > 0 ? roleId : 5, // Default Employee Role
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            newEmployee.PasswordHash = hasher.HashPassword(newEmployee, "Monitor@2026");

            _context.Users.Add(newEmployee);
            await _context.SaveChangesAsync();

            // Link conversion
            offer.ConvertedToEmployeeId = newEmployee.UserId;
            offer.Status = "Accepted";
            if (offer.Application != null) offer.Application.Stage = "Hired";

            _context.OfferLetters.Update(offer);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Candidate '{offer.Candidate.FullName}' converted into Employee ({userCode}) successfully!";
            return RedirectToAction(nameof(OfferLetters));
        }

        [HttpGet]
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

            return View("PrintOffer", offer);
        }
    }
}
