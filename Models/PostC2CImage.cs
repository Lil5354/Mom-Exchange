// Models/PostC2CImage.cs - Images for C2C Posts
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace B_M.Models
{
    public class PostC2CImage
    {
        [Key]
        public long ImageID { get; set; }

        [Required]
        public long PostID { get; set; }

        [Required]
        [StringLength(1024)]
        public string ImageUrl { get; set; }

        public bool IsPrimary { get; set; } = false;

        // Navigation property
        [ForeignKey("PostID")]
        public virtual PostC2C Post { get; set; }
    }
}
