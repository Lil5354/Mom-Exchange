# Mom-Exchange Web

Nền tảng trao đổi và mua bán đồ dùng cho mẹ và bé - được xây dựng với ASP.NET MVC và **Entity Framework Code First**.

## 📋 Tổng quan

Mom-Exchange là một trang web cộng đồng cho phép các bà mẹ:
- Trao đổi và mua bán đồ dùng cho bé
- Cho tặng sữa mẹ
- Chia sẻ kinh nghiệm nuôi con
- Kết nối với cộng đồng các bà mẹ

## ✨ Tính năng

### 🔐 Xác thực & Bảo mật
- ✅ Đăng ký tài khoản với username/email/số điện thoại
- ✅ Đăng nhập với email hoặc username + password
- ✅ Đăng nhập bằng Google OAuth 2.0
- ✅ **Form hoàn thiện hồ sơ cho Google OAuth** ⭐ **MỚI**
- ✅ Hash password bảo mật (PBKDF2 + SHA256)
- ✅ Forms Authentication
- ✅ Session management
- ✅ "Remember Me" functionality
- ✅ Đăng xuất
- ✅ Phân quyền người dùng (Admin, Mom, Brand)

### 🛍️ Chức năng chính
- Trao đổi đồ dùng mẹ và bé
- Thanh lý sản phẩm
- Cho tặng sữa mẹ
- Blog chia sẻ kinh nghiệm
- Cộng đồng thảo luận
- Danh mục nhãn hàng
- Hồ sơ người dùng với điểm uy tín

## 🏗️ Kiến trúc - Code First Approach

Project sử dụng **Entity Framework 6 Code First**:
- ✅ Database được tạo từ C# Models
- ✅ Migrations để quản lý schema changes
- ✅ Repository Pattern với DbContext
- ✅ Strongly-typed queries với LINQ

## 🆕 **Tính năng mới: Form hoàn thiện hồ sơ Google OAuth**

### **Khi đăng ký Google lần đầu:**
1. **User click Google** → Chọn tài khoản Google
2. **Hệ thống tạo tài khoản** với thông tin từ Google (Email, FullName)
3. **Hiển thị form hoàn thiện hồ sơ** với các trường:
   - **Tên đăng nhập** (bắt buộc) - để đăng nhập bằng cách khác
   - **Mật khẩu mới** (bắt buộc) - để đăng nhập bằng username/password
   - **Số điện thoại** (tùy chọn)
   - **Địa chỉ** (tùy chọn)
4. **Sau khi hoàn thiện:** User có thể đăng nhập bằng:
   - Email + Password
   - Username + Password  
   - Google OAuth

### **Khi đăng nhập Google lần sau:**
- **Đăng nhập trực tiếp** không cần form hoàn thiện
- **Session được tạo** với thông tin đã có

---

## 🚀 Cài đặt

### Yêu cầu
- Visual Studio 2019 hoặc cao hơn
- .NET Framework 4.7.2
- Microsoft SQL Server (Express hoặc phiên bản đầy đủ)
- Entity Framework 6.4.4

### Các bước cài đặt

#### 1. Clone repository
```bash
git clone https://github.com/yourusername/Mom-Exchange-Web.git
cd Mom-Exchange-Web
```

#### 2. Khôi phục NuGet packages
Mở solution trong Visual Studio và restore packages:
- Nhấp chuột phải vào Solution → "Restore NuGet Packages"
- Hoặc: `Tools → NuGet Package Manager → Package Manager Console`
  ```powershell
  Update-Package -reinstall
  ```

#### 3. Cấu hình Connection String

Mở `Web.config` và cập nhật connection string:
```xml
<connectionStrings>
  <add name="MomExchangeDB" 
       connectionString="Data Source=localhost;Initial Catalog=MomExchangeDB;Integrated Security=True;TrustServerCertificate=True" 
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

**Lưu ý:** 
- Thay `localhost` bằng tên SQL Server của bạn
- Nếu dùng SQL Authentication:
  ```
  Data Source=localhost;Initial Catalog=MomExchangeDB;User ID=sa;Password=yourpassword;TrustServerCertificate=True
  ```

#### 4. Tạo Database với Migrations (Code First)

Mở **Package Manager Console** trong Visual Studio:

```powershell
# Bước 1: Tạo migration đầu tiên
Add-Migration InitialCreate

