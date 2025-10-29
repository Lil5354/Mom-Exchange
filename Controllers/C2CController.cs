using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using B_M.Models;
using B_M.Repositories;

namespace B_M.Controllers
{
    public class C2CController : BaseController
    {
        private readonly ApplicationDbContext db;
        private readonly UserRepository userRepository;

        public C2CController()
        {
            db = new ApplicationDbContext();
            userRepository = new UserRepository();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db?.Dispose();
                userRepository?.Dispose();
            }
            base.Dispose(disposing);
        }

        // GET: /C2C
        public ActionResult Index(string filter = null, int? categoryId = null, int? listingType = null,
                                  decimal? minPrice = null, decimal? maxPrice = null, string province = null,
                                  string condition = null, string q = null, int page = 1, int pageSize = 20)
        {
            var query = db.PostC2Cs
                .Include(p => p.Images)
                .Include(p => p.User.UserDetails)
                .Include(p => p.ExchangePreferences.Select(ep => ep.Category))
                .Where(p => p.Status == 1);

            // Filter by navigation tab
            if (!string.IsNullOrEmpty(filter))
            {
                if (string.Equals(filter, "sell", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(p => p.ListingType == 1 || p.ListingType == 3);
                }
                else if (string.Equals(filter, "exchange", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(p => p.ListingType == 2 || p.ListingType == 3);
                }
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryID == categoryId.Value);
            }

            if (listingType.HasValue)
            {
                query = query.Where(p => p.ListingType == listingType.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price.HasValue && p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price.HasValue && p.Price <= maxPrice.Value);
            }

            if (!string.IsNullOrWhiteSpace(province))
            {
                query = query.Where(p => p.User.UserDetails != null && p.User.UserDetails.Address.Contains(province));
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(p => p.Title.Contains(q) || p.Content.Contains(q));
            }

            if (!string.IsNullOrEmpty(condition))
            {
                query = query.Where(p => p.Condition == condition);
            }

            var totalCount = query.Count();

            var items = query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList()
                .Select(p => new C2CPostCardViewModel
                {
                    PostID = p.PostID,
                    Title = p.Title,
                    ContentSnippet = p.Content.Length > 140 ? p.Content.Substring(0, 140) + "..." : p.Content,
                    Condition = p.Condition,
                    ListingType = p.ListingType,
                    Price = p.Price,
                    Location = p.User?.UserDetails?.Address ?? "",
                    PrimaryImageUrl = p.Images
                        .OrderByDescending(i => i.IsPrimary)
                        .ThenBy(i => i.ImageID)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault() ?? "/images/avatar-default.jpg",
                    SellerUserID = p.UserID,
                    SellerName = p.User?.UserDetails?.FullName ?? (p.User?.Email ?? "Người bán"),
                    CreatedAt = p.CreatedAt,
                    ExchangeCategoryNames = (p.ListingType == 2 || p.ListingType == 3)
                        ? p.ExchangePreferences.Select(ep => ep.Category.CategoryName).Take(3)
                        : new List<string>()
                })
                .ToList();

            ViewBag.Categories = db.Categories
                .Where(c => c.IsC2CEnabled)
                .OrderBy(c => c.CategoryName)
                .Select(c => new SelectListItem { Value = c.CategoryID.ToString(), Text = c.CategoryName })
                .ToList();

            ViewBag.Provinces = GetProvinceSelectList();
            var condOptions = GetConditionOptions();
            foreach (var o in condOptions) { o.Selected = string.Equals(o.Value, condition, StringComparison.Ordinal); }
            ViewBag.ConditionOptions = condOptions;
            ViewBag.TotalCount = totalCount;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Filter = filter;
            ViewBag.SelectedCondition = condition;
            ViewBag.Query = q;

            return View(items);
        }

        // GET: /C2C/Details/5
        public ActionResult Details(long id)
        {
            var post = db.PostC2Cs
                .Include(p => p.Images)
                .Include(p => p.User.UserDetails)
                .Include(p => p.Category)
                .FirstOrDefault(p => p.PostID == id);

            if (post == null)
            {
                return HttpNotFound();
            }

            var currentUser = GetCurrentUser();
            ViewBag.IsOwner = currentUser != null && currentUser.UserID == post.UserID;

            return View(post);
        }

        // GET: /C2C/Create
        [Authorize]
        public ActionResult Create()
        {
            if (!EnsureCurrentUserIsClient())
            {
                TempData["ErrorMessage"] = "Chỉ tài khoản Khách hàng mới được đăng tin.";
                return RedirectToAction("Index");
            }

            ViewBag.Categories = db.Categories
                .Where(c => c.IsC2CEnabled)
                .OrderBy(c => c.CategoryName)
                .Select(c => new SelectListItem { Value = c.CategoryID.ToString(), Text = c.CategoryName })
                .ToList();

            ViewBag.Provinces = GetProvinceSelectList();
            ViewBag.ConditionOptions = GetConditionOptions();

            return View(new C2CCreateViewModel());
        }

        // POST: /C2C/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create(C2CCreateViewModel model)
        {
            if (!EnsureCurrentUserIsClient())
            {
                return Json(new { success = false, message = "Chỉ tài khoản Khách hàng mới được đăng tin." });
            }

            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Dynamic validation per ListingType
            if (model.ListingType == 1 || model.ListingType == 3)
            {
                if (!model.Price.HasValue || model.Price.Value <= 0)
                {
                    ModelState.AddModelError("Price", "Giá bắt buộc và phải lớn hơn 0 với hình thức bán.");
                }
            }
            else if (model.ListingType == 2)
            {
                model.Price = null;
                if (model.ExchangeCategoryIDs == null || model.ExchangeCategoryIDs.Count < 2 || model.ExchangeCategoryIDs.Count > 5)
                {
                    ModelState.AddModelError("ExchangeCategoryIDs", "Chọn từ 2 đến 5 danh mục bạn muốn đổi.");
                }
            }

            // Basic policy check: disallow used diapers posts (example rule)
            var text = (model.Title ?? "") + " " + (model.Content ?? "");
            var lower = text.ToLower();
            if ((lower.Contains("tã") || lower.Contains("bỉm") || lower.Contains("ta ") || lower.Contains("bim "))
                && !string.Equals(model.Condition, "Mới 100%", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("Content", "Không được đăng tin tã/bỉm đã qua sử dụng.");
            }

            var hasAtLeastOneImage = (model.ImageFiles != null && model.ImageFiles.Any(f => f != null && f.ContentLength > 0))
                                      || (model.ImageUrls != null && model.ImageUrls.Any(u => !string.IsNullOrWhiteSpace(u)));
            if (!hasAtLeastOneImage)
            {
                ModelState.AddModelError("ImageFiles", "Cần ít nhất 1 hình ảnh.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = db.Categories
                    .Where(c => c.IsC2CEnabled)
                    .OrderBy(c => c.CategoryName)
                    .Select(c => new SelectListItem { Value = c.CategoryID.ToString(), Text = c.CategoryName })
                    .ToList();

                ViewBag.Provinces = GetProvinceSelectList();
                ViewBag.ConditionOptions = GetConditionOptions();
                return View(model);
            }

            var post = new PostC2C
            {
                UserID = currentUser.UserID,
                CategoryID = model.CategoryID,
                Title = model.Title.Trim(),
                Content = model.Content.Trim(),
                Condition = model.Condition,
                ListingType = model.ListingType,
                Price = model.Price,
                Status = 1,
                CreatedAt = DateTime.Now
            };

            db.PostC2Cs.Add(post);
            db.SaveChanges();

            var imageUrls = new List<string>();

            // 1) Handle file uploads
            if (model.ImageFiles != null)
            {
                foreach (var file in model.ImageFiles)
                {
                    var saved = SaveC2CImage(file);
                    if (!string.IsNullOrEmpty(saved))
                    {
                        imageUrls.Add(saved);
                    }
                }
            }

            // 2) Handle pasted URLs
            if (model.ImageUrls != null)
            {
                foreach (var url in model.ImageUrls)
                {
                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        imageUrls.Add(url.Trim());
                    }
                }
            }

            // Persist images
            bool first = true;
            foreach (var url in imageUrls.Take(8))
            {
                db.PostC2CImages.Add(new PostC2CImage
                {
                    PostID = post.PostID,
                    ImageUrl = url,
                    IsPrimary = first
                });
                first = false;
            }
            db.SaveChanges();

            // Exchange preferences
            if (model.ListingType == 2 || model.ListingType == 3)
            {
                if (model.ExchangeCategoryIDs != null)
                {
                    foreach (var catId in model.ExchangeCategoryIDs.Distinct().Take(5))
                    {
                        db.PostC2CExchangePreferences.Add(new PostC2CExchangePreference
                        {
                            PostID = post.PostID,
                            CategoryID = catId
                        });
                    }
                    db.SaveChanges();
                }
            }

            TempData["SuccessMessage"] = "Đăng tin thành công!";
            return RedirectToAction("Details", new { id = post.PostID });
        }

        // GET: /C2C/Edit/5
        [Authorize]
        public ActionResult Edit(long id)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return RedirectToAction("Login", "Account");

            var post = db.PostC2Cs.Include(p => p.ExchangePreferences).FirstOrDefault(p => p.PostID == id);
            if (post == null) return HttpNotFound();
            if (post.UserID != currentUser.UserID) return new HttpUnauthorizedResult();

            ViewBag.Categories = db.Categories
                .Where(c => c.IsC2CEnabled)
                .OrderBy(c => c.CategoryName)
                .Select(c => new SelectListItem { Value = c.CategoryID.ToString(), Text = c.CategoryName })
                .ToList();
            ViewBag.ConditionOptions = GetConditionOptions();

            var vm = new C2CCreateViewModel
            {
                CategoryID = post.CategoryID,
                Title = post.Title,
                Content = post.Content,
                Condition = post.Condition,
                ListingType = post.ListingType,
                Price = post.Price,
                ExchangeCategoryIDs = post.ExchangePreferences?.Select(e => e.CategoryID).ToList() ?? new List<int>()
            };

            return View(vm);
        }

        // POST: /C2C/Edit/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(long id, C2CCreateViewModel model)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return RedirectToAction("Login", "Account");

            var post = db.PostC2Cs.Include(p => p.ExchangePreferences).FirstOrDefault(p => p.PostID == id);
            if (post == null) return HttpNotFound();
            if (post.UserID != currentUser.UserID) return new HttpUnauthorizedResult();

            // Validation similar to Create
            if (model.ListingType == 1 || model.ListingType == 3)
            {
                if (!model.Price.HasValue || model.Price.Value <= 0)
                {
                    ModelState.AddModelError("Price", "Giá bắt buộc và phải lớn hơn 0 với hình thức bán.");
                }
            }
            else if (model.ListingType == 2)
            {
                model.Price = null;
                if (model.ExchangeCategoryIDs == null || model.ExchangeCategoryIDs.Count < 2 || model.ExchangeCategoryIDs.Count > 5)
                {
                    ModelState.AddModelError("ExchangeCategoryIDs", "Chọn từ 2 đến 5 danh mục bạn muốn đổi.");
                }
            }

            // Basic policy check: disallow used diapers posts (example rule)
            var text = (model.Title ?? "") + " " + (model.Content ?? "");
            var lower = text.ToLower();
            if ((lower.Contains("tã") || lower.Contains("bỉm") || lower.Contains("ta ") || lower.Contains("bim "))
                && !string.Equals(model.Condition, "Mới 100%", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("Content", "Không được đăng tin tã/bỉm đã qua sử dụng.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = db.Categories
                    .Where(c => c.IsC2CEnabled)
                    .OrderBy(c => c.CategoryName)
                    .Select(c => new SelectListItem { Value = c.CategoryID.ToString(), Text = c.CategoryName })
                    .ToList();
                ViewBag.ConditionOptions = GetConditionOptions();
                return View(model);
            }

            // Update fields
            post.CategoryID = model.CategoryID;
            post.Title = model.Title.Trim();
            post.Content = model.Content.Trim();
            post.Condition = model.Condition;
            post.ListingType = model.ListingType;
            post.Price = model.Price;

            // Update exchange preferences
            var existing = post.ExchangePreferences?.ToList() ?? new List<PostC2CExchangePreference>();
            foreach (var ep in existing)
            {
                db.PostC2CExchangePreferences.Remove(ep);
            }
            if (model.ListingType == 2 || model.ListingType == 3)
            {
                if (model.ExchangeCategoryIDs != null)
                {
                    foreach (var catId in model.ExchangeCategoryIDs.Distinct().Take(5))
                    {
                        db.PostC2CExchangePreferences.Add(new PostC2CExchangePreference
                        {
                            PostID = post.PostID,
                            CategoryID = catId
                        });
                    }
                }
            }

            db.SaveChanges();
            TempData["SuccessMessage"] = "Cập nhật tin đăng thành công!";
            return RedirectToAction("Details", new { id = post.PostID });
        }

        // POST: /C2C/Close/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Close(long id)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return RedirectToAction("Login", "Account");

            var post = db.PostC2Cs.FirstOrDefault(p => p.PostID == id);
            if (post == null) return HttpNotFound();
            if (post.UserID != currentUser.UserID) return new HttpUnauthorizedResult();

            post.Status = 2; // Sold/Exchanged
            db.SaveChanges();
            TempData["SuccessMessage"] = "Đã đóng tin đăng.";
            return RedirectToAction("MyPosts");
        }

        // GET: /C2C/MyPosts
        [Authorize]
        public ActionResult MyPosts()
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return RedirectToAction("Login", "Account");

            var myPosts = db.PostC2Cs
                .Include(p => p.Images)
                .Where(p => p.UserID == currentUser.UserID)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            return View(myPosts);
        }

