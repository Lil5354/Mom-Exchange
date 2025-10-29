// Controllers/ProfileController.cs
using B_M.Models;
using B_M.Helpers;
using B_M.Repositories;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity.Owin;

namespace B_M.Controllers
{
    [Authorize]
    public class ProfileController : BaseController
    {
        private readonly UserRepository userRepository;
        private readonly ApplicationDbContext db;

        public ProfileController()
        {
            userRepository = new UserRepository();
            db = new ApplicationDbContext();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                userRepository?.Dispose();
                db?.Dispose();
            }
            base.Dispose(disposing);
        }

        // GET: Profile
        public ActionResult Index()
        {
            try
            {
                var userIdentity = User.Identity.Name;
                System.Diagnostics.Debug.WriteLine($"=== PROFILE INDEX DEBUG ===");
                System.Diagnostics.Debug.WriteLine($"User.Identity.Name: {userIdentity}");
                
                // Try to get user by email first, then by username
                var user = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                System.Diagnostics.Debug.WriteLine($"User found: {user != null}");
                if (user != null)
                {
                    System.Diagnostics.Debug.WriteLine($"User Email: {user.Email}");
                    System.Diagnostics.Debug.WriteLine($"User UserName: {user.UserName}");
                }
                System.Diagnostics.Debug.WriteLine($"============================");
                
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var viewModel = new ProfileViewModel
                {
                    UserID = user.UserID,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    FullName = user.UserDetails?.FullName ?? "",
                    Address = user.UserDetails?.Address ?? "",
                    ProfilePictureURL = !string.IsNullOrEmpty(user.UserDetails?.ProfilePictureURL) ? user.UserDetails.ProfilePictureURL : "https://via.placeholder.com/120x120/667eea/ffffff?text=" + (user.UserDetails?.FullName?.Substring(0,1) ?? "U"),
                    ReputationScore = user.UserDetails?.ReputationScore ?? 0,
                    MilkDonationStatus = user.MilkDonationStatus,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                };

                // Check if user has Google account linked
                ViewBag.IsGoogleLinked = !string.IsNullOrEmpty(user.GoogleId);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin hồ sơ: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Profile/Edit
        public ActionResult Edit()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== PROFILE EDIT GET ===");
                
                var userIdentity = User.Identity.Name;
                System.Diagnostics.Debug.WriteLine($"User.Identity.Name: {userIdentity}");
                
                if (string.IsNullOrEmpty(userIdentity))
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: User.Identity.Name is null or empty");
                    TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                    return RedirectToAction("Login", "Account");
                }
                
                // Try to get user by email first, then by username
                var user = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                System.Diagnostics.Debug.WriteLine($"Lookup by email: {userRepository.GetUserByEmail(userIdentity) != null}");
                System.Diagnostics.Debug.WriteLine($"Lookup by username: {userRepository.GetUserByUsername(userIdentity) != null}");
                System.Diagnostics.Debug.WriteLine($"User found: {user != null}");
                
                if (user == null)
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: User not found in database");
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng.";
                    return RedirectToAction("Login", "Account");
                }

                System.Diagnostics.Debug.WriteLine($"User ID: {user.UserID}, Email: {user.Email}");
                System.Diagnostics.Debug.WriteLine($"UserDetails exists: {user.UserDetails != null}");

                var viewModel = new EditProfileViewModel
                {
                    UserID = user.UserID,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber ?? "",
                    FullName = user.UserDetails?.FullName ?? "",
                    Address = user.UserDetails?.Address ?? "",
                    ProfilePictureURL = !string.IsNullOrEmpty(user.UserDetails?.ProfilePictureURL) ? user.UserDetails.ProfilePictureURL : "https://via.placeholder.com/120x120/667eea/ffffff?text=" + (user.UserDetails?.FullName?.Substring(0,1) ?? "U")
                };

                // Check if user has Google account linked
                ViewBag.IsGoogleLinked = !string.IsNullOrEmpty(user.GoogleId);

                System.Diagnostics.Debug.WriteLine("ViewModel created successfully");
                System.Diagnostics.Debug.WriteLine("======================");
                
