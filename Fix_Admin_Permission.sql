-- ===================================================
-- Cấp quyền Admin cho: angelletta2610@gmail.com
-- ===================================================

USE MomExchangeDB;
GO

-- Bước 1: Kiểm tra user hiện tại
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
WHERE Email = 'angelletta2610@gmail.com';

-- Bước 2: Cấp quyền Admin
UPDATE Users 
SET Role = 1,        -- 1 = Admin
    IsActive = 1     -- Đảm bảo tài khoản active
WHERE Email = 'angelletta2610@gmail.com';

-- Bước 3: Kiểm tra lại sau khi update
SELECT 
    UserID,
    Email,
    UserName,
    Role,
    CASE 
        WHEN Role = 1 THEN '✅ Admin - CÓ QUYỀN TRUY CẬP'
        WHEN Role = 2 THEN '❌ Mẹ bỉm - KHÔNG CÓ QUYỀN'  
        WHEN Role = 3 THEN '❌ Nhãn hàng - KHÔNG CÓ QUYỀN'
        ELSE '❌ Không xác định'
    END AS RoleName,
    IsActive
FROM Users
WHERE Email = 'angelletta2610@gmail.com';

-- Thông báo
PRINT '=================================================';
PRINT '✅ ĐÃ CẤP QUYỀN ADMIN THÀNH CÔNG!';
PRINT '📧 Email: angelletta2610@gmail.com';
PRINT '🔑 Role: 1 (Admin)';
PRINT '🟢 Status: Active';
PRINT '';
PRINT '⚠️  QUAN TRỌNG: PHẢI LÀM CÁC BƯỚC SAU:';
PRINT '1️⃣  ĐĂNG XUẤT khỏi website';
PRINT '2️⃣  XÓA CACHE trình duyệt (Ctrl+Shift+Delete)';
PRINT '3️⃣  ĐĂNG NHẬP LẠI với email: angelletta2610@gmail.com';
PRINT '4️⃣  Truy cập: https://localhost:44335/Admin/Category';
PRINT '=================================================';
GO




