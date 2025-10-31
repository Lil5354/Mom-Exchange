using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace B_M.Models
{
    public class PasswordResetCode
    {
        [Key]
        public int Id { get; set; }

        [Index]
        public int UserID { get; set; }

        [Required]
        [StringLength(6)]
        public string Code { get; set; }

        // Optional long token for future-proofing (not required for numeric code flow)
        [StringLength(200)]
        public string Token { get; set; }

        public DateTime ExpiresAt { get; set; }

        public int Attempts { get; set; }

        public DateTime? UsedAt { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }
    }
}


