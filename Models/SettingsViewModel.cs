using System;
using System.Collections.Generic;

namespace B_M.Models
{
    public class SettingsViewModel
    {
        public EmailSettingsViewModel EmailSettings { get; set; }
        public SecuritySettingsViewModel SecuritySettings { get; set; }
        public NotificationSettingsViewModel NotificationSettings { get; set; }
        public SystemConfigurationViewModel SystemConfiguration { get; set; }
        public BackupSettingsViewModel BackupSettings { get; set; }
        public MonitoringSettingsViewModel MonitoringSettings { get; set; }
        
        public SettingsViewModel()
        {
            EmailSettings = new EmailSettingsViewModel();
            SecuritySettings = new SecuritySettingsViewModel();
            NotificationSettings = new NotificationSettingsViewModel();
            SystemConfiguration = new SystemConfigurationViewModel();
            BackupSettings = new BackupSettingsViewModel();
            MonitoringSettings = new MonitoringSettingsViewModel();
        }
    }
}






