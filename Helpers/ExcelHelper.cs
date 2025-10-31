using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using OfficeOpenXml;
using B_M.Models;
using B_M.Services;
using B_M.Repositories;

namespace B_M.Helpers
{
    public static class ExcelHelper
    {
        public static B_M.Models.AdminImportResultViewModel ProcessExcelFile(HttpPostedFileBase file, B_M.Models.AdminImportUsersViewModel model, B_M.Repositories.UserRepository userRepository)
        {
            var result = new B_M.Models.AdminImportResultViewModel
            {
                FileName = file.FileName,
                ImportTime = DateTime.Now
            };

            try
            {
                // Set EPPlus license context
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage(file.InputStream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        result.Errors.Add(new B_M.Models.ImportUserError
                        {
                            RowNumber = 0,
                            ErrorMessage = "File Excel không có worksheet nào"
                        });
                        return result;
                    }

                    var rowCount = worksheet.Dimension?.Rows ?? 0;
                    if (rowCount < 2) // At least header + 1 data row
                    {
                        result.Errors.Add(new B_M.Models.ImportUserError
                        {
                            RowNumber = 0,
                            ErrorMessage = "File Excel không có dữ liệu"
                        });
                        return result;
                    }

                    result.TotalRows = rowCount - 1; // Exclude header

                    // Process each row
                    for (int row = 2; row <= rowCount; row++)
                    {
                        try
                        {
                            var userData = ExtractUserDataFromRow(worksheet, row);
                            var validationResult = ValidateUserData(userData, userRepository, model);

                            if (validationResult.IsValid)
                            {
                                var user = CreateUserFromData(userData, model);
                                var userDetails = CreateUserDetailsFromData(userData);

                                if (userRepository.CreateUser(user, userDetails))
                                {
                                    result.SuccessCount++;
                                    
                                    var successUser = new B_M.Models.ImportUserSuccess
                                    {
                                        RowNumber = row,
                                        Email = user.Email,
                                        FullName = userDetails.FullName,
                                        UserName = user.UserName,
                                        Role = user.Role,
                                        RoleName = GetRoleName(user.Role),
                                        GeneratedPassword = model.GenerateRandomPassword ? userData.GeneratedPassword : null
                                    };
                                    
                                    // Send email notification if enabled
                                    if (model.SendEmailNotification)
                                    {
                                        try
                                        {
                                            var emailService = new EmailService();
                                            var emailResult = emailService.SendWelcomeEmail(
                                                user.Email,
                                                userDetails.FullName,
                                                user.Email,
                                                model.GenerateRandomPassword ? userData.GeneratedPassword : null
                                            );
                                            
                                            successUser.EmailSent = emailResult.Success;
                                            successUser.EmailMessage = emailResult.Message;
                                            
                                            if (emailResult.Success)
                                            {
                                                result.EmailsSentCount++;
                                            }
                                        }
                                        catch (Exception emailEx)
                                        {
                                            successUser.EmailSent = false;
                                            successUser.EmailMessage = $"Lỗi gửi email: {emailEx.Message}";
                                            System.Diagnostics.Debug.WriteLine($"Email error for {user.Email}: {emailEx.Message}");
                                        }
                                    }
                                    
                                    result.SuccessUsers.Add(successUser);
                                }
                                else
                                {
                                    result.ErrorCount++;
                                    result.Errors.Add(new B_M.Models.ImportUserError
                                    {
                                        RowNumber = row,
                                        Email = userData.Email,
                                        ErrorMessage = "Không thể tạo tài khoản trong database"
                                    });
                                }
                            }
                            else
                            {
                                result.ErrorCount++;
                                result.Errors.AddRange(validationResult.Errors.Select(e => new B_M.Models.ImportUserError
                                {
                                    RowNumber = row,
                                    Email = userData.Email,
                                    ErrorMessage = e,
                                    FieldName = "Validation"
                                }));
                            }
                        }
                        catch (Exception ex)
                        {
                            result.ErrorCount++;
                            result.Errors.Add(new B_M.Models.ImportUserError
                            {
                                RowNumber = row,
                                ErrorMessage = $"Lỗi xử lý dòng: {ex.Message}"
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(new B_M.Models.ImportUserError
                {
                    RowNumber = 0,
                    ErrorMessage = $"Lỗi đọc file Excel: {ex.Message}"
                });
            }

            return result;
        }

        private static ExcelUserData ExtractUserDataFromRow(ExcelWorksheet worksheet, int row)
        {
            return new ExcelUserData
            {
                Email = GetCellValue(worksheet, row, 1), // Column A
                UserName = GetCellValue(worksheet, row, 2), // Column B
                FullName = GetCellValue(worksheet, row, 3), // Column C
                PhoneNumber = GetCellValue(worksheet, row, 4), // Column D
                Address = GetCellValue(worksheet, row, 5), // Column E
                Role = GetCellValue(worksheet, row, 6), // Column F
                GeneratedPassword = PasswordGenerator.GeneratePassword()
            };
        }

        private static string GetCellValue(ExcelWorksheet worksheet, int row, int col)
        {
            var cellValue = worksheet.Cells[row, col].Value;
            return cellValue?.ToString()?.Trim() ?? string.Empty;
        }

        private static ValidationResult ValidateUserData(ExcelUserData userData, UserRepository userRepository, B_M.Models.AdminImportUsersViewModel model)
        {
            var result = new ValidationResult();

            // Validate email
            if (string.IsNullOrEmpty(userData.Email))
            {
                result.Errors.Add("Email là bắt buộc");
            }
            else if (!IsValidEmail(userData.Email))
            {
                result.Errors.Add("Email không hợp lệ");
            }
            else if (userRepository.EmailExists(userData.Email))
            {
                if (!model.SkipDuplicateEmails)
                {
                    result.Errors.Add("Email đã tồn tại");
                }
                else
                {
                    result.IsSkipped = true;
                    return result;
                }
            }

            // Validate username (optional)
            if (!string.IsNullOrEmpty(userData.UserName))
            {
                if (userRepository.UsernameExists(userData.UserName))
                {
                    if (!model.SkipDuplicateUsernames)
                    {
                        result.Errors.Add("Tên đăng nhập đã tồn tại");
                    }
                    else
                    {
                        userData.UserName = null; // Clear username if duplicate
                    }
                }
            }

            // Validate full name
            if (string.IsNullOrEmpty(userData.FullName))
            {
                result.Errors.Add("Họ và tên là bắt buộc");
            }

            // Validate phone number
            if (!string.IsNullOrEmpty(userData.PhoneNumber) && !IsValidPhoneNumber(userData.PhoneNumber))
            {
                result.Errors.Add("Số điện thoại không hợp lệ");
            }

            // Validate role
            if (!string.IsNullOrEmpty(userData.Role))
            {
                if (!byte.TryParse(userData.Role, out byte role) || role < 1 || role > 3)
                {
                    result.Errors.Add("Vai trò không hợp lệ (1=Admin, 2=Mom, 3=Brand)");
                }
                else
                {
                    userData.ParsedRole = role;
                }
            }
            else
            {
                userData.ParsedRole = model.DefaultRole;
            }

            result.IsValid = result.Errors.Count == 0;
            return result;
        }

        private static User CreateUserFromData(ExcelUserData userData, B_M.Models.AdminImportUsersViewModel model)
        {
            return new User
            {
                Email = userData.Email,
                UserName = string.IsNullOrEmpty(userData.UserName) ? null : userData.UserName,
                PhoneNumber = string.IsNullOrEmpty(userData.PhoneNumber) ? null : userData.PhoneNumber,
                PasswordHash = PasswordHelper.HashPassword(userData.GeneratedPassword),
                Role = userData.ParsedRole,
                IsActive = model.IsActive,
                CreatedAt = DateTime.Now
            };
        }

        private static UserDetails CreateUserDetailsFromData(ExcelUserData userData)
        {
            return new UserDetails
            {
                FullName = userData.FullName,
                Address = string.IsNullOrEmpty(userData.Address) ? null : userData.Address,
                ReputationScore = 0
            };
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsValidPhoneNumber(string phoneNumber)
        {
            return Regex.IsMatch(phoneNumber, @"^[0-9+\-\s()]*$");
        }

        private static string GetRoleName(byte role)
        {
            switch (role)
            {
                case 1: return "Quản trị viên";
                case 2: return "Khách hàng";
                default: return "Không xác định";
            }
        }

        public static byte[] CreateExcelTemplate()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Users");

                // Headers
                worksheet.Cells[1, 1].Value = "Email";
                worksheet.Cells[1, 2].Value = "UserName";
                worksheet.Cells[1, 3].Value = "FullName";
                worksheet.Cells[1, 4].Value = "PhoneNumber";
                worksheet.Cells[1, 5].Value = "Address";
                worksheet.Cells[1, 6].Value = "Role";

                // Style headers
                using (var range = worksheet.Cells[1, 1, 1, 6])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);
                }

                // Add sample data with various roles
                var sampleData = new[]
                {
                    new { Email = "nguyenvana@gmail.com", Username = "nguyenvana", FullName = "Nguyễn Văn A", Phone = "0901234567", Address = "123 Đường ABC, Quận 1, TP.HCM", Role = "2" },
                    new { Email = "tranthib@gmail.com", Username = "tranthib", FullName = "Trần Thị B", Phone = "0912345678", Address = "456 Đường XYZ, Quận 2, TP.HCM", Role = "2" },
                    new { Email = "phamvanc@gmail.com", Username = "phamvanc", FullName = "Phạm Văn C", Phone = "0923456789", Address = "789 Đường DEF, Quận 3, TP.HCM", Role = "2" },
                    new { Email = "lehoangd@gmail.com", Username = "lehoangd", FullName = "Lê Hoàng D", Phone = "0934567890", Address = "147 Đường GHI, Quận 5, TP.HCM", Role = "2" },
                    new { Email = "vothie@gmail.com", Username = "vothie", FullName = "Võ Thị E", Phone = "0945678901", Address = "258 Đường JKL, Quận 7, TP.HCM", Role = "2" },
                    new { Email = "doanvanf@gmail.com", Username = "doanvanf", FullName = "Đoàn Văn F", Phone = "0956789012", Address = "369 Đường MNO, Quận 10, Hà Nội", Role = "2" },
                    new { Email = "ngothig@gmail.com", Username = "ngothig", FullName = "Ngô Thị G", Phone = "0967890123", Address = "741 Đường PQR, Quận Tân Bình, TP.HCM", Role = "2" },
                    new { Email = "buivanh@gmail.com", Username = "buivanh", FullName = "Bùi Văn H", Phone = "0978901234", Address = "852 Đường STU, Quận Bình Thạnh, TP.HCM", Role = "2" },
                    new { Email = "hoangthii@gmail.com", Username = "hoangthii", FullName = "Hoàng Thị I", Phone = "0989012345", Address = "963 Đường VWX, Quận Phú Nhuận, TP.HCM", Role = "2" }
                };

                int row = 2;
                foreach (var data in sampleData)
                {
                    worksheet.Cells[row, 1].Value = data.Email;
                    worksheet.Cells[row, 2].Value = data.Username;
                    worksheet.Cells[row, 3].Value = data.FullName;
                    worksheet.Cells[row, 4].Value = data.Phone;
                    worksheet.Cells[row, 5].Value = data.Address;
                    worksheet.Cells[row, 6].Value = data.Role;
                    
                    // Alternate row colors for better readability
                    if (row % 2 == 0)
                    {
                        using (var range = worksheet.Cells[row, 1, row, 6])
                        {
                            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(242, 242, 242));
                        }
                    }
                    
                    row++;
                }

                // Add instructions/notes sheet
                var notesSheet = package.Workbook.Worksheets.Add("Hướng dẫn");
                notesSheet.Cells[1, 1].Value = "HƯỚNG DẪN SỬ DỤNG TEMPLATE IMPORT USERS";
                notesSheet.Cells[1, 1].Style.Font.Bold = true;
                notesSheet.Cells[1, 1].Style.Font.Size = 14;
                
                notesSheet.Cells[3, 1].Value = "CÁC CỘT TRONG FILE:";
                notesSheet.Cells[3, 1].Style.Font.Bold = true;
                
                notesSheet.Cells[4, 1].Value = "Email (Bắt buộc):";
                notesSheet.Cells[4, 2].Value = "Địa chỉ email của người dùng. Phải là email hợp lệ và chưa tồn tại trong hệ thống.";
                
                notesSheet.Cells[5, 1].Value = "UserName (Tùy chọn):";
                notesSheet.Cells[5, 2].Value = "Tên đăng nhập. Nếu để trống, người dùng sẽ đăng nhập bằng email.";
                
                notesSheet.Cells[6, 1].Value = "FullName (Bắt buộc):";
                notesSheet.Cells[6, 2].Value = "Họ và tên đầy đủ của người dùng.";
                
                notesSheet.Cells[7, 1].Value = "PhoneNumber (Tùy chọn):";
                notesSheet.Cells[7, 2].Value = "Số điện thoại của người dùng.";
                
                notesSheet.Cells[8, 1].Value = "Address (Tùy chọn):";
                notesSheet.Cells[8, 2].Value = "Địa chỉ của người dùng.";
                
                notesSheet.Cells[9, 1].Value = "Role (Tùy chọn):";
                notesSheet.Cells[9, 2].Value = "Vai trò: 1 = Quản trị viên, 2 = Khách hàng. Mặc định là 2 (Khách hàng).";
                
                notesSheet.Cells[11, 1].Value = "LƯU Ý:";
                notesSheet.Cells[11, 1].Style.Font.Bold = true;
                notesSheet.Cells[11, 1].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                
                notesSheet.Cells[12, 1].Value = "• Không xóa hàng tiêu đề (hàng đầu tiên)";
                notesSheet.Cells[13, 1].Value = "• Mật khẩu sẽ được tạo tự động cho tất cả tài khoản";
                notesSheet.Cells[14, 1].Value = "• Email trùng lặp sẽ được bỏ qua (không tạo)";
                notesSheet.Cells[15, 1].Value = "• Tài khoản được tạo sẽ ở trạng thái hoạt động";
                notesSheet.Cells[16, 1].Value = "• Có thể bật gửi email thông báo cho người dùng mới";
                
                notesSheet.Cells.AutoFitColumns();
                
                // Set column widths for main sheet
                worksheet.Column(1).Width = 30; // Email
                worksheet.Column(2).Width = 20; // Username
                worksheet.Column(3).Width = 25; // FullName
                worksheet.Column(4).Width = 15; // Phone
                worksheet.Column(5).Width = 40; // Address
                worksheet.Column(6).Width = 8;  // Role

                return package.GetAsByteArray();
            }
        }

