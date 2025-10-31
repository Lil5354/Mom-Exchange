using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace B_M.Models.ViewModels
{
    public class ProductEditViewModel
    {
        [Display(Name = "Mã sản phẩm")]
        public int Id { get; set; }
        [Display(Name = "Nhãn hàng")]
        public int BrandId { get; set; }

        [Display(Name = "Tên sản phẩm")]
        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        public string Name { get; set; }

        [Display(Name = "Danh mục")]
        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public string Category { get; set; }

        [Display(Name = "Giá bán")]
        [Required(ErrorMessage = "Giá không được để trống")]
        public string Price { get; set; }

        [Display(Name = "Mô tả ngắn")]
        public string ShortDescription { get; set; }
        [Display(Name = "Mô tả chi tiết")]
        public string DetailedDescription { get; set; }
        [Display(Name = "Tình trạng")]
        public string Condition { get; set; }
        [Display(Name = "Vị trí lưu trữ")]
        public string Location { get; set; }
        [Display(Name = "Đang bán")]
        public bool IsActive { get; set; }

        [Display(Name = "Hình ảnh hiện có")]
        public List<string> ExistingImageUrls { get; set; }

        // File upload mới
        [Display(Name = "Hình ảnh mới")]
        public IEnumerable<HttpPostedFileBase> ProductImages { get; set; }

        // Danh sách URL ảnh đã xóa
        [Display(Name = "Ảnh đã xóa")]
        public string DeletedImageUrls { get; set; }

        // Thứ tự ảnh sau khi reorder (danh sách URL, phân tách dấu ,)
        [Display(Name = "Thứ tự ảnh hiện có")]
        public string ReorderedImageUrls { get; set; }
    }
}