                return View("Edit",viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EXCEPTION in Edit: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditProfileViewModel model, HttpPostedFileBase profileImage)
        {
            System.Diagnostics.Debug.WriteLine("=== PROFILE EDIT POST ===");
            System.Diagnostics.Debug.WriteLine($"Model valid: {ModelState.IsValid}");
            
            if (!ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine("Model validation failed");
                return View("Edit", model);
            }

            try
            {
                var userIdentity = User.Identity.Name;
                var user = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng.";
                    return View(model);
                }

                // Xử lý upload ảnh profile nếu có
                string profileImageUrl = model.ProfilePictureURL;
                if (profileImage != null && profileImage.ContentLength > 0)
                {
                    profileImageUrl = SaveProfileImage(profileImage, user.UserID);
                    if (profileImageUrl == null)
                    {
                        ModelState.AddModelError("", "Có lỗi khi tải lên ảnh đại diện.");
                        return View(model);
                    }
                }

                // Cập nhật thông tin user
                user.PhoneNumber = model.PhoneNumber;
                
                // Cập nhật UserDetails
                if (user.UserDetails == null)
                {
                    user.UserDetails = new UserDetails
                    {
                        UserID = user.UserID,
                        ReputationScore = 0
                    };
                }

                user.UserDetails.FullName = model.FullName;
                user.UserDetails.Address = model.Address;
                user.UserDetails.ProfilePictureURL = profileImageUrl;

                // Lưu vào database
                bool result = userRepository.UpdateUser(user);
                
                if (result)
                {
                    System.Diagnostics.Debug.WriteLine("=== PROFILE UPDATE SUCCESS ===");
                    TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
                    return RedirectToAction("Edit");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: Failed to update profile");
                    ModelState.AddModelError("", "Có lỗi khi cập nhật hồ sơ.");
                    return View("Edit", model);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EXCEPTION in Edit POST: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                return View("Edit", model);
            }
        }

        // GET: Profile/MilkDonation
        public ActionResult MilkDonation()
        {
            try
            {
                var userIdentity = User.Identity.Name;
                var user = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var viewModel = new MilkDonationStatusViewModel
                {
                    UserID = user.UserID,
                    FullName = user.UserDetails?.FullName ?? "",
                    MilkDonationStatus = user.MilkDonationStatus,
                    HasLifestyleSurvey = db.UserLifestyleSurveys.Any(s => s.UserID == user.UserID),
                    HasMedicalRecords = db.UserMedicalRecords.Any(r => r.UserID == user.UserID && r.VerificationStatus != 2),
                    ApprovedMedicalRecordsCount = db.UserMedicalRecords.Count(r => r.UserID == user.UserID && r.VerificationStatus == 1)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: Profile/LinkGoogle
        public ActionResult LinkGoogle()
        {
            try
            {
                var userIdentity = User.Identity.Name;
                var user = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Tạo Google OAuth request cho linking
                var redirectUri = Url.Action("LinkGoogleCallback", "Profile", null, Request.Url.Scheme);
                var challengeResult = new ChallengeResult("Google", redirectUri);
                
                // Store current user info in session for callback
                Session["LinkGoogleUserID"] = user.UserID;
                Session["LinkGoogleUserEmail"] = !string.IsNullOrEmpty(user.Email) ? user.Email : user.UserName;
                
                System.Diagnostics.Debug.WriteLine($"LINK GOOGLE INITIATED: {user.Email}");
                
                return challengeResult;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in LinkGoogle: {ex.Message}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi kết nối với Google. Vui lòng thử lại.";
                return RedirectToAction("Edit");
            }
        }

        // GET: Profile/LinkGoogleCallback
        [AllowAnonymous]
        public async Task<ActionResult> LinkGoogleCallback()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== LINK GOOGLE CALLBACK ===");
                
                // Check if user is in a linking session
                if (Session["LinkGoogleUserID"] == null)
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: No linking session found");
                    TempData["ErrorMessage"] = "Phiên kết nối đã hết hạn. Vui lòng thử lại.";
                    return RedirectToAction("Login", "Account");
                }

                var loginInfo = await System.Web.HttpContext.Current.GetOwinContext().Authentication.GetExternalLoginInfoAsync();
                if (loginInfo == null)
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: Google loginInfo is null");
                    TempData["ErrorMessage"] = "Không thể lấy thông tin từ Google. Vui lòng thử lại.";
                    return RedirectToAction("Edit");
                }

                var googleEmail = loginInfo.Email;
                var googleName = loginInfo.ExternalIdentity.Name;
                var currentUserID = (int)Session["LinkGoogleUserID"];
                var currentUserEmail = Session["LinkGoogleUserEmail"]?.ToString();

                System.Diagnostics.Debug.WriteLine($"Google Email: {googleEmail}");
                System.Diagnostics.Debug.WriteLine($"Current User: {currentUserEmail}");

                if (string.IsNullOrEmpty(googleEmail))
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: Google email is null");
                    TempData["ErrorMessage"] = "Google không cung cấp địa chỉ email. Vui lòng thử lại.";
                    return RedirectToAction("Edit");
                }

