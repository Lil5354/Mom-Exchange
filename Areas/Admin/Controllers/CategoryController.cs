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
    public class CategoryController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Category
        public ActionResult Index()
        {
            try
            {
                var categories = db.Categories.ToList();
                var rootCategories = BuildCategoryTree(categories, null);

                var viewModel = new CategoryIndexViewModel
                {
                    RootCategories = rootCategories,
                    TotalCategories = categories.Count,
                    B2CCategories = categories.Count(c => c.IsB2CEnabled),
                    C2CCategories = categories.Count(c => c.IsC2CEnabled),
                    BothEnabledCategories = categories.Count(c => c.IsB2CEnabled && c.IsC2CEnabled)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi khi tải danh sách danh mục: " + ex.Message;
                return View(new CategoryIndexViewModel());
            }
        }

        // GET: Admin/Category/Create
        public ActionResult Create(int? parentId)
        {
            try
            {
                var model = new AdminCategoryViewModel
                {
                    ParentCategoryID = parentId,
                    IsB2CEnabled = false,
                    IsC2CEnabled = false
                };

                // Load parent category info if creating subcategory
                if (parentId.HasValue)
                {
                    var parentCategory = db.Categories.Find(parentId.Value);
                    if (parentCategory != null)
                    {
                        model.ParentCategoryName = parentCategory.CategoryName;
                    }
                }

                // Load available parent categories (exclude current category and its descendants)
                model.AvailableParentCategories = db.Categories
                    .Where(c => c.ParentCategoryID == null) // Only root categories can be parents
                    .ToList();

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi khi tải trang tạo danh mục: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Admin/Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AdminCategoryViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Reload available parent categories
                    model.AvailableParentCategories = db.Categories
                        .Where(c => c.ParentCategoryID == null)
                        .ToList();
                    return View(model);
                }

                // Check for duplicate category name
                bool isDuplicate = db.Categories.Any(c => 
                    c.CategoryName.ToLower() == model.CategoryName.ToLower() &&
                    c.ParentCategoryID == model.ParentCategoryID);

                if (isDuplicate)
                {
                    ModelState.AddModelError("CategoryName", "Tên danh mục đã tồn tại trong cùng cấp.");
                    model.AvailableParentCategories = db.Categories
                        .Where(c => c.ParentCategoryID == null)
                        .ToList();
                    return View(model);
                }

                var category = new Category
                {
                    CategoryName = model.CategoryName.Trim(),
                    Description = model.Description?.Trim(),
                    ParentCategoryID = model.ParentCategoryID,
                    IsB2CEnabled = model.IsB2CEnabled,
                    IsC2CEnabled = model.IsC2CEnabled
                };

                db.Categories.Add(category);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Tạo danh mục thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi khi tạo danh mục: " + ex.Message;
                model.AvailableParentCategories = db.Categories
                    .Where(c => c.ParentCategoryID == null)
                    .ToList();
                return View(model);
            }
        }

        // GET: Admin/Category/Edit/5
        public ActionResult Edit(int id)
        {
            try
            {
                var category = db.Categories.Find(id);
                if (category == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy danh mục.";
                    return RedirectToAction("Index");
                }

                var model = new AdminCategoryViewModel
                {
                    CategoryID = category.CategoryID,
                    CategoryName = category.CategoryName,
                    Description = category.Description,
                    ParentCategoryID = category.ParentCategoryID,
                    IsB2CEnabled = category.IsB2CEnabled,
                    IsC2CEnabled = category.IsC2CEnabled
                };

                // Load parent category info
                if (category.ParentCategoryID.HasValue)
                {
                    var parentCategory = db.Categories.Find(category.ParentCategoryID.Value);
                    if (parentCategory != null)
                    {
                        model.ParentCategoryName = parentCategory.CategoryName;
                    }
                }

                // Load available parent categories (exclude current category and its descendants)
                model.AvailableParentCategories = db.Categories
                    .Where(c => c.CategoryID != id && c.ParentCategoryID == null)
                    .ToList();

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi khi tải thông tin danh mục: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Admin/Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AdminCategoryViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.AvailableParentCategories = db.Categories
                        .Where(c => c.CategoryID != model.CategoryID && c.ParentCategoryID == null)
                        .ToList();
                    return View(model);
                }

                var category = db.Categories.Find(model.CategoryID);
                if (category == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy danh mục.";
                    return RedirectToAction("Index");
                }

                // Check for duplicate category name (exclude current category)
                bool isDuplicate = db.Categories.Any(c => 
                    c.CategoryID != model.CategoryID &&
                    c.CategoryName.ToLower() == model.CategoryName.ToLower() &&
                    c.ParentCategoryID == model.ParentCategoryID);

                if (isDuplicate)
                {
                    ModelState.AddModelError("CategoryName", "Tên danh mục đã tồn tại trong cùng cấp.");
                    model.AvailableParentCategories = db.Categories
                        .Where(c => c.CategoryID != model.CategoryID && c.ParentCategoryID == null)
                        .ToList();
                    return View(model);
                }

                category.CategoryName = model.CategoryName.Trim();
                category.Description = model.Description?.Trim();
                category.ParentCategoryID = model.ParentCategoryID;
                category.IsB2CEnabled = model.IsB2CEnabled;
                category.IsC2CEnabled = model.IsC2CEnabled;

                db.SaveChanges();

                TempData["SuccessMessage"] = "Cập nhật danh mục thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi khi cập nhật danh mục: " + ex.Message;
                model.AvailableParentCategories = db.Categories
                    .Where(c => c.CategoryID != model.CategoryID && c.ParentCategoryID == null)
                    .ToList();
                return View(model);
            }
        }

        // POST: Admin/Category/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Delete(int id)
        {
            try
            {
                var category = db.Categories
                    .Include(c => c.SubCategories)
                    .Include(c => c.PostC2Cs)
                    .FirstOrDefault(c => c.CategoryID == id);

                if (category == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy danh mục." });
                }

                // Check if category has subcategories
                if (category.SubCategories.Any())
                {
                    return Json(new { success = false, message = "Không thể xóa danh mục có danh mục con. Vui lòng xóa các danh mục con trước." });
                }

                // Check if category has posts
                if (category.PostC2Cs.Any())
                {
                    return Json(new { success = false, message = "Không thể xóa danh mục đang có bài đăng. Vui lòng di chuyển hoặc xóa các bài đăng trước." });
                }

                db.Categories.Remove(category);
                db.SaveChanges();

                return Json(new { success = true, message = "Xóa danh mục thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi khi xóa danh mục: " + ex.Message });
            }
        }

        // Helper method to build category tree
        private List<CategoryTreeNode> BuildCategoryTree(List<Category> categories, int? parentId, int level = 0)
        {
            return categories
                .Where(c => c.ParentCategoryID == parentId)
                .Select(c => new CategoryTreeNode
                {
                    CategoryID = c.CategoryID,
                    CategoryName = c.CategoryName,
                    Description = c.Description,
                    ParentCategoryID = c.ParentCategoryID,
                    IsB2CEnabled = c.IsB2CEnabled,
                    IsC2CEnabled = c.IsC2CEnabled,
                    Level = level,
                    Children = BuildCategoryTree(categories, c.CategoryID, level + 1)
                })
                .ToList();
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
