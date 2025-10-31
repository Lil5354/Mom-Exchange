using System.Web.Mvc;

namespace B_M.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            // Đăng ký route với namespace rõ ràng để tránh conflict với controllers thường
            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new { controller = "Admin", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "B_M.Areas.Admin.Controllers" } // Chỉ tìm controller trong namespace Admin
            ).DataTokens["area"] = "Admin"; // Đảm bảo area được chỉ định rõ ràng
        }
    }
}