                // CRITICAL: Check if Google email is already linked to ANY user (including current user)
                System.Diagnostics.Debug.WriteLine($"=== GOOGLE EMAIL UNIQUENESS CHECK ===");
                System.Diagnostics.Debug.WriteLine($"Checking if {googleEmail} is already linked...");
                
                // Check if this Google email is already linked to any user
                bool isGoogleEmailLinked = userRepository.IsGoogleEmailLinked(googleEmail, currentUserID);
                System.Diagnostics.Debug.WriteLine($"IsGoogleEmailLinked (excluding current user): {isGoogleEmailLinked}");
                
                if (isGoogleEmailLinked)
                {
                    System.Diagnostics.Debug.WriteLine($"BLOCKED: Google email {googleEmail} is already linked to another user");
                    TempData["ErrorMessage"] = $"Email Google '{googleEmail}' đã được liên kết với một tài khoản khác. Mỗi email Google chỉ có thể liên kết với một tài khoản duy nhất.";
                    return RedirectToAction("Edit");
                }
                
                // Check if current user already has this Google email linked
                var currentUser = userRepository.GetUserById(currentUserID);
                if (currentUser != null && currentUser.Email == googleEmail && !string.IsNullOrEmpty(currentUser.GoogleId))
                {
                    System.Diagnostics.Debug.WriteLine($"ALREADY LINKED: {googleEmail} is already linked to current user");
                    TempData["InfoMessage"] = "Tài khoản Google này đã được liên kết với tài khoản của bạn.";
                    
                    // Clean up session
                    Session.Remove("LinkGoogleUserID");
                    Session.Remove("LinkGoogleUserEmail");
                    
                    return RedirectToAction("Edit");
                }
                
                // Check for any existing user with this email (for legacy data)
                var existingGoogleUser = userRepository.GetUserByEmail(googleEmail);
                if (existingGoogleUser != null && existingGoogleUser.UserID != currentUserID)
                {
                    System.Diagnostics.Debug.WriteLine($"CONFLICT: Email {googleEmail} belongs to different user (UserID: {existingGoogleUser.UserID})");
                    TempData["ErrorMessage"] = $"Email '{googleEmail}' đã được sử dụng bởi một tài khoản khác. Không thể liên kết.";
                    return RedirectToAction("Edit");
                }
                
                System.Diagnostics.Debug.WriteLine($"VALIDATION PASSED: {googleEmail} is available for linking");
                System.Diagnostics.Debug.WriteLine($"=====================================");
                // Google email is available for linking
                // Always show confirmation for linking (security best practice)
                Session["PendingGoogleEmail"] = googleEmail;
                Session["PendingGoogleName"] = googleName;
                
                var model = new LinkGoogleAccountViewModel
                {
                    GoogleEmail = googleEmail,
                    GoogleName = googleName,
                    CurrentEmail = currentUserEmail,
                    CurrentFullName = (userRepository.GetUserByEmail(currentUserEmail) ?? userRepository.GetUserByUsername(currentUserEmail))?.UserDetails?.FullName ?? "User",
                    Action = "link"
                };
                
                return View("ConfirmGoogleLink", model);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in LinkGoogleCallback: {ex.Message}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi kết nối Google. Vui lòng thử lại.";
                
                // Clean up session
                Session.Remove("LinkGoogleUserID");
                Session.Remove("LinkGoogleUserEmail");
                
