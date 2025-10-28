// Controllers/MilkDonationController.cs
using B_M.Models;
using B_M.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;

namespace B_M.Controllers
{
    public class MilkDonationController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly B_M.Repositories.UserRepository userRepository;

        public MilkDonationController()
        {
            db = new ApplicationDbContext();
            userRepository = new B_M.Repositories.UserRepository();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db?.Dispose();
                userRepository?.Dispose();
            }
            base.Dispose(disposing);
        }

        // GET: MilkDonation
        public ActionResult Index()
        {
            try
            {
                // Lấy posts từ MilkDonationPosts table
                var milkPosts = db.MilkDonationPosts
                    .Where(p => p.Status == 1) // Open posts only
                    .OrderByDescending(p => p.VerificationTier) // Ưu tiên Tầng 2 (Health Verified)
                    .ThenByDescending(p => p.CreatedAt)
                    .Take(20)
                    .ToList();

                List<MilkDonationPostViewModel> posts = new List<MilkDonationPostViewModel>();

                foreach (var milkPost in milkPosts)
                {
                    // Parse content để lấy thông tin chi tiết
                    var content = milkPost.Content ?? "";
                    var lines = content.Split('\n');
                    
                    var location = ExtractValueFromContent(lines, "Địa điểm:");
                    var dateStr = ExtractValueFromContent(lines, "Ngày vắt:");
                    var dietInfo = ExtractValueFromContent(lines, "Chế độ ăn:");
                    var storageInfo = ExtractValueFromContent(lines, "Bảo quản:");
                    var note = ExtractValueFromContent(lines, "Ghi chú:");

                    DateTime.TryParseExact(dateStr, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime expressionDate);
                    
                    posts.Add(new MilkDonationPostViewModel
                    {
                        Id = (int)milkPost.PostID,
                        DonorUserId = milkPost.UserID,
                        DonorName = milkPost.User?.UserDetails?.FullName ?? "Người dùng",
                        Location = location,
                        DateOfExpression = expressionDate != DateTime.MinValue ? expressionDate : milkPost.CreatedAt.Date,
                        DietInfo = dietInfo,
                        StorageInfo = storageInfo,
                        Note = note,
                        DonorAvatarUrl = milkPost.User?.UserDetails?.ProfilePictureURL ?? "/images/avatar-default.jpg",
                        VerificationTier = milkPost.VerificationTier,
                        PostedAt = milkPost.CreatedAt,
                        Status = milkPost.Status
                    });
                }

                // Nếu không có posts từ DB, fallback to sample data
                if (!posts.Any())
                {
                    posts = GetSamplePosts();
                }

                return View(posts);
            }
            catch (Exception ex)
            {
                // Fallback to sample data on error
            var posts = GetSamplePosts();
            return View(posts);
            }
        }

        // GET: MilkDonation/Details/1
        public ActionResult Details(int id)
        {
            try
            {
                // Lấy user hiện tại để kiểm tra request status
                var userIdentity = User.Identity.Name;
                var currentUser = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Tìm trong MilkDonationPosts trước
                var post = db.MilkDonationPosts
                    .Include(p => p.User)
                    .Include(p => p.User.UserDetails)
                    .FirstOrDefault(p => p.PostID == id && p.Status == 1);

                if (post != null)
                {
                    var content = post.Content ?? "";
                    var lines = content.Split('\n');
                    
                    var location = ExtractValueFromContent(lines, "Địa điểm:");
                    var dateStr = ExtractValueFromContent(lines, "Ngày vắt:");
                    var dietInfo = ExtractValueFromContent(lines, "Chế độ ăn:");
                    var storageInfo = ExtractValueFromContent(lines, "Bảo quản:");
                    var note = ExtractValueFromContent(lines, "Ghi chú:");

                    DateTime.TryParseExact(dateStr, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime expressionDate);
                    
                    // Kiểm tra xem user hiện tại đã gửi request chưa
                    var existingRequest = db.MilkDonationRequests
                        .FirstOrDefault(r => r.PostID == id && r.RecipientUserID == currentUser.UserID);
                    
                    var viewModel = new MilkDonationPostViewModel
                    {
                        Id = (int)post.PostID,
                        DonorUserId = post.UserID,
                        DonorName = post.User?.UserDetails?.FullName ?? "Người dùng",
                        Location = location,
                        DateOfExpression = expressionDate != DateTime.MinValue ? expressionDate : post.CreatedAt.Date,
                        DietInfo = dietInfo,
                        StorageInfo = storageInfo,
                        Note = note,
                        DonorAvatarUrl = post.User?.UserDetails?.ProfilePictureURL ?? "/images/avatar-default.jpg",
                        VerificationTier = post.VerificationTier,
                        PostedAt = post.CreatedAt,
                        Status = post.Status,
                        HasUserRequested = existingRequest != null,
                        UserRequestStatus = existingRequest?.Status
                    };

                    return View(viewModel);
                }

                // Fallback to sample data
                var samplePost = GetSamplePosts().FirstOrDefault(p => p.Id == id);
                if (samplePost == null)
                {
                    return HttpNotFound();
                }
                return View(samplePost);
            }
            catch (Exception ex)
            {
                // Fallback to sample data on error
                var samplePost = GetSamplePosts().FirstOrDefault(p => p.Id == id);
                if (samplePost == null)
            {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                    return RedirectToAction("Index");
            }
                return View(samplePost);
            }
        }

        private List<MilkDonationPostViewModel> GetSamplePosts()
        {
            return new List<MilkDonationPostViewModel>
            {
                new MilkDonationPostViewModel {
                    Id = 1,
                    DonorUserId = 1,
                    DonorName = "Mẹ An Nhiên",
                    Location = "Quận 1, TP.HCM",
                    DateOfExpression = new DateTime(2025, 10, 22),
                    DietInfo = "Ăn uống đa dạng, đủ chất, không sử dụng chất kích thích. Uống vitamin tổng hợp.",
                    StorageInfo = "Sữa được hút bằng máy Medela, trữ trong túi ZipLock chuyên dụng và cấp đông ngay trong tủ đông -18°C.",
                    Note = "Mình có nhiều sữa nên muốn chia sẻ cho các bé có nhu cầu. Chỉ nhận trao đổi tại nhà.",
                    DonorAvatarUrl = "https://i.pinimg.com/1200x/7e/43/35/7e4335dbd0265d9b027ee31ca69e2702.jpg",
                    VerificationTier = 3, // Community Donor
                    PostedAt = DateTime.Now.AddHours(-2),
                    Status = 1 // Open
                },
                new MilkDonationPostViewModel {
                    Id = 2,
                    DonorUserId = 2,
                    DonorName = "Mẹ Bối Bối",
                    Location = "Quận Ba Đình, Hà Nội",
                    DateOfExpression = new DateTime(2025, 10, 20),
                    DietInfo = "Chế độ ăn uống bình thường, lành mạnh.",
                    StorageInfo = "Trữ đông trong tủ lạnh gia đình.",
                    Note = "Sữa cho bé trai, mong muốn tặng cho các mẹ có hoàn cảnh khó khăn.",
                    DonorAvatarUrl = "https://i.pinimg.com/1200x/8f/d7/d6/8fd7d605b7a9ba192913746bf692865b.jpg",
                    VerificationTier = 2, // Health Verified
                    PostedAt = DateTime.Now.AddHours(-5),
                    Status = 1 // Open
                },
                new MilkDonationPostViewModel {
                    Id = 3,
                    DonorUserId = 3,
                    DonorName = "Mẹ Thúy Hằng",
                    Location = "Quận 7, TP.HCM",
                    DateOfExpression = new DateTime(2025, 10, 25),
                    DietInfo = "Ăn chay trường, bổ sung đầy đủ vitamin và khoáng chất theo chỉ định bác sĩ.",
                    StorageInfo = "Hút bằng máy điện, bảo quản ngay lập tức trong tủ đông -20°C.",
                    Note = "Lần đầu tặng sữa, mong được các mẹ hỗ trợ kinh nghiệm. Sẵn sàng gặp mặt trực tiếp.",
                    DonorAvatarUrl = "https://i.pinimg.com/1200x/b1/a2/c3/b1a2c3d4e5f6789012345678901234ab.jpg",
                    VerificationTier = 3, // Community Donor
                    PostedAt = DateTime.Now.AddMinutes(-30),
                    Status = 1 // Open
                }
            };
        }

        // ===== TIER 1 VERIFICATION ACTIONS =====

        // GET: MilkDonation/LifestyleSurvey
        [Authorize]
        public ActionResult LifestyleSurvey()
        {
            try
            {
                // Lấy user từ Identity.Name - có thể là email hoặc username
                var userIdentity = User.Identity.Name;
                var user = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (user == null)
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "Bạn chưa đăng nhập." }, JsonRequestBehavior.AllowGet);
                    }
                    return RedirectToAction("Login", "Account");
                }

                // Kiểm tra xem đã có survey chưa
                var existingSurvey = db.UserLifestyleSurveys.FirstOrDefault(s => s.UserID == user.UserID);
                if (existingSurvey != null)
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "Bạn đã hoàn thành khảo sát lối sống rồi." }, JsonRequestBehavior.AllowGet);
                    }
                    TempData["InfoMessage"] = "Bạn đã hoàn thành khảo sát lối sống. Tiếp tục upload hồ sơ y tế.";
                    return RedirectToAction("MedicalRecords");
                }

                var viewModel = new LifestyleSurveyViewModel
                {
                    UserID = user.UserID,
                    FullName = user.UserDetails?.FullName ?? ""
                };

                // Nếu là AJAX request, trả về partial view
                if (Request.IsAjaxRequest())
                {
                    return PartialView("_LifestyleSurveyPartial", viewModel);
                }

                // Redirect về Index để mở popup thay vì hiển thị trang riêng
                TempData["OpenLifestyleSurveyModal"] = true;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message }, JsonRequestBehavior.AllowGet);
                }
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index", "Profile");
            }
        }

        // POST: MilkDonation/LifestyleSurvey
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult LifestyleSurvey(LifestyleSurveyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Lấy user từ Identity.Name - có thể là email hoặc username
                var userIdentity = User.Identity.Name;
                var user = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (user == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy thông tin người dùng.");
                    return View(model);
                }

                // Kiểm tra xem đã có survey chưa
                var existingSurvey = db.UserLifestyleSurveys.FirstOrDefault(s => s.UserID == user.UserID);
                if (existingSurvey != null)
                {
                    TempData["InfoMessage"] = "Bạn đã hoàn thành khảo sát lối sống.";
                    return RedirectToAction("MedicalRecords");
                }

                // Kiểm tra logic từ chối: nếu không cam kết về ma túy hoặc bệnh truyền nhiễm
                if (!model.CommitNoDrugs || !model.CommitNoInfectiousDiseases)
                {
                    // Cập nhật MilkDonationStatus = 4 (Rejected)
                    user.MilkDonationStatus = 4;
                    userRepository.UpdateUser(user);
                    db.SaveChanges();
                    
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "Rất tiếc, bạn không đủ điều kiện tham gia cho tặng sữa mẹ do không đáp ứng các cam kết an toàn cần thiết." });
                    }
                    TempData["ErrorMessage"] = "Rất tiếc, bạn không đủ điều kiện tham gia cho tặng sữa mẹ do không đáp ứng các cam kết an toàn cần thiết.";
                    return RedirectToAction("Index");
                }

                // Tạo survey mới
                var survey = new UserLifestyleSurvey
                {
                    UserID = user.UserID,
                    IsSmoker = model.IsSmoker,
                    UsesAlcohol = model.UsesAlcohol,
                    UsesMedication = model.UsesMedication,
                    MedicationDetails = model.UsesMedication ? model.MedicationDetails : null,
                    CommitNoDrugs = model.CommitNoDrugs,
                    CommitNoInfectiousDiseases = model.CommitNoInfectiousDiseases,
                    SubmittedAt = DateTime.Now
                };

                db.UserLifestyleSurveys.Add(survey);
                
                // Cập nhật trạng thái user thành BasicDeclared (Tầng 1) - KHÔNG cần chờ duyệt
                if (user.MilkDonationStatus == 0)
                {
                    user.MilkDonationStatus = 1; // BasicDeclared - Tầng 1 đã hoàn thành
                    userRepository.UpdateUser(user);
                }
                
                db.SaveChanges();

                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = true, message = "Hoàn thành khai báo y tế cơ bản! Bạn đã trở thành Người cho tặng Tầng 1." });
                }
                
                TempData["SuccessMessage"] = "Hoàn thành khảo sát lối sống thành công! Bạn đã đạt Tầng 1 và có thể đăng bài tặng sữa ngay. Muốn nâng cấp lên Tầng 2, hãy upload hồ sơ y tế.";
                return RedirectToAction("CreatePost");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                return View(model);
            }
        }

        // GET: MilkDonation/MedicalRecords
        [Authorize]
        public ActionResult MedicalRecords()
        {
            try
            {
                // Lấy user từ Identity.Name - có thể là email hoặc username
                var userIdentity = User.Identity.Name;
                var user = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (user == null)
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "Vui lòng đăng nhập lại." }, JsonRequestBehavior.AllowGet);
                    }
                    return RedirectToAction("Login", "Account");
                }

                // Kiểm tra xem đã có lifestyle survey chưa
                var hasSurvey = db.UserLifestyleSurveys.Any(s => s.UserID == user.UserID);
                if (!hasSurvey)
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "Vui lòng hoàn thành khảo sát lối sống trước." }, JsonRequestBehavior.AllowGet);
                    }
                    TempData["InfoMessage"] = "Vui lòng hoàn thành khảo sát lối sống trước.";
                    return RedirectToAction("LifestyleSurvey");
                }

                var viewModel = new MedicalRecordsViewModel
                {
                    UserID = user.UserID,
                    FullName = user.UserDetails?.FullName ?? "",
                    ExistingRecords = db.UserMedicalRecords
                        .Where(r => r.UserID == user.UserID)
                        .OrderByDescending(r => r.UploadedAt)
                        .ToList()
                };

                if (Request.IsAjaxRequest())
                {
                    return PartialView("_MedicalRecordsPartial", viewModel);
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message }, JsonRequestBehavior.AllowGet);
                }
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index", "Profile");
            }
        }

        // POST: MilkDonation/UploadMedicalRecord
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult UploadMedicalRecord(HttpPostedFileBase medicalFile)
        {
            try
            {
                if (medicalFile == null || medicalFile.ContentLength == 0)
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "Vui lòng chọn file hồ sơ y tế." });
                    }
                    TempData["ErrorMessage"] = "Vui lòng chọn file hồ sơ y tế.";
                    return RedirectToAction("MedicalRecords");
                }

                // Lấy user từ Identity.Name - có thể là email hoặc username
                var userIdentity = User.Identity.Name;
                var user = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng.";
                    return RedirectToAction("Login", "Account");
                }

                // Validate file
                string[] allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
                string fileExtension = Path.GetExtension(medicalFile.FileName).ToLower();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "Chỉ chấp nhận file: PDF, JPG, PNG, DOC, DOCX." });
                    }
                    TempData["ErrorMessage"] = "Chỉ chấp nhận file: PDF, JPG, PNG, DOC, DOCX.";
                    return RedirectToAction("MedicalRecords");
                }

                // Check file size (max 10MB)
                if (medicalFile.ContentLength > 10 * 1024 * 1024)
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "File quá lớn. Kích thước tối đa 10MB." });
                    }
                    TempData["ErrorMessage"] = "File quá lớn. Kích thước tối đa 10MB.";
                    return RedirectToAction("MedicalRecords");
                }

                // Save file
                string fileName = $"medical_{user.UserID}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                string uploadPath = Server.MapPath("~/App_Data/MedicalRecords/");
                
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                string filePath = Path.Combine(uploadPath, fileName);
                medicalFile.SaveAs(filePath);

                // Save to database
                var record = new UserMedicalRecord
                {
                    UserID = user.UserID,
                    FileName = medicalFile.FileName,
                    FileUrl = fileName, // Store filename, not full path for security
                    VerificationStatus = 0, // Pending
                    UploadedAt = DateTime.Now
                };

                db.UserMedicalRecords.Add(record);
                
                // Cập nhật trạng thái user thành PendingVerification nếu chưa
                if (user.MilkDonationStatus < 2)
                {
                    user.MilkDonationStatus = 2; // PendingVerification
                    userRepository.UpdateUser(user);
                }

                db.SaveChanges();

                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = true, message = "Upload hồ sơ y tế thành công! Hồ sơ của bạn đang chờ admin duyệt." });
                }
                TempData["SuccessMessage"] = "Upload hồ sơ y tế thành công! Hồ sơ của bạn đang chờ admin duyệt.";
                return RedirectToAction("MedicalRecords");
            }
            catch (Exception ex)
            {
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra khi upload file: " + ex.Message });
                }
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi upload file: " + ex.Message;
                return RedirectToAction("MedicalRecords");
            }
        }


        // GET: MilkDonation/CreatePost
        [Authorize]
        public ActionResult CreatePost()
        {
            try
            {
                // Lấy user từ Identity.Name - có thể là email hoặc username
                var userIdentity = User.Identity.Name;
                var user = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Kiểm tra trạng thái milk donation - cho phép Tầng 1 (1), Pending (2) và Tầng 2 (3) đăng bài
                if (user.MilkDonationStatus == 0 || user.MilkDonationStatus == 4)
                {
                    TempData["ErrorMessage"] = "Bạn cần hoàn thành khai báo lối sống (Tầng 1) trước khi đăng bài tặng sữa.";
                    return RedirectToAction("LifestyleSurvey");
                }

                var viewModel = new CreateMilkDonationPostViewModel
                {
                    UserID = user.UserID,
                    DonorName = user.UserDetails?.FullName ?? "",
                    VerificationTier = user.MilkDonationStatus
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: MilkDonation/CreatePost
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult CreatePost(CreateMilkDonationPostViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Lấy user từ Identity.Name - có thể là email hoặc username
                var userIdentity = User.Identity.Name;
                var user = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (user == null || user.MilkDonationStatus == 0 || user.MilkDonationStatus == 4)
                {
                    TempData["ErrorMessage"] = "Bạn cần hoàn thành khai báo lối sống trước khi đăng bài tặng sữa.";
                    return RedirectToAction("LifestyleSurvey");
                }

                // Snapshot verification tier logic
                int tierToSave = 1; // Mặc định Tầng 1
                if (user.MilkDonationStatus == 3) // Nếu đã được duyệt y tế
                {
                    tierToSave = 3; // Tầng 2
                }
                // Status 1 (BasicDeclared) và 2 (PendingVerification) đều lưu là Tầng 1

                // Tạo milk donation post mới
                var milkPost = new MilkDonationPost
                {
                    UserID = user.UserID,
                    Title = $"Tặng sữa mẹ tại {model.Location}",
                    Content = $"Địa điểm: {model.Location}\n" +
                             $"Ngày vắt: {model.CollectionDate:dd/MM/yyyy}\n" +
                             $"Chế độ ăn: {model.MotherDietInfo}\n" +
                             $"Bảo quản: {model.StorageMethod}\n" +
                             $"Ghi chú: {model.Note}",
                    VerificationTier = tierToSave, // Snapshot tier
                    Status = 1, // Open
                    CreatedAt = DateTime.Now
                };

                db.MilkDonationPosts.Add(milkPost);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Đăng bài tặng sữa mẹ thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                return View(model);
            }
        }

        // GET: MilkDonation/MyPosts
        [Authorize]
        public ActionResult MyPosts()
        {
            try
            {
                // Lấy user từ Identity.Name - có thể là email hoặc username
                var userIdentity = User.Identity.Name;
                var user = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var myPosts = db.MilkDonationPosts
                    .Where(p => p.UserID == user.UserID)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToList();

                var posts = new List<MilkDonationPostViewModel>();
                foreach (var post in myPosts)
                {
                    var content = post.Content ?? "";
                    var lines = content.Split('\n');
                    
                    var location = ExtractValueFromContent(lines, "Địa điểm:");
                    var dateStr = ExtractValueFromContent(lines, "Ngày vắt:");
                    var dietInfo = ExtractValueFromContent(lines, "Chế độ ăn:");
                    var storageInfo = ExtractValueFromContent(lines, "Bảo quản:");
                    var note = ExtractValueFromContent(lines, "Ghi chú:");

                    DateTime.TryParseExact(dateStr, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime expressionDate);
                    
                    posts.Add(new MilkDonationPostViewModel
                    {
                        Id = (int)post.PostID,
                        DonorUserId = post.UserID,
                        DonorName = post.User?.UserDetails?.FullName ?? "Người dùng",
                        Location = location,
                        DateOfExpression = expressionDate != DateTime.MinValue ? expressionDate : post.CreatedAt.Date,
                        DietInfo = dietInfo,
                        StorageInfo = storageInfo,
                        Note = note,
                        DonorAvatarUrl = post.User?.UserDetails?.ProfilePictureURL ?? "/images/avatar-default.jpg",
                        VerificationTier = post.VerificationTier,
                        PostedAt = post.CreatedAt,
                        Status = post.Status
                    });
                }

                return View(posts);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index", "Profile");
            }
        }

        // POST: MilkDonation/HidePost
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult HidePost(long postId)
        {
            try
            {
                // Lấy user từ Identity.Name - có thể là email hoặc username
                var userIdentity = User.Identity.Name;
                var user = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dùng." });
                }

                var post = db.MilkDonationPosts.FirstOrDefault(p => p.PostID == postId && p.UserID == user.UserID);
                if (post == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bài đăng." });
                }

                post.Status = 2; // Closed
                db.SaveChanges();

                return Json(new { success = true, message = "Đã ẩn bài đăng thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // POST: MilkDonation/DeletePost
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult DeletePost(long postId)
        {
            try
            {
                // Lấy user từ Identity.Name - có thể là email hoặc username
                var userIdentity = User.Identity.Name;
                var user = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dùng." });
                }

                var post = db.MilkDonationPosts.FirstOrDefault(p => p.PostID == postId && p.UserID == user.UserID);
                if (post == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bài đăng." });
                }

                db.MilkDonationPosts.Remove(post);
                db.SaveChanges();

                return Json(new { success = true, message = "Đã xóa bài đăng thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // GET: MilkDonation/CreateRequest
        [Authorize]
        public ActionResult CreateRequest(long postId)
        {
            try
            {
                var userIdentity = User.Identity.Name;
                var currentUser = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var post = db.MilkDonationPosts
                    .Include(p => p.User)
                    .Include(p => p.User.UserDetails)
                    .FirstOrDefault(p => p.PostID == postId && p.Status == 1);

                if (post == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy bài đăng này.";
                    return RedirectToAction("Index");
                }

                // Kiểm tra xem có phải người đăng tin không
                if (post.UserID == currentUser.UserID)
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "Bạn không thể gửi yêu cầu cho bài đăng của chính mình." }, JsonRequestBehavior.AllowGet);
                    }
                    TempData["InfoMessage"] = "Bạn không thể gửi yêu cầu cho bài đăng của chính mình.";
                    return RedirectToAction("Details", new { id = postId });
                }

                // Kiểm tra xem đã gửi request chưa
                var existingRequest = db.MilkDonationRequests
                    .FirstOrDefault(r => r.PostID == postId && r.RecipientUserID == currentUser.UserID);

                if (existingRequest != null)
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "Bạn đã gửi yêu cầu cho bài đăng này rồi." }, JsonRequestBehavior.AllowGet);
                    }
                    TempData["InfoMessage"] = "Bạn đã gửi yêu cầu cho bài đăng này rồi.";
                    return RedirectToAction("Details", new { id = postId });
                }

                var viewModel = new CreateRequestViewModel
                {
                    PostID = postId,
                    DonorUserID = post.UserID,
                    PostTitle = post.Title,
                    DonorName = post.User.UserDetails?.FullName ?? post.User.UserName,
                    VerificationTier = post.VerificationTier
                };

                if (Request.IsAjaxRequest())
                {
                    return PartialView("_CreateRequestPartial", viewModel);
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message }, JsonRequestBehavior.AllowGet);
                }
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: MilkDonation/CreateRequest
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult CreateRequest(CreateRequestViewModel model)
        {
            try
            {
                var userIdentity = User.Identity.Name;
                var currentUser = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (currentUser == null)
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "Vui lòng đăng nhập lại." });
                    }
                    return RedirectToAction("Login", "Account");
                }

                if (ModelState.IsValid)
                {
                    // Kiểm tra xem có phải người đăng tin không
                    var post = db.MilkDonationPosts.FirstOrDefault(p => p.PostID == model.PostID);
                    if (post != null && post.UserID == currentUser.UserID)
                    {
                        if (Request.IsAjaxRequest())
                        {
                            return Json(new { success = false, message = "Bạn không thể gửi yêu cầu cho bài đăng của chính mình." });
                        }
                        TempData["InfoMessage"] = "Bạn không thể gửi yêu cầu cho bài đăng của chính mình.";
                        return RedirectToAction("Details", new { id = model.PostID });
                    }

                    // Kiểm tra xem đã gửi request chưa
                    var existingRequest = db.MilkDonationRequests
                        .FirstOrDefault(r => r.PostID == model.PostID && r.RecipientUserID == currentUser.UserID);

                    if (existingRequest != null)
                    {
                        if (Request.IsAjaxRequest())
                        {
                            return Json(new { success = false, message = "Bạn đã gửi yêu cầu cho bài đăng này rồi." });
                        }
                        TempData["InfoMessage"] = "Bạn đã gửi yêu cầu cho bài đăng này rồi.";
                        return RedirectToAction("Details", new { id = model.PostID });
                    }

                    // Tạo request mới
                    var request = new MilkDonationRequest
                    {
                        PostID = model.PostID,
                        RecipientUserID = currentUser.UserID,
                        DonorUserID = model.DonorUserID,
                        Status = 0, // Pending
                        Note = model.Note,
                        RequestedAt = DateTime.Now
                    };

                    db.MilkDonationRequests.Add(request);
                    db.SaveChanges();

                    // Send notification to donor
                    SendNotification(model.DonorUserID, "Yêu cầu nhận sữa mới", 
                        "Bạn có yêu cầu nhận sữa mới từ " + (currentUser.UserDetails?.FullName ?? currentUser.UserName) + ".", 1, model.PostID, request.RequestID);

                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = true, message = "Gửi yêu cầu thành công! Người cho sẽ được thông báo." });
                    }
                    TempData["SuccessMessage"] = "Gửi yêu cầu thành công! Người cho sẽ được thông báo.";
                    return RedirectToAction("Details", new { id = model.PostID });
                }

                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
                }
                return View(model);
            }
            catch (Exception ex)
            {
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
                }
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: MilkDonation/WaitingList
        [Authorize]
        public ActionResult WaitingList(long postId)
        {
            try
            {
                var userIdentity = User.Identity.Name;
                var currentUser = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Kiểm tra xem có phải người đăng tin không
                var post = db.MilkDonationPosts
                    .Include(p => p.User)
                    .FirstOrDefault(p => p.PostID == postId);

                if (post == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy bài đăng này.";
                    return RedirectToAction("MyPosts");
                }

                if (post.UserID != currentUser.UserID)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền xem danh sách chờ của bài đăng này.";
                    return RedirectToAction("MyPosts");
                }

                // Lấy danh sách yêu cầu đang chờ
                var pendingRequests = db.MilkDonationRequests
                    .Include(r => r.RecipientUser)
                    .Include(r => r.RecipientUser.UserDetails)
                    .Where(r => r.PostID == postId && r.Status == 0)
                    .OrderBy(r => r.RequestedAt)
                    .ToList();

                var viewModel = new List<MilkDonationRequestViewModel>();
                foreach (var request in pendingRequests)
                {
                    viewModel.Add(new MilkDonationRequestViewModel
                    {
                        RequestID = request.RequestID,
                        PostID = request.PostID,
                        PostTitle = post.Title,
                        RecipientUserID = request.RecipientUserID,
                        RecipientName = request.RecipientUser.UserDetails?.FullName ?? request.RecipientUser.UserName,
                        RecipientAvatarUrl = request.RecipientUser.UserDetails?.ProfilePictureURL ?? "/images/avatar-default.jpg",
                        DonorUserID = request.DonorUserID,
                        DonorName = currentUser.UserDetails?.FullName ?? currentUser.UserName,
                        Status = request.Status,
                        RequestedAt = request.RequestedAt,
                        Note = request.Note
                    });
                }

                ViewBag.PostId = postId;
                ViewBag.PostTitle = post.Title;
                ViewBag.PostStatus = post.Status;

                if (Request.IsAjaxRequest())
                {
                    return PartialView("_WaitingListPartial", viewModel);
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message }, JsonRequestBehavior.AllowGet);
                }
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("MyPosts");
            }
        }

        // POST: MilkDonation/AcceptRequest
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult AcceptRequest(long requestId)
        {
            try
            {
                var userIdentity = User.Identity.Name;
                var currentUser = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (currentUser == null)
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "Vui lòng đăng nhập lại." });
                    }
                    return RedirectToAction("Login", "Account");
                }

                // Lấy request và kiểm tra quyền
                var request = db.MilkDonationRequests
                    .Include(r => r.Post)
                    .FirstOrDefault(r => r.RequestID == requestId);

                if (request == null)
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "Không tìm thấy yêu cầu này." });
                    }
                    TempData["ErrorMessage"] = "Không tìm thấy yêu cầu này.";
                    return RedirectToAction("MyPosts");
                }

                if (request.DonorUserID != currentUser.UserID)
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "Bạn không có quyền chấp nhận yêu cầu này." });
                    }
                    TempData["ErrorMessage"] = "Bạn không có quyền chấp nhận yêu cầu này.";
                    return RedirectToAction("MyPosts");
                }

                if (request.Status != 0)
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "Yêu cầu này đã được xử lý rồi." });
                    }
                    TempData["ErrorMessage"] = "Yêu cầu này đã được xử lý rồi.";
                    return RedirectToAction("MyPosts");
                }

                // Logic "Accept 1 = Reject All"
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // 1. Đóng bài đăng
                        request.Post.Status = 2; // Closed
                        db.Entry(request.Post).State = EntityState.Modified;

                        // 2. Chấp nhận yêu cầu được chọn
                        request.Status = 1; // Accepted
                        db.Entry(request).State = EntityState.Modified;

                        // 3. Từ chối tất cả yêu cầu khác cho bài đăng này
                        var otherRequests = db.MilkDonationRequests
                            .Where(r => r.PostID == request.PostID && r.RequestID != requestId && r.Status == 0)
                            .ToList();

                        foreach (var otherRequest in otherRequests)
                        {
                            otherRequest.Status = 2; // Declined
                            db.Entry(otherRequest).State = EntityState.Modified;
                        }

                        db.SaveChanges();
                        transaction.Commit();

                        // Send notifications
                        SendNotification(request.RecipientUserID, "Yêu cầu được chấp nhận", 
                            "Yêu cầu nhận sữa của bạn đã được chấp nhận! Người cho sẽ liên hệ với bạn qua chat.", 2, request.PostID, request.RequestID);
                        
                        foreach (var otherRequest in otherRequests)
                        {
                            SendNotification(otherRequest.RecipientUserID, "Yêu cầu bị từ chối", 
                                "Rất tiếc, người cho đã tặng sữa cho một mẹ khác. Chúc bạn may mắn lần sau.", 3, request.PostID, otherRequest.RequestID);
                        }

                        if (Request.IsAjaxRequest())
                        {
                            return Json(new { 
                                success = true, 
                                message = "Chấp nhận yêu cầu thành công! Bài đăng đã được đóng và chat sẽ được mở.",
                                redirectUrl = Url.Action("Conversation", "Chat", new { userId = request.RecipientUserID })
                            });
                        }
                        TempData["SuccessMessage"] = "Chấp nhận yêu cầu thành công! Bài đăng đã được đóng.";
                        return RedirectToAction("Conversation", "Chat", new { userId = request.RecipientUserID });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                }
            }
            catch (Exception ex)
            {
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
                }
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("MyPosts");
            }
        }

        // POST: MilkDonation/DeclineRequest
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult DeclineRequest(long requestId)
        {
            try
            {
                var userIdentity = User.Identity.Name;
                var currentUser = userRepository.GetUserByEmail(userIdentity) ?? userRepository.GetUserByUsername(userIdentity);
                
                if (currentUser == null)
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "Vui lòng đăng nhập lại." });
                    }
                    return RedirectToAction("Login", "Account");
                }

                // Lấy request và kiểm tra quyền
                var request = db.MilkDonationRequests
                    .FirstOrDefault(r => r.RequestID == requestId);

                if (request == null)
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "Không tìm thấy yêu cầu này." });
                    }
                    TempData["ErrorMessage"] = "Không tìm thấy yêu cầu này.";
                    return RedirectToAction("MyPosts");
                }

                if (request.DonorUserID != currentUser.UserID)
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "Bạn không có quyền từ chối yêu cầu này." });
                    }
                    TempData["ErrorMessage"] = "Bạn không có quyền từ chối yêu cầu này.";
                    return RedirectToAction("MyPosts");
                }

                if (request.Status != 0)
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "Yêu cầu này đã được xử lý rồi." });
                    }
                    TempData["ErrorMessage"] = "Yêu cầu này đã được xử lý rồi.";
                    return RedirectToAction("MyPosts");
                }

                // Từ chối yêu cầu
                request.Status = 2; // Declined
                db.Entry(request).State = EntityState.Modified;
                db.SaveChanges();

                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = true, message = "Đã từ chối yêu cầu." });
                }
                TempData["SuccessMessage"] = "Đã từ chối yêu cầu.";
                return RedirectToAction("WaitingList", new { postId = request.PostID });
            }
            catch (Exception ex)
            {
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
                }
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("MyPosts");
            }
        }

        // Helper method để gửi thông báo
        private void SendNotification(int userId, string title, string message, int type, long? postId = null, long? requestId = null)
        {
            try
            {
                var notification = new Notification
                {
                    UserID = userId,
                    Title = title,
                    Message = message,
                    Type = type,
                    RelatedPostID = postId,
                    RelatedRequestID = requestId,
                    CreatedAt = DateTime.Now
                };

                db.Notifications.Add(notification);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                // Log error but don't break the main flow
                System.Diagnostics.Debug.WriteLine($"Error sending notification: {ex.Message}");
            }
        }

        // Helper method để parse content
        private string ExtractValueFromContent(string[] lines, string prefix)
        {
            var line = lines.FirstOrDefault(l => l.StartsWith(prefix));
            if (line != null)
            {
                return line.Substring(prefix.Length).Trim();
            }
            return "";
        }
    }
}