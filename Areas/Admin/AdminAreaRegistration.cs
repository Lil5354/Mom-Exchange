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
            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new { controller = "Admin", action = "Index", id = UrlParameter.Optional },
                new[] { "B_M.Areas.Admin.Controllers" } // Specify namespaces to prevent conflict
            ).DataTokens["area"] = "Admin";
        }
    }
}
