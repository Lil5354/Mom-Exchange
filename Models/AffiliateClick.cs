// Models/AffiliateClick.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace B_M.Models
{
    public class AffiliateClick
    {
        public long ClickID { get; set; }

        [Required]
        public int AffiliatorUserID { get; set; }

        [Required]
        public long ProductID { get; set; } // Changed to BIGINT for B2C products

        [Required]
        [StringLength(255)]
        public string VisitorSessionID { get; set; }

        public DateTime ClickedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("AffiliatorUserID")]
        public virtual User AffiliatorUser { get; set; }

        [ForeignKey("ProductID")]
        public virtual ProductB2C Product { get; set; }
    }
}