# Bước 2: Tạo database từ models
Update-Database
```

**Lệnh này sẽ:**
- Quét các Models (User, UserDetails)
- Tạo database MomExchangeDB
- Tạo bảng Users và UserDetails
- Thiết lập relationships và constraints
- Track migrations trong bảng __MigrationHistory

**Xem SQL script trước khi apply (tùy chọn):**
```powershell
Update-Database -Script
```

#### 5. Chạy ứng dụng
- Nhấn **F5** trong Visual Studio
- Hoặc nhấp vào "IIS Express" để chạy

## 📁 Cấu trúc Project

```
Mom-Exchange-Web/
│
├── Controllers/           # MVC Controllers
│   ├── AccountController.cs      # Xử lý đăng ký/đăng nhập
│   └── ...
│
├── Models/               # Models & DbContext (Code First)
│   ├── ApplicationDbContext.cs   # ⭐ EF DbContext
│   ├── User.cs                   # ⭐ User entity
│   ├── UserDetails.cs            # ⭐ UserDetails entity
│   ├── UserRepository.cs         # Repository pattern
│   ├── LoginViewModel.cs
│   ├── RegisterViewModel.cs
│   └── ...
│
├── Migrations/           # ⭐ EF Migrations (Code First)
│   ├── Configuration.cs          # Migration configuration
│   └── [Timestamp]_InitialCreate.cs
│
├── Views/                # Razor Views
│   ├── Account/
│   │   └── Login.cshtml
│   └── ...
│
├── Helpers/
│   └── PasswordHelper.cs
│
├── Web.config            # ⭐ EF configuration
└── MIGRATIONS_GUIDE.md   # ⭐ Chi tiết về Migrations

⭐ = Code First specific files
```

## 🔒 Bảo mật

### Password Hashing
- **Thuật toán**: PBKDF2 (Password-Based Key Derivation Function 2)
- **Hash Algorithm**: SHA256
- **Salt Size**: 16 bytes (128 bit)
- **Key Size**: 32 bytes (256 bit)
- **Iterations**: 10,000

### Authentication
- Forms Authentication với cookie-based sessions
- Anti-forgery tokens cho tất cả POST requests
- Model validation để ngăn chặn invalid data

## 💾 Database Schema (Code First)

### Models định nghĩa Database

#### User Entity

```csharp
public class User
{
    [Key]
    public int UserID { get; set; }
    
    [MaxLength(50), Index(IsUnique = true)]
    public string UserName { get; set; }
    
    [Required, MaxLength(255), Index(IsUnique = true)]
    public string Email { get; set; }
    
    [MaxLength(20), Index(IsUnique = true)]
    public string PhoneNumber { get; set; }
    
    [Required]
    public string PasswordHash { get; set; }
    
    public byte Role { get; set; }  // 1=Admin, 2=Mom, 3=Brand
    
    public bool IsActive { get; set; }
    
    [Column(TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; }
    
    // Navigation property (1-1 relationship)
    public virtual UserDetails UserDetails { get; set; }
}
```

#### UserDetails Entity

```csharp
public class UserDetails
{
    [Key, ForeignKey("User")]
    public int UserID { get; set; }
    
    [Required, MaxLength(100)]
    public string FullName { get; set; }
    
    [MaxLength(500)]
    public string ProfilePictureURL { get; set; }
    
    [MaxLength(500)]
    public string Address { get; set; }
    
    public double ReputationScore { get; set; }
    
    // Navigation property
    public virtual User User { get; set; }
}
```

### Database Tables (Tự động tạo)

**Users Table:**
```sql
CREATE TABLE [dbo].[Users] (
    [UserID] INT IDENTITY(1,1) PRIMARY KEY,
    [UserName] NVARCHAR(50) UNIQUE,
    [Email] NVARCHAR(255) NOT NULL UNIQUE,
    [PhoneNumber] NVARCHAR(20) UNIQUE,
    [PasswordHash] NVARCHAR(MAX) NOT NULL,
    [Role] TINYINT NOT NULL,
    [IsActive] BIT NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL
)
```

**UserDetails Table:**
```sql
CREATE TABLE [dbo].[UserDetails] (
    [UserID] INT PRIMARY KEY,
    [FullName] NVARCHAR(100) NOT NULL,
    [ProfilePictureURL] NVARCHAR(500),
    [Address] NVARCHAR(500),
    [ReputationScore] FLOAT NOT NULL,
    CONSTRAINT [FK_UserDetails_Users] 
        FOREIGN KEY ([UserID]) 
        REFERENCES [Users]([UserID]) 
        ON DELETE CASCADE
)
```

## 🔧 Entity Framework API

### ApplicationDbContext

```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<UserDetails> UserDetails { get; set; }
    
