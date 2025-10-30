// Models/AdminCategoryViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace B_M.Models
{
    public class AdminCategoryViewModel
    {
        public int CategoryID { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
        [Display(Name = "Tên danh mục")]
        public string CategoryName { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Display(Name = "Danh mục cha")]
        public int? ParentCategoryID { get; set; }

        [Display(Name = "Cho phép B2C")]
        public bool IsB2CEnabled { get; set; }

        [Display(Name = "Cho phép C2C")]
        public bool IsC2CEnabled { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties for display
        public string ParentCategoryName { get; set; }
        public List<AdminCategoryViewModel> SubCategories { get; set; } = new List<AdminCategoryViewModel>();
        public int PostC2CCount { get; set; }
        public int SubCategoryCount { get; set; }
        public int Level { get; set; } // For tree display

        // Computed properties
        public string StatusText => (IsB2CEnabled || IsC2CEnabled) ? "Hoạt động" : "Không hoạt động";
        public string StatusClass => (IsB2CEnabled || IsC2CEnabled) ? "status-active" : "status-inactive";
        
        public string EnabledModules 
        { 
            get 
            {
                var modules = new List<string>();
                if (IsB2CEnabled) modules.Add("B2C");
                if (IsC2CEnabled) modules.Add("C2C");
                return modules.Count > 0 ? string.Join(", ", modules) : "Không có";
            } 
        }
    }

    public class CategoryIndexViewModel
    {
        public List<AdminCategoryViewModel> RootCategories { get; set; } = new List<AdminCategoryViewModel>();
        public List<AdminCategoryViewModel> Categories { get; set; } = new List<AdminCategoryViewModel>();
        public int TotalCategories { get; set; }
        public int ActiveCategories { get; set; }
        public int B2CCategories { get; set; }
        public int C2CCategories { get; set; }
        public int BothEnabledCategories { get; set; }
        
        // For category creation dropdown
        public List<Category> AllCategoriesForDropdown { get; set; } = new List<Category>();
    }
}
