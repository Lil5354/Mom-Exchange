using B_M.Models;
using B_M.Models.Entities;
using B_M.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace B_M.Controllers
{
    public class OrderUserController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: OrderUser/Index
        public ActionResult Index()
        {
            // Check if user is logged in
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = (int)Session["UserID"];

            // Auto-cancel expired pending orders (avoid holding stock)
            const int qrValidityMinutes = 15;
            var expiredCutoff = DateTime.Now.AddMinutes(-qrValidityMinutes);
            var expiredPendingOrders = db.Orders
                .Where(o => o.CustomerId == userId && o.Status == 0 && o.CreatedAt <= expiredCutoff)
                .ToList();

            if (expiredPendingOrders.Any())
            {
                foreach (var order in expiredPendingOrders)
                {
                    order.Status = 5; // Cancelled
                    db.Entry(order).State = System.Data.Entity.EntityState.Modified;

                    var payLink = db.PayOSPaymentLinks.FirstOrDefault(pl => pl.OrderCode == order.OrderCode);
                    if (payLink != null && payLink.Status == 0)
                    {
                        payLink.Status = 2; // Cancelled
                        db.Entry(payLink).State = System.Data.Entity.EntityState.Modified;
                    }
                }
                db.SaveChanges();
                
                if (expiredPendingOrders.Count > 0)
                {
                    TempData["Info"] = $"Đã tự động hủy {expiredPendingOrders.Count} đơn hàng quá hạn thanh toán.";
                }
            }

            // Query orders with proper includes
            var orders = db.Orders
                .Include(o => o.Brand)
                .Include(o => o.OrderItems)
                .Where(o => o.CustomerId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();
            
            // Ensure lists are initialized even if empty
            var allOrdersList = orders ?? new List<Order>();
            
            // Create ViewModel with grouped orders
            var viewModel = new OrderUserIndexViewModel
            {
                AllOrders = allOrdersList,
                PendingOrders = allOrdersList.Where(o => o.Status == 0).ToList(),
                PaidOrders = allOrdersList.Where(o => o.Status == 1).ToList(),
                ProcessingOrders = allOrdersList.Where(o => o.Status == 2).ToList(),
                ShippedOrders = allOrdersList.Where(o => o.Status == 3).ToList(),
                CompletedOrders = allOrdersList.Where(o => o.Status == 4).ToList(),
                CancelledOrders = allOrdersList.Where(o => o.Status == 5).ToList(),
                TotalCount = allOrdersList.Count,
                PendingCount = allOrdersList.Count(o => o.Status == 0),
                PaidCount = allOrdersList.Count(o => o.Status == 1),
                ProcessingCount = allOrdersList.Count(o => o.Status == 2),
                ShippedCount = allOrdersList.Count(o => o.Status == 3),
                CompletedCount = allOrdersList.Count(o => o.Status == 4),
                CancelledCount = allOrdersList.Count(o => o.Status == 5)
            };
            
            return View(viewModel);
        }

        // GET: OrderUser/Details/5
        public ActionResult Details(int id)
        {
            // Check if user is logged in
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = (int)Session["UserID"];
            var order = db.Orders
                .Include(o => o.Brand)
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.Id == id);
            
            if (order == null)
            {
                return HttpNotFound();
            }

            // Verify this order belongs to current user
            if (order.CustomerId != userId)
            {
                return new HttpUnauthorizedResult();
            }

            // Auto-cancel if pending and expired (avoid holding stock)
            if (order.Status == 0)
            {
                const int qrValidityMinutes = 15;
                var expireAt = order.CreatedAt.AddMinutes(qrValidityMinutes);
                if (DateTime.Now >= expireAt)
                {
                    order.Status = 5; // Cancelled
                    db.Entry(order).State = System.Data.Entity.EntityState.Modified;

                    var payLink = db.PayOSPaymentLinks.FirstOrDefault(pl => pl.OrderCode == order.OrderCode);
                    if (payLink != null && payLink.Status == 0)
                    {
                        payLink.Status = 2; // Cancelled
                        db.Entry(payLink).State = System.Data.Entity.EntityState.Modified;
                    }

                    db.SaveChanges();
                    TempData["Info"] = "Đơn hàng đã quá hạn và được hủy tự động.";
                }
            }

            // Brand info is included
            ViewBag.Brand = order.Brand;

            return View(order);
        }

        // POST: OrderUser/CancelOrder
        [HttpPost]
        public JsonResult CancelOrder(int orderId)
        {
            try
            {
                var userId = (int)Session["UserID"];
                var order = db.Orders.FirstOrDefault(o => o.Id == orderId);
                
                if (order == null)
                {
                    return Json(new { success = false, message = "Đơn hàng không tồn tại" });
                }

                // Verify order belongs to user
                if (order.CustomerId != userId)
                {
                    return Json(new { success = false, message = "Không có quyền hủy đơn hàng này" });
                }

                // Only allow cancellation if order is pending or paid
                if (order.Status != 0 && order.Status != 1)
                {
                    return Json(new { success = false, message = "Không thể hủy đơn hàng ở trạng thái này" });
                }

                order.Status = 5; // Cancelled
                db.Entry(order).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                return Json(new { success = true, message = "Đã hủy đơn hàng thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: OrderUser/CancelIfExpired - cancel pending order if QR validity window passed
        [HttpPost]
        public JsonResult CancelIfExpired(int orderId)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập." });
                }

                var userId = (int)Session["UserID"];
                var order = db.Orders.FirstOrDefault(o => o.Id == orderId);
                if (order == null)
                {
                    return Json(new { success = false, message = "Đơn hàng không tồn tại" });
                }

                if (order.CustomerId != userId)
                {
                    return Json(new { success = false, message = "Không có quyền xử lý đơn này" });
                }

                if (order.Status != 0)
                {
                    return Json(new { success = false, message = "Chỉ áp dụng cho đơn chờ thanh toán" });
                }

                // QR validity window (minutes)
                const int qrValidityMinutes = 15;
                var expireAt = order.CreatedAt.AddMinutes(qrValidityMinutes);
                if (DateTime.Now < expireAt)
                {
                    var remain = (expireAt - DateTime.Now);
                    return Json(new { success = false, message = $"QR chưa hết hạn. Còn lại {Math.Ceiling(remain.TotalMinutes)} phút." });
                }

                // Mark order cancelled
                order.Status = 5; // Cancelled
                db.Entry(order).State = System.Data.Entity.EntityState.Modified;

                // Also mark any related PayOS link as cancelled if still pending
                var payLink = db.PayOSPaymentLinks.FirstOrDefault(pl => pl.OrderCode == order.OrderCode);
                if (payLink != null && payLink.Status == 0)
                {
                    payLink.Status = 2; // Cancelled
                    db.Entry(payLink).State = System.Data.Entity.EntityState.Modified;
                }

                db.SaveChanges();
                return Json(new { success = true, message = "Đã hủy đơn hàng do quá hạn thanh toán." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}

