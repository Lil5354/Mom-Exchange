// Models/LifestyleSurveyViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace B_M.Models
{
    public class LifestyleSurveyViewModel
    {
        public int UserID { get; set; }
        
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng trả lời câu hỏi về hút thuốc")]
        [Display(Name = "Bạn có hút thuốc không?")]
        public bool IsSmoker { get; set; }

        [Required(ErrorMessage = "Vui lòng trả lời câu hỏi về rượu bia")]
        [Display(Name = "Bạn có sử dụng rượu bia không?")]
        public bool UsesAlcohol { get; set; }

        [Display(Name = "Chi tiết về thuốc đang sử dụng (nếu có)")]
        [StringLength(500, ErrorMessage = "Mô tả không được quá 500 ký tự")]
        public string MedicationDetails { get; set; }
    }
}
