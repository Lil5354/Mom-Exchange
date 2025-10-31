USE MomExchange
/*
===================================================================
KỊCH BẢN THÊM DANH MỤC CHO DỰ ÁN MOMEXCHANGE (CẬP NHẬT)
===================================================================
Mô tả:
- Cấu trúc Cha-Con (Parent-Child) theo yêu cầu.
- Các mục trong ảnh là Parent.
- Tự động sinh 2-3 Child cho mỗi Parent.
- IsB2CEnabled = 1: Brand (Doanh nghiệp) được phép bán.
- IsC2CEnabled = 1: Mom (Mẹ bỉm) được phép thanh lý.
*/

-- Tắt kiểm tra khóa ngoại để xóa
EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT ALL";
GO
DELETE FROM dbo.Categories;
GO
-- Bật lại kiểm tra khóa ngoại
EXEC sp_MSforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL";
GO
-- Reset ID tự tăng về 0
DBCC CHECKIDENT ('dbo.Categories', RESEED, 0);
GO

-- Khai báo biến
DECLARE @ParentID_SuaBot INT;
DECLARE @ParentID_BimTa INT;
DECLARE @ParentID_SuaTuoi INT;
DECLARE @ParentID_AnDam INT;
DECLARE @ParentID_Vitamin INT;
DECLARE @ParentID_ChamSoc INT;
DECLARE @ParentID_DoDung INT;
DECLARE @ParentID_GheOto INT;
DECLARE @ParentID_DoChoi INT;

-- ---------------------------------
-- 1. PARENT: Sữa bột cao cấp
-- Logic: Chỉ Brand (B2C) được bán
-- ---------------------------------
INSERT INTO dbo.Categories (CategoryName, ParentCategoryID, IsB2CEnabled, IsC2CEnabled, Description)
VALUES (N'Sữa bột cao cấp', NULL, 1, 0, N'Sữa bột công thức cho bé');
SET @ParentID_SuaBot = SCOPE_IDENTITY();

INSERT INTO dbo.Categories (CategoryName, ParentCategoryID, IsB2CEnabled, IsC2CEnabled)
VALUES 
(N'Sữa cho bé 0-6 tháng', @ParentID_SuaBot, 1, 0),
(N'Sữa cho bé 6-12 tháng', @ParentID_SuaBot, 1, 0),
(N'Sữa cho bé trên 1 tuổi', @ParentID_SuaBot, 1, 0);

-- ---------------------------------
-- 2. PARENT: Bỉm Tã
-- Logic: Chỉ Brand (B2C) được bán
-- ---------------------------------
INSERT INTO dbo.Categories (CategoryName, ParentCategoryID, IsB2CEnabled, IsC2CEnabled, Description)
VALUES (N'Bỉm Tã', NULL, 1, 0, N'Bỉm, tã, khăn ướt cho bé');
SET @ParentID_BimTa = SCOPE_IDENTITY();

INSERT INTO dbo.Categories (CategoryName, ParentCategoryID, IsB2CEnabled, IsC2CEnabled)
VALUES 
(N'Tã dán', @ParentID_BimTa, 1, 0),
(N'Tã quần', @ParentID_BimTa, 1, 0),
(N'Khăn ướt', @ParentID_BimTa, 1, 0);

-- ---------------------------------
-- 3. PARENT: Sữa tươi các loại
-- Logic: Chỉ Brand (B2C) được bán
-- ---------------------------------
INSERT INTO dbo.Categories (CategoryName, ParentCategoryID, IsB2CEnabled, IsC2CEnabled, Description)
VALUES (N'Sữa tươi các loại', NULL, 1, 0, N'Sữa tươi, sữa chua, váng sữa');
SET @ParentID_SuaTuoi = SCOPE_IDENTITY();

INSERT INTO dbo.Categories (CategoryName, ParentCategoryID, IsB2CEnabled, IsC2CEnabled)
VALUES 
(N'Sữa tươi tiệt trùng', @ParentID_SuaTuoi, 1, 0),
(N'Sữa chua & Váng sữa', @ParentID_SuaTuoi, 1, 0);

-- ---------------------------------
-- 4. PARENT: Ăn dặm, dinh dưỡng
-- Logic: Chỉ Brand (B2C) được bán
-- ---------------------------------
INSERT INTO dbo.Categories (CategoryName, ParentCategoryID, IsB2CEnabled, IsC2CEnabled, Description)
VALUES (N'Ăn dặm, dinh dưỡng', NULL, 1, 0, N'Bột, cháo, bánh ăn dặm cho bé');
SET @ParentID_AnDam = SCOPE_IDENTITY();

INSERT INTO dbo.Categories (CategoryName, ParentCategoryID, IsB2CEnabled, IsC2CEnabled)
VALUES 
(N'Bột & Cháo ăn dặm', @ParentID_AnDam, 1, 0),
(N'Bánh ăn dặm', @ParentID_AnDam, 1, 0),
(N'Dầu ăn & Gia vị bé', @ParentID_AnDam, 1, 0);

