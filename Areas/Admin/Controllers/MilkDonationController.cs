using B_M.Filters;
using B_M.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace B_M.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class MilkDonationController : Controller
    {
        private readonly ApplicationDbContext db;

        public MilkDonationController()
        {
            db = new ApplicationDbContext();
        }

        // GET: Admin/MilkDonation - Danh sách bài đăng
        public ActionResult Index(int? page, int? statusFilter)
        {
            try
            {
                int pageSize = 20;
                int pageNumber = page ?? 1;

                var query = db.MilkDonationPosts.AsQueryable();

                if (statusFilter.HasValue)
                {
                    query = query.Where(p => p.Status == statusFilter.Value);
                }

                query = query.OrderByDescending(p => p.CreatedAt);

                var posts = query
                    .Include(p => p.User)
                    .Include(p => p.User.UserDetails)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList()
                    .Select(p =>
                    {
                        var lines = (p.Content ?? "").Split('\n');
                        return new AdminMilkPostItemViewModel
                        {
                            PostID = p.PostID,
                            UserID = p.UserID,
                            DonorName = p.User?.UserDetails?.FullName ?? "Chưa cập nhật",
                            DonorEmail = p.User?.Email ?? "",
                            DonorAvatarUrl = p.User?.UserDetails?.ProfilePictureURL,
                            Location = ExtractValue(lines, "Địa điểm:"),
                            DateOfExpression = ParseDate(ExtractValue(lines, "Ngày vắt:")),
                            DietInfo = ExtractValue(lines, "Chế độ ăn:"),
                            StorageInfo = ExtractValue(lines, "Bảo quản:"),
                            VerificationTier = p.VerificationTier,
                            Status = p.Status,
                            CreatedAt = p.CreatedAt
                        };
                    })
                    .ToList();

                int totalPosts = query.Count();

                var viewModel = new AdminMilkPostsViewModel
                {
                    Posts = posts,
                    CurrentPage = pageNumber,
                    TotalPages = (int)Math.Ceiling((double)totalPosts / pageSize),
                    TotalPosts = totalPosts,
                    StatusFilter = statusFilter
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi: " + ex.Message;
                return View(new AdminMilkPostsViewModel());
            }
        }

        // GET: Admin/MilkDonation/Details/5
        public ActionResult Details(long id)
        {
            try
            {
                var post = db.MilkDonationPosts
                    .Include(p => p.User)
                    .Include(p => p.User.UserDetails)
                    .FirstOrDefault(p => p.PostID == id);

                if (post == null)
                {
                    ViewBag.Error = "Không tìm thấy bài đăng.";
                    return RedirectToAction("Index");
                }

                var lines = (post.Content ?? "").Split('\n');
                var viewModel = new AdminMilkPostDetailViewModel
                {
                    PostID = post.PostID,
                    UserID = post.UserID,
                    DonorName = post.User?.UserDetails?.FullName ?? "Chưa cập nhật",
                    DonorEmail = post.User?.Email ?? "",
                    DonorAvatarUrl = post.User?.UserDetails?.ProfilePictureURL,
                    Location = ExtractValue(lines, "Địa điểm:"),
                    DateOfExpression = ParseDate(ExtractValue(lines, "Ngày vắt:")),
                    DietInfo = ExtractValue(lines, "Chế độ ăn:"),
                    StorageInfo = ExtractValue(lines, "Bảo quản:"),
                    Note = ExtractValue(lines, "Ghi chú:"),
                    VerificationTier = post.VerificationTier,
                    Status = post.Status,
                    CreatedAt = post.CreatedAt
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Admin/MilkDonation/ApprovePost
        [HttpPost]
        public JsonResult ApprovePost(long postId)
        {
            try
            {
                var post = db.MilkDonationPosts.Find(postId);
                if (post == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bài đăng" });
                }

                post.Status = 1; // Open
                db.SaveChanges();

                return Json(new { success = true, message = "Duyệt bài đăng thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Admin/MilkDonation/HidePost
        [HttpPost]
        public JsonResult HidePost(long postId, string reason)
        {
            try
            {
                var post = db.MilkDonationPosts.Find(postId);
                if (post == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bài đăng" });
                }

                post.Status = 2; // Closed
                db.SaveChanges();

                return Json(new { success = true, message = "Ẩn bài đăng thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Admin/MilkDonation/DeletePost
        [HttpPost]
        public JsonResult DeletePost(long postId, string reason)
        {
            try
            {
                var post = db.MilkDonationPosts.Find(postId);
                if (post == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bài đăng" });
                }

                db.MilkDonationPosts.Remove(post);
                db.SaveChanges();

                return Json(new { success = true, message = "Xóa bài đăng thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        private string ExtractValue(string[] lines, string key)
        {
            var line = lines.FirstOrDefault(l => l.StartsWith(key));
            if (line != null)
            {
                return line.Substring(key.Length).Trim();
            }
            return "";
        }

        private DateTime ParseDate(string dateStr)
        {
            if (DateTime.TryParseExact(dateStr, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime result))
            {
                return result;
            }
            return DateTime.Today;
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
