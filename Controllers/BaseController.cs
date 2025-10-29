using B_M.Models;
<<<<<<< HEAD
using B_M.Repositories;
=======
>>>>>>> Khoa
using System.Web.Mvc;

namespace B_M.Controllers
{
    public class BaseController : Controller
    {
<<<<<<< HEAD
        private readonly B_M.Repositories.UserRepository userRepository;

        public BaseController()
        {
            userRepository = new B_M.Repositories.UserRepository();
=======
        private readonly UserRepository userRepository;

        public BaseController()
        {
            userRepository = new UserRepository();
>>>>>>> Khoa
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Set user avatar for header if user is authenticated
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    var userIdentity = User.Identity.Name;
                    var user = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                    
                    if (user != null && user.UserDetails != null)
                    {
                        // Only set avatar URL if user has actually uploaded a profile picture
                        if (!string.IsNullOrEmpty(user.UserDetails.ProfilePictureURL))
                        {
                            ViewBag.UserAvatarURL = user.UserDetails.ProfilePictureURL;
                        }
                        else
                        {
                            ViewBag.UserAvatarURL = null; // Use default icon
                        }
                        
                        ViewBag.UserFullName = user.UserDetails.FullName ?? "User";
                    }
                    else
                    {
                        ViewBag.UserAvatarURL = null; // Use default icon
                        ViewBag.UserFullName = "User";
                    }
                }
                catch
                {
                    ViewBag.UserAvatarURL = null; // Use default icon
                    ViewBag.UserFullName = "User";
                }
            }

            base.OnActionExecuting(filterContext);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                userRepository?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
