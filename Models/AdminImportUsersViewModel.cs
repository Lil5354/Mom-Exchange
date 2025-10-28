using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace B_M.Models
{
    public class AdminImportUsersViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn file Excel")]
        public HttpPostedFileBase ExcelFile { get; set; }

        [Required(ErrorMessage = "Vai trò mặc định là bắt buộc")]
        [Range(1, 3, ErrorMessage = "Vai trò không hợp lệ")]
        public byte DefaultRole { get; set; } = 2; // Default to Mom

        public bool IsActive { get; set; } = true;

        public bool SendEmailNotification { get; set; } = false;

        public bool GenerateRandomPassword { get; set; } = true;

        public bool SkipDuplicateEmails { get; set; } = true;

        public bool SkipDuplicateUsernames { get; set; } = true;

        // Helper properties
        public string DefaultRoleName
        {
            get
            {
                switch (DefaultRole)
                {
                    case 1: return "Quản trị viên";
                    case 2: return "Mẹ bỉm";
                    case 3: return "Nhãn hàng";
                    default: return "Không xác định";
                }
            }
        }
    }

    public class AdminImportResultViewModel
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public int SkippedCount { get; set; }
        public int EmailsSentCount { get; set; }
        public List<ImportUserError> Errors { get; set; } = new List<ImportUserError>();
        public List<ImportUserSuccess> SuccessUsers { get; set; } = new List<ImportUserSuccess>();
        public DateTime ImportTime { get; set; } = DateTime.Now;
        public string FileName { get; set; }
    }

    public class ImportUserError
    {
        public int RowNumber { get; set; }
        public string Email { get; set; }
        public string ErrorMessage { get; set; }
        public string FieldName { get; set; }
    }

    public class ImportUserSuccess
    {
        public int RowNumber { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public byte Role { get; set; }
        public string RoleName { get; set; }
        public string GeneratedPassword { get; set; }
        public bool EmailSent { get; set; }
        public string EmailMessage { get; set; }
    }
}




