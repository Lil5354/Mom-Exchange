using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace B_M.Models.ViewModels
{
    public class ProductCreateViewModel
    {
        [Display(Name = "Tên sản phẩm")]
        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        public string Name { get; set; }

        [Display(Name = "Nhãn hàng")]
        [Required(ErrorMessage = "Vui lòng chọn nhãn hàng")]
        public int BrandId { get; set; }

        [Display(Name = "Danh mục")]
        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public string Category { get; set; }

        [Display(Name = "Giá bán")]
        [Required(ErrorMessage = "Giá không được để trống")]
        public string Price { get; set; }

        [Display(Name = "Mô tả ngắn")]
        [Required(ErrorMessage = "Mô tả ngắn không được để trống")]
        public string ShortDescription { get; set; }

        [Display(Name = "Mô tả chi tiết")]
        public string DetailedDescription { get; set; }
        
        [Display(Name = "Tình trạng")]
        [Required(ErrorMessage = "Vui lòng chọn tình trạng")]
        public string Condition { get; set; }
        
        [Display(Name = "Vị trí lưu trữ")]
        public string Location { get; set; }
        
        [Display(Name = "Đang bán")]
        public bool IsActive { get; set; } = true;

        // File upload cho images
        [Display(Name = "Hình ảnh sản phẩm")]
        public HttpPostedFileBase[] ProductImages { get; set; }
    }
}