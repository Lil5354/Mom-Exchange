using System;
using System.ComponentModel.DataAnnotations;

namespace B_M.Models
{
    public class NotificationSettingsViewModel
    {
        public bool EnableEmailNotifications { get; set; }
        public bool EnablePushNotifications { get; set; }
        public bool EnableSMSNotifications { get; set; }

        // Email notification preferences
        public bool NotifyNewUserRegistration { get; set; }
        public bool NotifyPasswordReset { get; set; }
        public bool NotifyAccountLocked { get; set; }
        public bool NotifySystemMaintenance { get; set; }
        public bool NotifySecurityAlerts { get; set; }

        // Push notification settings
        public string PushNotificationTitle { get; set; }
        public string PushNotificationMessage { get; set; }
        public bool EnablePushSound { get; set; }
        public bool EnablePushVibration { get; set; }

        // SMS settings (for future use)
        public string SMSProvider { get; set; }
        public string SMSApiKey { get; set; }
        public string SMSFromNumber { get; set; }

        // Notification templates
        public string WelcomeEmailTemplate { get; set; }
        public string PasswordResetTemplate { get; set; }
        public string MaintenanceTemplate { get; set; }

        public DateTime LastModified { get; set; }
        public string ModifiedBy { get; set; }
    }
}






