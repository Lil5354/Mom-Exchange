// Models/AdminMedicalRecordsViewModel.cs
using System;
using System.Collections.Generic;

namespace B_M.Models
{
    public class AdminMedicalRecordsListViewModel
    {
        public List<AdminMedicalRecordItemViewModel> Records { get; set; } = new List<AdminMedicalRecordItemViewModel>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public int? StatusFilter { get; set; }
    }

    public class AdminMedicalRecordItemViewModel
    {
        public long RecordID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string UserAvatarUrl { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public int VerificationStatus { get; set; }
        public string ReviewNotes { get; set; }
        public DateTime UploadedAt { get; set; }
        public int? AdminReviewerID { get; set; }
        public string AdminReviewerName { get; set; }
        public int UserMilkDonationStatus { get; set; }

        public string VerificationStatusText
        {
            get
            {
                switch (VerificationStatus)
                {
                    case 0: return "Chờ duyệt";
                    case 1: return "Đã duyệt";
                    case 2: return "Từ chối";
                    default: return "Không xác định";
                }
            }
        }

        public string VerificationStatusClass
        {
            get
            {
                switch (VerificationStatus)
                {
                    case 0: return "badge-warning";
                    case 1: return "badge-success";
                    case 2: return "badge-danger";
                    default: return "badge-secondary";
                }
            }
        }

        public string UserMilkDonationStatusText
        {
            get
            {
                switch (UserMilkDonationStatus)
                {
                    case 0: return "Chưa đăng ký";
                    case 1: return "Tầng 1 - Khai báo cơ bản";
                    case 2: return "Chờ duyệt";
                    case 3: return "Tầng 2 - Đã xác thực";
                    case 4: return "Bị từ chối";
                    default: return "Không xác định";
                }
            }
        }
    }

    public class AdminMedicalRecordDetailViewModel : AdminMedicalRecordItemViewModel
    {
        public string FullFilePath { get; set; }
        public int UserApprovedRecordsCount { get; set; }
        public int UserPendingRecordsCount { get; set; }
    }
}
