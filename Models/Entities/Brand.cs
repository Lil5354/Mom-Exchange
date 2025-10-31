// Models/Brand.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace B_M.Models.Entities
{
    public class Brand
    {
        public int BrandID { get; set; }
        
        [Required(ErrorMessage = "Tên thương hiệu là bắt buộc")]
        [Display(Name = "Tên thương hiệu")]
        public string BrandName { get; set; }
        
        [Required(ErrorMessage = "Mô tả thương hiệu là bắt buộc")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }
        public string LogoUrl { get; set; } // Đường dẫn đến file logo
        
        // Navigation properties
        public virtual ICollection<Product> Products { get; set; }

        public Brand()
        {
            Products = new List<Product>();
        }
    }
}