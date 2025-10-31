using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.IO;
using B_M.Filters;
using B_M.Models;
using B_M.Models.Entities;

namespace B_M.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class BrandController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Brand
        public ActionResult Index(string search)
        {
            var query = db.Brands.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();
                query = query.Where(b => b.BrandName.Contains(keyword) || b.Description.Contains(keyword));
            }

            var brands = query.OrderBy(b => b.BrandName).ToList();
            ViewBag.Search = search;
            return View(brands);
        }

        // GET: Admin/Brand/Create
        public ActionResult Create()
        {
            return View(new Brand());
        }

        // POST: Admin/Brand/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Brand model, HttpPostedFileBase logoFile)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Handle logo upload
            if (logoFile != null && logoFile.ContentLength > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(logoFile.FileName);
                var physicalDir = Server.MapPath("~/images/brands/");
                Directory.CreateDirectory(physicalDir);
                var path = Path.Combine(physicalDir, fileName);
                logoFile.SaveAs(path);
                model.LogoUrl = "~/images/brands/" + fileName;
            }

            db.Brands.Add(model);
            db.SaveChanges();
            TempData["Success"] = "Tạo nhãn hàng thành công.";
            return RedirectToAction("Index");
        }

        // GET: Admin/Brand/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var brand = db.Brands.Find(id);
            if (brand == null)
            {
                return HttpNotFound();
            }

            return View(brand);
        }

        // POST: Admin/Brand/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Brand model, HttpPostedFileBase logoFile)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Handle logo upload (replace existing if new file provided)
            if (logoFile != null && logoFile.ContentLength > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(logoFile.FileName);
                var physicalDir = Server.MapPath("~/images/brands/");
                Directory.CreateDirectory(physicalDir);
                var path = Path.Combine(physicalDir, fileName);
                logoFile.SaveAs(path);
                model.LogoUrl = "~/images/brands/" + fileName;
            }

            db.Entry(model).State = EntityState.Modified;
            db.SaveChanges();
            TempData["Success"] = "Cập nhật nhãn hàng thành công.";
            return RedirectToAction("Index");
        }

        // GET: Admin/Brand/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var brand = db.Brands.Find(id);
            if (brand == null)
            {
                return HttpNotFound();
            }

            return View(brand);
        }

        // POST: Admin/Brand/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var brand = db.Brands.Find(id);
            if (brand == null)
            {
                return HttpNotFound();
            }

            db.Brands.Remove(brand);
            db.SaveChanges();
            TempData["Success"] = "Đã xóa nhãn hàng.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}


