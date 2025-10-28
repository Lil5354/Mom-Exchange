using System;
using System.Web;
using System.Web.Mvc;
using B_M.Repositories;

namespace B_M.Filters
{
    /// <summary>
    /// Filter để kiểm tra quyền admin
    /// Chỉ cho phép user có Role = 1 (Admin) truy cập
    /// </summary>
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            // Kiểm tra xem action có [AllowAnonymous] không
            if (filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), inherit: true) ||
                filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), inherit: true))
            {
                // Action có [AllowAnonymous], bỏ qua authorization
                return;
            }

            // Kiểm tra OWIN authentication trước
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                // Chưa đăng nhập - chuyển hướng đến trang login
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary
                    {
                        { "area", "" },
                        { "controller", "Account" },
                        { "action", "Login" },
                        { "returnUrl", HttpContext.Current.Request.Url.PathAndQuery }
                    });
                return;
            }

            // Kiểm tra session data
            if (HttpContext.Current.Session["UserID"] == null)
            {
                // Nếu OWIN authenticated nhưng session chưa có, reload session
                var email = filterContext.HttpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(email))
                {
                    try
                    {
                        using (var userRepo = new B_M.Repositories.UserRepository())
                        {
                            var user = userRepo.GetUserByEmail(email);
                            if (user != null)
                            {
                                // Reload session data
                                HttpContext.Current.Session["UserID"] = user.UserID;
                                HttpContext.Current.Session["UserEmail"] = user.Email;
                                HttpContext.Current.Session["FullName"] = user.UserDetails?.FullName ?? "User";
                                HttpContext.Current.Session["Role"] = user.Role;
                                HttpContext.Current.Session["IsActive"] = user.IsActive;
                            }
                            else
                            {
                                // User không tồn tại - redirect về login
                                filterContext.Result = new RedirectToRouteResult(
                                    new System.Web.Routing.RouteValueDictionary
                                    {
                                        { "area", "" },
                                        { "controller", "Account" },
                                        { "action", "Login" }
                                    });
                                return;
                            }
                        }
                    }
                    catch
                    {
                        // Nếu không thể reload session, redirect về login
                        filterContext.Result = new RedirectToRouteResult(
                            new System.Web.Routing.RouteValueDictionary
                            {
                                { "area", "" },
                                { "controller", "Account" },
                                { "action", "Login" }
                            });
                        return;
                    }
                }
                else
                {
                    // Không có email trong OWIN identity - redirect về login
                    filterContext.Result = new RedirectToRouteResult(
                        new System.Web.Routing.RouteValueDictionary
                        {
                            { "area", "" },
                            { "controller", "Account" },
                            { "action", "Login" }
                        });
                    return;
                }
            }


            // Kiểm tra quyền admin
            var userRole = HttpContext.Current.Session["Role"];
            if (userRole == null || (byte)userRole != 1) // Role 1 = Admin
            {
                // Không có quyền admin - chuyển hướng đến trang 403
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary
                    {
                        { "controller", "Error" },
                        { "action", "Forbidden" }
                    });
                return;
            }

            // Kiểm tra tài khoản có active không
            var isActive = HttpContext.Current.Session["IsActive"];
            if (isActive == null || !(bool)isActive)
            {
                // Tài khoản bị vô hiệu hóa
                HttpContext.Current.Session.Clear();
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary
                    {
                        { "area", "" },
                        { "controller", "Account" },
                        { "action", "Login" }
                    });
                return;
            }

            base.OnAuthorization(filterContext);
        }
    }
}

