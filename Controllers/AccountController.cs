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
using System.Data.Entity;
using B_M.Services;

namespace B_M.Controllers
{
    public class AccountController : Controller
    {
        private readonly B_M.Repositories.UserRepository userRepository;
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        public AccountController()
        {
            userRepository = new B_M.Repositories.UserRepository();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                userRepository?.Dispose();
                _db?.Dispose();
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

        // ======== CHANGE PASSWORD (Modal) ========
        [Authorize]
        public ActionResult ChangePassword()
        {
            int userId = (int)Session["UserID"];
            var user = userRepository.GetUserById(userId);
            
            var model = new ChangePasswordViewModel();
            if (user != null)
            {
                model.Username = user.UserName ?? "Chưa đặt";
                model.Email = user.Email;
            }
            
            return PartialView("_ChangePasswordPartial", model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return PartialView("_ChangePasswordPartial", model);

            int userId = (int)Session["UserID"];
            var user = userRepository.GetUserById(userId);
            if (user == null) return Json(new { success = false, message = "Phiên làm việc đã hết hạn." });

            if (!Helpers.PasswordHelper.VerifyPassword(model.CurrentPassword, user.PasswordHash))
            {
                return Json(new { success = false, message = "Mật khẩu hiện tại không đúng." });
            }

            user.PasswordHash = Helpers.PasswordHelper.HashPassword(model.NewPassword);
            if (!userRepository.UpdateUser(user))
            {
                return Json(new { success = false, message = "Không thể cập nhật mật khẩu. Vui lòng thử lại." });
            }

            return Json(new { success = true, message = "Đổi mật khẩu thành công." });
        }

        // ======== EMAIL TEST (Debug only) ========
        [AllowAnonymous]
        public ActionResult TestEmailPage()
        {
            return View("TestEmail");
        }
        
        [AllowAnonymous]
        public ActionResult TestSimple()
        {
            return Json(new { 
                success = true, 
                message = "Simple test works!",
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                config = new {
                    host = System.Configuration.ConfigurationManager.AppSettings["EmailSmtpHost"],
                    port = System.Configuration.ConfigurationManager.AppSettings["EmailSmtpPort"],
                    username = System.Configuration.ConfigurationManager.AppSettings["EmailUsername"],
                    passwordLength = System.Configuration.ConfigurationManager.AppSettings["EmailPassword"]?.Length ?? 0
                }
            }, JsonRequestBehavior.AllowGet);
        }
        
        [AllowAnonymous]
        public ActionResult TestEmail()
        {
            try
            {
                var email = new EmailService();
                var result = email.SendEmail("dttthao.5354@gmail.com", "Test Email", "<h1>Test từ MomExchange</h1><p>Nếu nhận được email này, cấu hình SMTP đã hoạt động!</p>", true);
                
                return Json(new { 
                    success = result.Success, 
                    message = result.Message,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = ex.Message,
                    innerException = ex.InnerException?.Message,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                }, JsonRequestBehavior.AllowGet);
            }
        }
        
        [AllowAnonymous]
        public ActionResult TestSmtpConnection()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[TEST SMTP] Starting SMTP connection test...");
                
                var password = System.Configuration.ConfigurationManager.AppSettings["EmailPassword"];
                System.Diagnostics.Debug.WriteLine($"[TEST SMTP] Password length: {password?.Length ?? 0}");
                
                if (string.IsNullOrEmpty(password))
                {
                    return Json(new { 
                        success = false, 
                        message = "EmailPassword not found in Web.config",
                        timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    }, JsonRequestBehavior.AllowGet);
                }
                
                using (var client = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587))
                {
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new System.Net.NetworkCredential("dttthao.5354@gmail.com", password);
                    client.Timeout = 10000;
                    
                    System.Diagnostics.Debug.WriteLine("[TEST SMTP] Attempting to connect...");
                    
                    // Test connection by sending a simple email
                    using (var message = new System.Net.Mail.MailMessage())
                    {
                        message.From = new System.Net.Mail.MailAddress("dttthao.5354@gmail.com", "Test");
                        message.To.Add("dttthao.5354@gmail.com");
                        message.Subject = "Connection Test";
                        message.Body = "Test";
                        
                        client.Send(message);
                    }
                    
                    System.Diagnostics.Debug.WriteLine("[TEST SMTP] Connection successful!");
                    
                    return Json(new { 
                        success = true, 
                        message = "SMTP connection successful!",
                        timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TEST SMTP] Error: {ex.Message}");
                if (ex.InnerException != null)
                    System.Diagnostics.Debug.WriteLine($"[TEST SMTP] Inner: {ex.InnerException.Message}");
                
                return Json(new { 
                    success = false, 
                    message = ex.Message,
                    innerException = ex.InnerException?.Message,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // ======== FORGOT PASSWORD (Modals) ========
        [AllowAnonymous]
        public ActionResult ForgotPasswordRequest()
        {
            return PartialView("_ForgotPasswordRequestPartial", new ForgotPasswordRequestViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPasswordRequest(ForgotPasswordRequestViewModel model)
        {
            if (!ModelState.IsValid) return PartialView("_ForgotPasswordRequestPartial", model);

            var user = userRepository.GetUserByEmailOrUsername(model.UsernameOrEmail);
            
            // Check if user exists first
            if (user == null)
            {
                return Json(new { 
                    success = false, 
                    message = "Tài khoản không tồn tại. Vui lòng kiểm tra lại tên đăng nhập hoặc email." 
                });
            }

            // Check if user has email
            if (string.IsNullOrEmpty(user.Email) || user.Email.EndsWith("@local.temp"))
            {
                return Json(new { 
                    success = false, 
                    message = "Tài khoản này không có email hợp lệ. Vui lòng liên hệ hỗ trợ để đặt lại mật khẩu." 
                });
            }

            // Invalidate previous active codes
            var currentTime = DateTime.Now;
            var actives = _db.PasswordResetCodes.Where(x => x.UserID == user.UserID && x.UsedAt == null && x.ExpiresAt > currentTime).ToList();
            var expiredTime = DateTime.Now.AddMinutes(-1);
            foreach (var a in actives) { a.ExpiresAt = expiredTime; }
            _db.SaveChanges();

            var code = new Random().Next(100000, 999999).ToString();
            var pr = new PasswordResetCode
            {
                UserID = user.UserID,
                Code = code,
                Token = Guid.NewGuid().ToString("N"),
                ExpiresAt = DateTime.Now.AddMinutes(10),
                Attempts = 0
            };
            _db.PasswordResetCodes.Add(pr);
            _db.SaveChanges();

            var email = new EmailService();
            var html = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                        <h1 style='margin: 0;'>🔐 Đặt lại mật khẩu</h1>
                    </div>
                    <div style='background: #fff; padding: 30px; border: 1px solid #e0e0e0; border-top: none;'>
                        <p>Xin chào <strong>{(user.UserDetails?.FullName ?? user.UserName ?? user.Email)}</strong>,</p>
                        <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
                        <p>Mã xác thực của bạn là:</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <div style='font-size: 32px; font-weight: bold; letter-spacing: 8px; background: #f8f9fa; color: #007bff; padding: 20px; border-radius: 10px; border: 3px solid #007bff; display: inline-block;'>{code}</div>
                        </div>
                        <div style='background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0;'>
                            <p style='margin: 0;'><strong>⚠️ Lưu ý quan trọng:</strong></p>
                            <ul style='margin: 10px 0 0 0;'>
                                <li>Mã này chỉ có hiệu lực trong <strong>10 phút</strong></li>
                                <li>Không chia sẻ mã này với bất kỳ ai</li>
                                <li>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này</li>
                            </ul>
                        </div>
                        <p>Trân trọng,<br><strong>Đội ngũ MomExchange</strong></p>
                    </div>
                    <div style='background: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666; border-radius: 0 0 10px 10px;'>
                        <p>&copy; {DateTime.Now.Year} MomExchange. All rights reserved.</p>
                    </div>
                </div>";
            
            System.Diagnostics.Debug.WriteLine($"[FORGOT PASSWORD] Attempting to send email to: {user.Email}");
            var emailResult = email.SendEmail(user.Email, "Mã xác thực đặt lại mật khẩu - MomExchange", html, true);
            System.Diagnostics.Debug.WriteLine($"[EMAIL RESULT] Success: {emailResult.Success}, Message: {emailResult.Message}");
            
            if (!emailResult.Success)
            {
                System.Diagnostics.Debug.WriteLine($"[EMAIL FAILED] Failed to send reset code to {user.Email}: {emailResult.Message}");
                return Json(new { 
                    success = false, 
                    message = "Không thể gửi email. Vui lòng thử lại sau hoặc liên hệ hỗ trợ." 
                });
            }

            // Return success with next step URL - pass the actual email for display
            var nextUrl = Url.Action("ForgotPasswordVerify", "Account", new { u = user.Email });
            return Json(new { 
                success = true, 
                message = "Mã xác thực đã được gửi đến email của bạn. Vui lòng kiểm tra hộp thư.",
                next = nextUrl
            });
        }

        [AllowAnonymous]
        public ActionResult ForgotPasswordVerify(string u = null)
        {
            return PartialView("_ForgotPasswordVerifyPartial", new ForgotPasswordVerifyViewModel { UsernameOrEmail = u });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPasswordVerify(ForgotPasswordVerifyViewModel model)
        {
            if (!ModelState.IsValid) return PartialView("_ForgotPasswordVerifyPartial", model);

            var user = userRepository.GetUserByEmailOrUsername(model.UsernameOrEmail);
            if (user == null)
            {
                ModelState.AddModelError("", "Thông tin không hợp lệ.");
                return PartialView("_ForgotPasswordVerifyPartial", model);
            }

            var code = _db.PasswordResetCodes
                .Where(x => x.UserID == user.UserID && x.UsedAt == null)
                .OrderByDescending(x => x.Id)
                .FirstOrDefault();

            var currentTime = DateTime.Now;
            if (code == null || code.ExpiresAt < currentTime)
            {
                return Json(new { success = false, message = "Mã xác thực đã hết hạn. Vui lòng yêu cầu lại." });
            }

            if (!string.Equals(code.Code, model.Code))
            {
                code.Attempts += 1;
                _db.SaveChanges();
                if (code.Attempts >= 5)
                {
                    code.ExpiresAt = DateTime.Now.AddMinutes(-1);
                    _db.SaveChanges();
                    return Json(new { success = false, message = "Bạn đã nhập sai quá số lần cho phép. Vui lòng yêu cầu mã mới." });
                }
                return Json(new { success = false, message = "Mã xác thực không đúng." });
            }

            // Mark verified and allow reset within 10 minutes
            code.UsedAt = DateTime.Now;
            _db.SaveChanges();

            var nextUrl = Url.Action("ResetPassword", "Account", new { u = model.UsernameOrEmail });
            return Json(new { success = true, message = "Xác thực thành công.", next = nextUrl });
        }

        [AllowAnonymous]
        public ActionResult ResetPassword(string u)
        {
            var model = new ResetPasswordViewModel { UsernameOrEmail = u };
            
            // Load user information to display
            if (!string.IsNullOrEmpty(u))
            {
                var user = userRepository.GetUserByEmailOrUsername(u);
                if (user != null)
                {
                    model.Username = user.UserName ?? "Chưa đặt";
                    model.Email = user.Email;
                }
            }
            
            return PartialView("_ResetPasswordPartial", model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return PartialView("_ResetPasswordPartial", model);

            var user = userRepository.GetUserByEmailOrUsername(model.UsernameOrEmail);
            if (user == null) return Json(new { success = false, message = "Phiên xác thực không hợp lệ." });

            // Only if a code was verified within last 10 minutes
            var tenMinutesAgo = DateTime.Now.AddMinutes(-10);
            var verified = _db.PasswordResetCodes
                .Where(x => x.UserID == user.UserID && x.UsedAt != null && x.UsedAt > tenMinutesAgo)
                .OrderByDescending(x => x.UsedAt)
                .FirstOrDefault();

            if (verified == null)
                return Json(new { success = false, message = "Phiên đặt lại mật khẩu đã hết hạn. Vui lòng yêu cầu mã mới." });

            user.PasswordHash = Helpers.PasswordHelper.HashPassword(model.NewPassword);
            userRepository.UpdateUser(user);

            return Json(new { success = true, message = "Đặt lại mật khẩu thành công. Vui lòng đăng nhập." });
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

                            System.Diagnostics.Debug.WriteLine($"NEW USER CREATED: {createdUser.Email} - Redirecting to Home with Complete Profile Modal");
                            TempData["InfoMessage"] = $"Chào mừng, {createdUser.UserDetails?.FullName ?? name}! Hãy hoàn thiện thông tin để sử dụng đầy đủ tính năng.";
                            
                            // Redirect to Home with flag to show Complete Profile Modal
                            TempData["ShowCompleteProfileModal"] = true;
                            return RedirectToAction("Index", "Home");
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
        // This endpoint is only used for AJAX requests to load the modal content
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

            // Only return partial view for modal - no full page support
            return PartialView("_CompleteProfilePartial", model);
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