        private bool EnsureCurrentUserIsClient()
        {
            var user = GetCurrentUser();
            return user != null && user.Role != 1;
        }

        private User GetCurrentUser()
        {
            var identity = User.Identity.Name;
            return userRepository.GetUserByEmail(identity) ?? userRepository.GetUserByUsername(identity);
        }

        private List<SelectListItem> GetProvinceSelectList()
        {
            var provinces = new List<string>
            {
                "TP Hà Nội", "TP Huế", "Quảng Ninh", "Cao Bằng", "Lạng Sơn",
                "Lai Châu", "Điện Biên", "Sơn La", "Thanh Hóa", "Nghệ An",
                "Hà Tĩnh", "Tuyên Quang", "Lào Cai", "Thái Nguyên", "Phú Thọ",
                "Bắc Ninh", "Hưng Yên", "TP Hải Phòng", "Ninh Bình", "Quảng Trị",
                "TP Đà Nẵng", "Quảng Ngãi", "Gia Lai", "Khánh Hòa", "Lâm Đồng",
                "Đắk Lắk", "TPHCM", "Đồng Nai", "Tây Ninh", "TP Cần Thơ",
                "Vĩnh Long", "Đồng Tháp", "Cà Mau", "An Giang"
            };

            return provinces.Select(p => new SelectListItem { Value = p, Text = p }).ToList();
        }

        private List<SelectListItem> GetConditionOptions()
        {
            var list = new[] { "Mới 100%", "Mới 95%", "Mới 90%", "Còn tốt 80%", "Đã sử dụng" };
            return list.Select(x => new SelectListItem { Value = x, Text = x }).ToList();
        }

        private string SaveC2CImage(HttpPostedFileBase file)
        {
            try
            {
                if (file == null || file.ContentLength == 0) return null;

                var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var ext = Path.GetExtension(file.FileName)?.ToLower();
                if (!allowed.Contains(ext)) return null;
                if (file.ContentLength > 5 * 1024 * 1024) return null; // 5MB

                var now = DateTime.Now;
                var dir = HttpContext.Server.MapPath($"~/images/c2c/{now:yyyy}/{now:MM}/");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                var fileName = $"c2c_{now:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{ext}";
                var path = Path.Combine(dir, fileName);
                file.SaveAs(path);

                return $"/images/c2c/{now:yyyy}/{now:MM}/{fileName}";
            }
            catch
            {
                return null;
            }
        }
    }
}


