using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using B_M.Models;
using B_M.Models.Entities;
using B_M.Models.ViewModels;

namespace B_M.Controllers
{
    public class BrandController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // Public: list all brands
        public ActionResult Index()
        {
            var brands = db.Brands.OrderBy(b => b.BrandName).ToList();
            return View(brands);
        }

        [ChildActionOnly]
        public ActionResult BrandDropdown()
        {
            var brands = db.Brands.OrderBy(b => b.BrandName).ToList();
            return PartialView("_BrandDropdown", brands);
        }

        // GET: Brand/Details/2
        public ActionResult Details(int id)
        {
            var brand = db.Brands.FirstOrDefault(b => b.BrandID == id);
            if (brand == null)
            {
                return HttpNotFound();
            }

            // Get all active products for this brand
            var products = db.Products
                .Include(p => p.Brand)
                .Include(p => p.ProductImages)
                .Where(p => p.BrandId == id && p.IsActive)
                .OrderByDescending(p => p.Id)
                .ToList();

            var viewModel = new BrandDetailViewModel
            {
                BrandInfo = brand,
                Products = products
            };

            // Explicitly specify view path to avoid routing conflicts
            return View("~/Views/Brand/Details.cshtml", viewModel);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}


