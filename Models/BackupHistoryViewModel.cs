using System.Collections.Generic;

namespace ERP_System.Models
{
    public class BackupHistoryViewModel
    {
        public List<AdminBackupLog> Backups { get; set; } = new List<AdminBackupLog>();
        public BackupScheduleModel Schedule { get; set; } = new BackupScheduleModel();
        public double StorageUsedMb { get; set; }
        public double StorageLimitMb { get; set; } = 1000.0; // 1 GB
        public int TotalSnapshots { get; set; }
        public string LastBackupStatus { get; set; } = "Success";
        public string LastBackupTime { get; set; } = "Never";
    }

    public class BackupScheduleModel
    {
        public bool Enabled { get; set; } = true;
        public string Frequency { get; set; } = "Daily"; // Daily, Weekly, Monthly
        public string ExecutionTime { get; set; } = "00:00";
        public int RetentionDays { get; set; } = 30;
    }
}