        // ========== CATEGORY IMPORT METHODS ==========

        public static B_M.Models.AdminImportCategoriesResultViewModel ProcessCategoryExcelFile(
            HttpPostedFileBase file,
            B_M.Models.AdminImportCategoriesViewModel model,
            ApplicationDbContext db)
        {
            var result = new B_M.Models.AdminImportCategoriesResultViewModel
            {
                FileName = file.FileName,
                ImportTime = DateTime.Now
            };

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage(file.InputStream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        result.Errors.Add(new B_M.Models.ImportCategoryError
                        {
                            RowNumber = 0,
                            ErrorMessage = "File Excel không có worksheet nào"
                        });
                        return result;
                    }

                    var rowCount = worksheet.Dimension?.Rows ?? 0;
                    if (rowCount < 2)
                    {
                        result.Errors.Add(new B_M.Models.ImportCategoryError
                        {
                            RowNumber = 0,
                            ErrorMessage = "File Excel không có dữ liệu"
                        });
                        return result;
                    }

                    result.TotalRows = rowCount - 1;

                    // Đọc tất cả categories hiện có để map parent
                    var existingCategories = db.Categories.ToDictionary(
                        c => c.CategoryName.ToLower(),
                        c => c.CategoryID,
                        StringComparer.OrdinalIgnoreCase);

                    // Process each row - process parents first
                    var categoriesToAdd = new List<Category>();
                    var categoryDataList = new List<ExcelCategoryData>();

                    // First pass: read all data
                    for (int row = 2; row <= rowCount; row++)
                    {
                        try
                        {
                            var categoryData = ExtractCategoryDataFromRow(worksheet, row);
                            categoryDataList.Add(categoryData);
                        }
                        catch (Exception ex)
                        {
                            result.ErrorCount++;
                            result.Errors.Add(new B_M.Models.ImportCategoryError
                            {
                                RowNumber = row,
                                ErrorMessage = $"Lỗi đọc dữ liệu dòng: {ex.Message}"
                            });
                        }
                    }

                    // Process categories in order (parents before children)
                    foreach (var categoryData in categoryDataList.OrderBy(c => string.IsNullOrEmpty(c.ParentCategoryName) ? 0 : 1))
                    {
                        try
                        {
                            var validationResult = ValidateCategoryData(categoryData, db, model, existingCategories);

                            if (validationResult.IsValid)
                            {
                                // Check duplicate at same level
                                int? parentId = null;
                                if (!string.IsNullOrWhiteSpace(categoryData.ParentCategoryName))
                                {
                                    var parentNameLower = categoryData.ParentCategoryName.Trim().ToLower();
                                    if (existingCategories.ContainsKey(parentNameLower))
                                    {
                                        parentId = existingCategories[parentNameLower];
                                    }
                                    else
                                    {
                                        result.ErrorCount++;
                                        result.Errors.Add(new B_M.Models.ImportCategoryError
                                        {
                                            RowNumber = categoryData.RowNumber,
                                            CategoryName = categoryData.CategoryName,
                                            ErrorMessage = $"Không tìm thấy danh mục cha: {categoryData.ParentCategoryName}"
                                        });
                                        continue;
                                    }
                                }

                                var isDuplicate = db.Categories.Any(c =>
                                    c.CategoryName.ToLower() == categoryData.CategoryName.Trim().ToLower() &&
                                    c.ParentCategoryID == parentId);

                                if (isDuplicate && model.SkipDuplicateNames)
                                {
                                    result.SkippedCount++;
                                    continue;
                                }

                                if (!isDuplicate)
                                {
                                    var category = CreateCategoryFromData(categoryData, parentId);
                                    db.Categories.Add(category);
                                    db.SaveChanges();

                                    // Update dictionary
                                    existingCategories[category.CategoryName.ToLower()] = category.CategoryID;

                                    result.SuccessCount++;
                                    result.SuccessCategories.Add(new B_M.Models.ImportCategorySuccess
                                    {
                                        RowNumber = categoryData.RowNumber,
                                        CategoryName = category.CategoryName,
                                        ParentCategoryName = categoryData.ParentCategoryName ?? "(Không có)",
                                        IsB2CEnabled = category.IsB2CEnabled,
                                        IsC2CEnabled = category.IsC2CEnabled
                                    });
                                }
                                else
                                {
                                    result.ErrorCount++;
                                    result.Errors.Add(new B_M.Models.ImportCategoryError
                                    {
                                        RowNumber = categoryData.RowNumber,
                                        CategoryName = categoryData.CategoryName,
                                        ErrorMessage = "Danh mục đã tồn tại ở cùng cấp"
                                    });
                                }
                            }
                            else
                            {
                                result.ErrorCount++;
                                result.Errors.AddRange(validationResult.Errors.Select(e => new B_M.Models.ImportCategoryError
                                {
                                    RowNumber = categoryData.RowNumber,
                                    CategoryName = categoryData.CategoryName,
                                    ErrorMessage = e,
                                    FieldName = "Validation"
                                }));
                            }
                        }
                        catch (Exception ex)
                        {
                            result.ErrorCount++;
                            result.Errors.Add(new B_M.Models.ImportCategoryError
                            {
                                RowNumber = categoryData.RowNumber,
                                CategoryName = categoryData.CategoryName ?? "N/A",
                                ErrorMessage = $"Lỗi xử lý dòng: {ex.Message}"
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(new B_M.Models.ImportCategoryError
                {
                    RowNumber = 0,
                    ErrorMessage = $"Lỗi đọc file Excel: {ex.Message}"
                });
            }

            return result;
        }

        private static ExcelCategoryData ExtractCategoryDataFromRow(ExcelWorksheet worksheet, int row)
        {
            return new ExcelCategoryData
            {
                RowNumber = row,
                CategoryName = GetCellValue(worksheet, row, 1), // Column A
                Description = GetCellValue(worksheet, row, 2), // Column B
                ParentCategoryName = GetCellValue(worksheet, row, 3), // Column C
                IsB2CEnabled = GetCellValue(worksheet, row, 4).ToLower() == "true" || GetCellValue(worksheet, row, 4) == "1" || GetCellValue(worksheet, row, 4).ToLower() == "yes", // Column D
                IsC2CEnabled = GetCellValue(worksheet, row, 5).ToLower() == "true" || GetCellValue(worksheet, row, 5) == "1" || GetCellValue(worksheet, row, 5).ToLower() == "yes" // Column E
            };
        }

        private static ValidationResult ValidateCategoryData(
            ExcelCategoryData categoryData,
            ApplicationDbContext db,
            B_M.Models.AdminImportCategoriesViewModel model,
            Dictionary<string, int> existingCategories)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(categoryData.CategoryName))
            {
                result.Errors.Add("Tên danh mục là bắt buộc");
            }

            if (!categoryData.IsB2CEnabled && !categoryData.IsC2CEnabled)
            {
                result.Errors.Add("Phải chọn ít nhất một quyền: B2C hoặc C2C (1/true/yes cho B2C hoặc C2C)");
            }

            result.IsValid = result.Errors.Count == 0;
            return result;
        }

        private static Category CreateCategoryFromData(ExcelCategoryData categoryData, int? parentId)
        {
            return new Category
            {
                CategoryName = categoryData.CategoryName.Trim(),
                Description = string.IsNullOrWhiteSpace(categoryData.Description) ? null : categoryData.Description.Trim(),
                ParentCategoryID = parentId,
                IsB2CEnabled = categoryData.IsB2CEnabled,
                IsC2CEnabled = categoryData.IsC2CEnabled
            };
        }

        public static byte[] CreateCategoryExcelTemplate()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Categories");

                // Headers
                worksheet.Cells[1, 1].Value = "CategoryName";
                worksheet.Cells[1, 2].Value = "Description";
                worksheet.Cells[1, 3].Value = "ParentCategoryName";
                worksheet.Cells[1, 4].Value = "IsB2CEnabled";
                worksheet.Cells[1, 5].Value = "IsC2CEnabled";

                // Style headers
                using (var range = worksheet.Cells[1, 1, 1, 5])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);
                }

