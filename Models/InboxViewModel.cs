<<<<<<< HEAD
using System;
=======
﻿using System;
>>>>>>> f33209e (updated)
using System.Collections.Generic;

namespace ERP_System.Models
{
    public class MessageItem
    {
        public int MessageId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderRole { get; set; } = string.Empty;
        public string SenderAvatar { get; set; } = "/profile_images/admin-avatar.jpg";
        public string RecipientName { get; set; } = string.Empty;
        public string RecipientEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string BodyContent { get; set; } = string.Empty;
        public string Category { get; set; } = "General"; // General, Payroll, Support, System, Task
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
        public bool IsStarred { get; set; } = false;
        public bool IsTrash { get; set; } = false;
        public string Priority { get; set; } = "Normal"; // High, Normal, Low
    }

    public class InboxViewModel
    {
        public List<MessageItem> Messages { get; set; } = new List<MessageItem>();
        public MessageItem? SelectedMessage { get; set; }
        public string SelectedFolder { get; set; } = "inbox"; // inbox, starred, sent, trash
        public int UnreadCount { get; set; }
        public int StarredCount { get; set; }
        public int SentCount { get; set; }
        public int TrashCount { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public List<User> AvailableUsers { get; set; } = new List<User>();
    }
}
