using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using B_M.Models;
using B_M.Repositories;

namespace B_M.Services
{
    /// <summary>
    /// Service layer for settings management
    /// Provides high-level operations for different setting categories
    /// </summary>
    public class SettingsService : IDisposable
    {
        private readonly SettingsRepository settingsRepo;

        public SettingsService()
        {
            settingsRepo = new SettingsRepository();
        }

        #region Email Settings

        /// <summary>
        /// Load email settings from database
        /// </summary>
        public EmailSettingsViewModel GetEmailSettings()
        {
            return new EmailSettingsViewModel
            {
                SmtpHost = settingsRepo.GetSetting("Email", "SmtpHost", "smtp.gmail.com"),
                SmtpPort = settingsRepo.GetSettingInt("Email", "SmtpPort", 587),
                EnableSSL = settingsRepo.GetSettingBool("Email", "EnableSSL", true),
                Username = settingsRepo.GetSetting("Email", "Username", ""),
                Password = settingsRepo.GetSetting("Email", "Password", ""), // Will be decrypted
                FromEmail = settingsRepo.GetSetting("Email", "FromEmail", "noreply@momexchange.com"),
                FromName = settingsRepo.GetSetting("Email", "FromName", "MomExchange System"),
                IsEnabled = settingsRepo.GetSettingBool("Email", "IsEnabled", true)
            };
        }