                // Sample data
                var sampleData = new[]
                {
                    new { CategoryName = "Đồ dùng cho bé", Description = "Các sản phẩm thiết yếu cho trẻ sơ sinh và trẻ nhỏ", ParentCategoryName = "", IsB2CEnabled = "1", IsC2CEnabled = "1" },
                    new { CategoryName = "Quần áo trẻ em", Description = "Quần áo, phụ kiện cho bé từ 0-5 tuổi", ParentCategoryName = "Đồ dùng cho bé", IsB2CEnabled = "1", IsC2CEnabled = "1" },
                    new { CategoryName = "Đồ chơi trẻ em", Description = "Đồ chơi giáo dục, an toàn cho bé", ParentCategoryName = "Đồ dùng cho bé", IsB2CEnabled = "1", IsC2CEnabled = "1" },
                    new { CategoryName = "Sữa và thực phẩm", Description = "Sữa bột, sữa tươi, thực phẩm bổ sung cho bé", ParentCategoryName = "Đồ dùng cho bé", IsB2CEnabled = "1", IsC2CEnabled = "0" },
                    new { CategoryName = "Xe đẩy & Ghế ăn", Description = "Các loại xe đẩy, ghế ăn dặm cho bé", ParentCategoryName = "Đồ dùng cho bé", IsB2CEnabled = "1", IsC2CEnabled = "1" },
                    new { CategoryName = "Đồ dùng cho mẹ", Description = "Các sản phẩm dành cho mẹ bầu và sau sinh", ParentCategoryName = "", IsB2CEnabled = "1", IsC2CEnabled = "1" },
                    new { CategoryName = "Thời trang bầu", Description = "Quần áo, váy bầu thoải mái và phong cách", ParentCategoryName = "Đồ dùng cho mẹ", IsB2CEnabled = "1", IsC2CEnabled = "1" },
                    new { CategoryName = "Chăm sóc sức khỏe mẹ", Description = "Sản phẩm bổ sung, chăm sóc cá nhân cho mẹ", ParentCategoryName = "Đồ dùng cho mẹ", IsB2CEnabled = "1", IsC2CEnabled = "0" },
                    new { CategoryName = "Đồ dùng cho con bú", Description = "Máy hút sữa, bình sữa, phụ kiện cho con bú", ParentCategoryName = "Đồ dùng cho mẹ", IsB2CEnabled = "1", IsC2CEnabled = "1" },
                    new { CategoryName = "Sách và giáo dục", Description = "Sách về nuôi dạy con, phát triển bản thân cho mẹ", ParentCategoryName = "Đồ dùng cho mẹ", IsB2CEnabled = "1", IsC2CEnabled = "1" }
                };

