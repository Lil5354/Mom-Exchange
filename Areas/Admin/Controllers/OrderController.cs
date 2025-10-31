using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using B_M.Models;
using B_M.Models.Entities;
using B_M.Filters;

namespace B_M.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        protected override void Dispose(bool disposing)
        {
            if (disposing) db?.Dispose();
            base.Dispose(disposing);
        }

        // GET: Admin/Order (Queue of paid orders)
        public ActionResult Index()
        {
            // In current codebase: 1 = Paid, 2 = Processing, 3 = Shipped
            var orders = db.Orders
                .Include(o => o.Brand)
                .Include(o => o.OrderItems)
                .Where(o => o.Status != 0 && o.Status != 5)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();
            return View(orders);
        }

        // GET: Admin/Order/Details/5
        public ActionResult Details(int id)
        {
            var order = db.Orders
                .Include(o => o.Brand)
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.Id == id);
            if (order == null) return HttpNotFound();
            return View(order);
        }

        [HttpPost]
        public JsonResult UpdateStatus(int id, byte status)
        {
            try
            {
                var order = db.Orders.Include(o => o.OrderItems).FirstOrDefault(o => o.Id == id);
                if (order == null)
                {
                    return Json(new { success = false, message = "Đơn hàng không tồn tại" });
                }

                order.Status = status;

                // Update relevant dates
                switch (status)
                {
                    case 1: // Paid
                        order.PaidAt = DateTime.Now;
                        break;
                    case 2: // Processing (Confirmed)
                        order.ConfirmedAt = DateTime.Now;
                        break;
                    case 3: // Shipped
                        order.ShippedAt = DateTime.Now;
                        // Notify buyer shipped
                        db.Notifications.Add(new Notification
                        {
                            UserID = order.CustomerId,
                            Title = "Đơn hàng đã được gửi",
                            Message = $"Đơn hàng #{order.Id} - {order.OrderCode} của bạn đã được gửi đi.",
                            IsRead = false,
                            CreatedAt = DateTime.Now
                        });
                        break;
                    case 4: // Delivered
                        order.DeliveredAt = DateTime.Now;
                        break;
                    case 5: // Cancelled
                        break;
                }

                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();

                return Json(new { success = true, message = "Cập nhật trạng thái thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}

 