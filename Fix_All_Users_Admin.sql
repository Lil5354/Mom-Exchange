-- ===================================================
-- Cấp quyền Admin cho TẤT CẢ user hiện có
-- (Để test nhanh, sau này có thể thu hồi)
-- ===================================================

USE MomExchangeDB;
GO

-- Bước 1: Xem tất cả user hiện có
SELECT 
    UserID,
    Email,
    UserName,
    Role,
    CASE 
        WHEN Role = 1 THEN '🔴 Admin'
        WHEN Role = 2 THEN '🟡 Mẹ bỉm'  
        WHEN Role = 3 THEN '🔵 Nhãn hàng'
        ELSE '⚫ Không xác định'
    END AS RoleName,
    IsActive
FROM Users
ORDER BY UserID;

-- Bước 2: Cấp quyền Admin cho TOÀN BỘ user (để test)
-- LƯU Ý: Chỉ nên làm khi đang dev/test
UPDATE Users 
SET Role = 1,        -- 1 = Admin
    IsActive = 1     -- Active
WHERE IsActive = 1;  -- Chỉ update user đang active

-- Bước 3: Kiểm tra lại
SELECT 
    UserID,
    Email,
    UserName,
    Role,
    CASE 
        WHEN Role = 1 THEN '✅ Admin - CÓ QUYỀN'
        WHEN Role = 2 THEN '❌ Mẹ bỉm - KHÔNG CÓ QUYỀN'  
        WHEN Role = 3 THEN '❌ Nhãn hàng - KHÔNG CÓ QUYỀN'
        ELSE '❌ Không xác định'
    END AS RoleName,
    IsActive
FROM Users
ORDER BY UserID;

PRINT '=================================================';
PRINT '✅ ĐÃ CẤP QUYỀN ADMIN CHO TẤT CẢ USER ACTIVE!';
PRINT '';
PRINT '⚠️  QUAN TRỌNG: PHẢI LÀM NGAY SAU KHI CHẠY SQL:';
PRINT '1️⃣  ĐĂNG XUẤT: /Account/Logout';
PRINT '2️⃣  XÓA CACHE: Ctrl+Shift+Delete';
PRINT '3️⃣  ĐĂNG NHẬP LẠI';
PRINT '4️⃣  Truy cập: /Admin/Category';
PRINT '=================================================';
GO





