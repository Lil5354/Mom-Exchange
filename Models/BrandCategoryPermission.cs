// Models/BrandCategoryPermission.cs - Brand Category Permissions
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace B_M.Models
{
    public class BrandCategoryPermission
    {
        [Key]
        public long BrandCategoryPermissionID { get; set; }

        [Required]
        public int BrandID { get; set; }

        [Required]
        public int CategoryID { get; set; }

        // Navigation properties
        [ForeignKey("BrandID")]
        public virtual Brand Brand { get; set; }

        [ForeignKey("CategoryID")]
        public virtual Category Category { get; set; }
    }
}