-- ---------------------------------
-- 5. PARENT: Vitamin & sức khỏe
-- Logic: Chỉ Brand (B2C) được bán
-- ---------------------------------
INSERT INTO dbo.Categories (CategoryName, ParentCategoryID, IsB2CEnabled, IsC2CEnabled, Description)
VALUES (N'Vitamin & sức khỏe', NULL, 1, 0, N'Vitamin, TPCN cho mẹ và bé');
SET @ParentID_Vitamin = SCOPE_IDENTITY();

INSERT INTO dbo.Categories (CategoryName, ParentCategoryID, IsB2CEnabled, IsC2CEnabled)
VALUES 
(N'Vitamin cho bé', @ParentID_Vitamin, 1, 0),
(N'Vitamin cho mẹ', @ParentID_Vitamin, 1, 0),
(N'Tăng đề kháng', @ParentID_Vitamin, 1, 0);

-- ---------------------------------
-- 6. PARENT: Chăm sóc da & Vệ sinh
-- Logic: Chỉ Brand (B2C) được bán
-- ---------------------------------
INSERT INTO dbo.Categories (CategoryName, ParentCategoryID, IsB2CEnabled, IsC2CEnabled, Description)
VALUES (N'Chăm sóc da & Vệ sinh', NULL, 1, 0, N'Sữa tắm, kem hăm, nước giặt xả...');
SET @ParentID_ChamSoc = SCOPE_IDENTITY();

INSERT INTO dbo.Categories (CategoryName, ParentCategoryID, IsB2CEnabled, IsC2CEnabled)
VALUES 
(N'Sữa tắm & Dầu gội', @ParentID_ChamSoc, 1, 0),
(N'Kem hăm & Dưỡng da bé', @ParentID_ChamSoc, 1, 0),
(N'Nước giặt & Xả vải', @ParentID_ChamSoc, 1, 0);

-- ---------------------------------
-- 7. PARENT: Đồ dùng mẹ & bé
-- Logic: Cả B2C (Brand) và C2C (Mẹ bỉm)
-- ---------------------------------
INSERT INTO dbo.Categories (CategoryName, ParentCategoryID, IsB2CEnabled, IsC2CEnabled, Description)
VALUES (N'Đồ dùng mẹ & bé', NULL, 1, 1, N'Máy hút sữa, xe đẩy, địu, nôi cũi...');
SET @ParentID_DoDung = SCOPE_IDENTITY();

INSERT INTO dbo.Categories (CategoryName, ParentCategoryID, IsB2CEnabled, IsC2CEnabled)
VALUES 
(N'Máy hút sữa & Phụ kiện', @ParentID_DoDung, 1, 1),
(N'Máy tiệt trùng & Hâm sữa', @ParentID_DoDung, 1, 1),
(N'Xe đẩy & Địu', @ParentID_DoDung, 1, 1);

-- ---------------------------------
-- 8. PARENT: Ghế ngồi ô tô
-- Logic: Cả B2C (Brand) và C2C (Mẹ bỉm)
-- ---------------------------------
INSERT INTO dbo.Categories (CategoryName, ParentCategoryID, IsB2CEnabled, IsC2CEnabled, Description)
VALUES (N'Ghế ngồi ô tô', NULL, 1, 1, N'Ghế an toàn cho bé khi đi ô tô');
SET @ParentID_GheOto = SCOPE_IDENTITY();

INSERT INTO dbo.Categories (CategoryName, ParentCategoryID, IsB2CEnabled, IsC2CEnabled)
VALUES 
(N'Ghế cho bé 0-1 tuổi', @ParentID_GheOto, 1, 1),
(N'Ghế cho bé 1-12 tuổi', @ParentID_GheOto, 1, 1);

-- ---------------------------------
-- 9. PARENT: Đồ chơi, học tập
-- Logic: Cả B2C (Brand) và C2C (Mẹ bỉm)
-- ---------------------------------
INSERT INTO dbo.Categories (CategoryName, ParentCategoryID, IsB2CEnabled, IsC2CEnabled, Description)
VALUES (N'Đồ chơi, học tập', NULL, 1, 1, N'Đồ chơi vận động, sách, đồ chơi giáo dục');
SET @ParentID_DoChoi = SCOPE_IDENTITY();

INSERT INTO dbo.Categories (CategoryName, ParentCategoryID, IsB2CEnabled, IsC2CEnabled)
VALUES 
(N'Đồ chơi vận động', @ParentID_DoChoi, 1, 1),
(N'Đồ chơi giáo dục & Lắp ráp', @ParentID_DoChoi, 1, 1),
(N'Sách & Truyện', @ParentID_DoChoi, 1, 1);

PRINT N'Hoàn tất thêm các danh mục (cấu trúc Cha-Con) cho MomExchange.';
GO