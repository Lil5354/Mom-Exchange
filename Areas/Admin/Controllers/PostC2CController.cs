using B_M.Filters;
using B_M.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace B_M.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class PostC2CController : Controller
    {
        private readonly ApplicationDbContext db;

        public PostC2CController()
        {
            db = new ApplicationDbContext();
        }

        // GET: Admin/PostC2C - Danh sách bài đăng C2C
        public ActionResult Index(int? page, int? statusFilter, int? categoryFilter, int? listingTypeFilter)
        {
            try
            {
                int pageSize = 20;
                int pageNumber = page ?? 1;

                var query = db.PostC2Cs
                    .Include(p => p.User)
                    .Include(p => p.User.UserDetails)
                    .Include(p => p.Category)
                    .Include(p => p.Images)
                    .AsQueryable();

                // Apply filters
                if (statusFilter.HasValue)
                {
                    query = query.Where(p => p.Status == statusFilter.Value);
                }

                if (categoryFilter.HasValue)
                {
                    query = query.Where(p => p.CategoryID == categoryFilter.Value);
                }

                if (listingTypeFilter.HasValue)
                {
                    query = query.Where(p => p.ListingType == listingTypeFilter.Value);
                }

                query = query.OrderByDescending(p => p.CreatedAt);

                var totalPosts = query.Count();

                var posts = query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList()
                    .Select(p => new AdminPostC2CItemViewModel
                    {
                        PostID = p.PostID,
                        UserID = p.UserID,
                        UserName = p.User?.UserDetails?.FullName ?? "Chưa cập nhật",
                        UserEmail = p.User?.Email ?? "",
                        UserAvatarUrl = p.User?.UserDetails?.ProfilePictureURL,
                        Title = p.Title,
                        CategoryName = p.Category?.CategoryName ?? "Chưa phân loại",
                        Condition = p.Condition,
                        Price = p.Price,
                        ListingType = p.ListingType,
                        Status = p.Status,
                        CreatedAt = p.CreatedAt,
                        ImageCount = p.Images?.Count ?? 0
                    })
                    .ToList();

                // Get statistics
                var allPosts = db.PostC2Cs.ToList();
                var viewModel = new AdminPostC2CListViewModel
                {
                    Posts = posts,
                    CurrentPage = pageNumber,
                    TotalPages = (int)Math.Ceiling((double)totalPosts / pageSize),
                    TotalPosts = totalPosts,
                    StatusFilter = statusFilter,
                    CategoryFilter = categoryFilter,
                    ListingTypeFilter = listingTypeFilter,
                    ActivePosts = allPosts.Count(p => p.Status == 1),
                    SoldPosts = allPosts.Count(p => p.Status == 2),
                    SalePosts = allPosts.Count(p => p.ListingType == 1 || p.ListingType == 3),
                    ExchangePosts = allPosts.Count(p => p.ListingType == 2 || p.ListingType == 3)
                };

                // Load categories for filter dropdown
                ViewBag.Categories = db.Categories.Where(c => c.IsC2CEnabled).ToList();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi: " + ex.Message;
                return View(new AdminPostC2CListViewModel());
            }
        }

        // GET: Admin/PostC2C/Details/5
        public ActionResult Details(long id)
        {
            try
            {
                var post = db.PostC2Cs
                    .Include(p => p.User)
                    .Include(p => p.User.UserDetails)
                    .Include(p => p.Category)
                    .Include(p => p.Images)
                    .FirstOrDefault(p => p.PostID == id);

                if (post == null)
                {
                    ViewBag.Error = "Không tìm thấy bài đăng.";
                    return RedirectToAction("Index");
                }

                var viewModel = new AdminPostC2CDetailViewModel
                {
                    PostID = post.PostID,
                    UserID = post.UserID,
                    UserName = post.User?.UserDetails?.FullName ?? "Chưa cập nhật",
                    UserEmail = post.User?.Email ?? "",
                    UserAvatarUrl = post.User?.UserDetails?.ProfilePictureURL,
                    Title = post.Title,
                    Content = post.Content,
                    CategoryName = post.Category?.CategoryName ?? "Chưa phân loại",
                    Condition = post.Condition,
                    Price = post.Price,
                    ListingType = post.ListingType,
                    Status = post.Status,
                    CreatedAt = post.CreatedAt,
                    ImageCount = post.Images?.Count ?? 0,
                    Images = post.Images?.Select(img => new PostC2CImageViewModel
                    {
                        ImageID = img.ImageID,
                        ImageUrl = img.ImageUrl,
                        FileName = System.IO.Path.GetFileName(img.ImageUrl)
                    }).ToList() ?? new List<PostC2CImageViewModel>()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Admin/PostC2C/ApprovePost
        [HttpPost]
        public JsonResult ApprovePost(long postId)
        {
            try
            {
                var post = db.PostC2Cs.Find(postId);
                if (post == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bài đăng" });
                }

                post.Status = 1; // Active
                db.SaveChanges();

                return Json(new { success = true, message = "Duyệt bài đăng thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Admin/PostC2C/HidePost
        [HttpPost]
        public JsonResult HidePost(long postId, string reason)
        {
            try
            {
                var post = db.PostC2Cs.Find(postId);
                if (post == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bài đăng" });
                }

                post.Status = 2; // Sold/Exchanged (Hidden)
                db.SaveChanges();

                return Json(new { success = true, message = "Ẩn bài đăng thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Admin/PostC2C/DeletePost
        [HttpPost]
        public JsonResult DeletePost(long postId, string reason)
        {
            try
            {
                var post = db.PostC2Cs
                    .Include(p => p.Images)
                    .FirstOrDefault(p => p.PostID == postId);

                if (post == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bài đăng" });
                }

                // Delete associated images first
                if (post.Images != null && post.Images.Any())
                {
                    db.PostC2CImages.RemoveRange(post.Images);
                }

                // Delete the post
                db.PostC2Cs.Remove(post);
                db.SaveChanges();

                return Json(new { success = true, message = "Xóa bài đăng thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
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
