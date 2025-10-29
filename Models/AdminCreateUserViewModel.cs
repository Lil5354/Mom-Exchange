using System;
using System.ComponentModel.DataAnnotations;

namespace B_M.Models
{
    public class AdminCreateUserViewModel
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(255, ErrorMessage = "Email không được quá 255 ký tự")]
        public string Email { get; set; }

        [StringLength(50, ErrorMessage = "Tên đăng nhập không được quá 50 ký tự")]
        public string UserName { get; set; }

        [StringLength(20, ErrorMessage = "Số điện thoại không được quá 20 ký tự")]
        [RegularExpression(@"^[0-9+\-\s()]*$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ và tên không được quá 100 ký tự")]
        public string FullName { get; set; }

        [StringLength(500, ErrorMessage = "Địa chỉ không được quá 500 ký tự")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải từ 8-100 ký tự")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "Mật khẩu phải có ít nhất 1 chữ thường, 1 chữ hoa và 1 số")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Xác nhận mật khẩu không khớp")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        [Range(1, 2, ErrorMessage = "Vai trò không hợp lệ")]
        public byte Role { get; set; }

        public bool IsActive { get; set; } = true;

        public bool SendEmailNotification { get; set; } = false;

        public bool GenerateRandomPassword { get; set; } = false;

        // Helper properties
        public string RoleName
        {
            get
            {
                switch (Role)
                {
                    case 1: return "Quản trị viên";
                    case 2: return "Khách hàng";
                    default: return "Không xác định";
                }
            }
        }

        public string GeneratedPassword { get; set; }
    }
}




