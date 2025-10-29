using System.Web.Mvc;

namespace B_M.Controllers
{
    public class ErrorController : Controller
    {
        // GET: /Error/NotFound - Main 404 page
        public ActionResult NotFound()
        {
            Response.StatusCode = 404;
            return View("NotFound");
        }

        // GET: /Error/ServerError - 500 errors
        public ActionResult ServerError()
        {
            Response.StatusCode = 500;
            Response.TrySkipIisCustomErrors = true;
            
            // Return simple HTML to avoid infinite loop if Layout has errors
            return Content(@"
                <!DOCTYPE html>
                <html>
                <head>
                    <title>500 - Lỗi máy chủ</title>
                    <style>
                        body { font-family: Arial, sans-serif; text-align: center; padding: 50px; background: #f8f9fa; }
                        .error-box { background: white; padding: 40px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); max-width: 600px; margin: 0 auto; }
                        h1 { color: #e74a3b; font-size: 72px; margin: 0; }
                        h2 { color: #333; }
                        p { color: #666; }
                        .btn { display: inline-block; padding: 10px 20px; margin: 10px; background: #d63384; color: white; text-decoration: none; border-radius: 5px; }
                        .btn:hover { background: #b02a6a; }
                    </style>
                </head>
                <body>
                    <div class='error-box'>
                        <h1>500</h1>
                        <h2>Oops! Lỗi máy chủ</h2>
                        <p>Có vẻ như máy chủ đang gặp sự cố. Chúng tôi đang khắc phục vấn đề này.</p>
                        <a href='/' class='btn'>Về trang chủ</a>
                        <a href='javascript:history.back()' class='btn'>Quay lại</a>
                    </div>
                </body>
                </html>
            ", "text/html");
        }

        // GET: /Error/Forbidden - 403 errors
        public ActionResult Forbidden()
        {
            Response.StatusCode = 403;
            return View("Forbidden");
        }

        // GET: /Error/Unauthorized - 401 errors redirect to 404
        public ActionResult Unauthorized()
        {
            Response.StatusCode = 404;
            return View("NotFound");
        }

        // Catch-all method for any other errors
        public ActionResult Index()
        {
            Response.StatusCode = 404;
            return View("NotFound");
        }

        // Handle all unmatched routes
        public ActionResult HandleNotFound()
        {
            Response.StatusCode = 404;
            return View("NotFound");
        }
    }
}
