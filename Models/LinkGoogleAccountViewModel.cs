// File: Models/LinkGoogleAccountViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace B_M.Models
{
    public class LinkGoogleAccountViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu để xác nhận")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string CurrentPassword { get; set; }

        // Hidden fields from session
        public string GoogleEmail { get; set; }
        public string GoogleName { get; set; }
        
        // Current user info for display
        public string CurrentEmail { get; set; }
        public string CurrentFullName { get; set; }
        
        // Action type
        public string Action { get; set; } // "link" or "replace"
    }
}
