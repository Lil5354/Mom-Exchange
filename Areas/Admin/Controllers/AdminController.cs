using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using B_M.Models;
using B_M.Filters;
using B_M.Helpers;
using B_M.Services;
using B_M.Repositories;

namespace B_M.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class AdminController : Controller
    {
        private readonly B_M.Repositories.UserRepository userRepository;

        public AdminController()
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

        // GET: Admin
        public ActionResult Index()
        {
            try
            {
                // Lấy thống kê tổng quan
                var stats = GetDashboardStats();
                return View(stats);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải trang quản trị: " + ex.Message;
                return View(new AdminDashboardViewModel());
            }
        }

        // GET: Admin/Debug - Debug authentication
        [AllowAnonymous]
        public ActionResult Debug()
        {
            var debugInfo = new
            {
                IsAuthenticated = User.Identity.IsAuthenticated,
                UserName = User.Identity.Name,
                SessionUserID = Session["UserID"],
                SessionRole = Session["Role"],
                SessionIsActive = Session["IsActive"],
                CurrentTime = DateTime.Now.ToString(),
                RequestUrl = Request.Url?.ToString()
            };
            
            return Content($"Debug Info:<br/>" +
                $"IsAuthenticated: {debugInfo.IsAuthenticated}<br/>" +
                $"UserName: {debugInfo.UserName}<br/>" +
                $"Session UserID: {debugInfo.SessionUserID}<br/>" +
                $"Session Role: {debugInfo.SessionRole}<br/>" +
                $"Session IsActive: {debugInfo.SessionIsActive}<br/>" +
                $"Current Time: {debugInfo.CurrentTime}<br/>" +
                $"Request URL: {debugInfo.RequestUrl}");
        }


        // GET: Admin/Users
        public ActionResult Users(int? page, string search, string roleFilter, 
            string emailSearch, string usernameSearch, string fullNameSearch, 
            string phoneSearch, string addressSearch, string statusFilter,
            DateTime? createdFrom, DateTime? createdTo, string sortBy, string sortOrder,
            bool showAdvancedSearch = false, bool caseSensitive = false, bool exactMatch = false)
        {
            try
            {
                var users = userRepository.GetAllUsers();
                var totalUsersCount = users.Count;
                
                // Apply advanced search filters
                users = ApplyAdvancedSearchFilters(users, search, roleFilter, emailSearch, 
                    usernameSearch, fullNameSearch, phoneSearch, addressSearch, 
                    statusFilter, createdFrom, createdTo, caseSensitive, exactMatch);
                
                // Apply sorting
                users = ApplySorting(users, sortBy, sortOrder);
                
                // Pagination
                int pageSize = 10;
                int pageNumber = page ?? 1;
                var pagedUsers = users.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                var viewModel = new AdminUsersViewModel
                {
                    Users = pagedUsers,
                    CurrentPage = pageNumber,
                    TotalPages = (int)Math.Ceiling((double)users.Count / pageSize),
                    TotalUsers = totalUsersCount,
                    
                    // Basic search
                    SearchTerm = search,
                    RoleFilter = roleFilter,
                    
                    // Advanced search
                    EmailSearch = emailSearch,
                    UsernameSearch = usernameSearch,
                    FullNameSearch = fullNameSearch,
                    PhoneSearch = phoneSearch,
                    AddressSearch = addressSearch,
                    StatusFilter = statusFilter,
                    CreatedFrom = createdFrom,
                    CreatedTo = createdTo,
                    SortBy = sortBy,
                    SortOrder = sortOrder,
                    
                    // Search options
                    ShowAdvancedSearch = showAdvancedSearch,
                    CaseSensitive = caseSensitive,
                    ExactMatch = exactMatch
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách người dùng: " + ex.Message;
                return View(new AdminUsersViewModel());
            }
        }

        // GET: Admin/UserDetails/5
        public ActionResult UserDetails(int id)
        {
            try
            {
                var user = userRepository.GetUserForAdminEdit(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                    return RedirectToAction("Users");
                }

                var viewModel = new B_M.Models.AdminUserEditViewModel
                {
                    UserID = user.UserID,
                    Email = user.Email,
                    UserName = user.UserName ?? "Chưa thiết lập",
                    PhoneNumber = user.PhoneNumber ?? "Chưa cập nhật",
                    FullName = user.UserDetails?.FullName ?? "Chưa cập nhật",
                    Address = user.UserDetails?.Address ?? "Chưa cập nhật",
                    IsActive = user.IsActive,
                    Role = user.Role,
                    RoleName = GetRoleName(user.Role),
                    StatusName = user.IsActive ? "HOẠT ĐỘNG" : "BỊ KHÓA",
                    CreatedAt = user.CreatedAt,
                    ReputationScore = user.UserDetails?.ReputationScore ?? 0
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin người dùng: " + ex.Message;
                return RedirectToAction("Users");
            }
        }

        // POST: Admin/ToggleUserStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleUserStatus(int UserID, bool IsActive)
        {
            try
            {
                var user = userRepository.GetUserById(UserID);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy người dùng." });
                }

                // Không cho phép admin tự vô hiệu hóa tài khoản của mình
                if (user.UserID == (int)Session["UserID"])
                {
                    return Json(new { success = false, message = "Bạn không thể khóa tài khoản của chính mình." });
                }

                bool result = userRepository.UpdateUserStatus(UserID, IsActive);

                if (result)
                {
                    string statusName = IsActive ? "HOẠT ĐỘNG" : "BỊ KHÓA";
                    string actionText = IsActive ? "mở khóa" : "khóa";
                    
                    return Json(new { 
                        success = true, 
                        message = $"Đã {actionText} tài khoản thành công.",
                        data = new {
                            isActive = IsActive,
                            statusName = statusName,
                            statusClass = IsActive ? "badge-success" : "badge-danger",
                            buttonText = IsActive ? "Khóa" : "Mở khóa",
                            buttonIcon = IsActive ? "lock" : "unlock"
                        }
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật trạng thái tài khoản." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // POST: Admin/ChangeUserRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeUserRole(int UserID, byte NewRole)
        {
            try
            {
                var user = userRepository.GetUserById(UserID);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy người dùng." });
                }

                // Không cho phép admin tự thay đổi role của mình
                if (user.UserID == (int)Session["UserID"])
                {
                    return Json(new { success = false, message = "Bạn không thể thay đổi quyền của chính mình." });
                }

                // Kiểm tra role hợp lệ
                if (NewRole < 1 || NewRole > 3)
                {
                    return Json(new { success = false, message = "Quyền không hợp lệ." });
                }

                bool result = userRepository.UpdateUserRole(UserID, NewRole);

                if (result)
                {
                    string roleName = GetRoleName(NewRole);
                    return Json(new { 
                        success = true, 
                        message = $"Đã thay đổi quyền thành {roleName}.",
                        data = new {
                            role = NewRole,
                            roleName = roleName,
                            roleClass = GetRoleClass(NewRole)
                        }
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật quyền." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // POST: Admin/UpdateUserProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateUserProfile(B_M.Models.AdminUserEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
                }

                // Get current user
                var user = userRepository.GetUserForAdminEdit(model.UserID);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy người dùng." });
                }

                // Validation: Check for duplicate email (excluding current user)
                if (userRepository.EmailExistsExcludingUser(model.Email, model.UserID))
                {
                    return Json(new { success = false, message = "Email này đã được sử dụng bởi người dùng khác." });
                }

                // Validation: Check for duplicate username (excluding current user)
                if (!string.IsNullOrEmpty(model.UserName) && 
                    userRepository.UsernameExistsExcludingUser(model.UserName, model.UserID))
                {
                    return Json(new { success = false, message = "Tên đăng nhập này đã được sử dụng bởi người dùng khác." });
                }

                // Update User information
                user.Email = model.Email;
                user.UserName = string.IsNullOrEmpty(model.UserName) ? null : model.UserName;
                user.PhoneNumber = string.IsNullOrEmpty(model.PhoneNumber) ? null : model.PhoneNumber;

                // Update UserDetails
                var userDetails = userRepository.GetUserDetails(model.UserID);
                if (userDetails != null)
                {
                    userDetails.FullName = model.FullName;
                    userDetails.Address = string.IsNullOrEmpty(model.Address) ? null : model.Address;
                }
                else
                {
                    // Create UserDetails if not exists
                    userDetails = new UserDetails
                    {
                        UserID = model.UserID,
                        FullName = model.FullName,
                        Address = string.IsNullOrEmpty(model.Address) ? null : model.Address,
                        ReputationScore = 0
                    };
                }

                // Save changes
                bool result = userRepository.UpdateUserProfile(user, userDetails);

                if (result)
                {
                    return Json(new { 
                        success = true, 
                        message = "Cập nhật thông tin người dùng thành công.",
                        data = new {
                            email = user.Email,
                            username = user.UserName ?? "Chưa thiết lập",
                            phoneNumber = user.PhoneNumber ?? "Chưa cập nhật",
                            fullName = userDetails.FullName,
                            address = userDetails.Address ?? "Chưa cập nhật"
                        }
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật thông tin." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }


        // GET: Admin/Reports
        public ActionResult Reports()
        {
            try
            {
                // Debug: Log bắt đầu
                System.Diagnostics.Debug.WriteLine("=== REPORTS ACTION STARTED ===");
                
                // Get dashboard stats for reports
                var stats = GetDashboardStats();
                System.Diagnostics.Debug.WriteLine($"Stats loaded: Total={stats.TotalUsers}");
                
                // Create reports view model
                var reportsViewModel = new AdminDashboardViewModel
                {
                    TotalUsers = stats.TotalUsers,
                    ActiveUsers = stats.ActiveUsers,
                    AdminUsers = stats.AdminUsers,
                    MomUsers = stats.MomUsers,
                    BrandUsers = stats.BrandUsers,
                    NewUsersThisMonth = stats.NewUsersThisMonth,
                    RecentUsers = stats.RecentUsers
                };
                
                System.Diagnostics.Debug.WriteLine($"Model created: Total={reportsViewModel.TotalUsers}");
                
                return View(reportsViewModel);
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết
                System.Diagnostics.Debug.WriteLine($"=== ERROR IN REPORTS ===");
                System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                
                // Hiển thị lỗi cho user
                TempData["ErrorMessage"] = $"LỖI CHI TIẾT: {ex.Message}";
                
                // Return view với model rỗng
                return View(new AdminDashboardViewModel());
            }
        }

        // GET: Admin/GetChartData - API for chart data
        [HttpGet]
        public JsonResult GetChartData(string chartType)
        {
            try
            {
                object data = null;
                
                switch (chartType?.ToLower())
                {
                    case "usergrowth":
                        data = GetUserGrowthData();
                        break;
                    case "roledistribution":
                        data = GetRoleDistributionData();
                        break;
                    case "monthlyactivity":
                        data = GetMonthlyActivityData();
                        break;
                    case "accountstatus":
                        data = GetAccountStatusData();
                        break;
                    case "posttrends":
                        data = GetPostTrendsData();
                        break;
                    case "regionstats":
                        data = GetRegionStatsData();
                        break;
                    default:
                        return Json(new { success = false, message = "Invalid chart type" }, JsonRequestBehavior.AllowGet);
                }
                
                return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetChartData Error: {ex.Message}");
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Helper: Get user growth data (6 months)
        private object GetUserGrowthData()
        {
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);
            var users = userRepository.GetAllUsers();
            
            var monthlyData = Enumerable.Range(0, 6)
                .Select(i =>
                {
                    var monthDate = sixMonthsAgo.AddMonths(i);
                    var endOfMonth = new DateTime(monthDate.Year, monthDate.Month, 1).AddMonths(1).AddDays(-1);
                    
                    var usersUpToMonth = users.Count(u => u.CreatedAt <= endOfMonth);
                    var newUsersInMonth = users.Count(u => u.CreatedAt.Year == monthDate.Year && u.CreatedAt.Month == monthDate.Month);
                    
                    return new
                    {
                        Month = monthDate.ToString("MM/yyyy"),
                        MonthName = "Tháng " + monthDate.Month,
                        TotalUsers = usersUpToMonth,
                        NewUsers = newUsersInMonth
                    };
                }).ToList();
            
            return new
            {
                months = monthlyData.Select(m => m.MonthName).ToArray(),
                totalUsers = monthlyData.Select(m => m.TotalUsers).ToArray(),
                newUsers = monthlyData.Select(m => m.NewUsers).ToArray()
            };
        }

        // Helper: Get role distribution data
        private object GetRoleDistributionData()
        {
            var users = userRepository.GetAllUsers();
            return new
            {
                labels = new[] { "Quản trị viên", "Mẹ bỉm", "Nhãn hàng" },
                series = new[] 
                {
                    users.Count(u => u.Role == 1),
                    users.Count(u => u.Role == 2),
                    users.Count(u => u.Role == 3)
                }
            };
        }

        // Helper: Get monthly activity data
        private object GetMonthlyActivityData()
        {
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);
            var users = userRepository.GetAllUsers();
            
            var monthlyData = Enumerable.Range(0, 6)
                .Select(i =>
                {
                    var monthDate = sixMonthsAgo.AddMonths(i);
                    var newUsersInMonth = users.Count(u => u.CreatedAt.Year == monthDate.Year && u.CreatedAt.Month == monthDate.Month);
                    
                    // Simulate posts and interactions data (would come from actual posts table)
                    var posts = (int)(newUsersInMonth * 2.5);
                    var interactions = (int)(newUsersInMonth * 5);
                    
                    return new
                    {
                        Month = "T" + monthDate.Month,
                        NewUsers = newUsersInMonth,
                        Posts = posts,
                        Interactions = interactions
                    };
                }).ToList();
            
            return new
            {
                months = monthlyData.Select(m => m.Month).ToArray(),
                newUsers = monthlyData.Select(m => m.NewUsers).ToArray(),
                posts = monthlyData.Select(m => m.Posts).ToArray(),
                interactions = monthlyData.Select(m => m.Interactions).ToArray()
            };
        }

        // Helper: Get account status data
        private object GetAccountStatusData()
        {
            var users = userRepository.GetAllUsers();
            return new
            {
                active = users.Count(u => u.IsActive),
                inactive = users.Count(u => !u.IsActive),
                total = users.Count
            };
        }

        // Helper: Get post trends data
        private object GetPostTrendsData()
        {
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);
            var users = userRepository.GetAllUsers();
            
            // Simulate post trends based on user growth
            var monthlyPosts = Enumerable.Range(0, 6)
                .Select(i =>
                {
                    var monthDate = sixMonthsAgo.AddMonths(i);
                    var usersInMonth = users.Count(u => u.CreatedAt <= monthDate.AddMonths(1).AddDays(-1));
                    
                    // Estimate posts based on active users (each user ~2 posts per month on average)
                    var estimatedPosts = (int)(usersInMonth * 0.6 * 2);
                    
                    return new
                    {
                        Month = "Tháng " + monthDate.Month,
                        Posts = estimatedPosts
                    };
                }).ToList();
            
            return new
            {
                months = monthlyPosts.Select(m => m.Month).ToArray(),
                posts = monthlyPosts.Select(m => m.Posts).ToArray()
            };
        }

        // Helper: Get region statistics data
        private object GetRegionStatsData()
        {
            var users = userRepository.GetAllUsers();
            
            // Group users by address/region (simplified - would need actual region data)
            var regionData = new Dictionary<string, int>();
            
            foreach (var user in users)
            {
                var address = user.UserDetails?.Address ?? "Không rõ";
                
                // Categorize by major cities (simplified logic)
                string region = "Khác";
                if (address.Contains("Hà Nội") || address.Contains("Ha Noi"))
                    region = "Hà Nội";
                else if (address.Contains("Hồ Chí Minh") || address.Contains("TP.HCM") || address.Contains("Sài Gòn"))
                    region = "TP.HCM";
                else if (address.Contains("Đà Nẵng") || address.Contains("Da Nang"))
                    region = "Đà Nẵng";
                else if (address.Contains("Hải Phòng") || address.Contains("Hai Phong"))
                    region = "Hải Phòng";
                else if (address.Contains("Cần Thơ") || address.Contains("Can Tho"))
                    region = "Cần Thơ";
                
                if (regionData.ContainsKey(region))
                    regionData[region]++;
                else
                    regionData[region] = 1;
            }
            
            // Get top 5 regions
            var topRegions = regionData
                .OrderByDescending(kv => kv.Value)
                .Take(5)
                .ToList();
            
            return new
            {
                regions = topRegions.Select(r => r.Key).ToArray(),
                users = topRegions.Select(r => r.Value).ToArray()
            };
        }

        // GET: Admin/TestReports - Test action
        public ActionResult TestReports()
        {
            return Content("TEST SUCCESS - " + DateTime.Now + " - Reports functionality is working!");
        }

        // GET: Admin/TestSimple - Test đơn giản nhất
        public ActionResult TestSimple()
        {
            return Content("SIMPLE TEST OK");
        }

        // GET: Admin/TestReportsModel - Test với model đơn giản
        public ActionResult TestReportsModel()
        {
            try
            {
                // Test với model đơn giản
                var testModel = new AdminDashboardViewModel
                {
                    TotalUsers = 100,
                    ActiveUsers = 80,
                    AdminUsers = 5,
                    MomUsers = 70,
                    BrandUsers = 25,
                    NewUsersThisMonth = 10,
                    RecentUsers = new List<User>()
                };
                
                return View("Reports", testModel);
            }
            catch (Exception ex)
            {
                return Content("ERROR: " + ex.Message);
            }
        }

        // GET: Admin/TestDatabase - Test database connection
        public ActionResult TestDatabase()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== TESTING DATABASE CONNECTION ===");
                
                var users = userRepository.GetAllUsers();
                System.Diagnostics.Debug.WriteLine($"Database test: Found {users.Count} users");
                
                var result = $"DATABASE TEST SUCCESS!<br/>" +
                           $"Found {users.Count} users<br/>" +
                           $"Active users: {users.Count(u => u.IsActive)}<br/>" +
                           $"Mom users: {users.Count(u => u.Role == 2)}<br/>" +
                           $"Brand users: {users.Count(u => u.Role == 3)}<br/>" +
                           $"Time: {DateTime.Now}";
                
                return Content(result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DATABASE ERROR: {ex.Message}");
                return Content($"DATABASE ERROR: {ex.Message}<br/>Stack: {ex.StackTrace}");
            }
        }

        // GET: Admin/TestStats - Test GetDashboardStats method
        public ActionResult TestStats()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== TESTING GETDASHBOARDSTATS ===");
                
                var stats = GetDashboardStats();
                System.Diagnostics.Debug.WriteLine($"Stats test: Total={stats.TotalUsers}, Active={stats.ActiveUsers}");
                
                var result = $"STATS TEST SUCCESS!<br/>" +
                           $"Total Users: {stats.TotalUsers}<br/>" +
                           $"Active Users: {stats.ActiveUsers}<br/>" +
                           $"Admin Users: {stats.AdminUsers}<br/>" +
                           $"Mom Users: {stats.MomUsers}<br/>" +
                           $"Brand Users: {stats.BrandUsers}<br/>" +
                           $"New This Month: {stats.NewUsersThisMonth}<br/>" +
                           $"Time: {DateTime.Now}";
                
                return Content(result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"STATS ERROR: {ex.Message}");
                return Content($"STATS ERROR: {ex.Message}<br/>Stack: {ex.StackTrace}");
            }
        }


        // POST: Admin/DeleteUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteUser(int userId)
        {
            try
            {
                var user = userRepository.GetUserById(userId);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy người dùng." });
                }

                // Không cho phép admin tự xóa tài khoản của mình
                if (user.UserID == (int)Session["UserID"])
                {
                    return Json(new { success = false, message = "Bạn không thể xóa tài khoản của chính mình." });
                }

                bool result = userRepository.DeleteUser(userId);

                if (result)
                {
                    return Json(new { 
                        success = true, 
                        message = "Đã xóa tài khoản thành công."
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra khi xóa tài khoản." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // GET: Admin/ExportUsers
        public ActionResult ExportUsers()
        {
            try
            {
                var users = userRepository.GetAllUsers();
                // TODO: Implement CSV/Excel export
                return Json(new { success = false, message = "Chức năng export chưa được implement." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Helper methods
        private AdminDashboardViewModel GetDashboardStats()
        {
            var users = userRepository.GetAllUsers();
            
            return new AdminDashboardViewModel
            {
                TotalUsers = users.Count,
                ActiveUsers = users.Count(u => u.IsActive),
                AdminUsers = users.Count(u => u.Role == 1),
                MomUsers = users.Count(u => u.Role == 2),
                BrandUsers = users.Count(u => u.Role == 3),
                NewUsersThisMonth = users.Count(u => u.CreatedAt >= DateTime.Now.AddMonths(-1)),
                RecentUsers = users.OrderByDescending(u => u.CreatedAt).Take(5).ToList()
            };
        }


        private string GetRoleName(byte role)
        {
            switch (role)
            {
                case 1: return "Quản trị viên";
                case 2: return "Mẹ bỉm";
                case 3: return "Nhãn hàng";
                default: return "Không xác định";
            }
        }

        private string GetRoleClass(byte role)
        {
            switch (role)
            {
                case 1: return "badge-danger";
                case 2: return "badge-warning";
                case 3: return "badge-info";
                default: return "badge-secondary";
            }
        }

        // Helper methods for advanced search
        private List<User> ApplyAdvancedSearchFilters(List<User> users, string search, string roleFilter,
            string emailSearch, string usernameSearch, string fullNameSearch, string phoneSearch,
            string addressSearch, string statusFilter, DateTime? createdFrom, DateTime? createdTo,
            bool caseSensitive, bool exactMatch)
        {
            // Basic search (backward compatibility)
            if (!string.IsNullOrEmpty(search))
            {
                users = users.Where(u => 
                    ContainsText(u.Email, search, caseSensitive, exactMatch) || 
                    ContainsText(u.UserName, search, caseSensitive, exactMatch) ||
                    (u.UserDetails != null && ContainsText(u.UserDetails.FullName, search, caseSensitive, exactMatch))
                ).ToList();
            }

            // Role filter
            if (!string.IsNullOrEmpty(roleFilter) && int.TryParse(roleFilter, out int role))
            {
                users = users.Where(u => u.Role == role).ToList();
            }

            // Advanced search filters
            if (!string.IsNullOrEmpty(emailSearch))
            {
                users = users.Where(u => ContainsText(u.Email, emailSearch, caseSensitive, exactMatch)).ToList();
            }

            if (!string.IsNullOrEmpty(usernameSearch))
            {
                users = users.Where(u => ContainsText(u.UserName, usernameSearch, caseSensitive, exactMatch)).ToList();
            }

            if (!string.IsNullOrEmpty(fullNameSearch))
            {
                users = users.Where(u => u.UserDetails != null && 
                    ContainsText(u.UserDetails.FullName, fullNameSearch, caseSensitive, exactMatch)).ToList();
            }

            if (!string.IsNullOrEmpty(phoneSearch))
            {
                users = users.Where(u => ContainsText(u.PhoneNumber, phoneSearch, caseSensitive, exactMatch)).ToList();
            }

            if (!string.IsNullOrEmpty(addressSearch))
            {
                users = users.Where(u => u.UserDetails != null && 
                    ContainsText(u.UserDetails.Address, addressSearch, caseSensitive, exactMatch)).ToList();
            }

            // Status filter
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "all")
            {
                bool isActive = statusFilter == "active";
                users = users.Where(u => u.IsActive == isActive).ToList();
            }

            // Date range filter
            if (createdFrom.HasValue)
            {
                users = users.Where(u => u.CreatedAt >= createdFrom.Value).ToList();
            }

            if (createdTo.HasValue)
            {
                users = users.Where(u => u.CreatedAt <= createdTo.Value.AddDays(1).AddTicks(-1)).ToList();
            }

            return users;
        }

        private List<User> ApplySorting(List<User> users, string sortBy, string sortOrder)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                sortBy = "created";
            }

            if (string.IsNullOrEmpty(sortOrder))
            {
                sortOrder = "desc";
            }

            bool isAscending = sortOrder.ToLower() == "asc";

            switch (sortBy.ToLower())
            {
                case "name":
                    return isAscending 
                        ? users.OrderBy(u => u.UserDetails?.FullName ?? "").ToList()
                        : users.OrderByDescending(u => u.UserDetails?.FullName ?? "").ToList();
                
                case "email":
                    return isAscending 
                        ? users.OrderBy(u => u.Email).ToList()
                        : users.OrderByDescending(u => u.Email).ToList();
                
                case "role":
                    return isAscending 
                        ? users.OrderBy(u => u.Role).ToList()
                        : users.OrderByDescending(u => u.Role).ToList();
                
                case "status":
                    return isAscending 
                        ? users.OrderBy(u => u.IsActive).ToList()
                        : users.OrderByDescending(u => u.IsActive).ToList();
                
                case "created":
                default:
                    return isAscending 
                        ? users.OrderBy(u => u.CreatedAt).ToList()
                        : users.OrderByDescending(u => u.CreatedAt).ToList();
            }
        }

        private bool ContainsText(string text, string searchTerm, bool caseSensitive, bool exactMatch)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(searchTerm))
                return false;

            if (exactMatch)
            {
                return caseSensitive 
                    ? text.Equals(searchTerm, StringComparison.Ordinal)
                    : text.Equals(searchTerm, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return caseSensitive 
                    ? text.Contains(searchTerm)
                    : text.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0;
            }
        }

        // GET: Admin/CreateUser
        public ActionResult CreateUser()
        {
            try
            {
                var viewModel = new B_M.Models.AdminCreateUserViewModel
                {
                    Role = 2, // Default to Mom
                    IsActive = true,
                    SendEmailNotification = false,
                    GenerateRandomPassword = false
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải trang tạo người dùng: " + ex.Message;
                return RedirectToAction("Users");
            }
        }

        // POST: Admin/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateUser(B_M.Models.AdminCreateUserViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Check for duplicate email
                if (userRepository.EmailExists(model.Email))
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                    return View(model);
                }

                // Check for duplicate username (if provided)
                if (!string.IsNullOrEmpty(model.UserName) && userRepository.UsernameExists(model.UserName))
                {
                    ModelState.AddModelError("UserName", "Tên đăng nhập này đã được sử dụng.");
                    return View(model);
                }

                // Generate password if requested
                string password = model.Password;
                string temporaryPassword = null;
                
                if (model.GenerateRandomPassword)
                {
                    password = B_M.Helpers.PasswordGenerator.GeneratePassword();
                    temporaryPassword = password;
                    model.GeneratedPassword = password;
                }

                // Create user
                var user = new User
                {
                    Email = model.Email,
                    UserName = string.IsNullOrEmpty(model.UserName) ? null : model.UserName,
                    PhoneNumber = string.IsNullOrEmpty(model.PhoneNumber) ? null : model.PhoneNumber,
                    PasswordHash = PasswordHelper.HashPassword(password),
                    Role = model.Role,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.Now
                };

                // Create user details
                var userDetails = new UserDetails
                {
                    FullName = model.FullName,
                    Address = string.IsNullOrEmpty(model.Address) ? null : model.Address,
                    ReputationScore = 0
                };

                // Save to database
                bool success = userRepository.CreateUser(user, userDetails);

                if (success)
                {
                    // Send welcome email if requested
                    if (model.SendEmailNotification)
                    {
                        try
                        {
                            var emailService = new EmailService();
                            var emailResult = emailService.SendWelcomeEmail(
                                model.Email, 
                                model.FullName, 
                                model.Email,
                                temporaryPassword
                            );

                            if (emailResult.Success)
                            {
                                TempData["SuccessMessage"] = $"Đã tạo tài khoản thành công cho {model.FullName} ({model.Email}). Email thông báo đã được gửi.";
                            }
                            else
                            {
                                TempData["SuccessMessage"] = $"Đã tạo tài khoản thành công cho {model.FullName} ({model.Email})";
                                TempData["WarningMessage"] = $"Không thể gửi email thông báo: {emailResult.Message}";
                            }
                        }
                        catch (Exception emailEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Email sending error: {emailEx.Message}");
                            TempData["SuccessMessage"] = $"Đã tạo tài khoản thành công cho {model.FullName} ({model.Email})";
                            TempData["WarningMessage"] = "Không thể gửi email thông báo. Vui lòng kiểm tra cấu hình email.";
                        }
                    }
                    else
                    {
                        TempData["SuccessMessage"] = $"Đã tạo tài khoản thành công cho {model.FullName} ({model.Email})";
                    }
                    
                    if (model.GenerateRandomPassword)
                    {
                        TempData["GeneratedPassword"] = password;
                        TempData["ShowPassword"] = true;
                    }

                    return RedirectToAction("Users");
                }
                else
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi tạo tài khoản. Vui lòng thử lại.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                return View(model);
            }
        }

        // GET: Admin/ImportUsers
        public ActionResult ImportUsers()
        {
            try
            {
                var viewModel = new B_M.Models.AdminImportUsersViewModel
                {
                    DefaultRole = 2, // Default to Mom
                    IsActive = true,
                    SendEmailNotification = false,
                    GenerateRandomPassword = true,
                    SkipDuplicateEmails = true,
                    SkipDuplicateUsernames = true
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải trang import người dùng: " + ex.Message;
                return RedirectToAction("Users");
            }
        }

        // POST: Admin/ImportUsers
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ImportUsers(B_M.Models.AdminImportUsersViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                if (model.ExcelFile == null || model.ExcelFile.ContentLength == 0)
                {
                    ModelState.AddModelError("ExcelFile", "Vui lòng chọn file Excel.");
                    return View(model);
                }

                // Validate file type
                var allowedExtensions = new[] { ".xlsx", ".xls" };
                var fileExtension = System.IO.Path.GetExtension(model.ExcelFile.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ExcelFile", "Chỉ chấp nhận file Excel (.xlsx, .xls).");
                    return View(model);
                }

                // Validate file size (max 10MB)
                if (model.ExcelFile.ContentLength > 10 * 1024 * 1024)
                {
                    ModelState.AddModelError("ExcelFile", "File quá lớn. Kích thước tối đa là 10MB.");
                    return View(model);
                }

                // Process Excel file
                var result = B_M.Helpers.ExcelHelper.ProcessExcelFile(model.ExcelFile, model, userRepository);

                // Store result in TempData for display
                TempData["ImportResult"] = result;

                if (result.SuccessCount > 0)
                {
                    TempData["SuccessMessage"] = $"Import thành công {result.SuccessCount} người dùng.";
                }

                if (result.ErrorCount > 0)
                {
                    TempData["ErrorMessage"] = $"Có {result.ErrorCount} lỗi trong quá trình import.";
                }

                return RedirectToAction("ImportResult");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi import file: " + ex.Message);
                return View(model);
            }
        }

        // GET: Admin/ImportResult
        public ActionResult ImportResult()
        {
            try
            {
                var result = TempData["ImportResult"] as B_M.Models.AdminImportResultViewModel;
                if (result == null)
                {
                    TempData["ErrorMessage"] = "Không có kết quả import để hiển thị.";
                    return RedirectToAction("ImportUsers");
                }

                return View(result);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi hiển thị kết quả import: " + ex.Message;
                return RedirectToAction("Users");
            }
        }


        // GET: Admin/DownloadTemplate
        public ActionResult DownloadTemplate()
        {
            try
            {
                var templateBytes = B_M.Helpers.ExcelHelper.CreateExcelTemplate();
                var fileName = $"UserImportTemplate_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                
                return File(templateBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo template: " + ex.Message;
                return RedirectToAction("ImportUsers");
            }
        }

    }
}