        /// <summary>
        /// Save email settings to database
        /// </summary>
        public bool SaveEmailSettings(EmailSettingsViewModel model, int? updatedBy = null)
        {
            try
            {
                var settings = new Dictionary<string, object>
                {
                    { "SmtpHost", model.SmtpHost },
                    { "SmtpPort", model.SmtpPort },
                    { "EnableSSL", model.EnableSSL },
                    { "Username", model.Username },
                    { "Password", model.Password }, // Will be encrypted
                    { "FromEmail", model.FromEmail },
                    { "FromName", model.FromName },
                    { "IsEnabled", model.IsEnabled }
                };

                return settingsRepo.SaveSettings("Email", settings, updatedBy);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveEmailSettings Error: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Security Settings

        /// <summary>
        /// Load security settings from database
        /// </summary>
        public SecuritySettingsViewModel GetSecuritySettings()
        {
            return new SecuritySettingsViewModel
            {
                MinPasswordLength = settingsRepo.GetSettingInt("Security", "MinPasswordLength", 8),
                RequireSpecialChars = settingsRepo.GetSettingBool("Security", "RequireSpecialChars", true),
                RequireNumbers = settingsRepo.GetSettingBool("Security", "RequireNumbers", true),
                RequireUppercase = settingsRepo.GetSettingBool("Security", "RequireUppercase", true),
                SessionTimeoutMinutes = settingsRepo.GetSettingInt("Security", "SessionTimeoutMinutes", 30),
                MaxLoginAttempts = settingsRepo.GetSettingInt("Security", "MaxLoginAttempts", 5),
                EnableTwoFactor = settingsRepo.GetSettingBool("Security", "EnableTwoFactor", false),
                AccountLockoutMinutes = settingsRepo.GetSettingInt("Security", "AccountLockoutMinutes", 15),
                LogSecurityEvents = settingsRepo.GetSettingBool("Security", "LogSecurityEvents", true),
                PasswordChangeDays = settingsRepo.GetSettingInt("Security", "PasswordChangeDays", 90)
            };
        }

        /// <summary>
        /// Save security settings to database
        /// </summary>
        public bool SaveSecuritySettings(SecuritySettingsViewModel model, int? updatedBy = null)
        {
            try
            {
                var settings = new Dictionary<string, object>
                {
                    { "MinPasswordLength", model.MinPasswordLength },
                    { "RequireSpecialChars", model.RequireSpecialChars },
                    { "RequireNumbers", model.RequireNumbers },
                    { "RequireUppercase", model.RequireUppercase },
                    { "SessionTimeoutMinutes", model.SessionTimeoutMinutes },
                    { "MaxLoginAttempts", model.MaxLoginAttempts },
                    { "EnableTwoFactor", model.EnableTwoFactor },
                    { "AccountLockoutMinutes", model.AccountLockoutMinutes },
                    { "LogSecurityEvents", model.LogSecurityEvents },
                    { "PasswordChangeDays", model.PasswordChangeDays }
                };

                return settingsRepo.SaveSettings("Security", settings, updatedBy);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveSecuritySettings Error: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Notification Settings

        /// <summary>
        /// Load notification settings from database
        /// </summary>
        public NotificationSettingsViewModel GetNotificationSettings()
        {
            return new NotificationSettingsViewModel
            {
                EnableEmailNotifications = settingsRepo.GetSettingBool("Notification", "EnableEmailNotifications", true),
                EnablePushNotifications = settingsRepo.GetSettingBool("Notification", "EnablePushNotifications", true),
                EnableSMSNotifications = settingsRepo.GetSettingBool("Notification", "EnableSMSNotifications", false),
                NotifyNewUserRegistration = settingsRepo.GetSettingBool("Notification", "NotifyNewUserRegistration", true),
                NotifyPasswordReset = settingsRepo.GetSettingBool("Notification", "NotifyPasswordReset", true),
                NotifyAccountLocked = settingsRepo.GetSettingBool("Notification", "NotifyAccountLocked", true),
                NotifySystemMaintenance = settingsRepo.GetSettingBool("Notification", "NotifySystemMaintenance", true),
                NotifySecurityAlerts = settingsRepo.GetSettingBool("Notification", "NotifySecurityAlerts", true)
            };
        }

        /// <summary>
        /// Save notification settings to database
        /// </summary>
        public bool SaveNotificationSettings(NotificationSettingsViewModel model, int? updatedBy = null)
        {
            try
            {
                var settings = new Dictionary<string, object>
                {
                    { "EnableEmailNotifications", model.EnableEmailNotifications },
                    { "EnablePushNotifications", model.EnablePushNotifications },
                    { "EnableSMSNotifications", model.EnableSMSNotifications },
                    { "NotifyNewUserRegistration", model.NotifyNewUserRegistration },
                    { "NotifyPasswordReset", model.NotifyPasswordReset },
                    { "NotifyAccountLocked", model.NotifyAccountLocked },
                    { "NotifySystemMaintenance", model.NotifySystemMaintenance },
                    { "NotifySecurityAlerts", model.NotifySecurityAlerts }
                };

                return settingsRepo.SaveSettings("Notification", settings, updatedBy);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveNotificationSettings Error: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region System Configuration

        /// <summary>
        /// Load system configuration from database
        /// </summary>
        public SystemConfigurationViewModel GetSystemConfiguration()
        {
            return new SystemConfigurationViewModel
            {
                SiteName = settingsRepo.GetSetting("System", "SiteName", "MomExchange"),
                SiteUrl = settingsRepo.GetSetting("System", "SiteUrl", "https://localhost:44300"),
                SiteDescription = settingsRepo.GetSetting("System", "SiteDescription", "Nền tảng trao đổi và chia sẻ cho các bà mẹ"),
                ContactEmail = settingsRepo.GetSetting("System", "ContactEmail", "contact@momexchange.com"),
                ContactPhone = settingsRepo.GetSetting("System", "ContactPhone", ""),
                MaxFileUploadSizeMB = settingsRepo.GetSettingInt("System", "MaxFileUploadSizeMB", 10),
                MaxFileUploadCount = settingsRepo.GetSettingInt("System", "MaxFileUploadCount", 5),
                AllowedFileExtensions = settingsRepo.GetSetting("System", "AllowedFileExtensions", "jpg,jpeg,png,gif,pdf,doc,docx"),
                ApiRateLimitPerMinute = settingsRepo.GetSettingInt("System", "ApiRateLimitPerMinute", 100),
                MaintenanceMode = settingsRepo.GetSettingBool("System", "MaintenanceMode", false),
                EnableCaching = settingsRepo.GetSettingBool("System", "EnableCaching", true)
            };
        }

        /// <summary>
        /// Save system configuration to database
        /// </summary>
        public bool SaveSystemConfiguration(SystemConfigurationViewModel model, int? updatedBy = null)
        {
            try
            {
                var settings = new Dictionary<string, object>
                {
                    { "SiteName", model.SiteName },
                    { "SiteUrl", model.SiteUrl },
                    { "SiteDescription", model.SiteDescription },
                    { "ContactEmail", model.ContactEmail },
                    { "ContactPhone", model.ContactPhone },
                    { "MaxFileUploadSizeMB", model.MaxFileUploadSizeMB },
                    { "MaxFileUploadCount", model.MaxFileUploadCount },
                    { "AllowedFileExtensions", model.AllowedFileExtensions },
                    { "ApiRateLimitPerMinute", model.ApiRateLimitPerMinute },
                    { "MaintenanceMode", model.MaintenanceMode },
                    { "EnableCaching", model.EnableCaching }
                };

                return settingsRepo.SaveSettings("System", settings, updatedBy);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveSystemConfiguration Error: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Backup Settings

        /// <summary>
        /// Load backup settings from database
        /// </summary>
        public BackupSettingsViewModel GetBackupSettings()
        {
            return new BackupSettingsViewModel
            {
                EnableAutoBackup = settingsRepo.GetSettingBool("Backup", "EnableAutoBackup", false),
                BackupFrequencyDays = settingsRepo.GetSettingInt("Backup", "BackupFrequencyDays", 7),
                KeepBackupCount = settingsRepo.GetSettingInt("Backup", "KeepBackupCount", 5),
                BackupLocation = settingsRepo.GetSetting("Backup", "BackupLocation", "/backups/"),
                CompressBackup = settingsRepo.GetSettingBool("Backup", "CompressBackup", true),
                IncludeFiles = settingsRepo.GetSettingBool("Backup", "IncludeFiles", true),
                IncludeDatabase = settingsRepo.GetSettingBool("Backup", "IncludeDatabase", true),
                EnableEmailNotification = settingsRepo.GetSettingBool("Backup", "EnableEmailNotification", false)
            };
        }

        /// <summary>
        /// Save backup settings to database
        /// </summary>
        public bool SaveBackupSettings(BackupSettingsViewModel model, int? updatedBy = null)
        {
            try
            {
                var settings = new Dictionary<string, object>
                {
                    { "EnableAutoBackup", model.EnableAutoBackup },
                    { "BackupFrequencyDays", model.BackupFrequencyDays },
                    { "KeepBackupCount", model.KeepBackupCount },
                    { "BackupLocation", model.BackupLocation },
                    { "CompressBackup", model.CompressBackup },
                    { "IncludeFiles", model.IncludeFiles },
                    { "IncludeDatabase", model.IncludeDatabase },
                    { "EnableEmailNotification", model.EnableEmailNotification }
                };

                return settingsRepo.SaveSettings("Backup", settings, updatedBy);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveBackupSettings Error: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Monitoring Settings

        /// <summary>
        /// Load monitoring settings from database
        /// </summary>
        public MonitoringSettingsViewModel GetMonitoringSettings()
        {
            return new MonitoringSettingsViewModel
            {
                EnableSystemMonitoring = settingsRepo.GetSettingBool("Monitoring", "EnableSystemMonitoring", true),
                EnableErrorTracking = settingsRepo.GetSettingBool("Monitoring", "EnableErrorTracking", true),
                EnablePerformanceMonitoring = settingsRepo.GetSettingBool("Monitoring", "EnablePerformanceMonitoring", false),
                EnableUserActivityLogging = settingsRepo.GetSettingBool("Monitoring", "EnableUserActivityLogging", true),
                LogRetentionDays = settingsRepo.GetSettingInt("Monitoring", "LogRetentionDays", 30),
                MaxLogEntries = settingsRepo.GetSettingInt("Monitoring", "MaxLogEntries", 1000),
                LogLevel = settingsRepo.GetSetting("Monitoring", "LogLevel", "Info"),
                EnableRealTimeLogging = settingsRepo.GetSettingBool("Monitoring", "EnableRealTimeLogging", false),
                EnableEmailAlerts = settingsRepo.GetSettingBool("Monitoring", "EnableEmailAlerts", false),
                DiskSpaceThresholdPercent = settingsRepo.GetSettingInt("Monitoring", "DiskSpaceThresholdPercent", 80),
                MemoryThresholdPercent = settingsRepo.GetSettingInt("Monitoring", "MemoryThresholdPercent", 85),
                CPUThresholdPercent = settingsRepo.GetSettingInt("Monitoring", "CPUThresholdPercent", 90)
            };
        }

        /// <summary>
        /// Save monitoring settings to database
        /// </summary>
        public bool SaveMonitoringSettings(MonitoringSettingsViewModel model, int? updatedBy = null)
        {
            try
            {
                var settings = new Dictionary<string, object>
                {
                    { "EnableSystemMonitoring", model.EnableSystemMonitoring },
                    { "EnableErrorTracking", model.EnableErrorTracking },
                    { "EnablePerformanceMonitoring", model.EnablePerformanceMonitoring },
                    { "EnableUserActivityLogging", model.EnableUserActivityLogging },
                    { "LogRetentionDays", model.LogRetentionDays },
                    { "MaxLogEntries", model.MaxLogEntries },
                    { "LogLevel", model.LogLevel },
                    { "EnableRealTimeLogging", model.EnableRealTimeLogging },
                    { "EnableEmailAlerts", model.EnableEmailAlerts },
                    { "DiskSpaceThresholdPercent", model.DiskSpaceThresholdPercent },
                    { "MemoryThresholdPercent", model.MemoryThresholdPercent },
                    { "CPUThresholdPercent", model.CPUThresholdPercent }
                };

                return settingsRepo.SaveSettings("Monitoring", settings, updatedBy);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveMonitoringSettings Error: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region All Settings

        /// <summary>
        /// Load all settings for Settings page
        /// Safe: Returns defaults if database not ready
        /// </summary>
        public SettingsViewModel GetAllSettings()
        {
            try
            {
                return new SettingsViewModel
                {
                    EmailSettings = GetEmailSettings(),
                    SecuritySettings = GetSecuritySettings(),
                    NotificationSettings = GetNotificationSettings(),
                    SystemConfiguration = GetSystemConfiguration(),
                    BackupSettings = GetBackupSettings(),
                    MonitoringSettings = GetMonitoringSettings()
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAllSettings Error (returning defaults): {ex.Message}");
                // Return default settings if database not ready
                return new SettingsViewModel();
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            settingsRepo?.Dispose();
        }

        #endregion
    }
}

