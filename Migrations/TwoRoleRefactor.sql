/*
===================================================================
KỊCH BẢN TÁI CẤU TRÚC DB SANG 2 ROLE (ADMIN & CLIENT)
===================================================================
*/

-- Bước 0: Gộp Role Nhãn hàng (3) vào Khách hàng (2)
UPDATE dbo.Users SET Role = 2 WHERE Role = 3;

-- Bước 1: Gỡ bỏ Bảng Phân quyền (Không còn cần thiết)
IF OBJECT_ID('dbo.Brand_Category_Permissions', 'U') IS NOT NULL
    DROP TABLE dbo.Brand_Category_Permissions;

-- Bước 2: Tinh chỉnh Bảng Brands (xóa liên kết UserID)
DECLARE @fk NVARCHAR(128);
SELECT TOP (1) @fk = fk.[name]
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns c ON c.object_id = fk.parent_object_id AND c.column_id = fkc.parent_column_id
WHERE fk.parent_object_id = OBJECT_ID('dbo.Brands') AND c.[name] = 'UserID';

IF @fk IS NOT NULL
BEGIN
    EXEC('ALTER TABLE dbo.Brands DROP CONSTRAINT ' + QUOTENAME(@fk));
END

IF COL_LENGTH('dbo.Brands', 'UserID') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Brands DROP COLUMN UserID;
END

PRINT N'Hoàn tất tái cấu trúc DB cho logic 2-Role.';


