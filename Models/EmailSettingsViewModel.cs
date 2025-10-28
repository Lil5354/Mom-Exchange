using System;
using System.ComponentModel.DataAnnotations;

namespace B_M.Models
{
    public class EmailSettingsViewModel
    {
        [Required(ErrorMessage = "SMTP Host là bắt buộc")]
        [StringLength(255, ErrorMessage = "SMTP Host không được vượt quá 255 ký tự")]
        public string SmtpHost { get; set; }

        [Required(ErrorMessage = "SMTP Port là bắt buộc")]
        [Range(1, 65535, ErrorMessage = "SMTP Port phải từ 1 đến 65535")]
        public int SmtpPort { get; set; }

        public bool EnableSSL { get; set; }

        [Required(ErrorMessage = "Username là bắt buộc")]
        [StringLength(100, ErrorMessage = "Username không được vượt quá 100 ký tự")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password là bắt buộc")]
        [StringLength(100, ErrorMessage = "Password không được vượt quá 100 ký tự")]
        public string Password { get; set; }

        [Required(ErrorMessage = "From Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "From Email không hợp lệ")]
        [StringLength(255, ErrorMessage = "From Email không được vượt quá 255 ký tự")]
        public string FromEmail { get; set; }

        [Required(ErrorMessage = "From Name là bắt buộc")]
        [StringLength(100, ErrorMessage = "From Name không được vượt quá 100 ký tự")]
        public string FromName { get; set; }

        public bool IsEnabled { get; set; }
        public DateTime LastTested { get; set; }
        public bool LastTestResult { get; set; }
    }
}






