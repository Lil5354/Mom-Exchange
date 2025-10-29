// Models/Brand.cs - Restructured for Module 3
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace B_M.Models
{
    public class Brand
    {
        [Key]
        public int BrandID { get; set; }

        [Required]
        [StringLength(255)]
        public string BrandName { get; set; }

        [StringLength(1024)]
        public string LogoUrl { get; set; }

        public string Description { get; set; }

        [Required]
        public int UserID { get; set; } // FK to Users (Role=3)

        // Navigation properties
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        // Note: ProductB2C đã được xóa theo yêu cầu chỉ giữ 2 role Admin/User
        public virtual ICollection<BrandCategoryPermission> CategoryPermissions { get; set; } = new List<BrandCategoryPermission>();
    }
}