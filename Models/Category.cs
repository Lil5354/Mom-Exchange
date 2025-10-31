             // Models/Category.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace B_M.Models
{
    public class Category
    {
        public int CategoryID { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        // New fields for Module 3
        public int? ParentCategoryID { get; set; }

        [Required]
        public bool IsB2CEnabled { get; set; } = false; // 1 = Admin cho phép Brand bán

        [Required]
        public bool IsC2CEnabled { get; set; } = false; // 1 = Admin cho phép Khách hàng thanh lý

        // Navigation properties
        [ForeignKey("ParentCategoryID")]
        public virtual Category ParentCategory { get; set; }

        public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
        public virtual ICollection<PostC2C> PostC2Cs { get; set; } = new List<PostC2C>();
    }
}
