using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using B_M.Models;
using B_M.Models.Entities;
using B_M.Helpers;
using B_M.Services;
using CartItemAlias = B_M.Helpers.CartItem;

namespace B_M.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();
        private readonly PayOSService payOSService = new PayOSService();

        // GET: /Product/Details/5
        public ActionResult Details(int id)
        {
            var product = db.Products
                .Include(p => p.Brand)
                .Include(p => p.ProductImages)
                .FirstOrDefault(p => p.Id == id && p.IsActive);

            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
        }

        // POST: /Product/AddToCart
        [HttpPost]
        public ActionResult AddToCart(int productId, int quantity = 1)
        {
            // Require login before adding to cart
            if (Session["UserID"] == null)
            {
                return Json(new { success = false, loginRequired = true, loginUrl = Url.Action("Login", "Account") });
            }

            var product = db.Products.Include(p => p.ProductImages).FirstOrDefault(p => p.Id == productId && p.IsActive);
            if (product == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại hoặc đã tạm dừng." });
            }

            if (quantity <= 0) quantity = 1;
            if (product.StockQuantity < quantity)
            {
                return Json(new { success = false, message = "Số lượng tồn kho không đủ." });
            }

            decimal unitPrice;
            decimal.TryParse((product.Price ?? "0").Replace(".", "").Replace(",", "").Replace("₫", ""), out unitPrice);

            var firstImage = product.ImageUrls != null && product.ImageUrls.Any() ? product.ImageUrls.First() : "~/images/No_Image_Available.png";
            var item = new CartItemAlias
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ProductImageUrl = firstImage,
                UnitPrice = unitPrice,
                Quantity = quantity
            };

            CartHelper.AddToCart(item);
            return Json(new { success = true, count = CartHelper.GetCartItemCount(), total = CartHelper.GetCartTotal().ToString("N0") + "₫" });
        }

        // GET: /Product/CartSummary - Returns JSON for cart summary
        public JsonResult Summary()
        {
            try
            {
                var count = CartHelper.GetCartItemCount();
                var total = CartHelper.GetCartTotal();
                return Json(new { Success = true, ItemCount = count, Total = total.ToString("N0") + "₫" }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { Success = false, ItemCount = 0, Total = "0₫" }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Product/Cart
        public ActionResult Cart()
        {
            var cart = CartHelper.GetCart();
            ViewBag.CartTotal = CartHelper.GetCartTotal();
            return View(cart);
        }

        // POST: /Product/RemoveFromCart
        [HttpPost]
        public ActionResult RemoveFromCart(int productId)
        {
            CartHelper.RemoveFromCart(productId);
            return Json(new { success = true, count = CartHelper.GetCartItemCount(), total = CartHelper.GetCartTotal().ToString("N0") + "₫" });
        }

        // POST: /Product/UpdateCartItem
        [HttpPost]
        public ActionResult UpdateCartItem(int productId, int quantity)
        {
            CartHelper.UpdateCartItem(productId, quantity);
            return Json(new { success = true, count = CartHelper.GetCartItemCount(), total = CartHelper.GetCartTotal().ToString("N0") + "₫" });
        }

        // GET: /Product/Checkout
        public ActionResult Checkout()
        {
            var cart = CartHelper.GetCart();
            if (cart == null || cart.Count == 0)
            {
                return RedirectToAction("Cart");
            }

            ViewBag.CartTotal = CartHelper.GetCartTotal();
            ViewBag.CartCount = CartHelper.GetCartItemCount();
            return View(cart);
        }

        // POST: /Product/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PlaceOrder(string shippingName, string shippingPhone, string shippingAddress, string note)
        {
            // Require login
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Checkout", "Product") });
            }

            var cart = CartHelper.GetCart();
            if (cart == null || cart.Count == 0)
            {
                TempData["Error"] = "Giỏ hàng trống.";
                return RedirectToAction("Cart");
            }

            // Group cart items by brand (materialize productIds first to avoid EF constant translation issues)
            var productIds = cart.Select(c => c.ProductId).ToList();
            var productIdToBrand = db.Products
                .Where(p => productIds.Contains(p.Id))
                .Select(p => new { p.Id, p.BrandId })
                .ToList()
                .ToDictionary(x => x.Id, x => x.BrandId);

            var groups = cart.GroupBy(ci => productIdToBrand.ContainsKey(ci.ProductId) ? productIdToBrand[ci.ProductId] : 0);

            var orderCodes = new List<string>();

            foreach (var grp in groups)
            {
                if (grp.Key == 0) continue; // skip if brand unresolved

                var order = new Order
                {
                    OrderCode = GenerateOrderCode(),
                    CustomerId = Convert.ToInt32(Session["UserID"] ?? 0),
                    BrandId = grp.Key,
                    Status = 0,
                    ShippingName = shippingName,
                    ShippingPhone = shippingPhone,
                    ShippingAddress = shippingAddress,
                    Note = note,
                    PaymentMethod = "PayOS",
                    CreatedAt = DateTime.Now
                };

                decimal subTotal = 0m;
                foreach (var ci in grp)
                {
                    var prod = db.Products.FirstOrDefault(p => p.Id == ci.ProductId);
                    if (prod == null) continue;

                    var item = new OrderItem
                    {
                        ProductId = prod.Id,
                        ProductName = prod.Name,
                        ProductPrice = prod.Price,
                        ProductImageUrl = ci.ProductImageUrl,
                        Quantity = ci.Quantity,
                        UnitPrice = ci.UnitPrice,
                        TotalPrice = ci.UnitPrice * ci.Quantity
                    };
                    subTotal += item.TotalPrice;
                    order.OrderItems.Add(item);
                }

                order.SubTotal = subTotal;
                order.Commission = Math.Round(subTotal * 0.05m, 0);
                order.TotalAmount = order.SubTotal;

                db.Orders.Add(order);
                db.SaveChanges();

                // Build PayOS items
                var paymentItems = new List<PaymentItem>();
                foreach (var ci in grp)
                {
                    paymentItems.Add(new PaymentItem
                    {
                        Name = ci.ProductName,
                        Quantity = ci.Quantity,
                        Price = (int)(ci.UnitPrice)
                    });
                }

                try
                {
                    var numericCode = GenerateNumericOrderCode();
                    var link = payOSService.CreatePaymentLink(order.TotalAmount, numericCode, paymentItems, order.OrderCode);
                    if (link != null && link.Data != null)
                    {
                        var payLink = new PayOSPaymentLink
                        {
                            PayOSLinkId = link.Data.OrderCode.ToString(),
                            OrderCode = order.OrderCode,
                            CheckoutUrl = link.Data.CheckoutUrl,
                            QrCode = link.Data.QrCode,
                            Amount = order.TotalAmount,
                            Status = 0
                        };
                        db.PayOSPaymentLinks.Add(payLink);
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"PayOS Create Link Error: {ex.Message}");
                }

                orderCodes.Add(order.OrderCode);
            }

            // Clear cart after creating orders
            CartHelper.ClearCart();

            return RedirectToAction("PaymentConfirmation", new { orderCodes = string.Join(",", orderCodes) });
        }

        public ActionResult PaymentConfirmation(string orderCodes)
        {
            if (string.IsNullOrEmpty(orderCodes))
            {
                return RedirectToAction("Index", "Home");
            }

            var codes = orderCodes.Split(',');
            var orders = db.Orders
                .Where(o => codes.Contains(o.OrderCode))
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            var links = db.PayOSPaymentLinks
                .Where(p => codes.Contains(p.OrderCode))
                .ToList()
                .GroupBy(p => p.OrderCode)
                .ToDictionary(g => g.Key, g => g.First());

            ViewBag.TotalAmount = orders.Sum(o => o.TotalAmount);
            ViewBag.PaymentLinks = links;
            return View("~/Views/Product/PaymentConfirmation.cshtml", orders);
        }

        private string GenerateOrderCode()
        {
            return "ME-" + DateTime.Now.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
        }

        private long GenerateNumericOrderCode()
        {
            var date = DateTime.Now.ToString("yyyyMMdd");
            var rnd = new Random();
            var unique = rnd.Next(10000, 99999);
            return long.Parse(date + unique.ToString());
        }
    }
}

 