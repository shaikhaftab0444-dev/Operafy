using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ERP_System.Models
{
    [Table("erp_Onboardings")]
    public class HROnboarding
    {
        [Key]
        public int OnboardingId { get; set; }
        [Required]
        [StringLength(150)]
        public string CandidateName { get; set; } = string.Empty;
        [Required]
        [StringLength(100)]
        public string Position { get; set; } = string.Empty;
        [Required]
        [StringLength(50)]
        public string DocumentsStatus { get; set; } = "Pending (0/4)"; // Uploaded (4/4), Pending (2/4)
        [Required]
        [StringLength(50)]
        public string BackgroundCheck { get; set; } = "In Progress"; // Verified, In Progress, Pending
        public int KycProgress { get; set; } = 0; // 0 to 100
        [Required]
        [StringLength(50)]
        public string OrientationStatus { get; set; } = "Pending"; // Completed, Pending
    }
    [Table("erp_Contracts")]
    public class HRContract
    {
        [Key]
        public int ContractId { get; set; }
        [Required]
        [StringLength(150)]
        public string EmployeeName { get; set; } = string.Empty;
        [Required]
        [StringLength(100)]
        public string DocumentType { get; set; } = string.Empty; // Employment Agreement, Non-Disclosure Agreement
        [Required]
        [StringLength(150)]
        public string FileName { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        [Required]
        [StringLength(50)]
        public string SigningStatus { get; set; } = "Draft"; // Signed, Pending Signature, Draft
    }
    [Table("erp_Transfers")]
    public class HRTransfer
    {
        [Key]
        public int TransferId { get; set; }
        [Required]
        [StringLength(150)]
        public string EmployeeName { get; set; } = string.Empty;
        [Required]
        [StringLength(50)]
        public string Type { get; set; } = "Transfer"; // Promotion, Transfer
        [Required]
        [StringLength(150)]
        public string FromDeptOrDesg { get; set; } = string.Empty;
        [Required]
        [StringLength(150)]
        public string ToDeptOrDesg { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        [Required]
        [StringLength(50)]
        public string ApprovalStatus { get; set; } = "Pending Approval"; // Approved, Pending Approval, Rejected
    }
    [Table("erp_Offboardings")]
    public class HROffboarding
    {
        [Key]
        public int OffboardingId { get; set; }
        [Required]
        [StringLength(150)]
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime ResignationDate { get; set; }
        public DateTime LastWorkingDay { get; set; }
        [Required]
        [StringLength(50)]
        public string AssetReturn { get; set; } = "Pending"; // Returned, Pending, N/A
        [Required]
        [StringLength(50)]
        public string ITClearance { get; set; } = "Pending"; // Cleared, Pending
        [Required]
        [StringLength(50)]
        public string FinanceClearance { get; set; } = "Pending"; // Cleared, Pending
        [Required]
        [StringLength(50)]
        public string ExitInterview { get; set; } = "Pending"; // Done, Pending
        [Required]
        [StringLength(50)]
        public string FfStatus { get; set; } = "In Progress"; // Settled, In Progress, Pending
    }
    [Table("erp_Holidays")]
    public class HRHoliday
    {
        [Key]
        public int HolidayId { get; set; }

        [Required(ErrorMessage = "Holiday Name is required")]
        [StringLength(150)]
        public string HolidayName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Holiday Date is required")]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = "National Holiday";

        public int? BranchId { get; set; }

        [ForeignKey("BranchId")]
        public Branch? Branch { get; set; }

        public bool IsPaid { get; set; } = true;

        public bool IsActive { get; set; } = true;

        [StringLength(300)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}