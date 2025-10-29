// File: Controllers/AccountController.cs
using B_M.Models;
using B_M.Helpers;
using B_M.Repositories;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.Owin.Security;
using System.Threading.Tasks;

namespace B_M.Controllers
{
    public class AccountController : Controller
    {
        private readonly B_M.Repositories.UserRepository userRepository;

        public AccountController()
        {
            userRepository = new B_M.Repositories.UserRepository();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                userRepository?.Dispose();
            }
            base.Dispose(disposing);
        }

        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            
            // Debug: Log OWIN context
            try
            {
                var owinContext = System.Web.HttpContext.Current.GetOwinContext();
                System.Diagnostics.Debug.WriteLine($"=== LOGIN PAGE DEBUG ===");
                System.Diagnostics.Debug.WriteLine($"OWIN Context exists: {owinContext != null}");
                System.Diagnostics.Debug.WriteLine($"Authentication exists: {owinContext?.Authentication != null}");
                System.Diagnostics.Debug.WriteLine($"Current URL: {Request.Url}");
                System.Diagnostics.Debug.WriteLine($"========================");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR getting OWIN context: {ex.Message}");
            }

            return View(new LoginViewModel());
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Lấy user từ database (theo email hoặc username)
                User user = userRepository.GetUserByEmailOrUsername(model.EmailOrUsername);

                if (user == null)
                {
                    ModelState.AddModelError("", "Email/Tên đăng nhập hoặc mật khẩu không đúng.");
                    return View(model);
                }

                // Kiểm tra tài khoản có active không
                if (!user.IsActive)
                {
                    ModelState.AddModelError("", "Tài khoản của bạn đã bị vô hiệu hóa.");
                    return View(model);
                }

                // Verify password
                if (!PasswordHelper.VerifyPassword(model.Password, user.PasswordHash))
                {
                    ModelState.AddModelError("", "Email/Tên đăng nhập hoặc mật khẩu không đúng.");
                    return View(model);
                }

                // Đăng nhập thành công
                // Tạo OWIN authentication cookie
                // Use the same input (email or username) that user used to login
                var identityName = model.EmailOrUsername;
                
                System.Diagnostics.Debug.WriteLine($"=== LOGIN SUCCESS DEBUG ===");
                System.Diagnostics.Debug.WriteLine($"User Email: {user.Email}");
                System.Diagnostics.Debug.WriteLine($"User UserName: {user.UserName}");
                System.Diagnostics.Debug.WriteLine($"Identity Name: {identityName}");
                System.Diagnostics.Debug.WriteLine($"============================");
                
