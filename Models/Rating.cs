// Models/Rating.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace B_M.Models
{
    public class Rating
    {
        public long RatingID { get; set; }

        [Required]
        public int RaterUserID { get; set; }

        [Required]
        public int RatedUserID { get; set; }

        [Required]
        public long PostID { get; set; } // Required FK to Posts_C2C

        [Required]
        public byte Score { get; set; } // 1-5

        [StringLength(1000)]
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("RaterUserID")]
        public virtual User RaterUser { get; set; }

        [ForeignKey("RatedUserID")]
        public virtual User RatedUser { get; set; }

        [ForeignKey("PostID")]
        public virtual PostC2C Post { get; set; }
    }
}

