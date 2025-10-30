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
                    var parent = db.Categories.Find(parentId.Value);
                    if (parent != null)
                    {
                        model.ParentCategoryName = parent.CategoryName;
                    }
                }

                // Load available parent categories for dropdown - CHỈ KHI KHÔNG CÓ PARENT
                // Vì View chỉ hiển thị dropdown này khi !ParentCategoryID.HasValue
                if (!parentId.HasValue)
                {
                    LoadParentCategoriesDropdown(null);
                }
                
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
                // Validate required fields
                if (string.IsNullOrWhiteSpace(model.CategoryName))
                {
                    ModelState.AddModelError("CategoryName", "Tên danh mục là bắt buộc");
                }

                // Validate permissions
                if (!model.IsB2CEnabled && !model.IsC2CEnabled)
                {
                    ModelState.AddModelError("", "Phải chọn ít nhất một quyền: B2C hoặc C2C");
                }

                // Check for duplicate name at same level
                var isDuplicate = db.Categories.Any(c => 
                    c.CategoryName.ToLower() == model.CategoryName.ToLower() &&
                    c.ParentCategoryID == model.ParentCategoryID);

                if (isDuplicate)
                {
                    ModelState.AddModelError("CategoryName", "Tên danh mục đã tồn tại ở cùng cấp này");
                }

                if (!ModelState.IsValid)
                {
                    LoadParentCategoriesDropdown(model.ParentCategoryID);
                    return View(model);
                }

                // Create new category
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

                TempData["SuccessMessage"] = $"Đã tạo danh mục '{category.CategoryName}' thành công!";
                return RedirectToAction("Index");
                
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi khi lưu danh mục: " + ex.Message);
                LoadParentCategoriesDropdown(model.ParentCategoryID);
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
                    TempData["ErrorMessage"] = "Không tìm thấy danh mục!";
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

                // Load parent name for display
                if (category.ParentCategoryID.HasValue)
                {
                    var parent = db.Categories.Find(category.ParentCategoryID.Value);
                    model.ParentCategoryName = parent?.CategoryName;
                }

                LoadParentCategoriesDropdown(model.ParentCategoryID, id);
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
                var category = db.Categories.Find(model.CategoryID);
                if (category == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy danh mục!";
                    return RedirectToAction("Index");
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(model.CategoryName))
                {
                    ModelState.AddModelError("CategoryName", "Tên danh mục là bắt buộc");
                }

                // Validate permissions
                if (!model.IsB2CEnabled && !model.IsC2CEnabled)
                {
                    ModelState.AddModelError("", "Phải chọn ít nhất một quyền: B2C hoặc C2C");
                }

                // Prevent setting parent to itself or its children
                if (model.ParentCategoryID.HasValue && 
                    (model.ParentCategoryID.Value == model.CategoryID || 
                     IsDescendantOf(model.CategoryID, model.ParentCategoryID.Value)))
                {
                    ModelState.AddModelError("ParentCategoryID", "Không thể chọn chính nó hoặc danh mục con làm danh mục cha");
                }

                // Check for duplicate name at same level (excluding current category)
                var isDuplicate = db.Categories.Any(c => 
                    c.CategoryID != model.CategoryID &&
                    c.CategoryName.ToLower() == model.CategoryName.ToLower() &&
                    c.ParentCategoryID == model.ParentCategoryID);

                if (isDuplicate)
                {
                    ModelState.AddModelError("CategoryName", "Tên danh mục đã tồn tại ở cùng cấp này");
                }

                if (!ModelState.IsValid)
                {
                    LoadParentCategoriesDropdown(model.ParentCategoryID, model.CategoryID);
                    return View(model);
                }

                // Update category
                category.CategoryName = model.CategoryName.Trim();
                category.Description = model.Description?.Trim();
                category.ParentCategoryID = model.ParentCategoryID;
                category.IsB2CEnabled = model.IsB2CEnabled;
                category.IsC2CEnabled = model.IsC2CEnabled;

                db.SaveChanges();

                TempData["SuccessMessage"] = $"Đã cập nhật danh mục '{category.CategoryName}' thành công!";
                return RedirectToAction("Index");
                
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi khi cập nhật danh mục: " + ex.Message);
                LoadParentCategoriesDropdown(model.ParentCategoryID, model.CategoryID);
                return View(model);
            }
        }

        // POST: Admin/Category/Delete/5
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var category = db.Categories.Find(id);
                if (category == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy danh mục!" });
                }

                // Check if has children
                if (db.Categories.Any(c => c.ParentCategoryID == id))
                {
                    return Json(new { success = false, message = "Không thể xóa danh mục có danh mục con!" });
                }

                // Check if category has C2C posts
                if (db.PostC2Cs.Any(p => p.CategoryID == id))
                {
                    return Json(new { success = false, message = "Không thể xóa danh mục đang có bài đăng C2C." });
                }

                // Remove category
                db.Categories.Remove(category);
                db.SaveChanges();

                return Json(new { success = true, message = $"Đã xóa danh mục '{category.CategoryName}' thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi khi xóa danh mục: " + ex.Message });
            }
        }

        // Helper Methods
        private List<AdminCategoryViewModel> BuildCategoryTree(List<Category> allCategories, int? parentId, int level = 0)
        {
            return allCategories
                .Where(c => c.ParentCategoryID == parentId)
                .Select(c => new AdminCategoryViewModel
                {
                    CategoryID = c.CategoryID,
                    CategoryName = c.CategoryName,
                    Description = c.Description,
                    ParentCategoryID = c.ParentCategoryID,
                    IsB2CEnabled = c.IsB2CEnabled,
                    IsC2CEnabled = c.IsC2CEnabled,
                    Level = level,
                    SubCategories = BuildCategoryTree(allCategories, c.CategoryID, level + 1),
                    SubCategoryCount = allCategories.Count(sub => sub.ParentCategoryID == c.CategoryID)
                })
                .OrderBy(c => c.CategoryName)
                .ToList();
        }

        private void LoadParentCategoriesDropdown(int? selectedId, int? excludeId = null)
        {
            try
            {
                var categories = db.Categories
                    .Where(c => !excludeId.HasValue || c.CategoryID != excludeId.Value)
                    .OrderBy(c => c.CategoryName)
                    .ToList()
                    .Select(c => new
                    {
                        c.CategoryID,
                        DisplayName = GetCategoryPath(c.CategoryID)
                    })
                    .ToList();

                ViewBag.ParentCategories = new SelectList(categories, "CategoryID", "DisplayName", selectedId);
            }
            catch (Exception)
            {
                ViewBag.ParentCategories = new SelectList(new List<object>(), "CategoryID", "DisplayName");
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

        private bool IsDescendantOf(int ancestorId, int descendantId)
        {
            var category = db.Categories.Find(descendantId);
            while (category?.ParentCategoryID.HasValue == true)
            {
                if (category.ParentCategoryID.Value == ancestorId)
                    return true;
                category = db.Categories.Find(category.ParentCategoryID.Value);
            }
            return false;
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