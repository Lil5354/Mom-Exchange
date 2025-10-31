using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using B_M.Models;
using B_M.Models.Entities;
using B_M.Services;

namespace B_M.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();
        private readonly PayOSService payOSService = new PayOSService();

        // GET: Payment/Success - Payment success page
        public ActionResult Success(string orderCode, string status, bool cancel)
        {
            try
            {
                if (!string.IsNullOrEmpty(orderCode))
                {
                    // Get payment link by PayOS link ID
                    var paymentLink = db.PayOSPaymentLinks.FirstOrDefault(x => x.PayOSLinkId == orderCode);
                    
                    if (paymentLink != null)
                    {
                        ViewBag.OrderCode = paymentLink.OrderCode;
                        var order = db.Orders.Include(o => o.OrderItems).FirstOrDefault(o => o.OrderCode == paymentLink.OrderCode);
                        
                        if (order != null)
                        {
                            ViewBag.Order = order;
                            
                            // Verify payment with PayOS before updating
                            if (status == "PAID" && !cancel)
                            {
                                var isPaymentVerified = payOSService.VerifyPaymentStatus(orderCode);
                                
                                if (isPaymentVerified)
                                {
                                    // Get full payment info from PayOS
                                    var paymentInfo = payOSService.GetPaymentInfo(orderCode);
                                    
                                    // Update order only if verified
                                    if (paymentInfo != null && paymentInfo.Data.Status == "PAID")
                                    {
                                        // Process and update inside a transaction
                                        using (var tx = db.Database.BeginTransaction())
                                        {
                                            try
                                            {
                                                ProcessPayment(order, paymentLink);
                                                db.SaveChanges();
                                                tx.Commit();
                                            }
                                            catch
                                            {
                                                tx.Rollback();
                                                throw;
                                            }
                                        }
                                        
                                        ViewBag.Message = "Thanh toán thành công!";
                                    }
                                    else
                                    {
                                        ViewBag.Message = "Đang xác thực thanh toán...";
                                    }
                                }
                                else
                                {
                                    ViewBag.Message = "Thanh toán đang được xử lý. Vui lòng đợi trong giây lát.";
                                }
                            }
                            else
                            {
                                ViewBag.Message = "Thanh toán không thành công hoặc đã bị hủy.";
                            }
                        }
                    }
                    else
                    {
                        ViewBag.Message = "Không tìm thấy thông tin thanh toán.";
                    }
                }
                
                return View("Success");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                ViewBag.Message = "Có lỗi xảy ra: " + ex.Message;
                return View("Success");
            }
        }

        // GET: Payment/Cancel - Payment cancelled page
        public ActionResult Cancel(string orderCode)
        {
            ViewBag.Message = "Bạn đã hủy thanh toán.";
            ViewBag.OrderCode = orderCode;
            
            if (!string.IsNullOrEmpty(orderCode))
            {
                var order = db.Orders.FirstOrDefault(o => o.OrderCode == orderCode);
                if (order != null)
                {
                    ViewBag.Order = order;
                }
            }
            
            return View();
        }

        // POST: Payment/Webhook - Handle PayOS webhook
        [HttpPost]
        public ActionResult Webhook(PayOSWebhookData data)
        {
            try
            {
                // Verify webhook signature
                var dataJson = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                var jsonObj = Newtonsoft.Json.Linq.JObject.Parse(dataJson);
                jsonObj.Remove("signature");
                
                var cryptoProvider = payOSService.GetCryptoProvider();
                var checksumKey = payOSService.GetChecksumKey();
                var dataDict = cryptoProvider.ConvertJObjectToDictionary(jsonObj);
                var calculatedSignature = cryptoProvider.CreateSignatureFromObject(dataDict, checksumKey);
                
                if (!calculatedSignature.Equals(data.Signature, StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpStatusCodeResult(400, "Invalid signature");
                }

                // Get payment link and order
                var payOSLinkId = data.Data.OrderCode.ToString();
                var paymentLink = db.PayOSPaymentLinks.FirstOrDefault(x => x.PayOSLinkId == payOSLinkId);
                
                if (paymentLink == null)
                {
                    return new HttpStatusCodeResult(404, "Payment link not found");
                }
                
                var order = db.Orders.Include(o => o.OrderItems).FirstOrDefault(o => o.OrderCode == paymentLink.OrderCode);

                if (order != null && data.Code == 0 && data.Data.Status == "PAID") // Success and PAID
                {
                    // Process payment only once
                    if (order.Status != 2)
                    {
                        using (var tx = db.Database.BeginTransaction())
                        {
                            try
                            {
                                ProcessPayment(order, paymentLink);
                                db.SaveChanges();
                                tx.Commit();
                            }
                            catch
                            {
                                tx.Rollback();
                                throw;
                            }
                        }
                    }
                }

                return new HttpStatusCodeResult(200);
            }
            catch (Exception ex)
            {
                // Log error - you should add proper logging here
                System.Diagnostics.Debug.WriteLine($"Webhook Error: {ex.Message}");
                return new HttpStatusCodeResult(500);
            }
        }

        // Process payment and update system by business steps
        private void ProcessPayment(Order order, PayOSPaymentLink paymentLink)
        {
            // 2) Re-check stock
            foreach (var oi in order.OrderItems)
            {
                if (oi.ProductId.HasValue)
                {
                    var product = db.Products.Find(oi.ProductId.Value);
                    if (product == null || product.StockQuantity < oi.Quantity)
                    {
                        throw new Exception($"Sản phẩm \"{oi.ProductName}\" không đủ số lượng trong kho.");
                    }
                }
            }

            // 3) Mark order paid (OrderStatus = 2)
            order.Status = 2; // Paid
            order.PaidAt = DateTime.Now;
            order.PaymentMethod = "PayOS";
            db.Entry(order).State = EntityState.Modified;

            // 4-5) Update stock
            foreach (var oi in order.OrderItems)
            {
                if (oi.ProductId.HasValue)
                {
                    var product = db.Products.Find(oi.ProductId.Value);
                    if (product != null)
                    {
                        product.StockQuantity -= oi.Quantity;
                        db.Entry(product).State = EntityState.Modified;
                    }
                }
            }

            // Update payment link
            paymentLink.Status = 1; // Paid
            paymentLink.PaidAt = DateTime.Now;
            db.Entry(paymentLink).State = EntityState.Modified;

            // 6) Notify buyer
            var buyerNotification = new Notification
            {
                UserID = order.CustomerId,
                Title = "Đặt hàng thành công!",
                Message = $"Đơn hàng {order.OrderCode} của bạn đã được thanh toán thành công.",
                IsRead = false,
                CreatedAt = DateTime.Now
            };
            db.Notifications.Add(buyerNotification);

            // 7) Notify all admins
            var adminUsers = db.Users.Where(u => u.Role == 1).ToList();
            foreach (var admin in adminUsers)
            {
                var adminNotification = new Notification
                {
                    UserID = admin.UserID,
                    Title = "Đơn hàng mới",
                    Message = $"Hệ thống có đơn hàng mới #{order.Id} - Mã đơn: {order.OrderCode}",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };
                db.Notifications.Add(adminNotification);
            }
        }
    }
}

