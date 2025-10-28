using System;
using System.ComponentModel.DataAnnotations;

namespace B_M.Models
{
    public class SecuritySettingsViewModel
    {
        [Required(ErrorMessage = "Độ dài mật khẩu tối thiểu là bắt buộc")]
        [Range(6, 20, ErrorMessage = "Độ dài mật khẩu phải từ 6 đến 20 ký tự")]
        public int MinPasswordLength { get; set; }

        public bool RequireSpecialChars { get; set; }
        public bool RequireNumbers { get; set; }
        public bool RequireUppercase { get; set; }

        [Required(ErrorMessage = "Thời gian timeout phiên là bắt buộc")]
        [Range(5, 480, ErrorMessage = "Thời gian timeout phiên phải từ 5 đến 480 phút")]
        public int SessionTimeoutMinutes { get; set; }

        [Required(ErrorMessage = "Số lần đăng nhập tối đa là bắt buộc")]
        [Range(3, 10, ErrorMessage = "Số lần đăng nhập tối đa phải từ 3 đến 10")]
        public int MaxLoginAttempts { get; set; }

        public bool EnableTwoFactor { get; set; }
        public bool EnableIPWhitelist { get; set; }
        public string IPWhitelist { get; set; }

        [Required(ErrorMessage = "Thời gian khóa tài khoản là bắt buộc")]
        [Range(5, 60, ErrorMessage = "Thời gian khóa tài khoản phải từ 5 đến 60 phút")]
        public int AccountLockoutMinutes { get; set; }

        public bool LogSecurityEvents { get; set; }
        public bool RequirePasswordChange { get; set; }
        public int PasswordChangeDays { get; set; }
    }
}






