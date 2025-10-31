using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using B_M.Models;
using System.Data.Entity;

namespace B_M.Controllers
{
    public class CommunityController : BaseController
    {
        private readonly ApplicationDbContext db;

        public CommunityController()
        {
            db = new ApplicationDbContext();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db?.Dispose();
            }
            base.Dispose(disposing);
        }

        // GET: Community
        public ActionResult Index()
        {
            try
            {
                // Get recent community activities
                var recentPosts = db.PostC2Cs
                    .Include(p => p.User.UserDetails)
                    .Include(p => p.Images)
                    .Where(p => p.Status == 1)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(8)
                    .ToList();

                var recentMilkDonations = db.MilkDonationPosts
                    .Include(p => p.User.UserDetails)
                    .Where(p => p.Status == 1)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(4)
                    .ToList();

                ViewBag.RecentPosts = recentPosts;
                ViewBag.RecentMilkDonations = recentMilkDonations;
                ViewBag.TotalMembers = db.Users.Count();
                ViewBag.TotalPosts = db.PostC2Cs.Where(p => p.Status == 1).Count();
                ViewBag.TotalMilkPosts = db.MilkDonationPosts.Where(p => p.Status == 1).Count();

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Có lỗi xảy ra khi tải trang cộng đồng: " + ex.Message;
                return View();
            }
        }
    }
}
