// Models/UserLifestyleSurvey.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace B_M.Models
{
    public class UserLifestyleSurvey
    {
        public long SurveyID { get; set; }

        [Required]
        public int UserID { get; set; }

        // Câu hỏi 1: Hút thuốc
        [Required]
        public bool IsSmoker { get; set; }

        // Câu hỏi 2: Sử dụng đồ uống có cồn
        [Required]
        public bool UsesAlcohol { get; set; }

        // Câu hỏi 3: Sử dụng thuốc
        [Required]
        public bool UsesMedication { get; set; }

        // Câu hỏi 4: Chi tiết thuốc (hiển thị nếu câu 3 = true)
        public string MedicationDetails { get; set; }

        // Câu hỏi 5: Cam kết không sử dụng chất kích thích/ma túy
        [Required]
        public bool CommitNoDrugs { get; set; }

        // Câu hỏi 6: Cam kết không mắc bệnh truyền nhiễm
        [Required]
        public bool CommitNoInfectiousDiseases { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("UserID")]
        public virtual User User { get; set; }
    }
}