                var identity = new System.Security.Claims.ClaimsIdentity(new[] 
                {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, identityName),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.UserID.ToString()),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, user.Role.ToString()),
                    new System.Security.Claims.Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "ASP.NET Identity"),
                    new System.Security.Claims.Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", user.UserID.ToString())
                }, "ApplicationCookie");

                var authManager = HttpContext.GetOwinContext().Authentication;
                authManager.SignIn(new Microsoft.Owin.Security.AuthenticationProperties
                {
                    IsPersistent = model.RememberMe
                }, identity);

                // Lưu thông tin user vào session
                Session["UserID"] = user.UserID;
                Session["UserEmail"] = user.Email;
                Session["FullName"] = user.UserDetails?.FullName ?? "User";
                Session["Role"] = user.Role;
                Session["IsActive"] = user.IsActive;
                Session["AvatarURL"] = !string.IsNullOrEmpty(user.UserDetails?.ProfilePictureURL) 
                    ? user.UserDetails.ProfilePictureURL 
                    : "/images/avatar-default.jpg";

                // Add success message for login
                TempData["SuccessMessage"] = $"Chào mừng trở lại, {user.UserDetails?.FullName ?? "User"}!";
                
                // Chuyển hướng
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LOGIN ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                ModelState.AddModelError("", "Đã xảy ra lỗi trong quá trình đăng nhập. Vui lòng thử lại.");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi trong quá trình đăng nhập. Vui lòng thử lại.";
                return View(model);
            }
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            System.Diagnostics.Debug.WriteLine("=== REGISTER POST START ===");
            System.Diagnostics.Debug.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            
            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    if (errors.Count > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"ModelState Error - {key}: {string.Join(", ", errors.Select(e => e.ErrorMessage))}");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("=== REGISTER POST DEBUG ===");
                    System.Diagnostics.Debug.WriteLine($"Username: {model.UserName}");
                    System.Diagnostics.Debug.WriteLine($"FullName: {model.FullName}");
                    System.Diagnostics.Debug.WriteLine($"Email: {model.Email}");
                    System.Diagnostics.Debug.WriteLine($"PhoneNumber: {model.PhoneNumber}");

                    // Kiểm tra username đã tồn tại chưa (username bắt buộc)
                    if (userRepository.UsernameExists(model.UserName))
                    {
                        System.Diagnostics.Debug.WriteLine($"ERROR: Username {model.UserName} already exists");
                        ModelState.AddModelError("UserName", "Tên đăng nhập này đã được sử dụng.");
                        ViewBag.ShowRegister = true;
                        return View("Login", model);
                    }

                    // Email không bắt buộc - skip validation nếu không có
                    // if (!string.IsNullOrEmpty(model.Email) && userRepository.EmailExists(model.Email))
                    // {
                    //     ModelState.AddModelError("Email", "Email này đã được đăng ký.");
                    //     ViewBag.ShowRegister = true;
                    //     return View("Login", model);
                    // }

                    var generatedEmail = string.IsNullOrEmpty(model.Email) ? $"{model.UserName}@local.temp" : model.Email;
                    System.Diagnostics.Debug.WriteLine($"Generated Email: {generatedEmail}");

                    // Tạo user mới
                    User newUser = new User
                    {
                        UserName = model.UserName,
                        Email = generatedEmail,
                        PhoneNumber = model.PhoneNumber,
                        PasswordHash = PasswordHelper.HashPassword(model.Password),
                        Role = 2, // Default role: Mom
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };

                    // Tạo user details
                    UserDetails newUserDetails = new UserDetails
                    {
                        FullName = model.FullName,
                        ReputationScore = 0
                    };

                    System.Diagnostics.Debug.WriteLine("Attempting to create user...");

                    // Lưu vào database
                    bool result = userRepository.CreateUser(newUser, newUserDetails);

                    System.Diagnostics.Debug.WriteLine($"CreateUser result: {result}");

                    if (result)
                    {
                        // Đăng ký thành công
                        System.Diagnostics.Debug.WriteLine("=== REGISTER SUCCESS ===");
                        TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                        return RedirectToAction("Login");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("ERROR: CreateUser returned false");
                        ModelState.AddModelError("", "Đã xảy ra lỗi trong quá trình đăng ký. Vui lòng thử lại.");
                        ViewBag.ShowRegister = true;
                        return View("Login", model);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"EXCEPTION in Register: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    }
                    
                    ModelState.AddModelError("", $"Đã xảy ra lỗi trong quá trình đăng ký: {ex.Message}");
                    ViewBag.ShowRegister = true;
                    return View("Login", model);
                }
            }

            // Nếu có lỗi, quay lại form với các lỗi được hiển thị
            ViewBag.ShowRegister = true;
            return View("Login", model);
        }

        // GET: /Account/Logout
        public ActionResult Logout()
        {
            // OWIN SignOut
            var authManager = HttpContext.GetOwinContext().Authentication;
            authManager.SignOut("ApplicationCookie");
            
            // Clear session
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/ReloadSession - Reload session từ database
        [AllowAnonymous]
        public ActionResult ReloadSession()
        {
            try
            {
                // Lấy email từ OWIN authentication
                var email = User.Identity.Name;
                
                if (string.IsNullOrEmpty(email))
                {
                    return Content("❌ Chưa đăng nhập. Vui lòng đăng nhập trước.");
                }

                // Lấy user từ database
                var user = userRepository.GetUserByEmail(email);
                
                if (user == null)
                {
                    return Content($"❌ Không tìm thấy user với email: {email}");
                }

                // Clear session cũ
                Session.Clear();

                // Reload session từ database
                Session["UserID"] = user.UserID;
                Session["UserEmail"] = user.Email;
                Session["FullName"] = user.UserDetails?.FullName ?? "User";
                Session["Role"] = user.Role;
                Session["IsActive"] = user.IsActive;

                // Determine role text using traditional switch
                string roleText;
                switch (user.Role)
                {
                    case 1:
                        roleText = "✅ Admin (CÓ QUYỀN truy cập trang quản lý)";
                        break;
                    case 2:
                        roleText = "🟡 Mẹ bỉm (KHÔNG có quyền admin)";
                        break;
                    case 3:
                        roleText = "🔵 Nhãn hàng (KHÔNG có quyền admin)";
                        break;
                    default:
                        roleText = "⚫ Không xác định";
                        break;
                }

                var result = $@"
✅ ĐÃ RELOAD SESSION THÀNH CÔNG!

📧 Email: {user.Email}
👤 Họ tên: {user.UserDetails?.FullName ?? "Chưa cập nhật"}
🔑 Role: {user.Role} - {roleText}
🟢 Trạng thái: {(user.IsActive ? "Active ✅" : "Inactive ❌")}

🔗 Bước tiếp theo:
{(user.Role == 1 ? 
    "✅ Bạn có quyền Admin! Truy cập: /Admin/Category" : 
    "❌ Bạn chưa có quyền Admin. Cần cấp quyền trong database.")}

📋 Test URLs:
- Kiểm tra quyền: /Admin/CheckRole
- Test route: /Admin/Category/Test
- Trang admin: /Admin/Category
";
                
                return Content(result, "text/plain");
            }
            catch (Exception ex)
            {
                return Content($"❌ Lỗi: {ex.Message}\n\nStack trace:\n{ex.StackTrace}", "text/plain");
            }
        }

        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Debug logging
            System.Diagnostics.Debug.WriteLine($"=== EXTERNAL LOGIN ===");
            System.Diagnostics.Debug.WriteLine($"Provider: {provider}");
            System.Diagnostics.Debug.WriteLine($"Return URL: {returnUrl}");
            
            // Request a redirect to the external login provider  
            // Use ExternalLoginCallback for better OAuth compatibility
            var redirectUri = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }, Request.Url.Scheme);
            System.Diagnostics.Debug.WriteLine($"Redirect URI: {redirectUri}");
            System.Diagnostics.Debug.WriteLine($"======================");
            
            return new ChallengeResult(provider, redirectUri);
        }

        // GET: /Account/LinkGoogleCallback - Alias for ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> LinkGoogleCallback(string returnUrl)
        {
            System.Diagnostics.Debug.WriteLine($"=== LinkGoogleCallback Called ===");
            System.Diagnostics.Debug.WriteLine($"Return URL: {returnUrl}");
            return await ExternalLoginCallback(returnUrl);
        }

        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== GOOGLE CALLBACK ===");
                System.Diagnostics.Debug.WriteLine($"Return URL: {returnUrl}");
                System.Diagnostics.Debug.WriteLine($"Request URL: {Request.Url}");
                System.Diagnostics.Debug.WriteLine($"Query String: {Request.QueryString}");

                var loginInfo = await System.Web.HttpContext.Current.GetOwinContext().Authentication.GetExternalLoginInfoAsync();
                if (loginInfo == null)
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: loginInfo is null - OAuth config issue");
                    System.Diagnostics.Debug.WriteLine($"Request URL: {Request.Url}");
                    TempData["ErrorMessage"] = "Lỗi xác thực Google. Vui lòng kiểm tra cấu hình hoặc thử lại.";
                    return RedirectToAction("Login");
                }

                // Lấy thông tin từ Google
                var email = loginInfo.Email;
                var name = loginInfo.ExternalIdentity.Name;
                
                System.Diagnostics.Debug.WriteLine($"Google Email: {email}");
                System.Diagnostics.Debug.WriteLine($"Google Name: {name}");

                if (string.IsNullOrEmpty(email))
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: Email is null or empty");
                    TempData["ErrorMessage"] = "Google không cung cấp địa chỉ email. Vui lòng thử lại.";
                    return RedirectToAction("Login");
                }

                // Kiểm tra user đã tồn tại chưa
                var existingUser = userRepository.GetUserByEmail(email);
                
                // CRITICAL: Prevent Google login if email exists but not properly linked
                if (existingUser != null)
                {
                    // Case 1: User exists but GoogleId is null (unlinked or username/password account)
                    if (string.IsNullOrEmpty(existingUser.GoogleId))
                    {
                        System.Diagnostics.Debug.WriteLine($"BLOCKED: Email {email} exists but not linked to Google (GoogleId is null)");
                        TempData["ErrorMessage"] = "Email này thuộc về một tài khoản khác không liên kết với Google. Vui lòng sử dụng tên đăng nhập và mật khẩu để đăng nhập.";
                        return RedirectToAction("Login");
                    }
                    
                    // Case 2: Verify GoogleId matches (additional security)
                    var googleId = loginInfo.ExternalIdentity?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(googleId) && existingUser.GoogleId != googleId)
                    {
                        System.Diagnostics.Debug.WriteLine($"BLOCKED: Email {email} linked to different GoogleId");
                        System.Diagnostics.Debug.WriteLine($"Expected GoogleId: {existingUser.GoogleId}");
                        System.Diagnostics.Debug.WriteLine($"Provided GoogleId: {googleId}");
                        TempData["ErrorMessage"] = "Có vấn đề với xác thực Google. Vui lòng liên hệ hỗ trợ.";
                        return RedirectToAction("Login");
                    }
                }
                
                if (existingUser != null)
                {
                    // User đã tồn tại - đăng nhập
                    System.Diagnostics.Debug.WriteLine($"EXISTING USER: {existingUser.Email}");
                    
                    // Sử dụng OWIN Authentication như Login action
                    var identity = new System.Security.Claims.ClaimsIdentity("ApplicationCookie");
                    identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, existingUser.Email));
                    identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, existingUser.UserID.ToString()));
                    identity.AddClaim(new System.Security.Claims.Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "ASP.NET Identity"));
                    
                    var authManager = HttpContext.GetOwinContext().Authentication;
                    authManager.SignIn(identity);
                    
                    Session["UserID"] = existingUser.UserID;
                    Session["UserEmail"] = existingUser.Email;
                    Session["FullName"] = existingUser.UserDetails?.FullName ?? name;
                    Session["Role"] = existingUser.Role;
                    Session["IsActive"] = existingUser.IsActive;
                    Session["AvatarURL"] = !string.IsNullOrEmpty(existingUser.UserDetails?.ProfilePictureURL) 
                        ? existingUser.UserDetails.ProfilePictureURL 
                        : "/images/avatar-default.jpg";

                    System.Diagnostics.Debug.WriteLine($"LOGIN SUCCESS: {existingUser.Email}");
                    TempData["SuccessMessage"] = $"Chào mừng trở lại, {existingUser.UserDetails?.FullName ?? name}!";
                    
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    // Tạo user mới từ Google account
                    System.Diagnostics.Debug.WriteLine($"CREATING NEW USER: {email}");
                    
                    // Get Google OAuth subject ID
                    var googleId = loginInfo.ExternalIdentity?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    
                    var newUser = new User
                    {
                        Email = email,
                        GoogleId = googleId, // Store Google OAuth ID
                        PasswordHash = PasswordHelper.HashPassword(Guid.NewGuid().ToString()), // Random password
                        Role = 2, // Mom role
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };

                    var newUserDetails = new UserDetails
                    {
                        FullName = name ?? email.Split('@')[0],
                        ReputationScore = 0
                    };

                    System.Diagnostics.Debug.WriteLine($"Attempting to create user: {email}");
                    System.Diagnostics.Debug.WriteLine($"User details: FullName={newUserDetails.FullName}");
                    
                    bool result = userRepository.CreateUser(newUser, newUserDetails);

                    if (result)
                    {
                        // Tạo account thành công - chuyển đến CompleteProfile
                        var createdUser = userRepository.GetUserByEmail(email);
                        if (createdUser != null)
                        {
                            // Set temp session variables for CompleteProfile
                            Session["TempUserID"] = createdUser.UserID;
                            Session["TempEmail"] = createdUser.Email;
                            Session["TempFullName"] = createdUser.UserDetails?.FullName ?? name;
                            Session["TempRole"] = createdUser.Role;

                            System.Diagnostics.Debug.WriteLine($"NEW USER CREATED: {createdUser.Email} - Redirecting to CompleteProfile");
                            TempData["InfoMessage"] = $"Chào mừng, {createdUser.UserDetails?.FullName ?? name}! Hãy hoàn thiện thông tin để sử dụng đầy đủ tính năng.";
                            
                            // Redirect to CompleteProfile instead of home
                            return RedirectToAction("CompleteProfile");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("ERROR: User created but cannot retrieve from database");
                            TempData["ErrorMessage"] = "Tài khoản đã được tạo nhưng không thể đăng nhập. Vui lòng thử đăng nhập thủ công.";
                            return RedirectToAction("Login");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("ERROR: Failed to create user - check database connection and structure");
                        TempData["ErrorMessage"] = "Không thể tạo tài khoản. Vui lòng kiểm tra kết nối database hoặc chạy script FixDatabase.sql.";
                        return RedirectToAction("Login");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== CRITICAL ERROR in ExternalLoginCallback ===");
                System.Diagnostics.Debug.WriteLine($"Exception Type: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                
                TempData["ErrorMessage"] = $"Lỗi Google OAuth: {ex.Message}. Vui lòng thử lại hoặc đăng nhập bằng tài khoản thường.";
                return RedirectToAction("Login");
            }
        }

        // GET: /Account/CompleteProfile
        [AllowAnonymous]
        public ActionResult CompleteProfile()
        {
            // Kiểm tra session có thông tin temp user không
            if (Session["TempUserID"] == null)
            {
                TempData["ErrorMessage"] = "Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login");
            }

            var model = new CompleteProfileViewModel
            {
                UserID = (int)Session["TempUserID"],
                Email = Session["TempEmail"]?.ToString(),
                FullName = Session["TempFullName"]?.ToString()
            };

            return View(model);
        }

        // POST: /Account/CompleteProfile
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult CompleteProfile(CompleteProfileViewModel model, string submitType)
        {
            try
            {
                // Kiểm tra session
                if (Session["TempUserID"] == null)
                {
                    TempData["ErrorMessage"] = "Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại.";
                    return RedirectToAction("Login");
                }

                // Lấy user hiện tại
                var user = userRepository.GetUserByEmail(model.Email);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng.";
                    return RedirectToAction("Login");
                }

                // Kiểm tra nếu user chọn skip
                if (submitType == "skip")
                {
                    System.Diagnostics.Debug.WriteLine($"USER SKIPPED COMPLETE PROFILE: {user.Email}");
                    
                    // Đăng nhập user với thông tin hiện tại
                    LoginUserAfterCompletion(user);
                    
                    TempData["InfoMessage"] = $"Chào mừng, {user.UserDetails?.FullName}! Bạn có thể cập nhật thông tin bổ sung trong phần hồ sơ cá nhân.";
                    return RedirectToAction("Index", "Home");
                }

                // Validation chỉ khi complete (không skip)
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Kiểm tra username đã tồn tại chưa
                if (!string.IsNullOrEmpty(model.UserName) && userRepository.UsernameExists(model.UserName))
                {
                    ModelState.AddModelError("UserName", "Tên đăng nhập này đã được sử dụng.");
                    return View(model);
                }

                // Cập nhật thông tin user
                user.UserName = model.UserName;
                user.PasswordHash = PasswordHelper.HashPassword(model.Password);
                user.PhoneNumber = model.PhoneNumber;

                // Cập nhật user details
                var userDetails = userRepository.GetUserDetails(user.UserID);
                if (userDetails != null)
                {
                    userDetails.Address = model.Address;
                }

                // Lưu thay đổi
                bool updateResult = userRepository.UpdateUser(user);
                if (userDetails != null)
                {
                    userRepository.UpdateUserDetails(userDetails);
                }

                if (updateResult)
                {
                    System.Diagnostics.Debug.WriteLine($"USER COMPLETED PROFILE: {user.Email}");
                    
                    // Đăng nhập user
                    LoginUserAfterCompletion(user);

                    TempData["SuccessMessage"] = $"Chào mừng đến với MomExchange, {user.UserDetails?.FullName}! Hồ sơ của bạn đã được hoàn thiện.";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật thông tin. Vui lòng thử lại.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in CompleteProfile: {ex.Message}");
                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật thông tin. Vui lòng thử lại.");
                return View(model);
            }
        }

        /// <summary>
        /// Helper method to login user after completing or skipping profile completion
        /// </summary>
        private void LoginUserAfterCompletion(User user)
        {
            // Xóa session temp
            Session.Remove("TempUserID");
            Session.Remove("TempEmail");
            Session.Remove("TempFullName");
            Session.Remove("TempRole");

            // Tạo OWIN authentication
            var identity = new System.Security.Claims.ClaimsIdentity(new[] 
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.Email),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, user.Role.ToString()),
                new System.Security.Claims.Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "ASP.NET Identity"),
                new System.Security.Claims.Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", user.UserID.ToString())
            }, "ApplicationCookie");

            var authManager = HttpContext.GetOwinContext().Authentication;
            authManager.SignIn(new Microsoft.Owin.Security.AuthenticationProperties
            {
                IsPersistent = false
            }, identity);

            // Set session variables
            Session["UserID"] = user.UserID;
            Session["UserEmail"] = user.Email;
            Session["FullName"] = user.UserDetails?.FullName;
            Session["Role"] = user.Role;
            Session["IsActive"] = user.IsActive;
            Session["AvatarURL"] = !string.IsNullOrEmpty(user.UserDetails?.ProfilePictureURL) 
                ? user.UserDetails.ProfilePictureURL 
                : "/images/avatar-default.jpg";
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        // Challenge Result for OAuth
        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                System.Web.HttpContext.Current.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
    }
}