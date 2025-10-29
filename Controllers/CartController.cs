// Controllers/CartController.cs
using B_M.Models;
<<<<<<< HEAD
using B_M.Repositories;
=======
>>>>>>> Khoa
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace B_M.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext db;
<<<<<<< HEAD
        private readonly B_M.Repositories.UserRepository userRepository;
=======
        private readonly UserRepository userRepository;
>>>>>>> Khoa

        public CartController()
        {
            db = new ApplicationDbContext();
<<<<<<< HEAD
            userRepository = new B_M.Repositories.UserRepository();
=======
            userRepository = new UserRepository();
>>>>>>> Khoa
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db?.Dispose();
                userRepository?.Dispose();
            }
            base.Dispose(disposing);
        }

        // GET: Cart
        public ActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

<<<<<<< HEAD
        // GET: Cart/Summary - AJAX endpoint for cart badge
        [AllowAnonymous]
        public JsonResult Summary()
        {
            try
            {
                var cart = GetCart();
                return Json(new
                {
                    Success = true,
                    ItemCount = cart.TotalItems,
                    TotalAmount = cart.TotalAmount
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in Cart Summary: {ex.Message}");
                return Json(new
                {
                    Success = false,
                    ItemCount = 0,
                    TotalAmount = 0
                }, JsonRequestBehavior.AllowGet);
            }
        }

=======
>>>>>>> Khoa
        // POST: Cart/Add
      
        // POST: Cart/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateQuantity(UpdateCartItemViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Thông tin không hợp lệ.";
                return RedirectToAction("Index");
            }

            try
            {
                var cart = GetCart();
                var item = cart.Items.FirstOrDefault(i => i.ProductID == model.ProductID);

                if (item != null)
                {
                    if (model.Quantity == 0)
                    {
                        // Remove item if quantity is 0
                        cart.Items.Remove(item);
                        TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
                    }
                    else if (model.Quantity <= item.MaxQuantity)
                    {
                        // Update quantity
                        item.Quantity = model.Quantity;
                        TempData["SuccessMessage"] = "Đã cập nhật số lượng.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = $"Chỉ còn {item.MaxQuantity} sản phẩm có sẵn.";
                        return RedirectToAction("Index");
                    }

                    SaveCart(cart);
                }

                if (Request.IsAjaxRequest())
                {
                    return Json(new
                    {
                        success = true,
                        itemCount = cart.TotalItems,
                        totalAmount = cart.TotalAmount,
                        subTotal = model.Quantity > 0 ? (item?.SubTotal ?? 0) : 0
                    });
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in Cart UpdateQuantity: {ex.Message}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật giỏ hàng.";
                
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra!" });
                }

                return RedirectToAction("Index");
            }
        }

        // POST: Cart/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Remove(int productId)
        {
            try
            {
                var cart = GetCart();
                var item = cart.Items.FirstOrDefault(i => i.ProductID == productId);

                if (item != null)
                {
                    cart.Items.Remove(item);
                    SaveCart(cart);
                    TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
                }

                if (Request.IsAjaxRequest())
                {
                    return Json(new
                    {
                        success = true,
                        itemCount = cart.TotalItems,
                        totalAmount = cart.TotalAmount
                    });
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in Cart Remove: {ex.Message}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa sản phẩm.";
                
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra!" });
                }

                return RedirectToAction("Index");
            }
        }

       

        #region Private Helper Methods

        private CartViewModel GetCart()
        {
            var cart = Session["Cart"] as CartViewModel;
            if (cart == null)
            {
                cart = new CartViewModel();
                Session["Cart"] = cart;
            }
            return cart;
        }

        private void SaveCart(CartViewModel cart)
        {
            Session["Cart"] = cart;
        }

        private void TrackAffiliateClick(int affiliateUserId, int productId)
        {
            try
            {
                var sessionId = Session.SessionID;
                
                // Check if click already tracked for this session
                var existingClick = db.AffiliateClicks
                    .FirstOrDefault(ac => ac.AffiliatorUserID == affiliateUserId 
                                       && ac.ProductID == productId 
                                       && ac.VisitorSessionID == sessionId);

                if (existingClick == null)
                {
                    var click = new AffiliateClick
                    {
                        AffiliatorUserID = affiliateUserId,
                        ProductID = productId,
                        VisitorSessionID = sessionId,
                        ClickedAt = DateTime.Now
                    };

                    db.AffiliateClicks.Add(click);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR tracking affiliate click: {ex.Message}");
            }
        }

        #endregion
    }
}

