                // Models/ProfileViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace B_M.Models
{
    public class ProfileViewModel
    {
        public int UserID { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string ProfilePictureURL { get; set; }
        public double ReputationScore { get; set; }
        public int MilkDonationStatus { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Computed properties
        public string MilkDonationStatusText
        {
            get
            {
                switch (MilkDonationStatus)
                {
                    case 0: return "Chưa đăng ký tặng sữa";
                    case 1: return "Tầng 1";
                    case 2: return "Đã duyệt Tầng 1 (Health Verified)";
                    case 3: return "Đã duyệt Tầng 2 (Community Donor)";
                    case 4: return "Bị từ chối";
                    default: return "Không xác định";
                }
            }
        }

        public string MilkDonationStatusClass
        {
            get
            {
                switch (MilkDonationStatus)
                {
                    case 0: return "status-none";
                    case 1: return "status-pending";
                    case 2: return "status-tier1";
                    case 3: return "status-tier2";
                    case 4: return "status-rejected";
                    default: return "status-unknown";
                }
            }
        }
    }

    public class EditProfileViewModel
    {
        public int UserID { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Số điện thoại")]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ và tên")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Display(Name = "Địa chỉ")]
        [StringLength(500)]
        public string Address { get; set; }

        public string ProfilePictureURL { get; set; }
    }

    public class MilkDonationStatusViewModel
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public int MilkDonationStatus { get; set; }
        public bool HasLifestyleSurvey { get; set; }
        public bool HasMedicalRecords { get; set; }
        public int ApprovedMedicalRecordsCount { get; set; }

        // Computed properties
        public string StatusText
        {
            get
            {
                switch (MilkDonationStatus)
                {
                    case 0: return "Chưa đăng ký";
                    case 1: return "Tầng 1";
                    case 2: return "Tầng 1 - Health Verified";
                    case 3: return "Tầng 2 - Community Donor";
                    case 4: return "Bị từ chối";
                    default: return "Không xác định";
                }
            }
        }

        public bool CanApplyForTier1 => MilkDonationStatus == 0;
        public bool CanApplyForTier2 => MilkDonationStatus == 2;
        public bool IsInProcess => MilkDonationStatus == 1;
    }
}

