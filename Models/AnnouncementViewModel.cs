using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ERP_System.Models
{
    public class AnnouncementViewModel
    {
        public int AnnouncementId { get; set; }

        [Required(ErrorMessage = "Announcement title is required.")]
        [StringLength(150, ErrorMessage = "Title cannot exceed 150 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Announcement message content is required.")]
        public string Content { get; set; } = string.Empty;

        [Required]
        public string Priority { get; set; } = "Normal"; // Normal, High, Urgent / Broadcast

        [Required]
        public string Category { get; set; } = "General"; // Maintenance, HR Policy, Compliance, Holiday & Events, Corporate News, IT Infrastructure

        public bool IsPinned { get; set; } = false;

        public IFormFile? AttachmentFile { get; set; }

        [Required]
        public string TargetAudience { get; set; } = "All Staff";

        [Required]
        public string TargetBranch { get; set; } = "All Branches";

        [DataType(DataType.Date)]
        public DateTime? ExpiryDate { get; set; }
    }
}
