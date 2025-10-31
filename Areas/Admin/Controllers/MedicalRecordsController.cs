using B_M.Filters;
using B_M.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.IO;

namespace B_M.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class MedicalRecordsController : Controller
    {
        private readonly ApplicationDbContext db;

        public MedicalRecordsController()
        {
            db = new ApplicationDbContext();
        }

        // GET: Admin/MedicalRecords - Danh sách hồ sơ y tế
        public ActionResult Index(int? page, int? statusFilter)
        {
            try
            {
                int pageSize = 20;
                int pageNumber = page ?? 1;

                var query = db.UserMedicalRecords
                    .Include(r => r.User)
                    .Include(r => r.User.UserDetails)
                    .Include(r => r.AdminReviewer)
                    .Include(r => r.AdminReviewer.UserDetails)
                    .AsQueryable();

                // Filter theo trạng thái
                if (statusFilter.HasValue)
                {
                    query = query.Where(r => r.VerificationStatus == statusFilter.Value);
                }

                // Order by: Pending first, then by upload date
                query = query.OrderByDescending(r => r.VerificationStatus == 0)
                            .ThenByDescending(r => r.UploadedAt);

                var totalRecords = query.Count();

                var records = query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList()
                    .Select(r => new AdminMedicalRecordItemViewModel
                    {
                        RecordID = r.RecordID,
                        UserID = r.UserID,
                        UserName = r.User?.UserDetails?.FullName ?? "Chưa cập nhật",
                        UserEmail = r.User?.Email ?? "",
                        UserPhone = r.User?.PhoneNumber ?? "",
                        UserFullName = r.User?.UserDetails?.FullName ?? "Chưa cập nhật",
                        UserAvatarUrl = r.User?.UserDetails?.ProfilePictureURL,
                        FileName = r.FileName,
                        FilePath = r.FileUrl,
                        VerificationStatus = r.VerificationStatus,
                        ReviewNotes = r.ReviewNotes,
                        UploadedAt = r.UploadedAt,
                        AdminReviewerID = r.AdminReviewerID,
                        ReviewedAt = null, // Not available in current data structure
                        AdminReviewerName = r.AdminReviewer?.UserDetails?.FullName ?? "",
                        MilkDonationStatus = r.User?.MilkDonationStatus ?? 0,
                        UserCreatedAt = r.User?.CreatedAt ?? DateTime.MinValue,
                        IsUserActive = r.User?.IsActive ?? false
                    })
                    .ToList();

                var viewModel = new AdminMedicalRecordsListViewModel
                {
                    Records = records,
                    CurrentPage = pageNumber,
                    TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                    TotalRecords = totalRecords,
                    StatusFilter = statusFilter?.ToString() ?? "all"
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi: " + ex.Message;
                return View(new AdminMedicalRecordsListViewModel());
            }
        }

        // GET: Admin/MedicalRecords/Details/5
        public ActionResult Details(long id)
        {
            try
            {
                var record = db.UserMedicalRecords
                    .Include(r => r.User)
                    .Include(r => r.User.UserDetails)
                    .Include(r => r.AdminReviewer)
                    .Include(r => r.AdminReviewer.UserDetails)
                    .FirstOrDefault(r => r.RecordID == id);

                if (record == null)
                {
                    ViewBag.Error = "Không tìm thấy hồ sơ y tế.";
                    return RedirectToAction("Index");
                }

                // Đếm số hồ sơ đã được duyệt và đang chờ duyệt của user này
                var approvedCount = db.UserMedicalRecords
                    .Count(r => r.UserID == record.UserID && r.VerificationStatus == 1);
                var pendingCount = db.UserMedicalRecords
                    .Count(r => r.UserID == record.UserID && r.VerificationStatus == 0);

                // Get all user's medical records for context
                var allUserRecords = db.UserMedicalRecords
                    .Where(r => r.UserID == record.UserID)
                    .OrderByDescending(r => r.UploadedAt)
                    .ToList();

                // Tạo full path để hiển thị file
                string filePath = Server.MapPath("~/App_Data/MedicalRecords/" + record.FileUrl);
                filePath = filePath.Replace("/", "\\");

                var viewModel = new AdminMedicalRecordDetailViewModel
                {
                    RecordID = record.RecordID,
                    UserID = record.UserID,
                    UserName = record.User?.UserDetails?.FullName ?? "Chưa cập nhật",
                    UserEmail = record.User?.Email ?? "",
                    UserPhone = record.User?.PhoneNumber ?? "",
                    UserFullName = record.User?.UserDetails?.FullName ?? "Chưa cập nhật",
                    UserAvatarUrl = record.User?.UserDetails?.ProfilePictureURL,
                    UserAddress = record.User?.UserDetails?.Address ?? "",
                    FileName = record.FileName,
                    FilePath = record.FileUrl,
                    VerificationStatus = record.VerificationStatus,
                    ReviewNotes = record.ReviewNotes,
                    UploadedAt = record.UploadedAt,
                    AdminReviewerID = record.AdminReviewerID,
                    AdminReviewerName = record.AdminReviewer?.UserDetails?.FullName ?? "",
                    ReviewedAt = null, // Not available in current data structure
                    MilkDonationStatus = record.User?.MilkDonationStatus ?? 0,
                    UserCreatedAt = record.User?.CreatedAt ?? DateTime.MinValue,
                    IsUserActive = record.User?.IsActive ?? false,
                    ReputationScore = record.User?.UserDetails?.ReputationScore ?? 0.0,
                    AllUserRecords = allUserRecords
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Admin/MedicalRecords/ApproveRecord
        [HttpPost]
        public JsonResult ApproveRecord(long recordId, string notes = "")
        {
            try
            {
                var record = db.UserMedicalRecords
                    .Include(r => r.User)
                    .FirstOrDefault(r => r.RecordID == recordId);

                if (record == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy hồ sơ y tế" });
                }

                if (record.VerificationStatus != 0)
                {
                    return Json(new { success = false, message = "Hồ sơ này đã được xử lý rồi" });
                }

                // Lấy admin hiện tại
                var adminIdentity = User.Identity.Name;
                var admin = db.Users.FirstOrDefault(u => (u.Email == adminIdentity || u.UserName == adminIdentity) && u.Role == 1);

                if (admin == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin admin" });
                }

                // 1. Đếm số hồ sơ đã được duyệt TRƯỚC KHI approve
                var currentApprovedCount = db.UserMedicalRecords
                    .Count(r => r.UserID == record.UserID && r.VerificationStatus == 1);

                // 2. Cập nhật trạng thái hồ sơ
                record.VerificationStatus = 1; // Approved
                record.AdminReviewerID = admin.UserID;
                record.ReviewNotes = notes ?? "";

                // 3. Cập nhật trạng thái user dựa trên số hồ sơ đã duyệt
                var user = record.User;
                bool upgradedToTier2 = false;
                int updatedPostsCount = 0;
                
                if (user != null)
                {
                    if (currentApprovedCount == 0) // Hồ sơ đầu tiên được duyệt
                    {
                        // Vẫn giữ trạng thái "Đang chờ duyệt" cho đến khi có đủ 2 hồ sơ
                        user.MilkDonationStatus = 2; // PendingVerification
                    }
                    else if (currentApprovedCount >= 1) // Hồ sơ thứ 2 trở lên được duyệt
                    {
                        // Nâng cấp lên Tier 2 - HealthVerified
                        user.MilkDonationStatus = 3; // HealthVerified - Tier 2
                        
                        // 4. UPDATE VerificationTier cho TẤT CẢ posts của user này lên Tier 2
                        var userPosts = db.MilkDonationPosts
                            .Where(p => p.UserID == user.UserID && p.VerificationTier == 1)
                            .ToList();
                        
                        foreach (var post in userPosts)
                        {
                            post.VerificationTier = 3; // Update lên Tầng 2 (THẺ XANH)
                        }
                        
                        updatedPostsCount = userPosts.Count;
                        upgradedToTier2 = true;
                    }
                }

                db.SaveChanges();

                string message = "Duyệt hồ sơ y tế thành công!";
                if (upgradedToTier2)
                {
                    message += " User đã được nâng cấp lên Tầng 2 - HealthVerified!";
                    if (updatedPostsCount > 0)
                    {
                        message += $" {updatedPostsCount} bài đăng đã được cập nhật lên Tầng 2 (Thẻ Xanh).";
                    }
                }
                else if (currentApprovedCount == 0)
                {
                    message += " User đã chuyển sang trạng thái 'Đang chờ duyệt'. Cần thêm 1 hồ sơ nữa để lên Tầng 2.";
                }
                else if (user != null && user.MilkDonationStatus == 3)
                {
                    message += " User đã ở Tầng 2.";
                }

                return Json(new { success = true, message = message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Admin/MedicalRecords/RejectRecord
        [HttpPost]
        public JsonResult RejectRecord(long recordId, string notes)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(notes))
                {
                    return Json(new { success = false, message = "Vui lòng nhập lý do từ chối" });
                }

                var record = db.UserMedicalRecords
                    .Include(r => r.User)
                    .FirstOrDefault(r => r.RecordID == recordId);

                if (record == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy hồ sơ y tế" });
                }

                if (record.VerificationStatus != 0)
                {
                    return Json(new { success = false, message = "Hồ sơ này đã được xử lý rồi" });
                }

                // Lấy admin hiện tại
                var adminIdentity = User.Identity.Name;
                var admin = db.Users.FirstOrDefault(u => (u.Email == adminIdentity || u.UserName == adminIdentity) && u.Role == 1);

                if (admin == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin admin" });
                }

                // Cập nhật trạng thái
                record.VerificationStatus = 2; // Rejected
                record.AdminReviewerID = admin.UserID;
                record.ReviewNotes = notes;

                db.SaveChanges();

                return Json(new { success = true, message = "Từ chối hồ sơ y tế thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Admin/MedicalRecords/DownloadFile/id
        public FileResult DownloadFile(long id)
        {
            try
            {
                var record = db.UserMedicalRecords.Find(id);
                if (record == null)
                {
                    return null;
                }

                string filePath = Server.MapPath("~/App_Data/MedicalRecords/" + record.FileUrl);
                if (!System.IO.File.Exists(filePath))
                {
                    return null;
                }

                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, record.FileName);
            }
            catch
            {
                return null;
            }
        }

        // GET: Admin/MedicalRecords/ViewFile/id
        public FileResult ViewFile(long id)
        {
            try
            {
                var record = db.UserMedicalRecords.Find(id);
                if (record == null)
                {
                    return null;
                }

                string filePath = Server.MapPath("~/App_Data/MedicalRecords/" + record.FileUrl);
                if (!System.IO.File.Exists(filePath))
                {
                    return null;
                }

                string mimeType = "application/pdf";
                var ext = Path.GetExtension(record.FileName)?.ToLower();
                switch (ext)
                {
                    case ".jpg":
                    case ".jpeg":
                        mimeType = "image/jpeg";
                        break;
                    case ".png":
                        mimeType = "image/png";
                        break;
                    case ".gif":
                        mimeType = "image/gif";
                        break;
                }

                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, mimeType);
            }
            catch
            {
                return null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

