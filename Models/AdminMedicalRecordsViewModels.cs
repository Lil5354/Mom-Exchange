// Models/AdminMedicalRecordsViewModels.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace B_M.Models
{
    public class AdminMedicalRecordsListViewModel
    {
        public List<AdminMedicalRecordItemViewModel> Records { get; set; } = new List<AdminMedicalRecordItemViewModel>();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public string StatusFilter { get; set; } = "all";

        // Statistics
        public int PendingRecords => Records.Count(r => r.VerificationStatus == 0);
        public int ApprovedRecords => Records.Count(r => r.VerificationStatus == 1);
        public int RejectedRecords => Records.Count(r => r.VerificationStatus == 2);
    }

    public class AdminMedicalRecordItemViewModel
    {
        public long RecordID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string UserPhone { get; set; }
        public string UserFullName { get; set; }
        public string UserAvatarUrl { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadedAt { get; set; }
        public int VerificationStatus { get; set; }
        public int? AdminReviewerID { get; set; }
        public string AdminReviewerName { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string ReviewNotes { get; set; }

        // User details
        public int MilkDonationStatus { get; set; }
        public DateTime UserCreatedAt { get; set; }
        public bool IsUserActive { get; set; }

        // Computed properties
        public string VerificationStatusText
        {
            get
            {
                switch (VerificationStatus)
                {
                    case 0: return "Chờ duyệt";
                    case 1: return "Đã duyệt";
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
                    case 0: return "status-pending";
                    case 1: return "status-approved";
                    case 2: return "status-rejected";
                    default: return "status-unknown";
                }
            }
        }

        public string MilkDonationStatusText
        {
            get
            {
                switch (MilkDonationStatus)
                {
                    case 0: return "Không phải người hiến sữa";
                    case 1: return "Tầng 1 - Khai báo cơ bản";
                    case 2: return "Chờ xác thực y tế";
                    case 3: return "Tầng 2 - Đã xác thực y tế";
                    case 4: return "Bị từ chối xác thực";
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
                    case 1: return "status-basic";
                    case 2: return "status-pending";
                    case 3: return "status-verified";
                    case 4: return "status-rejected";
                    default: return "status-unknown";
                }
            }
        }

        public bool CanReview => VerificationStatus == 0;
        public bool HasBeenReviewed => ReviewedAt.HasValue;
    }

    public class AdminMedicalRecordDetailViewModel
    {
        public long RecordID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string UserPhone { get; set; }
        public string UserFullName { get; set; }
        public string UserAvatarUrl { get; set; }
        public string UserAddress { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadedAt { get; set; }
        public int VerificationStatus { get; set; }
        public int? AdminReviewerID { get; set; }
        public string AdminReviewerName { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string ReviewNotes { get; set; }

        // User details
        public int MilkDonationStatus { get; set; }
        public DateTime UserCreatedAt { get; set; }
        public bool IsUserActive { get; set; }
        public double ReputationScore { get; set; }

        // All user's medical records for context
        public List<UserMedicalRecord> AllUserRecords { get; set; } = new List<UserMedicalRecord>();

        // Computed properties
        public string VerificationStatusText
        {
            get
            {
                switch (VerificationStatus)
                {
                    case 0: return "Chờ duyệt";
                    case 1: return "Đã duyệt";
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
                    case 0: return "status-pending";
                    case 1: return "status-approved";
                    case 2: return "status-rejected";
                    default: return "status-unknown";
                }
            }
        }

        public string MilkDonationStatusText
        {
            get
            {
                switch (MilkDonationStatus)
                {
                    case 0: return "Không phải người hiến sữa";
                    case 1: return "Tầng 1 - Khai báo cơ bản";
                    case 2: return "Chờ xác thực y tế";
                    case 3: return "Tầng 2 - Đã xác thực y tế";
                    case 4: return "Bị từ chối xác thực";
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
                    case 1: return "status-basic";
                    case 2: return "status-pending";
                    case 3: return "status-verified";
                    case 4: return "status-rejected";
                    default: return "status-unknown";
                }
            }
        }

        public bool CanReview => VerificationStatus == 0;
        public bool HasBeenReviewed => ReviewedAt.HasValue;
        public bool FileExists => !string.IsNullOrEmpty(FilePath) && System.IO.File.Exists(FilePath);
        
        public int ApprovedRecordsCount => AllUserRecords.Count(r => r.VerificationStatus == 1);
        public int PendingRecordsCount => AllUserRecords.Count(r => r.VerificationStatus == 0);
        public int RejectedRecordsCount => AllUserRecords.Count(r => r.VerificationStatus == 2);
        public bool HasMinimumApprovedRecords => ApprovedRecordsCount >= 2;

        public bool IsFilePdf => !string.IsNullOrEmpty(FileName) && FileName.ToLower().EndsWith(".pdf");
        public bool IsFileImage => !string.IsNullOrEmpty(FileName) && 
            (FileName.ToLower().EndsWith(".jpg") || FileName.ToLower().EndsWith(".jpeg") || 
             FileName.ToLower().EndsWith(".png") || FileName.ToLower().EndsWith(".gif") || 
             FileName.ToLower().EndsWith(".bmp") || FileName.ToLower().EndsWith(".webp"));
    }
}
