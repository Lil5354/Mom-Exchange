// Models/ProductB2CImage.cs - Images for B2C Products
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace B_M.Models
{
    public class ProductB2CImage
    {
        [Key]
        public long ImageID { get; set; }

        [Required]
        public long ProductID { get; set; }

        [Required]
        [StringLength(1024)]
        public string ImageUrl { get; set; }

        public bool IsPrimary { get; set; } = false;

        // Navigation property
        [ForeignKey("ProductID")]
        public virtual ProductB2C Product { get; set; }
    }
}
