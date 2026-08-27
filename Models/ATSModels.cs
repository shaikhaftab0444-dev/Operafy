using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_System.Models
{
    [Table("erp_JobOpenings")]
    public class JobOpening
    {
        [Key]
        public int JobId { get; set; }

        [Required(ErrorMessage = "Job Code is required")]
        [StringLength(50)]
        public string JobCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Job Title is required")]
        [StringLength(150)]
        public string JobTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }

        public int? DesignationId { get; set; }

        [ForeignKey("DesignationId")]
        public Designation? Designation { get; set; }

        public int? HiringManagerId { get; set; }

        [ForeignKey("HiringManagerId")]
        public User? HiringManager { get; set; }

        public int? RecruiterId { get; set; }

        [ForeignKey("RecruiterId")]
        public User? Recruiter { get; set; }

        [Required]
        [StringLength(50)]
        public string EmploymentType { get; set; } = "Full-Time"; // Full-Time, Part-Time, Contract, Internship

        public int Vacancies { get; set; } = 1;

        [StringLength(100)]
        public string JobLocation { get; set; } = "Headquarters";

        [Required]
        [StringLength(50)]
        public string WorkMode { get; set; } = "On-site"; // On-site, Remote, Hybrid

        [StringLength(50)]
        public string ExperienceRequired { get; set; } = "1-3 Years";

        [StringLength(100)]
        public string MinimumEducation { get; set; } = "Bachelor's Degree";

        [StringLength(500)]
        public string RequiredSkills { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? JobDescription { get; set; }

        [StringLength(2000)]
        public string? Responsibilities { get; set; }

        [StringLength(2000)]
        public string? Requirements { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MinimumSalary { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal MaximumSalary { get; set; } = 0;

        [StringLength(10)]
        public string Currency { get; set; } = "INR";

        public DateTime PostingDate { get; set; } = DateTime.Now;

        public DateTime? ClosingDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Open"; // Draft, Open, On Hold, Closed, Cancelled, Archived

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public int? CreatedBy { get; set; }

        // Navigation property for candidate applications
        public List<CandidateApplication> Applications { get; set; } = new List<CandidateApplication>();
    }

    [Table("erp_Candidates")]
    public class Candidate
    {
        [Key]
        public int CandidateId { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [StringLength(150)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [StringLength(30)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Address { get; set; }

        [StringLength(150)]
        public string? Education { get; set; }

        [StringLength(50)]
        public string? Experience { get; set; }

        [StringLength(500)]
        public string? Skills { get; set; }

        [StringLength(150)]
        public string? CurrentCompany { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CurrentSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ExpectedSalary { get; set; }

        [StringLength(50)]
        public string? NoticePeriod { get; set; }

        [StringLength(300)]
        public string? ResumePath { get; set; }

        [StringLength(250)]
        public string? LinkedIn { get; set; }

        [StringLength(250)]
        public string? Portfolio { get; set; }

        [StringLength(100)]
        public string ApplicationSource { get; set; } = "Direct / Portal"; // Portal, Referral, LinkedIn, Agency

        [StringLength(150)]
        public string? ReferredBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public List<CandidateApplication> Applications { get; set; } = new List<CandidateApplication>();
    }

    [Table("erp_CandidateApplications")]
    public class CandidateApplication
    {
        [Key]
        public int ApplicationId { get; set; }

        [Required]
        public int CandidateId { get; set; }

        [ForeignKey("CandidateId")]
        public Candidate? Candidate { get; set; }

        [Required]
        public int JobId { get; set; }

        [ForeignKey("JobId")]
        public JobOpening? JobOpening { get; set; }

        public DateTime ApplicationDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string Stage { get; set; } = "Applied"; 
        // Applied, Screening, Shortlisted, HR Interview, Technical Interview, Manager Interview, Final Interview, Selected, Offer Sent, Offer Accepted, Hired

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Active"; // Active, Rejected, On Hold, Withdrawn

        public int MatchScore { get; set; } = 80; // Match percentage

        [StringLength(1000)]
        public string? Notes { get; set; }

        [StringLength(200)]
        public string? Tags { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public List<CandidateStageHistory> StageHistories { get; set; } = new List<CandidateStageHistory>();
        public List<InterviewSchedule> Interviews { get; set; } = new List<InterviewSchedule>();
        public List<OfferLetter> Offers { get; set; } = new List<OfferLetter>();
    }

    [Table("erp_CandidateStageHistories")]
    public class CandidateStageHistory
    {
        [Key]
        public int HistoryId { get; set; }

        [Required]
        public int ApplicationId { get; set; }

        [ForeignKey("ApplicationId")]
        public CandidateApplication? Application { get; set; }

        [Required]
        [StringLength(50)]
        public string PreviousStage { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string NewStage { get; set; } = string.Empty;

        public int? ChangedByUserId { get; set; }

        [ForeignKey("ChangedByUserId")]
        public User? ChangedByUser { get; set; }

        public DateTime ChangeDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string? ReasonNotes { get; set; }
    }

    [Table("erp_InterviewSchedules")]
    public class InterviewSchedule
    {
        [Key]
        public int InterviewId { get; set; }

        [Required]
        public int ApplicationId { get; set; }

        [ForeignKey("ApplicationId")]
        public CandidateApplication? Application { get; set; }

        [Required]
        public int CandidateId { get; set; }

        [ForeignKey("CandidateId")]
        public Candidate? Candidate { get; set; }

        [Required]
        public int JobId { get; set; }

        [ForeignKey("JobId")]
        public JobOpening? JobOpening { get; set; }

        [Required]
        [StringLength(100)]
        public string InterviewRound { get; set; } = "HR Interview"; // HR Interview, Technical Round 1, Technical Round 2, Managerial Interview, Final Round

        [Required]
        [StringLength(50)]
        public string InterviewType { get; set; } = "HR Interview";

        [Required]
        [StringLength(50)]
        public string InterviewMode { get; set; } = "Online"; // Online, In-Person, Phone

        public DateTime ScheduledDate { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string StartTime { get; set; } = "10:00 AM";

        [StringLength(20)]
        public string EndTime { get; set; } = "11:00 AM";

        [StringLength(250)]
        public string? Location { get; set; }

        [StringLength(500)]
        public string? MeetingLink { get; set; }

        public int? InterviewerId { get; set; }

        [ForeignKey("InterviewerId")]
        public User? Interviewer { get; set; }

        [StringLength(250)]
        public string? InterviewerNames { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Confirmed, Completed, Rescheduled, Cancelled, No Show

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public List<InterviewFeedback> Feedbacks { get; set; } = new List<InterviewFeedback>();
    }

    [Table("erp_InterviewFeedbacks")]
    public class InterviewFeedback
    {
        [Key]
        public int FeedbackId { get; set; }

        [Required]
        public int InterviewId { get; set; }

        [ForeignKey("InterviewId")]
        public InterviewSchedule? Interview { get; set; }

        public int CandidateId { get; set; }

        public int JobId { get; set; }

        public int? InterviewerId { get; set; }

        [ForeignKey("InterviewerId")]
        public User? Interviewer { get; set; }

        public int TechnicalRating { get; set; } = 3; // 1 to 5

        public int CommunicationRating { get; set; } = 3;

        public int ExperienceRating { get; set; } = 3;

        public int ProblemSolvingRating { get; set; } = 3;

        public int CulturalFitRating { get; set; } = 3;

        [Column(TypeName = "decimal(3,2)")]
        public decimal OverallRating { get; set; } = 3.0m;

        [StringLength(1000)]
        public string? Strengths { get; set; }

        [StringLength(1000)]
        public string? Weaknesses { get; set; }

        [StringLength(1500)]
        public string? Comments { get; set; }

        [Required]
        [StringLength(100)]
        public string Recommendation { get; set; } = "Recommend"; // Strongly Recommend, Recommend, Neutral, Do Not Recommend, Strongly Do Not Recommend

        public bool IsFinalized { get; set; } = true;

        public DateTime SubmittedAt { get; set; } = DateTime.Now;
    }

    [Table("erp_OfferLetters")]
    public class OfferLetter
    {
        [Key]
        public int OfferId { get; set; }

        [Required]
        [StringLength(50)]
        public string OfferCode { get; set; } = string.Empty;

        [Required]
        public int ApplicationId { get; set; }

        [ForeignKey("ApplicationId")]
        public CandidateApplication? Application { get; set; }

        [Required]
        public int CandidateId { get; set; }

        [ForeignKey("CandidateId")]
        public Candidate? Candidate { get; set; }

        [Required]
        public int JobId { get; set; }

        [ForeignKey("JobId")]
        public JobOpening? JobOpening { get; set; }

        public int? DesignationId { get; set; }

        [ForeignKey("DesignationId")]
        public Designation? Designation { get; set; }

        public int? DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }

        [StringLength(50)]
        public string EmploymentType { get; set; } = "Full-Time";

        public DateTime ProposedJoiningDate { get; set; } = DateTime.Now.AddDays(15);

        public int? ReportingManagerId { get; set; }

        [ForeignKey("ReportingManagerId")]
        public User? ReportingManager { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OfferedCTC { get; set; }

        [StringLength(1500)]
        public string? SalaryStructure { get; set; }

        public DateTime OfferExpiryDate { get; set; } = DateTime.Now.AddDays(7);

        [StringLength(2000)]
        public string? TermsAndConditions { get; set; }

        [StringLength(1000)]
        public string? AdditionalNotes { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Draft"; // Draft, Pending Approval, Approved, Sent, Viewed, Accepted, Rejected, Expired, Withdrawn

        public DateTime? SentAt { get; set; }

        public DateTime? RespondedAt { get; set; }

        public int? ConvertedToEmployeeId { get; set; }

        [ForeignKey("ConvertedToEmployeeId")]
        public User? ConvertedEmployee { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int? CreatedBy { get; set; }
    }
}
