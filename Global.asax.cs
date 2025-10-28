using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Helpers;

namespace B_M
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            
            // Cấu hình AntiForgeryToken cho Claims-based authentication
            AntiForgeryConfig.UniqueClaimTypeIdentifier = System.Security.Claims.ClaimTypes.Name;
        }

        protected void Application_Error()
        {
            
            var exception = Server.GetLastError();
            var httpException = exception as HttpException;
            
            if (httpException != null)
            {
                var statusCode = httpException.GetHttpCode();
                
                if (statusCode == 404)
                {
                    Response.Clear();
                    Response.StatusCode = 404;
                    Response.Redirect("~/Error/NotFound");
                }
                else if (statusCode == 500)
                {
                    Response.Clear();
                    Response.StatusCode = 500;
                    Response.Redirect("~/Error/ServerError");
                }
            }
           
        }
    }
}
