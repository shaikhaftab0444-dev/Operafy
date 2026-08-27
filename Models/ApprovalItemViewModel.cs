namespace ERP_System.Models
{
    public class ApprovalItemViewModel
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string ClaimCategory { get; set; } = string.Empty; // Casual Leave, Attendance Regularization, Travel Reimbursement
        public string CategoryKey { get; set; } = string.Empty; // Leave, Regularization, Expense
        public string Duration { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string SubmittedDate { get; set; } = string.Empty;
        public bool HasAttachment { get; set; }
        public string? AttachmentName { get; set; }
        public string Status { get; set; } = "Pending";
    }
}
