// Models/MilkDonationViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace B_M.Models
{
    public class LifestyleSurveyViewModel
    {
        public int UserID { get; set; }
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thông tin về việc hút thuốc")]
        [Display(Name = "Bạn có hút thuốc lá (bao gồm cả thuốc lá điện tử - vape) không?")]
        public bool IsSmoker { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thông tin về việc sử dụng đồ uống có cồn")]
        [Display(Name = "Bạn có thường xuyên sử dụng đồ uống có cồn (bia, rượu) không?")]
        public bool UsesAlcohol { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thông tin về việc sử dụng thuốc")]
        [Display(Name = "Bạn có đang sử dụng bất kỳ loại thuốc nào (kê đơn hoặc không kê đơn) không?")]
        public bool UsesMedication { get; set; }

        [Display(Name = "Vui lòng liệt kê tên các loại thuốc bạn đang sử dụng")]
        [StringLength(500, ErrorMessage = "Chi tiết thuốc không được vượt quá 500 ký tự")]
        public string MedicationDetails { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận cam kết về chất kích thích")]
        [Display(Name = "Bạn có cam kết rằng mình KHÔNG sử dụng bất kỳ chất kích thích hoặc ma túy nào (bao gồm cả cần sa) không?")]
        public bool CommitNoDrugs { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận cam kết về bệnh truyền nhiễm")]
        [Display(Name = "Bạn có cam kết rằng (theo hiểu biết của bạn) bạn KHÔNG được chẩn đoán mắc các bệnh truyền nhiễm qua đường máu như Viêm gan B, C, HIV, Giang mai,... không?")]
        public bool CommitNoInfectiousDiseases { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận tính trung thực của thông tin")]
        [Display(Name = "Tôi cam đoan rằng tất cả các thông tin tôi đã khai báo ở trên là hoàn toàn trung thực và đúng sự thật.")]
        public bool ConfirmTruthfulness { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận hiểu rõ các cảnh báo")]
        [Display(Name = "Tôi đã đọc và hiểu rõ các cảnh báo. Tôi chấp nhận rằng tin đăng của tôi sẽ được gắn \"Thẻ Vàng - Khai báo Cơ bản\" và đi kèm khuyến nghị không dùng cho bé uống.")]
        public bool AcceptWarnings { get; set; }
    }

    public class MedicalRecordsViewModel
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public List<UserMedicalRecord> ExistingRecords { get; set; } = new List<UserMedicalRecord>();

        // Computed properties
        public int PendingRecordsCount => ExistingRecords.Count(r => r.VerificationStatus == 0);
        public int ApprovedRecordsCount => ExistingRecords.Count(r => r.VerificationStatus == 1);
        public int RejectedRecordsCount => ExistingRecords.Count(r => r.VerificationStatus == 2);
        public bool HasMinimumApprovedRecords => ApprovedRecordsCount >= 2;

        public string VerificationStatusText
        {
            get
            {
                if (ExistingRecords == null || !ExistingRecords.Any())
                    return "Chưa có hồ sơ y tế";

                var latestRecord = ExistingRecords.OrderByDescending(r => r.UploadedAt).First();
                switch (latestRecord.VerificationStatus)
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
                if (ExistingRecords == null || !ExistingRecords.Any())
                    return "status-none";

                var latestRecord = ExistingRecords.OrderByDescending(r => r.UploadedAt).First();
                switch (latestRecord.VerificationStatus)
                {
                    case 0: return "status-pending";
                    case 1: return "status-approved";
                    case 2: return "status-rejected";
                    default: return "status-none";
                }
            }
        }
    }


    public class CreateMilkDonationPostViewModel
    {
        public int UserID { get; set; }
        public string DonorName { get; set; }
        public int VerificationTier { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa điểm")]
        [Display(Name = "Địa điểm")]
        [StringLength(255, ErrorMessage = "Địa điểm không được quá 255 ký tự")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày vắt sữa")]
        [Display(Name = "Ngày vắt sữa")]
        [DataType(DataType.Date)]
        public DateTime CollectionDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Vui lòng mô tả chế độ ăn")]
        [Display(Name = "Chế độ ăn của mẹ")]
        [StringLength(1000, ErrorMessage = "Mô tả không được quá 1000 ký tự")]
        public string MotherDietInfo { get; set; }

        [Required(ErrorMessage = "Vui lòng mô tả cách bảo quản")]
        [Display(Name = "Cách bảo quản sữa")]
        [StringLength(1000, ErrorMessage = "Mô tả không được quá 1000 ký tự")]
        public string StorageMethod { get; set; }

        [Display(Name = "Ghi chú thêm")]
        [StringLength(2000, ErrorMessage = "Ghi chú không được quá 2000 ký tự")]
        public string Note { get; set; }

        [Display(Name = "Hình ảnh")]
        [StringLength(500, ErrorMessage = "URL hình ảnh không được quá 500 ký tự")]
        public string ImageUrl { get; set; }

        // Computed properties
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
    }

    public class MilkDonationPostViewModel
    {
        public int Id { get; set; }
        public int DonorUserId { get; set; }
        public string DonorName { get; set; }
        public string Title { get; set; }  // Missing property for chat integration
        public string Location { get; set; }
        public DateTime DateOfExpression { get; set; }
        public string DietInfo { get; set; }
        public string StorageInfo { get; set; }
        public string Note { get; set; }
        public string DonorAvatarUrl { get; set; }
        public string ImageUrl { get; set; }
        public int VerificationTier { get; set; }
        public DateTime PostedAt { get; set; }
        public int Status { get; set; }

        // Request-related properties
        public bool HasUserRequested { get; set; }
        public int? UserRequestStatus { get; set; } // null if no request, 0=pending, 1=accepted, 2=declined

        // Health declaration properties (for tier 1 and tier 2 posts)
        public bool? HasHealthDeclaration { get; set; }
        public bool? IsSmoker { get; set; }
        public bool? UsesAlcohol { get; set; }
        public bool? UsesMedication { get; set; }
        public string MedicationDetails { get; set; }
        public bool? CommitNoDrugs { get; set; }
        public bool? CommitNoInfectiousDiseases { get; set; }
        public DateTime? HealthDeclarationSubmittedAt { get; set; }

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

        public string VerificationTierClass
        {
            get
            {
                switch (VerificationTier)
                {
                    case 1: return "tier-basic";
                    case 3: return "tier-verified";
                    default: return "tier-none";
                }
            }
        }

        public string VerificationTierBadge
        {
            get
            {
                switch (VerificationTier)
                {
                    case 1: return "THẺ VÀNG";
                    case 3: return "THẺ XANH";
                    default: return "CHƯA XÁC MINH";
                }
            }
        }

        public string VerificationWarning
        {
            get
            {
                switch (VerificationTier)
                {
                    case 1: return "KHAI BÁO CƠ BẢN. Sữa chưa được xác thực y tế. Nền tảng khuyến nghị không dùng cho bé uống.";
                    case 3: return "✅ ĐÃ XÁC THỰC Y TẾ";
                    default: return "❓ CHƯA XÁC MINH";
                }
            }
        }

        public string RequestButtonText
        {
            get
            {
                if (!HasUserRequested) return "Gửi Yêu cầu Nhận";
                switch (UserRequestStatus)
                {
                    case 0: return "Đã gửi Yêu cầu";
                    case 1: return "Đã được chấp nhận";
                    case 2: return "Đã bị từ chối";
                    default: return "Gửi Yêu cầu Nhận";
                }
            }
        }

        public bool CanSendRequest => !HasUserRequested;
    }

    public class MilkDonationRequestViewModel
    {
        public long RequestID { get; set; }
        public long PostID { get; set; }
        public string PostTitle { get; set; }
        public int RecipientUserID { get; set; }
        public string RecipientName { get; set; }
        public string RecipientAvatarUrl { get; set; }
        public int DonorUserID { get; set; }
        public string DonorName { get; set; }
        public int Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public string Note { get; set; }

        // Computed properties
        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case 0: return "Đang chờ";
                    case 1: return "Đã chấp nhận";
                    case 2: return "Đã từ chối";
                    case 3: return "Tin đăng đã đóng";
                    default: return "Không xác định";
                }
            }
        }

        public string StatusClass
        {
            get
            {
                switch (Status)
                {
                    case 0: return "status-pending";
                    case 1: return "status-accepted";
                    case 2: return "status-declined";
                    case 3: return "status-closed";
                    default: return "status-unknown";
                }
            }
        }

        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.Now - RequestedAt;
                if (timeSpan.TotalMinutes < 1) return "Vừa xong";
                if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} phút trước";
                if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours} giờ trước";
                if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays} ngày trước";
                return RequestedAt.ToString("dd/MM/yyyy");
            }
        }
    }

    public class CreateRequestViewModel
    {
        [Required]
        public long PostID { get; set; }
        
        [Required]
        public int DonorUserID { get; set; }

        [StringLength(1000, ErrorMessage = "Lời nhắn không được vượt quá 1000 ký tự")]
        [Display(Name = "Lời nhắn cho người cho (Không bắt buộc)")]
        public string Note { get; set; }

        // For display purposes
        public string PostTitle { get; set; }
        public string DonorName { get; set; }
        public int VerificationTier { get; set; }
        public bool ShowWarning => VerificationTier == 1;
    }

    public enum MilkDonationStatus
    {
        NotDonor = 0,           // Chưa đăng ký cho tặng
        BasicDeclared = 1,      // Đã hoàn thành khai báo cơ bản - Tầng 1
        PendingVerification = 2, // Đã nộp hồ sơ y tế, chờ Admin duyệt
        HealthVerified = 3,     // Đã xác thực y tế đầy đủ - Tầng 2
        Rejected = 4            // Bị từ chối
    }

    public static class MilkDonationStatusExtensions
    {
        public static string GetDisplayText(this MilkDonationStatus status)
        {
            switch (status)
            {
                case MilkDonationStatus.NotDonor: return "Chưa đăng ký cho tặng";
                case MilkDonationStatus.BasicDeclared: return "Tầng 1 - Khai báo cơ bản";
                case MilkDonationStatus.PendingVerification: return "Chờ duyệt hồ sơ y tế";
                case MilkDonationStatus.HealthVerified: return "Tầng 2 - Đã xác thực y tế";
                case MilkDonationStatus.Rejected: return "Bị từ chối";
                default: return "Không xác định";
            }
        }

        public static string GetBadgeClass(this MilkDonationStatus status)
        {
            switch (status)
            {
                case MilkDonationStatus.NotDonor: return "badge-none";
                case MilkDonationStatus.BasicDeclared: return "badge-tier1";
                case MilkDonationStatus.PendingVerification: return "badge-pending";
                case MilkDonationStatus.HealthVerified: return "badge-tier2";
                case MilkDonationStatus.Rejected: return "badge-rejected";
                default: return "badge-unknown";
            }
        }
    }

    public class MilkDonationIndexViewModel
    {
        public List<MilkDonationPostViewModel> Posts { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalPosts { get; set; }
        public string ProvinceFilter { get; set; }
        public string TierFilter { get; set; }
    }
}