    // Fluent API configuration in OnModelCreating()
}
```

### UserRepository (sử dụng EF)

```csharp
// Tạo user (Code First - EF tự động handle transaction)
bool CreateUser(User user, UserDetails userDetails)

// Kiểm tra email (LINQ query)
bool EmailExists(string email)

// Lấy user với Include (Eager loading)
User GetUserByEmail(string email)

// Update operations
bool UpdateUser(User user)
bool UpdateUserDetails(UserDetails userDetails)
```

## 📝 Hướng dẫn sử dụng

### Đăng ký tài khoản
1. Truy cập `/Account/Login`
2. Nhấp "Đăng Ký"
3. Điền thông tin (Họ tên, Email, SĐT, Mật khẩu)
4. Tài khoản tự động có Role = Mom

### Đăng nhập
1. Nhập Email và Password
2. Chọn "Ghi nhớ tôi" (tùy chọn)
3. Đăng nhập thành công

## 🔄 Migrations Workflow

### Thay đổi Models

```csharp
// 1. Sửa Model
public class User
{
    // Thêm property mới
    public DateTime? LastLoginDate { get; set; }
}

// 2. Tạo Migration
Add-Migration AddLastLoginDate

// 3. Update Database
Update-Database
```

### Rollback Migration

```powershell
# Rollback về migration trước
Update-Database -TargetMigration: PreviousMigrationName

# Rollback tất cả
Update-Database -TargetMigration: 0
```

**Chi tiết:** Xem [MIGRATIONS_GUIDE.md](MIGRATIONS_GUIDE.md)

## 🛠️ Technologies

- **Backend**: ASP.NET MVC 5
- **ORM**: Entity Framework 6.4.4 (Code First)
- **Database**: Microsoft SQL Server
- **Authentication**: Forms Authentication
- **Frontend**: HTML5, CSS3, JavaScript
- **CSS Framework**: Bootstrap 5
- **Icons**: Font Awesome 6

## 📦 NuGet Packages

- EntityFramework (6.4.4) ⭐ **NEW**
- Microsoft.AspNet.Mvc (5.2.9)
- Microsoft.AspNet.Razor (3.2.9)
- Bootstrap (5.2.3)
- jQuery (3.7.0)
- Newtonsoft.Json (13.0.3)

## 🎨 **Giao diện mới - Chủ đề Mẹ và Bé**

### **Màu sắc chủ đạo:**
- **Trắng** - Background chính
- **Hồng pastel** - Màu accent (#d63384, #e91e63)
- **Hồng nhạt** - Background gradient (#fce4ec, #f8e8f0)
- **Xám nhạt** - Text phụ (#8e4a6b)

### **Header mới:**
- **Khi chưa đăng nhập:** Nút "Đăng nhập / Đăng ký" thay vì giỏ hàng
- **Khi đã đăng nhập:** 
  - Hiển thị "Xin chào, [Tên user]" với icon user
  - Nút "Giỏ hàng" với gradient hồng
  - Nút "Đăng xuất" với style nhẹ nhàng

### **Hiệu ứng mượt mà:**
- **Hover effects** - Transform và shadow
- **Gradient buttons** - Màu hồng đẹp mắt
- **Dropdown animation** - Smooth transition
- **Responsive design** - Mobile-friendly

### **Navbar cải tiến:**
- **Text không bị chèn xuống** - Sử dụng `white-space: nowrap` và `container-fluid`
- **User dropdown** - Click để mở menu cá nhân hóa
- **Menu cá nhân hóa** bao gồm:
  - Chỉnh sửa thông tin
  - Đổi mật khẩu
  - Quên mật khẩu
  - Đơn hàng của tôi
  - Yêu thích
  - Đăng xuất
- **Animation mượt mà** - Chevron icon xoay khi mở/đóng

## 🚀 **Trang 404 Not Found - Astronaut Theme**

### **Tính năng trang 404:**
- **Giao diện astronaut** - Chủ đề không gian đẹp mắt
- **Animation SVG** - Astronaut floating với Lottie.js
- **Gradient background** - Màu xanh tím không gian
- **Floating elements** - Sao và hành tinh bay lơ lửng
- **Responsive design** - Hoạt động tốt trên mobile
- **Action buttons** - Về trang chủ và quay lại

### **Cách sử dụng:**
1. **Truy cập trực tiếp:** `https://localhost:44335/Error/NotFound` để xem trang 404
2. **Truy cập trực tiếp:** `https://localhost:44335/Error/ServerError` để xem trang 500
3. **Tự động redirect:** Khi có lỗi 404, hệ thống tự động chuyển đến trang này
4. **Test URL lỗi:** Truy cập URL không tồn tại (ví dụ: `/test-404`, `/abc-xyz`)

