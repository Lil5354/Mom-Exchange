using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using B_M.Models;
using B_M.Models.Entities;
using B_M.Filters;
using B_M.Models.ViewModels;

namespace B_M.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        protected override void Dispose(bool disposing)
        {
            if (disposing) db?.Dispose();
            base.Dispose(disposing);
        }

        // GET: Admin/Product
        public ActionResult Index(string q, string category, int? brand)
        {
            var query = db.Products.Include(p => p.Brand).Include(p => p.ProductImages).AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(p => p.Name.Contains(q));
            }
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category == category);
            }
            if (brand.HasValue)
            {
                query = query.Where(p => p.BrandId == brand.Value);
            }

            ViewBag.Brands = new SelectList(db.Brands.OrderBy(b => b.BrandName).ToList(), "BrandID", "BrandName", brand);
            var categories = db.Categories.Where(c => c.IsB2CEnabled).OrderBy(c => c.CategoryName).ToList();
            ViewBag.Categories = categories.Select(c => new SelectListItem { Value = c.CategoryName, Text = c.CategoryName });

            var list = query.OrderByDescending(p => p.Id).ToList();
            return View(list);
        }

        // GET: Admin/Product/Create
        public ActionResult Create()
        {
            BindDropdowns();
            return View(new ProductCreateViewModel { IsActive = true });
        }

        // POST: Admin/Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                BindDropdowns(null, null);
                return View(model);
            }
            var product = new Product
            {
                BrandId = model.BrandId,
                Name = model.Name,
                Category = model.Category,
                Price = model.Price,
                ShortDescription = model.ShortDescription,
                DetailedDescription = model.DetailedDescription,
                Condition = model.Condition,
                Location = model.Location,
                IsActive = model.IsActive
            };
            db.Products.Add(product);
            db.SaveChanges();

            SaveImages(product, model.ProductImages);
            TempData["Success"] = "Đã tạo sản phẩm.";
            return RedirectToAction("Index");
        }

        // GET: Admin/Product/Edit/5
        public ActionResult Edit(int id)
        {
            var product = db.Products.Include(p => p.ProductImages).FirstOrDefault(p => p.Id == id);
            if (product == null) return HttpNotFound();
            // Ensure dropdown selected values are not overridden by stale ModelState
            ModelState.Remove("Category");
            ModelState.Remove("BrandId");
            BindDropdowns(product.Category, product.BrandId);
            var vm = new ProductEditViewModel
            {
                Id = product.Id,
                BrandId = product.BrandId,
                Name = product.Name,
                Category = product.Category,
                Price = product.Price,
                ShortDescription = product.ShortDescription,
                DetailedDescription = product.DetailedDescription,
                Condition = product.Condition,
                Location = product.Location,
                IsActive = product.IsActive,
                ExistingImageUrls = product.ProductImages?.OrderBy(pi => pi.SortOrder).Select(pi => pi.ImageUrl).ToList()
            };
            return View(vm);
        }

        // POST: Admin/Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProductEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                BindDropdowns(model.Category, model.BrandId);
                return View(model);
            }
            var product = db.Products.Include(p => p.ProductImages).FirstOrDefault(p => p.Id == model.Id);
            if (product == null) return HttpNotFound();

            product.BrandId = model.BrandId;
            product.Name = model.Name;
            product.Category = model.Category;
            product.Price = model.Price;
            product.ShortDescription = model.ShortDescription;
            product.DetailedDescription = model.DetailedDescription;
            product.Condition = model.Condition;
            product.Location = model.Location;
            product.IsActive = model.IsActive;
            db.Entry(product).State = EntityState.Modified;
            db.SaveChanges();

            if (!string.IsNullOrWhiteSpace(model.DeletedImageUrls))
            {
                var rawList = model.DeletedImageUrls
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => HttpUtility.UrlDecode(s.Trim()))
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();

                if (rawList.Any())
                {
                    // Build a normalized set of relative urls and filenames to match against DB
                    var normalizedUrlSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    var deleteFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var u in rawList)
                    {
                        try
                        {
                            var url = u;
                            // If absolute URL, take local path
                            Uri uri;
                            if (Uri.TryCreate(u, UriKind.Absolute, out uri))
                            {
                                url = uri.LocalPath; // e.g., /images/products/abc.png
                            }
                            // Ensure starts with ~/ or /
                            if (url.StartsWith("~/", StringComparison.Ordinal)) normalizedUrlSet.Add(url);
                            else if (url.StartsWith("/", StringComparison.Ordinal)) normalizedUrlSet.Add(url);
                            else normalizedUrlSet.Add("~/" + url.TrimStart('/'));

                            deleteFileNames.Add(Path.GetFileName(url));
                        }
                        catch
                        {
                            // Fallback just filename
                            deleteFileNames.Add(Path.GetFileName(u));
                        }
                    }

                    var imgs = db.ProductImages
                        .Where(pi => pi.ProductId == product.Id)
                        .ToList()
                        .Where(pi => normalizedUrlSet.Contains(pi.ImageUrl) ||
                                     normalizedUrlSet.Contains("/" + pi.ImageUrl.TrimStart('~')) ||
                                     normalizedUrlSet.Contains("~/" + pi.ImageUrl.TrimStart('/')) ||
                                     deleteFileNames.Contains(Path.GetFileName(pi.ImageUrl)))
                        .ToList();

                    foreach (var img in imgs)
                    {
                        try
                        {
                            // Attempt to delete physical file if present
                            var physicalPath = Server.MapPath(img.ImageUrl);
                            if (System.IO.File.Exists(physicalPath))
                            {
                                System.IO.File.Delete(physicalPath);
                            }
                        }
                        catch { /* ignore file IO errors */ }

                        db.ProductImages.Remove(img);
                    }
                    db.SaveChanges();
                }
            }

            SaveImages(product, model.ProductImages);
            TempData["Success"] = "Đã cập nhật sản phẩm.";
            return RedirectToAction("Index");
        }

        // GET: Admin/Product/Details/5
        public ActionResult Details(int id)
        {
            var product = db.Products.Include(p => p.Brand).Include(p => p.ProductImages).FirstOrDefault(p => p.Id == id);
            if (product == null) return HttpNotFound();
            return View(product);
        }

        // POST: Admin/Product/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var product = db.Products.Include(p => p.ProductImages).FirstOrDefault(p => p.Id == id);
            if (product == null) return HttpNotFound();
            db.Products.Remove(product);
            db.SaveChanges();
            TempData["Success"] = "Đã xóa sản phẩm.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ToggleStatus(int id)
        {
            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();
            product.IsActive = !product.IsActive;
            db.Entry(product).State = EntityState.Modified;
            db.SaveChanges();
            return Json(new { success = true, active = product.IsActive });
        }

        private void BindDropdowns(string categoryName = null, int? brandId = null)
        {
            ViewBag.BrandId = new SelectList(db.Brands.OrderBy(b => b.BrandName).ToList(), "BrandID", "BrandName", brandId);
            var categories = db.Categories.Where(c => c.IsB2CEnabled).OrderBy(c => c.CategoryName).ToList();
            // Provide both formats for compatibility with different views
            ViewBag.Category = new SelectList(categories, "CategoryName", "CategoryName", categoryName);
            ViewBag.Categories = categories.Select(c => new SelectListItem { Value = c.CategoryName, Text = c.CategoryName, Selected = c.CategoryName == categoryName }).ToList();
        }

        private void SaveImages(Product product, IEnumerable<HttpPostedFileBase> images)
        {
            if (images == null) return;
            var uploadDir = Server.MapPath("~/images/products");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);
            
            // Get max SortOrder and ensure unique sequential ordering
            var existingImages = db.ProductImages.Where(pi => pi.ProductId == product.Id).OrderBy(pi => pi.SortOrder).ToList();
            var nextSortOrder = existingImages.Any() ? existingImages.Max(pi => pi.SortOrder) + 1 : 0;
            
            var index = 0;
            foreach (var file in images)
            {
                if (file == null || file.ContentLength == 0) continue;
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var path = Path.Combine(uploadDir, fileName);
                file.SaveAs(path);
                var relative = "~/images/products/" + fileName;
                
                db.ProductImages.Add(new ProductImage
                {
                    ProductId = product.Id,
                    ImageUrl = relative,
                    SortOrder = nextSortOrder + index
                });
                index++;
            }
            db.SaveChanges();
        }

        // One-time migration to move images from App_Data/MedicalRecords to images/products and fix URLs
        [HttpPost]
        public ActionResult MigrateImageStorage()
        {
            var legacyPrefix = "~/App_Data/MedicalRecords/";
            var targetPrefix = "~/images/products/";
            var legacyDir = Server.MapPath("~/App_Data/MedicalRecords");
            var targetDir = Server.MapPath("~/images/products");
            if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

            var affected = db.ProductImages.Where(pi => pi.ImageUrl.StartsWith(legacyPrefix)).ToList();
            int moved = 0;
            foreach (var pi in affected)
            {
                try
                {
                    var fileName = Path.GetFileName(pi.ImageUrl);
                    var srcPath = Path.Combine(legacyDir, fileName);
                    if (!System.IO.File.Exists(srcPath)) continue;
                    var newName = Guid.NewGuid().ToString() + Path.GetExtension(fileName);
                    var dstPath = Path.Combine(targetDir, newName);
                    System.IO.File.Copy(srcPath, dstPath, true);
                    pi.ImageUrl = targetPrefix + newName;
                    db.Entry(pi).State = EntityState.Modified;
                    moved++;
                }
                catch { /* ignore; continue */ }
            }
            db.SaveChanges();
            return Json(new { success = true, moved });
        }
    }
}

 