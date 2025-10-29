// Models/PostC2CExchangePreference.cs - Desired exchange categories for a C2C post
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace B_M.Models
{
    public class PostC2CExchangePreference
    {
        [Key]
        public long ExchangePreferenceID { get; set; }

        [Required]
        public long PostID { get; set; }

        [Required]
        public int CategoryID { get; set; }

        [ForeignKey("PostID")]
        public virtual PostC2C Post { get; set; }

        [ForeignKey("CategoryID")]
        public virtual Category Category { get; set; }
    }
}


