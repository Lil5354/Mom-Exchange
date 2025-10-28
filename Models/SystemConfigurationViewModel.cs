using System;
using System.ComponentModel.DataAnnotations;

namespace B_M.Models
{
    public class SystemConfigurationViewModel
    {
        [Required(ErrorMessage = "Tên trang web là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên trang web không được vượt quá 100 ký tự")]
        public string SiteName { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả trang web không được vượt quá 500 ký tự")]
        public string SiteDescription { get; set; }

        [StringLength(255, ErrorMessage = "URL trang web không được vượt quá 255 ký tự")]
        [Url(ErrorMessage = "URL trang web không hợp lệ")]
        public string SiteUrl { get; set; }

        [StringLength(100, ErrorMessage = "Email liên hệ không được vượt quá 100 ký tự")]
        [EmailAddress(ErrorMessage = "Email liên hệ không hợp lệ")]
        public string ContactEmail { get; set; }

        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        public string ContactPhone { get; set; }

        public bool MaintenanceMode { get; set; }
        public string MaintenanceMessage { get; set; }

        [Range(1, 100, ErrorMessage = "Kích thước file upload phải từ 1 đến 100 MB")]
        public int MaxFileUploadSizeMB { get; set; }

        [Range(1, 1000, ErrorMessage = "Số lượng file upload tối đa phải từ 1 đến 1000")]
        public int MaxFileUploadCount { get; set; }

        public bool EnableCaching { get; set; }
        public int CacheExpirationMinutes { get; set; }

        [Range(1, 1000, ErrorMessage = "API rate limit phải từ 1 đến 1000 requests/phút")]
        public int ApiRateLimitPerMinute { get; set; }

        public string AllowedFileExtensions { get; set; }
        public string LogoPath { get; set; }
        public string FaviconPath { get; set; }

        public DateTime LastModified { get; set; }
        public string ModifiedBy { get; set; }
    }
}






