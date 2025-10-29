using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace B_M.Models
{
    public class AdminBrandViewModel
    {
        public int BrandID { get; set; }

        [Required(ErrorMessage = "Tên nhãn hàng là bắt buộc")]
        [StringLength(255, ErrorMessage = "Tên nhãn hàng không được vượt quá 255 ký tự")]
        [Display(Name = "Tên nhãn hàng")]
        public string BrandName { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [StringLength(1024, ErrorMessage = "URL logo không được vượt quá 1024 ký tự")]
        [Display(Name = "URL Logo")]
        public string LogoUrl { get; set; }

        [Required(ErrorMessage = "Phải chọn người dùng")]
        [Display(Name = "Người dùng")]
        public int UserID { get; set; }

        // Display properties
        public string UserEmail { get; set; }
        public string UserFullName { get; set; }
        public bool IsUserActive { get; set; }
        public int CategoryPermissionCount { get; set; }
        public int ProductCount { get; set; }
    }

    public class AdminBrandsViewModel
    {
        public List<AdminBrandViewModel> Brands { get; set; } = new List<AdminBrandViewModel>();
        public int TotalBrands { get; set; }
        public int ActiveBrands { get; set; }
        public int BrandUsers { get; set; }
    }

    public class BrandPermissionViewModel
    {
        public int BrandID { get; set; }
        public string BrandName { get; set; }
        public string UserEmail { get; set; }
        public string UserFullName { get; set; }
        
        public List<CategoryPermissionItem> Categories { get; set; } = new List<CategoryPermissionItem>();
    }

    public class CategoryPermissionItem
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public string FullPath { get; set; }
        public bool IsGranted { get; set; }
    }
}


