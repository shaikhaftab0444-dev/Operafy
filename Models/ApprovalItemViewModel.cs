namespace ERP_System.Models
{
    public class ApprovalTargetDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
    }

    public class ApprovalItemViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string EmployeeRole { get => Role; set => Role = value; }
        public string Avatar { get; set; } = string.Empty;
        public string ClaimCategory { get; set; } = string.Empty; // e.g. "Casual Leave", "Attendance Regularization", "Travel Reimbursement"
        public string CategoryKey { get; set; } = string.Empty; // "Leave", "Regularization", "Expense"
        public string Type { get => CategoryKey; set => CategoryKey = value; }
        public string Subcategory { get => ClaimCategory; set => ClaimCategory = value; }
        public string Duration { get; set; } = string.Empty;
        public string DurationDates { get => Duration; set => Duration = value; }
        public string Reason { get; set; } = string.Empty;
        public string SubmittedDate { get; set; } = string.Empty;
        public string SubmittedText { get => SubmittedDate; set => SubmittedDate = value; }
        public bool HasAttachment { get; set; }
        public string? AttachmentName { get; set; }
        public string Attachment { get => !string.IsNullOrEmpty(AttachmentName) ? AttachmentName : "— N/A"; set => AttachmentName = value; }
        public string Status { get; set; } = "Pending";
    }
}
