using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using B_M.Filters;
using B_M.Models;

namespace B_M.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class BrandsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Brands
        public ActionResult Index()
        {
            try
            {
                var brands = db.Brands
                    .Include(b => b.User)
                    .Include(b => b.User.UserDetails)
                    .Include(b => b.CategoryPermissions)
                    .OrderBy(b => b.BrandName)
                    .ToList();

                var viewModel = new AdminBrandsViewModel
                {
                    Brands = brands.Select(b => new AdminBrandViewModel
                    {
                        BrandID = b.BrandID,
                        BrandName = b.BrandName,
                        Description = b.Description,
                        LogoUrl = b.LogoUrl,
                        UserID = b.UserID,
                        UserEmail = b.User?.Email ?? "N/A",
                        UserFullName = b.User?.UserDetails?.FullName ?? "N/A",
                        IsUserActive = b.User?.IsActive ?? false,
                        CategoryPermissionCount = b.CategoryPermissions.Count,
                        ProductCount = b.Products.Count
                    }).ToList(),
                    TotalBrands = brands.Count,
                    ActiveBrands = brands.Count(b => b.User != null && b.User.IsActive),
                    BrandUsers = db.Users.Count(u => u.Role == 3) // Role 3 = Brand
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi khi tải danh sách nhãn hàng: " + ex.Message;
                return View(new AdminBrandsViewModel());
            }
        }

        // GET: Admin/Brands/Create
        public ActionResult Create()
        {
            try
            {
                var model = new AdminBrandViewModel();
                LoadBrandUsers();
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi khi tải trang tạo nhãn hàng: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Admin/Brands/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AdminBrandViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    LoadBrandUsers();
                    return View(model);
                }

                // Validate UserID exists and has Role = 3
                var user = db.Users.Find(model.UserID);
                if (user == null)
                {
                    ModelState.AddModelError("UserID", "Không tìm thấy người dùng");
                    LoadBrandUsers();
                    return View(model);
                }

                if (user.Role != 3)
                {
                    ModelState.AddModelError("UserID", "Người dùng này không có quyền Brand (Role = 3)");
                    LoadBrandUsers();
                    return View(model);
                }

                // Check if user already has a brand
                if (db.Brands.Any(b => b.UserID == model.UserID))
                {
                    ModelState.AddModelError("UserID", "Người dùng này đã có nhãn hàng");
                    LoadBrandUsers();
                    return View(model);
                }

                // Check duplicate brand name
                if (db.Brands.Any(b => b.BrandName.ToLower() == model.BrandName.ToLower()))
                {
                    ModelState.AddModelError("BrandName", "Tên nhãn hàng đã tồn tại");
                    LoadBrandUsers();
                    return View(model);
                }

                var brand = new Brand
                {
                    BrandName = model.BrandName.Trim(),
                    Description = model.Description?.Trim(),
                    LogoUrl = model.LogoUrl?.Trim(),
                    UserID = model.UserID
                };

                db.Brands.Add(brand);
                db.SaveChanges();

                TempData["SuccessMessage"] = $"Đã tạo nhãn hàng '{brand.BrandName}' thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi khi tạo nhãn hàng: " + ex.Message);
                LoadBrandUsers();
                return View(model);
            }
        }

        // GET: Admin/Brands/Edit/5
        public ActionResult Edit(int id)
        {
            try
            {
                var brand = db.Brands
                    .Include(b => b.User)
                    .Include(b => b.User.UserDetails)
                    .FirstOrDefault(b => b.BrandID == id);

                if (brand == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy nhãn hàng!";
                    return RedirectToAction("Index");
                }

                var model = new AdminBrandViewModel
                {
                    BrandID = brand.BrandID,
                    BrandName = brand.BrandName,
                    Description = brand.Description,
                    LogoUrl = brand.LogoUrl,
                    UserID = brand.UserID,
                    UserEmail = brand.User?.Email,
                    UserFullName = brand.User?.UserDetails?.FullName
                };

                LoadBrandUsers();
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi khi tải thông tin nhãn hàng: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Admin/Brands/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AdminBrandViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    LoadBrandUsers();
                    return View(model);
                }

                var brand = db.Brands.Find(model.BrandID);
                if (brand == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy nhãn hàng!";
                    return RedirectToAction("Index");
                }

                // Validate UserID exists and has Role = 3
                var user = db.Users.Find(model.UserID);
                if (user == null)
                {
                    ModelState.AddModelError("UserID", "Không tìm thấy người dùng");
                    LoadBrandUsers();
                    return View(model);
                }

                if (user.Role != 3)
                {
                    ModelState.AddModelError("UserID", "Người dùng này không có quyền Brand (Role = 3)");
                    LoadBrandUsers();
                    return View(model);
                }

                // Check if another user already has a brand (excluding current brand)
                if (db.Brands.Any(b => b.UserID == model.UserID && b.BrandID != model.BrandID))
                {
                    ModelState.AddModelError("UserID", "Người dùng này đã có nhãn hàng khác");
                    LoadBrandUsers();
                    return View(model);
                }

                // Check duplicate brand name (excluding current brand)
                if (db.Brands.Any(b => b.BrandName.ToLower() == model.BrandName.ToLower() && b.BrandID != model.BrandID))
                {
                    ModelState.AddModelError("BrandName", "Tên nhãn hàng đã tồn tại");
                    LoadBrandUsers();
                    return View(model);
                }

                brand.BrandName = model.BrandName.Trim();
                brand.Description = model.Description?.Trim();
                brand.LogoUrl = model.LogoUrl?.Trim();
                brand.UserID = model.UserID;

                db.SaveChanges();

                TempData["SuccessMessage"] = $"Đã cập nhật nhãn hàng '{brand.BrandName}' thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi khi cập nhật nhãn hàng: " + ex.Message);
                LoadBrandUsers();
                return View(model);
            }
        }

        // POST: Admin/Brands/Delete/5
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var brand = db.Brands.Find(id);
                if (brand == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy nhãn hàng!" });
                }

                // Check if brand has products
                if (db.ProductB2Cs.Any(p => p.BrandID == id))
                {
                    return Json(new { success = false, message = "Không thể xóa nhãn hàng đã có sản phẩm!" });
                }

                // Check if brand has orders
                if (db.Orders.Any(o => o.BrandID == id))
                {
                    return Json(new { success = false, message = "Không thể xóa nhãn hàng đã có đơn hàng!" });
                }

                // Remove category permissions first
                var permissions = db.BrandCategoryPermissions.Where(p => p.BrandID == id);
                db.BrandCategoryPermissions.RemoveRange(permissions);

                // Remove brand
                db.Brands.Remove(brand);
                db.SaveChanges();

                return Json(new { success = true, message = $"Đã xóa nhãn hàng '{brand.BrandName}' thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi khi xóa nhãn hàng: " + ex.Message });
            }
        }

        // GET: Admin/Brands/Permissions/5
        public ActionResult Permissions(int id)
        {
            try
            {
                var brand = db.Brands
                    .Include(b => b.User)
                    .Include(b => b.User.UserDetails)
                    .Include(b => b.CategoryPermissions)
                    .FirstOrDefault(b => b.BrandID == id);

                if (brand == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy nhãn hàng!";
                    return RedirectToAction("Index");
                }

                // Get all B2C-enabled categories
                var allCategories = db.Categories
                    .Where(c => c.IsB2CEnabled)
                    .OrderBy(c => c.CategoryName)
                    .ToList();

                // Get current permissions for this brand
                var currentPermissions = brand.CategoryPermissions
                    .Select(p => p.CategoryID)
                    .ToList();

                var viewModel = new BrandPermissionViewModel
                {
                    BrandID = brand.BrandID,
                    BrandName = brand.BrandName,
                    UserEmail = brand.User?.Email ?? "N/A",
                    UserFullName = brand.User?.UserDetails?.FullName ?? "N/A",
                    Categories = allCategories.Select(c => new CategoryPermissionItem
                    {
                        CategoryID = c.CategoryID,
                        CategoryName = c.CategoryName,
                        Description = c.Description,
                        FullPath = GetCategoryPath(c.CategoryID),
                        IsGranted = currentPermissions.Contains(c.CategoryID)
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi khi tải phân quyền nhãn hàng: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Admin/Brands/SavePermissions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SavePermissions(BrandPermissionViewModel model)
        {
            try
            {
                var brand = db.Brands.Find(model.BrandID);
                if (brand == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy nhãn hàng!";
                    return RedirectToAction("Index");
                }

                // Get selected category IDs
                var selectedCategoryIds = model.Categories
                    .Where(c => c.IsGranted)
                    .Select(c => c.CategoryID)
                    .ToList();

                // Remove all existing permissions for this brand
                var existingPermissions = db.BrandCategoryPermissions
                    .Where(p => p.BrandID == model.BrandID)
                    .ToList();

                db.BrandCategoryPermissions.RemoveRange(existingPermissions);

                // Add new permissions
                foreach (var categoryId in selectedCategoryIds)
                {
                    // Verify category is B2C enabled
                    var category = db.Categories.Find(categoryId);
                    if (category != null && category.IsB2CEnabled)
                    {
                        var permission = new BrandCategoryPermission
                        {
                            BrandID = model.BrandID,
                            CategoryID = categoryId
                        };
                        db.BrandCategoryPermissions.Add(permission);
                    }
                }

                db.SaveChanges();

                TempData["SuccessMessage"] = $"Đã cập nhật phân quyền cho nhãn hàng '{brand.BrandName}' thành công!";
                return RedirectToAction("Permissions", new { id = model.BrandID });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi khi lưu phân quyền: " + ex.Message;
                return RedirectToAction("Permissions", new { id = model.BrandID });
            }
        }

        // Helper Methods
        private void LoadBrandUsers()
        {
            try
            {
                // Get users with Role = 3 (Brand) who don't have a brand yet
                var existingBrandUserIds = db.Brands.Select(b => b.UserID).ToList();
                
                var availableUsers = db.Users
                    .Include(u => u.UserDetails)
                    .Where(u => u.Role == 3 && u.IsActive && !existingBrandUserIds.Contains(u.UserID))
                    .ToList()
                    .Select(u => new
                    {
                        u.UserID,
                        DisplayName = $"{u.UserDetails?.FullName ?? "N/A"} ({u.Email})"
                    })
                    .OrderBy(u => u.DisplayName)
                    .ToList();

                ViewBag.BrandUsers = new SelectList(availableUsers, "UserID", "DisplayName");
                
                // Also load all brand users for editing
                var allBrandUsers = db.Users
                    .Include(u => u.UserDetails)
                    .Where(u => u.Role == 3 && u.IsActive)
                    .ToList()
                    .Select(u => new
                    {
                        u.UserID,
                        DisplayName = $"{u.UserDetails?.FullName ?? "N/A"} ({u.Email})"
                    })
                    .OrderBy(u => u.DisplayName)
                    .ToList();

                ViewBag.AllBrandUsers = new SelectList(allBrandUsers, "UserID", "DisplayName");
            }
            catch (Exception)
            {
                ViewBag.BrandUsers = new SelectList(new List<object>(), "UserID", "DisplayName");
                ViewBag.AllBrandUsers = new SelectList(new List<object>(), "UserID", "DisplayName");
            }
        }

        private string GetCategoryPath(int categoryId)
        {
            var path = new List<string>();
            var category = db.Categories.Find(categoryId);

            while (category != null)
            {
                path.Insert(0, category.CategoryName);
                category = category.ParentCategoryID.HasValue
                    ? db.Categories.Find(category.ParentCategoryID.Value)
                    : null;
            }

            return string.Join(" > ", path);
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


