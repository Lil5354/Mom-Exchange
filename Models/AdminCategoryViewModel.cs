// Models/AdminCategoryViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace B_M.Models
{
    public class AdminCategoryViewModel
    {
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
        [Display(Name = "Tên danh mục")]
        public string CategoryName { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Display(Name = "Danh mục cha")]
        public int? ParentCategoryID { get; set; }
        public string ParentCategoryName { get; set; }

        [Display(Name = "Cho phép B2C (Nhãn hàng bán)")]
        public bool IsB2CEnabled { get; set; }

        [Display(Name = "Cho phép C2C (Mẹ bỉm thanh lý)")]
        public bool IsC2CEnabled { get; set; }

        // For tree view display
        public List<AdminCategoryViewModel> SubCategories { get; set; }
        public int Level { get; set; } // Độ sâu trong cây (0 = root)
        public int SubCategoryCount { get; set; }
    }

    public class CategoryIndexViewModel
    {
        public List<AdminCategoryViewModel> RootCategories { get; set; }
        public int TotalCategories { get; set; }
        public int B2CCategories { get; set; }
        public int C2CCategories { get; set; }
        public int BothEnabledCategories { get; set; }

        public CategoryIndexViewModel()
        {
            RootCategories = new List<AdminCategoryViewModel>();
        }
    }
}





