-- ===================================================
-- Insert sample categories để test
-- ===================================================

USE MomExchangeDB;
GO

-- Xóa categories cũ (nếu có)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Brand_Category_Permissions')
    DELETE FROM Brand_Category_Permissions;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Products_B2C')
    DELETE FROM Products_B2C;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Posts_C2C')
    DELETE FROM Posts_C2C;
DELETE FROM Categories;
GO

-- Reset identity
DBCC CHECKIDENT ('Categories', RESEED, 0);
GO

-- Insert root categories
INSERT INTO Categories (CategoryName, Description, ParentCategoryID, IsB2CEnabled, IsC2CEnabled)
VALUES 
    (N'Đồ chơi', N'Đồ chơi cho trẻ em', NULL, 1, 1),
    (N'Quần áo', N'Quần áo trẻ em', NULL, 1, 1),
    (N'Đồ dùng học tập', N'Sách vở, dụng cụ học tập', NULL, 1, 0),
    (N'Đồ ăn & Dinh dưỡng', N'Thực phẩm, sữa, dinh dưỡng', NULL, 1, 0);
GO

-- Get IDs
DECLARE @ToyID INT = (SELECT CategoryID FROM Categories WHERE CategoryName = N'Đồ chơi');
DECLARE @ClothesID INT = (SELECT CategoryID FROM Categories WHERE CategoryName = N'Quần áo');
DECLARE @StudyID INT = (SELECT CategoryID FROM Categories WHERE CategoryName = N'Đồ dùng học tập');
DECLARE @FoodID INT = (SELECT CategoryID FROM Categories WHERE CategoryName = N'Đồ ăn & Dinh dưỡng');

-- Insert subcategories for Đồ chơi
INSERT INTO Categories (CategoryName, Description, ParentCategoryID, IsB2CEnabled, IsC2CEnabled)
VALUES 
    (N'Đồ chơi gỗ', N'Đồ chơi làm từ gỗ tự nhiên', @ToyID, 1, 1),
    (N'Đồ chơi nhựa', N'Đồ chơi nhựa an toàn', @ToyID, 1, 1),
    (N'Xe đẩy & Xe đạp', N'Các loại xe cho trẻ em', @ToyID, 1, 0);

-- Insert subcategories for Quần áo
INSERT INTO Categories (CategoryName, Description, ParentCategoryID, IsB2CEnabled, IsC2CEnabled)
VALUES 
    (N'Áo sơ mi', N'Áo sơ mi cho bé', @ClothesID, 1, 1),
    (N'Quần short', N'Quần short mùa hè', @ClothesID, 1, 1),
    (N'Váy đầm', N'Váy đầm cho bé gái', @ClothesID, 1, 1);

-- Insert subcategories for Đồ dùng học tập
INSERT INTO Categories (CategoryName, Description, ParentCategoryID, IsB2CEnabled, IsC2CEnabled)
VALUES 
    (N'Sách', N'Sách truyện, sách học', @StudyID, 1, 0),
    (N'Vở & Tập', N'Vở viết, tập tô màu', @StudyID, 1, 0);

-- Insert subcategories for Đồ ăn
INSERT INTO Categories (CategoryName, Description, ParentCategoryID, IsB2CEnabled, IsC2CEnabled)
VALUES 
    (N'Sữa công thức', N'Sữa bột cho trẻ em', @FoodID, 1, 0),
    (N'Bình sữa', N'Bình sữa các loại', @FoodID, 1, 0);

GO

-- Kiểm tra kết quả
SELECT 
    CategoryID,
    CategoryName,
    Description,
    ParentCategoryID,
    CASE WHEN IsB2CEnabled = 1 THEN N'✅ B2C' ELSE N'❌' END AS B2C,
    CASE WHEN IsC2CEnabled = 1 THEN N'✅ C2C' ELSE N'❌' END AS C2C,
    CASE 
        WHEN ParentCategoryID IS NULL THEN N'[ROOT]'
        ELSE N'  └─ Level ' + CAST((SELECT COUNT(*) FROM Categories c2 WHERE c2.CategoryID = (SELECT ParentCategoryID FROM Categories WHERE CategoryID = Categories.CategoryID)) AS NVARCHAR)
    END AS Level
FROM Categories
ORDER BY 
    ISNULL(ParentCategoryID, 0),
    CategoryID;

GO

PRINT N'✅ Đã insert thành công!';
PRINT N'📊 Thống kê:';
PRINT N'   - Tổng danh mục: ' + CAST((SELECT COUNT(*) FROM Categories) AS NVARCHAR);
PRINT N'   - Danh mục gốc: ' + CAST((SELECT COUNT(*) FROM Categories WHERE ParentCategoryID IS NULL) AS NVARCHAR);
PRINT N'   - Danh mục con: ' + CAST((SELECT COUNT(*) FROM Categories WHERE ParentCategoryID IS NOT NULL) AS NVARCHAR);
PRINT N'   - Cho phép B2C: ' + CAST((SELECT COUNT(*) FROM Categories WHERE IsB2CEnabled = 1) AS NVARCHAR);
PRINT N'   - Cho phép C2C: ' + CAST((SELECT COUNT(*) FROM Categories WHERE IsC2CEnabled = 1) AS NVARCHAR);
GO

