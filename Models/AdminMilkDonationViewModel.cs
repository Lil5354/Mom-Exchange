// Models/AdminMilkDonationViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace B_M.Models
{
    // ViewModel cho danh sách bài đăng
    public class AdminMilkPostsViewModel
    {
        public List<AdminMilkPostItemViewModel> Posts { get; set; } = new List<AdminMilkPostItemViewModel>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalPosts { get; set; }
        public int? StatusFilter { get; set; }
    }

    // ViewModel cho từng bài đăng
    public class AdminMilkPostItemViewModel
    {
        public long PostID { get; set; }
        public int UserID { get; set; }
        public string DonorName { get; set; }
        public string DonorEmail { get; set; }
        public string DonorAvatarUrl { get; set; }
        public string Location { get; set; }
        public DateTime DateOfExpression { get; set; }
        public string DietInfo { get; set; }
        public string StorageInfo { get; set; }
        public int VerificationTier { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public string VerificationTierText
        {
            get
            {
                switch (VerificationTier)
                {
                    case 1: return "Tầng 1 - Khai báo cơ bản";
                    case 3: return "Tầng 2 - Đã xác thực y tế";
                    default: return "Chưa xác minh";
                }
            }
        }

        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case 1: return "Đang mở";
                    case 2: return "Đã đóng";
                    default: return "Không xác định";
                }
            }
        }
    }

    // ViewModel cho chi tiết bài đăng để duyệt
    public class AdminMilkPostDetailViewModel
    {
        public long PostID { get; set; }
        public int UserID { get; set; }
        public string DonorName { get; set; }
        public string DonorEmail { get; set; }
        public string DonorAvatarUrl { get; set; }
        public string Location { get; set; }
        public DateTime DateOfExpression { get; set; }
        public string DietInfo { get; set; }
        public string StorageInfo { get; set; }
        public string Note { get; set; }
        public int VerificationTier { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public string VerificationTierText
        {
            get
            {
                switch (VerificationTier)
                {
                    case 1: return "Tầng 1 - Khai báo cơ bản";
                    case 3: return "Tầng 2 - Đã xác thực y tế";
                    default: return "Chưa xác minh";
                }
            }
        }

        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case 1: return "Đang mở";
                    case 2: return "Đã đóng";
                    default: return "Không xác định";
                }
            }
        }
    }

    // ========== MEDICAL RECORDS VIEWMODELS ==========
    
    // ViewModel cho danh sách hồ sơ y tế
    public class AdminMedicalRecordsListViewModel
    {
        public List<AdminMedicalRecordItemViewModel> Records { get; set; } = new List<AdminMedicalRecordItemViewModel>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public int? StatusFilter { get; set; } // 0=Pending, 1=Approved, 2=Rejected
    }

    // ViewModel cho từng hồ sơ y tế
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
                    case 0: return "Đang chờ duyệt";
                    case 1: return "Đã được duyệt";
                    case 2: return "Bị từ chối";
                    default: return "Không xác định";
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
                    case 2: return "Đang chờ duyệt hồ sơ";
                    case 3: return "Tầng 2 - Đã xác thực y tế";
                    case 4: return "Bị từ chối";
                    default: return "Không xác định";
                }
            }
        }
    }

    // ViewModel cho chi tiết hồ sơ y tế
    public class AdminMedicalRecordDetailViewModel
    {
        public long RecordID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string UserAvatarUrl { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string FullFilePath { get; set; }
        public int VerificationStatus { get; set; }
        public string ReviewNotes { get; set; }
        public DateTime UploadedAt { get; set; }
        public int? AdminReviewerID { get; set; }
        public string AdminReviewerName { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public int UserMilkDonationStatus { get; set; }
        public int UserApprovedRecordsCount { get; set; }
        public int UserPendingRecordsCount { get; set; }

        public string VerificationStatusText
        {
            get
            {
                switch (VerificationStatus)
                {
                    case 0: return "Đang chờ duyệt";
                    case 1: return "Đã được duyệt";
                    case 2: return "Bị từ chối";
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
                    case 2: return "Đang chờ duyệt hồ sơ";
                    case 3: return "Tầng 2 - Đã xác thực y tế";
                    case 4: return "Bị từ chối";
                    default: return "Không xác định";
                }
            }
        }

        public bool IsFileImage
        {
            get
            {
                var ext = System.IO.Path.GetExtension(FileName)?.ToLower();
                return ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif";
            }
        }

        public bool IsFilePdf
        {
            get
            {
                var ext = System.IO.Path.GetExtension(FileName)?.ToLower();
                return ext == ".pdf";
            }
        }
    }
}

