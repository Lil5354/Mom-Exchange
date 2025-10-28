// Models/UserMedicalRecord.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace B_M.Models
{
    public class UserMedicalRecord
    {
        public long RecordID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; }

        [Required]
        [StringLength(1024)]
        public string FileUrl { get; set; }

        // 0: Pending, 1: Approved, 2: Rejected
        public int VerificationStatus { get; set; } = 0;

        public int? AdminReviewerID { get; set; }

        [StringLength(500)]
        public string ReviewNotes { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [ForeignKey("AdminReviewerID")]
        public virtual User AdminReviewer { get; set; }
    }
}

