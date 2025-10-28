using System;
using System.ComponentModel.DataAnnotations;

namespace B_M.Models
{
    public class BackupSettingsViewModel
    {
        public bool EnableAutoBackup { get; set; }
        
        [Range(1, 30, ErrorMessage = "Tần suất backup phải từ 1 đến 30 ngày")]
        public int BackupFrequencyDays { get; set; }
        
        [Range(1, 30, ErrorMessage = "Số lượng backup giữ lại phải từ 1 đến 30")]
        public int KeepBackupCount { get; set; }
        
        public string BackupLocation { get; set; }
        public bool CompressBackup { get; set; }
        public bool IncludeFiles { get; set; }
        public bool IncludeDatabase { get; set; }
        
        public DateTime LastBackupDate { get; set; }
        public string LastBackupStatus { get; set; }
        public long LastBackupSizeBytes { get; set; }
        
        public bool EnableEmailNotification { get; set; }
        public string BackupNotificationEmail { get; set; }
        
        public DateTime NextScheduledBackup { get; set; }
        public bool IsBackupRunning { get; set; }
    }
}