                return RedirectToAction("Edit");
            }
        }

        // POST: Profile/ConfirmGoogleLink
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmGoogleLink(LinkGoogleAccountViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Verify current user
                var userIdentity = User.Identity.Name;
                var user = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Verify password
                if (!PasswordHelper.VerifyPassword(model.CurrentPassword, user.PasswordHash))
                {
                    ModelState.AddModelError("CurrentPassword", "Mật khẩu không đúng.");
                    return View(model);
                }

                // Get pending Google info from session
                var googleEmail = Session["PendingGoogleEmail"]?.ToString();
                var googleName = Session["PendingGoogleName"]?.ToString();
                
                if (string.IsNullOrEmpty(googleEmail))
                {
                    TempData["ErrorMessage"] = "Phiên kết nối đã hết hạn. Vui lòng thử lại.";
                    return RedirectToAction("Edit");
                }

                return await ProcessGoogleLink(user.UserID, googleEmail, googleName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in ConfirmGoogleLink: {ex.Message}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xác nhận liên kết. Vui lòng thử lại.";
                return RedirectToAction("Edit");
            }
        }

        // GET: Profile/ConfirmUnlinkGoogle
        public ActionResult ConfirmUnlinkGoogle()
        {
            try
            {
                var userIdentity = User.Identity.Name;
                var user = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Check if user actually has Google linked
                if (string.IsNullOrEmpty(user.GoogleId))
                {
                    TempData["InfoMessage"] = "Tài khoản chưa được liên kết với Google.";
                    return RedirectToAction("Edit");
                }

                var model = new UnlinkGoogleViewModel
                {
                    CurrentEmail = user.Email,
                    CurrentFullName = user.UserDetails?.FullName ?? "User",
                    HasExistingCredentials = !string.IsNullOrEmpty(user.UserName) && !string.IsNullOrEmpty(user.PasswordHash)
                };

                return View(model);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in ConfirmUnlinkGoogle: {ex.Message}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra. Vui lòng thử lại.";
                return RedirectToAction("Edit");
            }
        }

        // POST: Profile/ProcessUnlinkGoogle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessUnlinkGoogle(UnlinkGoogleViewModel model, string action)
        {
            try
            {
                var userIdentity = User.Identity.Name;
                var user = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Check if user actually has Google linked
                if (string.IsNullOrEmpty(user.GoogleId))
                {
                    TempData["InfoMessage"] = "Tài khoản chưa được liên kết với Google.";
                    return RedirectToAction("Edit");
                }

                if (action == "confirm")
                {
                    // User already has credentials, just unlink Google
                    if (!string.IsNullOrEmpty(user.UserName) && !string.IsNullOrEmpty(user.PasswordHash))
                    {
                        user.GoogleId = null;
                        user.Email = null; // Set email to null to allow reuse
                        bool updateResult = userRepository.UpdateUser(user);
                        
                        if (updateResult)
                        {
                            System.Diagnostics.Debug.WriteLine($"GOOGLE UNLINKED: Email freed for reuse");
                            
                            // Complete logout - clear session and authentication
                            Session.Clear();
                            Session.Abandon();
                            System.Web.HttpContext.Current.GetOwinContext().Authentication.SignOut();
                            
                            // Set message for next request
                            TempData["SuccessMessage"] = "Đã hủy liên kết tài khoản Google thành công. Email Google này giờ có thể được sử dụng để đăng ký tài khoản mới. Vui lòng đăng nhập lại bằng tên đăng nhập và mật khẩu.";
                            return RedirectToAction("Login", "Account");
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Có lỗi xảy ra khi hủy liên kết. Vui lòng thử lại.";
                            return RedirectToAction("Edit");
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Tài khoản chưa có tên đăng nhập và mật khẩu. Vui lòng tạo trước khi hủy liên kết.";
                        return RedirectToAction("ConfirmUnlinkGoogle");
                    }
                }
                else if (action == "create")
                {
                    // Validate model for creating username/password
                    if (!ModelState.IsValid)
                    {
                        model.CurrentEmail = user.Email;
                        model.CurrentFullName = user.UserDetails?.FullName ?? "User";
                        model.HasExistingCredentials = false;
                        return View("ConfirmUnlinkGoogle", model);
                    }

                    // Check if username already exists
                    if (userRepository.UsernameExists(model.UserName))
                    {
                        ModelState.AddModelError("UserName", "Tên đăng nhập này đã được sử dụng.");
                        model.CurrentEmail = user.Email;
                        model.CurrentFullName = user.UserDetails?.FullName ?? "User";
                        model.HasExistingCredentials = false;
                        return View("ConfirmUnlinkGoogle", model);
                    }

                    // Create username/password and unlink Google
                    user.UserName = model.UserName;
                    user.PasswordHash = PasswordHelper.HashPassword(model.Password);
                    user.GoogleId = null; // Unlink Google
                    user.Email = null; // Set email to null to allow reuse

                    bool updateResult = userRepository.UpdateUser(user);
                    
                    if (updateResult)
                    {
                        System.Diagnostics.Debug.WriteLine($"USERNAME CREATED & GOOGLE UNLINKED: Email freed for reuse");
                        
                        // Complete logout - clear session and authentication
                        Session.Clear();
                        Session.Abandon();
                        System.Web.HttpContext.Current.GetOwinContext().Authentication.SignOut();
                        
                        // Set message for next request
                        TempData["SuccessMessage"] = $"Đã tạo tên đăng nhập '{model.UserName}' và hủy liên kết Google thành công. Email Google này giờ có thể được sử dụng để đăng ký tài khoản mới. Vui lòng đăng nhập lại bằng tên đăng nhập này.";
                        return RedirectToAction("Login", "Account");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo tài khoản. Vui lòng thử lại.";
                        return RedirectToAction("Edit");
                    }
                }

                return RedirectToAction("ConfirmUnlinkGoogle");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in ProcessUnlinkGoogle: {ex.Message}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra. Vui lòng thử lại.";
                return RedirectToAction("Edit");
            }
        }

        private async Task<ActionResult> ProcessGoogleLink(int userID, string googleEmail, string googleName)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"PROCESSING GOOGLE LINK: UserID={userID}, GoogleEmail={googleEmail}");
                
                // Get Google OAuth subject ID from current external login
                var loginInfo = await System.Web.HttpContext.Current.GetOwinContext().Authentication.GetExternalLoginInfoAsync();
                var googleId = loginInfo?.ExternalIdentity?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(googleId))
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: Cannot get Google ID from OAuth");
                    TempData["ErrorMessage"] = "Không thể lấy thông tin định danh từ Google. Vui lòng thử lại.";
                    return RedirectToAction("Edit");
                }

                // Update user with Google ID AND Email
                var user = userRepository.GetUserById(userID);
                if (user != null)
                {
                    System.Diagnostics.Debug.WriteLine($"BEFORE LINK: Email={user.Email}, GoogleId={user.GoogleId}");
                    
                    user.GoogleId = googleId;
                    user.Email = googleEmail; // CRITICAL: Update email when linking Google
                    
                    System.Diagnostics.Debug.WriteLine($"AFTER LINK: Email={user.Email}, GoogleId={user.GoogleId}");
                    
                    bool updateResult = userRepository.UpdateUser(user);
                    
                    if (updateResult)
                    {
                        System.Diagnostics.Debug.WriteLine($"GOOGLE LINKED: UserID={userID}, GoogleId={googleId}, Email={googleEmail}");
                        TempData["SuccessMessage"] = $"Đã liên kết thành công với tài khoản Google: {googleEmail}. Giờ bạn có thể đăng nhập bằng Google hoặc tên đăng nhập.";
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("ERROR: Failed to update user with GoogleId and Email");
                        TempData["ErrorMessage"] = "Có lỗi xảy ra khi lưu thông tin liên kết. Vui lòng thử lại.";
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR: User not found for UserID={userID}");
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng.";
                }
                
                // Clean up session
                Session.Remove("LinkGoogleUserID");
                Session.Remove("LinkGoogleUserEmail");
                Session.Remove("PendingGoogleEmail");
                Session.Remove("PendingGoogleName");
                
                return RedirectToAction("Edit");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in ProcessGoogleLink: {ex.Message}");
                throw;
            }
        }

        // Phương thức private để lưu ảnh profile
        private string SaveProfileImage(HttpPostedFileBase file, int userId)
        {
            try
            {
                // Kiểm tra định dạng file
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
                string fileExtension = Path.GetExtension(file.FileName).ToLower();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return null;
                }

                // Kiểm tra kích thước file (max 5MB)
                if (file.ContentLength > 5 * 1024 * 1024)
                {
                    return null;
                }

                // Tạo tên file duy nhất
                string fileName = $"profile_{userId}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                string uploadPath = Server.MapPath("~/images/profiles/");
                
                // Tạo thư mục nếu chưa có
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                string filePath = Path.Combine(uploadPath, fileName);
                file.SaveAs(filePath);

                return $"/images/profiles/{fileName}";
            }
            catch
            {
                return null;
            }
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

