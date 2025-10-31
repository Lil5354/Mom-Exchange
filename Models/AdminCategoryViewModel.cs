// Models/AdminCategoryViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace B_M.Models
{
    public class CategoryIndexViewModel
    {
        public List<CategoryTreeNode> RootCategories { get; set; } = new List<CategoryTreeNode>();
        public int TotalCategories { get; set; }
        public int B2CCategories { get; set; }
        public int C2CCategories { get; set; }
        public int BothEnabledCategories { get; set; }

        
    }

    public class CategoryTreeNode
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public int? ParentCategoryID { get; set; }
        public bool IsB2CEnabled { get; set; }
        public bool IsC2CEnabled { get; set; }
        public int Level { get; set; }
        public List<CategoryTreeNode> Children { get; set; } = new List<CategoryTreeNode>();

        public string EnabledText
        {
            get
            {
                if (IsB2CEnabled && IsC2CEnabled)
                    return "B2C + C2C";
                else if (IsB2CEnabled)
                    return "B2C";
                else if (IsC2CEnabled)
                    return "C2C";
                else
                    return "Không hoạt động";
            }
        }

        public string EnabledClass
        {
            get
            {
                if (IsB2CEnabled && IsC2CEnabled)
                    return "badge-success";
                else if (IsB2CEnabled || IsC2CEnabled)
                    return "badge-warning";
                else
                    return "badge-secondary";
            }
        }
    }

    public class AdminCategoryViewModel
    {
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
        public string CategoryName { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string Description { get; set; }

        public int? ParentCategoryID { get; set; }
        public bool IsB2CEnabled { get; set; }
        public bool IsC2CEnabled { get; set; }

        // For display purposes
        public string ParentCategoryName { get; set; }
        public List<Category> AvailableParentCategories { get; set; } = new List<Category>();

        // For tree view purposes
        public int Level { get; set; }
        public List<AdminCategoryViewModel> SubCategories { get; set; } = new List<AdminCategoryViewModel>();
        public int SubCategoryCount { get; set; }
    }
}