### **Files đã tạo:**
- `Views/Shared/NotFound.cshtml` - Trang 404 với giao diện astronaut SVG
- `Views/Shared/ServerError.cshtml` - Trang 500 với giao diện server error
- `Controllers/ErrorController.cs` - Controller xử lý các lỗi
- `App_Start/RouteConfig.cs` - Cấu hình routes cho error pages
- `Web.config` - Cấu hình redirect lỗi 404/500

### **Tính năng 404 Baby Theme (Theo mẫu Codepen mới):**
- **Gradient background** - Nền gradient pastel hồng xanh phù hợp chủ đề mẹ và bé
- **Baby illustration** - Em bé dễ thương với animation mượt mà
- **404 gradient text** - Chữ số 404 với gradient màu sắc đẹp mắt
- **Bottle animation** - Bình sữa bay lơ lửng bên cạnh em bé
- **Stars twinkle** - Các ngôi sao nhấp nháy rải rác khắp nơi
- **Interactive effects** - Hover effects và ripple effects cho buttons
- **Pure CSS animation** - Không cần thư viện bên ngoài, chỉ dùng CSS thuần
- **Responsive design** - Hoạt động tốt trên mọi thiết bị

### **Animation Effects:**
- **Baby floating** - Em bé di chuyển lên xuống nhẹ nhàng
- **Arms waving** - Tay em bé vẫy chào với animation xoay
- **Legs kicking** - Chân em bé đá nhẹ như đang chơi
- **404 bounce** - Các chữ số 404 nhảy lên xuống với delay khác nhau
- **Bottle floating** - Bình sữa bay lơ lửng với animation riêng
- **Stars twinkle** - Các ngôi sao nhấp nháy với timing ngẫu nhiên
- **Button ripple** - Hiệu ứng ripple khi click vào buttons
- **Hover effects** - Em bé phóng to khi hover

### **Bảng màu Baby Theme:**
- **Nền gradient:** Pastel hồng xanh (#fce4ec → #f8e8f0 → #e8f5e8)
- **Em bé da:** Gradient vàng (#ffeaa7 → #fdcb6e)
- **Quần áo em bé:** Gradient xanh (#74b9ff → #0984e3)
- **Chữ 404:** Gradient hồng cam (#ff6b9d → #c44569 → #f8b500)
- **Bình sữa:** Gradient tím (#a29bfe → #6c5ce7)
- **Núm bình:** Gradient hồng (#fd79a8 → #e84393)
- **Ngôi sao:** Gradient cam vàng (#fdcb6e → #e17055)
- **Text chính:** Xám đậm (#2d3436)
- **Text phụ:** Xám nhạt (#636e72)

## 🎯 Vai trò người dùng (Roles)

| Role | Giá trị | Mô tả |
|------|---------|-------|
| Admin | 1 | Quản trị viên |
| Mom | 2 | Người dùng mẹ (mặc định) |
| Brand | 3 | Nhãn hàng |

## 🚧 Code First vs Database First

### ✅ Ưu điểm Code First (đang dùng)

- **Version Control**: Schema changes trong code
- **Migrations**: Dễ rollback/forward
- **Team Work**: Không conflict SQL scripts
- **Type Safety**: IntelliSense, compile-time checking
- **Productivity**: Automatic database generation

### 📊 Migration History

Tất cả migrations được track trong bảng `__MigrationHistory`:

```sql
SELECT * FROM __MigrationHistory
```

## 🚧 Tính năng sắp tới

- [ ] Password reset với migrations
- [ ] Email verification
- [ ] Role-based authorization (sử dụng EF)
- [ ] User profile management
- [ ] Shopping cart (new entities)
- [ ] Order tracking (new migrations)

## 📚 Tài liệu

- [MIGRATIONS_GUIDE.md](MIGRATIONS_GUIDE.md) - Chi tiết về Migrations
- [Entity Framework Docs](https://docs.microsoft.com/en-us/ef/ef6/)

## 👥 Đóng góp

Khi thêm tính năng mới:
1. Tạo/sửa Models
2. `Add-Migration TenMigration`
3. Review migration code
4. `Update-Database`
5. Test thoroughly
6. Commit migration files

## 📄 License

Dự án học tập - phi thương mại

---

**Version**: 2.0.0 (Code First)  
**Last Updated**: October 14, 2024  
**Architecture**: Entity Framework Code First  
