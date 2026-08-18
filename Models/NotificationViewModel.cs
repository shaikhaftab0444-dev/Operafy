<<<<<<< HEAD
using System;
=======
﻿using System;
>>>>>>> f33209e (updated)
using System.Collections.Generic;

namespace ERP_System.Models
{
    public class NotificationItem
    {
        public int NotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "System"; // System, Sales, Inventory, Security, HR, Finance
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
        public string IconClass { get; set; } = "fa-bell";
        public string ColorClass { get; set; } = "text-primary";
        public string BgColorClass { get; set; } = "bg-primary-subtle";
        public string TargetUrl { get; set; } = "#";
    }

    public class NotificationViewModel
    {
        public List<NotificationItem> Notifications { get; set; } = new List<NotificationItem>();
        public int UnreadCount { get; set; }
        public int TotalCount { get; set; }
        public string SelectedCategory { get; set; } = string.Empty;
        public string FilterStatus { get; set; } = "all"; // all, unread
    }
}