                int row = 2;
                foreach (var data in sampleData)
                {
                    worksheet.Cells[row, 1].Value = data.CategoryName;
                    worksheet.Cells[row, 2].Value = data.Description;
                    worksheet.Cells[row, 3].Value = data.ParentCategoryName;
                    worksheet.Cells[row, 4].Value = data.IsB2CEnabled;
                    worksheet.Cells[row, 5].Value = data.IsC2CEnabled;

                    // Alternate row colors
                    if (row % 2 == 0)
                    {
                        using (var range = worksheet.Cells[row, 1, row, 5])
                        {
                            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(242, 242, 242));
                        }
                    }

                    row++;
                }

                // Add instructions sheet
                var notesSheet = package.Workbook.Worksheets.Add("Hướng dẫn");
                notesSheet.Cells[1, 1].Value = "HƯỚNG DẪN SỬ DỤNG TEMPLATE IMPORT CATEGORIES";
                notesSheet.Cells[1, 1].Style.Font.Bold = true;
                notesSheet.Cells[1, 1].Style.Font.Size = 14;

                notesSheet.Cells[3, 1].Value = "CÁC CỘT TRONG FILE:";
                notesSheet.Cells[3, 1].Style.Font.Bold = true;

                notesSheet.Cells[4, 1].Value = "CategoryName (Bắt buộc):";
                notesSheet.Cells[4, 2].Value = "Tên danh mục. Phải duy nhất ở cùng cấp.";

                notesSheet.Cells[5, 1].Value = "Description (Tùy chọn):";
                notesSheet.Cells[5, 2].Value = "Mô tả về danh mục.";

                notesSheet.Cells[6, 1].Value = "ParentCategoryName (Tùy chọn):";
                notesSheet.Cells[6, 2].Value = "Tên danh mục cha. Để trống nếu là danh mục gốc.";

                notesSheet.Cells[7, 1].Value = "IsB2CEnabled (Bắt buộc):";
                notesSheet.Cells[7, 2].Value = "1/true/yes để bật B2C, 0/false/no để tắt.";

                notesSheet.Cells[8, 1].Value = "IsC2CEnabled (Bắt buộc):";
                notesSheet.Cells[8, 2].Value = "1/true/yes để bật C2C, 0/false/no để tắt.";

                notesSheet.Cells[10, 1].Value = "LƯU Ý:";
                notesSheet.Cells[10, 1].Style.Font.Bold = true;
                notesSheet.Cells[10, 1].Style.Font.Color.SetColor(System.Drawing.Color.Red);

                notesSheet.Cells[11, 1].Value = "• Không xóa hàng tiêu đề (hàng đầu tiên)";
                notesSheet.Cells[12, 1].Value = "• Phải có ít nhất một trong B2C hoặc C2C được bật (1/true/yes)";
                notesSheet.Cells[13, 1].Value = "• Danh mục cha phải được tạo trước danh mục con";
                notesSheet.Cells[14, 1].Value = "• Tên danh mục trùng lặp ở cùng cấp sẽ bị bỏ qua";

                notesSheet.Cells.AutoFitColumns();

                // Set column widths for main sheet
                worksheet.Column(1).Width = 30; // CategoryName
                worksheet.Column(2).Width = 40; // Description
                worksheet.Column(3).Width = 30; // ParentCategoryName
                worksheet.Column(4).Width = 15; // IsB2CEnabled
                worksheet.Column(5).Width = 15; // IsC2CEnabled

                return package.GetAsByteArray();
            }
        }
    }

    public class ExcelUserData
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Role { get; set; }
        public byte ParsedRole { get; set; }
        public string GeneratedPassword { get; set; }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public bool IsSkipped { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    public class ExcelCategoryData
    {
        public int RowNumber { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public string ParentCategoryName { get; set; }
        public bool IsB2CEnabled { get; set; }
        public bool IsC2CEnabled { get; set; }
    }
}
