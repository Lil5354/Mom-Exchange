# ================================================
# Script: Cấp quyền Admin cho user
# Cách dùng: Chạy trong PowerShell
# ================================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   CẤP QUYỀN ADMIN CHO TÀI KHOẢN" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Lấy email từ user
$email = Read-Host "Nhập email tài khoản cần cấp quyền Admin (VD: pupu@gmail.com)"

if ([string]::IsNullOrWhiteSpace($email)) {
    Write-Host "❌ Email không được để trống!" -ForegroundColor Red
    exit
}

Write-Host ""
Write-Host "📋 Tạo SQL Script..." -ForegroundColor Green

# Tạo SQL script
$sqlScript = @"
-- Cấp quyền Admin cho: $email
USE MomExchangeDB;
GO

-- Kiểm tra user hiện tại
SELECT 
    UserID,
    Email,
    UserName,
    Role,
    CASE 
        WHEN Role = 1 THEN 'Admin'
        WHEN Role = 2 THEN 'Mẹ bỉm'
        WHEN Role = 3 THEN 'Nhãn hàng'
        ELSE 'Không xác định'
    END AS RoleName,
    IsActive
FROM Users
WHERE Email = '$email';
GO

-- Cấp quyền Admin
UPDATE Users 
SET Role = 1,  -- 1 = Admin
    IsActive = 1  -- Đảm bảo active
WHERE Email = '$email';
GO

-- Kiểm tra lại
SELECT 
    UserID,
    Email,
    UserName,
    Role,
    CASE 
        WHEN Role = 1 THEN 'Admin ✅'
        WHEN Role = 2 THEN 'Mẹ bỉm'
        WHEN Role = 3 THEN 'Nhãn hàng'
        ELSE 'Không xác định'
    END AS RoleName,
    IsActive
FROM Users
WHERE Email = '$email';
GO

PRINT '✅ Đã cấp quyền Admin cho: $email';
PRINT '⚠️ Vui lòng ĐĂNG XUẤT và ĐĂNG NHẬP LẠI để cập nhật quyền!';
GO
"@

# Lưu SQL script
$sqlFile = "CapQuyenAdmin_$($email -replace '@', '_' -replace '\.', '_').sql"
$sqlScript | Out-File -FilePath $sqlFile -Encoding UTF8

Write-Host "✅ Đã tạo file SQL: $sqlFile" -ForegroundColor Green
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   HƯỚNG DẪN TIẾP THEO:" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1️⃣  Mở SQL Server Management Studio (SSMS)" -ForegroundColor White
Write-Host "2️⃣  Kết nối đến database 'MomExchangeDB'" -ForegroundColor White
Write-Host "3️⃣  Mở file: $sqlFile" -ForegroundColor White
Write-Host "4️⃣  Chạy script (F5)" -ForegroundColor White
Write-Host "5️⃣  ĐĂNG XUẤT và ĐĂNG NHẬP LẠI vào website" -ForegroundColor White
Write-Host "6️⃣  Truy cập: https://localhost:44335/Admin/Category" -ForegroundColor White
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Mở file SQL trong notepad
$openFile = Read-Host "Bạn có muốn mở file SQL ngay bây giờ? (Y/N)"
if ($openFile -eq 'Y' -or $openFile -eq 'y') {
    notepad $sqlFile
}

Write-Host ""
Write-Host "✅ Hoàn tất!" -ForegroundColor Green
Write-Host ""





