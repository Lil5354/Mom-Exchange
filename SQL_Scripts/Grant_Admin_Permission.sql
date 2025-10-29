-- ============================================
-- Script: Cấp quyền Admin cho user
-- Mục đích: Thay đổi Role của user thành Admin (Role = 1)
-- ============================================

-- Bước 1: Kiểm tra user hiện tại và role
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
    IsActive,
    CreatedAt
FROM Users
ORDER BY UserID;

-- Bước 2: Cấp quyền Admin cho user cụ thể
-- Thay 'email@example.com' bằng email của user cần cấp quyền

-- Option 1: Cấp quyền theo Email
UPDATE Users 
SET Role = 1  -- 1 = Admin
WHERE Email = 'pupu@gmail.com';  -- Thay email phù hợp

-- Option 2: Cấp quyền theo UserID
-- UPDATE Users 
-- SET Role = 1
-- WHERE UserID = 1;  -- Thay UserID phù hợp

-- Bước 3: Kiểm tra lại sau khi update
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
WHERE Email = 'pupu@gmail.com';  -- Thay email phù hợp

-- Bước 4: Nếu user bị vô hiệu hóa, kích hoạt lại
-- UPDATE Users
-- SET IsActive = 1
-- WHERE Email = 'pupu@gmail.com';

GO

-- ============================================
-- Lưu ý quan trọng:
-- ============================================
-- Role trong hệ thống:
--   1 = Admin (Quản trị viên)
--   2 = Mom (Mẹ bỉm)
--   3 = Brand (Nhãn hàng)
--
-- Sau khi chạy script này:
-- 1. Đăng xuất khỏi hệ thống
-- 2. Đăng nhập lại với tài khoản vừa cấp quyền
-- 3. Truy cập /Admin/Category để kiểm tra
-- ============================================





