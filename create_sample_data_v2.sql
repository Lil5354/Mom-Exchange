-- Script tạo dữ liệu mẫu cho biểu đồ báo cáo (Đợt sửa lại)
-- Tạo 60 bài post C2C và 30 bài post Milk trong 6 tháng gần

-- 1. Xóa dữ liệu cũ
DELETE FROM Posts_C2C WHERE CreatedAt >= DATEADD(MONTH, -6, GETDATE());
DELETE FROM MilkDonationPosts WHERE CreatedAt >= DATEADD(MONTH, -6, GETDATE());

-- 2. Tạo dữ liệu Posts_C2C - Tháng 5 (10 bài)
INSERT INTO Posts_C2C (UserID, CategoryID, Title, Content, Condition, Price, ListingType, Status, CreatedAt)
VALUES
(1, 1, 'Quan ao tre em 1', 'Quan ao tre em dep', 'Moi 90%', 100000, 1, 1, '2025-05-05'),
(1, 1, 'Quan ao tre em 2', 'Quan ao tre em dep', 'Moi 90%', 110000, 1, 1, '2025-05-08'),
(1, 1, 'Quan ao tre em 3', 'Quan ao tre em dep', 'Moi 90%', 120000, 1, 1, '2025-05-12'),
(1, 1, 'Quan ao tre em 4', 'Quan ao tre em dep', 'Moi 90%', 130000, 1, 1, '2025-05-15'),
(1, 1, 'Quan ao tre em 5', 'Quan ao tre em dep', 'Moi 90%', 140000, 1, 1, '2025-05-18'),
(1, 1, 'Quan ao tre em 6', 'Quan ao tre em dep', 'Moi 90%', 150000, 1, 1, '2025-05-22'),
(1, 1, 'Quan ao tre em 7', 'Quan ao tre em dep', 'Moi 90%', 160000, 1, 1, '2025-05-25'),
(1, 1, 'Quan ao tre em 8', 'Quan ao tre em dep', 'Moi 90%', 170000, 1, 1, '2025-05-28');

-- 3. Tạo dữ liệu Posts_C2C - Tháng 6 (10 bài)
INSERT INTO Posts_C2C (UserID, CategoryID, Title, Content, Condition, Price, ListingType, Status, CreatedAt)
VALUES
(1, 1, 'Quan ao tre em 9', 'Quan ao tre em dep', 'Moi 90%', 180000, 1, 1, '2025-06-02'),
(1, 1, 'Quan ao tre em 10', 'Quan ao tre em dep', 'Moi 90%', 190000, 1, 1, '2025-06-05'),
(1, 1, 'Quan ao tre em 11', 'Quan ao tre em dep', 'Moi 90%', 200000, 1, 1, '2025-06-08'),
(1, 1, 'Quan ao tre em 12', 'Quan ao tre em dep', 'Moi 90%', 210000, 1, 1, '2025-06-12'),
(1, 1, 'Quan ao tre em 13', 'Quan ao tre em dep', 'Moi 90%', 220000, 1, 1, '2025-06-15'),
(1, 1, 'Quan ao tre em 14', 'Quan ao tre em dep', 'Moi 90%', 230000, 1, 1, '2025-06-18'),
(1, 1, 'Quan ao tre em 15', 'Quan ao tre em dep', 'Moi 90%', 240000, 1, 1, '2025-06-22'),
(1, 1, 'Quan ao tre em 16', 'Quan ao tre em dep', 'Moi 90%', 250000, 1, 1, '2025-06-25'),
(1, 1, 'Quan ao tre em 17', 'Quan ao tre em dep', 'Moi 90%', 260000, 1, 1, '2025-06-28'),
(1, 1, 'Quan ao tre em 18', 'Quan ao tre em dep', 'Moi 90%', 270000, 1, 1, '2025-06-30');

-- 4. Tạo dữ liệu Posts_C2C - Tháng 7 (8 bài)
INSERT INTO Posts_C2C (UserID, CategoryID, Title, Content, Condition, Price, ListingType, Status, CreatedAt)
VALUES
(1, 1, 'Quan ao tre em 19', 'Quan ao tre em dep', 'Moi 90%', 280000, 1, 1, '2025-07-03'),
(1, 1, 'Quan ao tre em 20', 'Quan ao tre em dep', 'Moi 90%', 290000, 1, 1, '2025-07-08'),
(1, 1, 'Quan ao tre em 21', 'Quan ao tre em dep', 'Moi 90%', 300000, 1, 1, '2025-07-12'),
(1, 1, 'Quan ao tre em 22', 'Quan ao tre em dep', 'Moi 90%', 310000, 1, 1, '2025-07-15'),
(1, 1, 'Quan ao tre em 23', 'Quan ao tre em dep', 'Moi 90%', 320000, 1, 1, '2025-07-18'),
(1, 1, 'Quan ao tre em 24', 'Quan ao tre em dep', 'Moi 90%', 330000, 1, 1, '2025-07-22'),
(1, 1, 'Quan ao tre em 25', 'Quan ao tre em dep', 'Moi 90%', 340000, 1, 1, '2025-07-25'),
(1, 1, 'Quan ao tre em 26', 'Quan ao tre em dep', 'Moi 90%', 350000, 1, 1, '2025-07-30');

-- 5. Tạo dữ liệu Posts_C2C - Tháng 8 (12 bài)
INSERT INTO Posts_C2C (UserID, CategoryID, Title, Content, Condition, Price, ListingType, Status, CreatedAt)
VALUES
(1, 1, 'Quan ao tre em 27', 'Quan ao tre em dep', 'Moi 90%', 360000, 1, 1, '2025-08-02'),
(1, 1, 'Quan ao tre em 28', 'Quan ao tre em dep', 'Moi 90%', 370000, 1, 1, '2025-08-05'),
(1, 1, 'Quan ao tre em 29', 'Quan ao tre em dep', 'Moi 90%', 380000, 1, 1, '2025-08-08'),
(1, 1, 'Quan ao tre em 30', 'Quan ao tre em dep', 'Moi 90%', 390000, 1, 1, '2025-08-12'),
(1, 1, 'Quan ao tre em 31', 'Quan ao tre em dep', 'Moi 90%', 400000, 1, 1, '2025-08-15'),
(1, 1, 'Quan ao tre em 32', 'Quan ao tre em dep', 'Moi 90%', 410000, 1, 1, '2025-08-18'),
(1, 1, 'Quan ao tre em 33', 'Quan ao tre em dep', 'Moi 90%', 420000, 1, 1, '2025-08-22'),
(1, 1, 'Quan ao tre em 34', 'Quan ao tre em dep', 'Moi 90%', 430000, 1, 1, '2025-08-25'),
(1, 1, 'Quan ao tre em 35', 'Quan ao tre em dep', 'Moi 90%', 440000, 1, 1, '2025-08-28'),
(1, 1, 'Quan ao tre em 36', 'Quan ao tre em dep', 'Moi 90%', 450000, 1, 1, '2025-08-30'),
(1, 1, 'Quan ao tre em 37', 'Quan ao tre em dep', 'Moi 90%', 460000, 1, 1, '2025-08-31'),
(1, 1, 'Quan ao tre em 38', 'Quan ao tre em dep', 'Moi 90%', 470000, 1, 1, '2025-08-31');

-- 6. Tạo dữ liệu Posts_C2C - Tháng 9 (10 bài)
INSERT INTO Posts_C2C (UserID, CategoryID, Title, Content, Condition, Price, ListingType, Status, CreatedAt)
VALUES
(1, 1, 'Quan ao tre em 39', 'Quan ao tre em dep', 'Moi 90%', 480000, 1, 1, '2025-09-05'),
(1, 1, 'Quan ao tre em 40', 'Quan ao tre em dep', 'Moi 90%', 490000, 1, 1, '2025-09-08'),
(1, 1, 'Quan ao tre em 41', 'Quan ao tre em dep', 'Moi 90%', 500000, 1, 1, '2025-09-12'),
(1, 1, 'Quan ao tre em 42', 'Quan ao tre em dep', 'Moi 90%', 510000, 1, 1, '2025-09-15'),
(1, 1, 'Quan ao tre em 43', 'Quan ao tre em dep', 'Moi 90%', 520000, 1, 1, '2025-09-18'),
(1, 1, 'Quan ao tre em 44', 'Quan ao tre em dep', 'Moi 90%', 530000, 1, 1, '2025-09-22'),
(1, 1, 'Quan ao tre em 45', 'Quan ao tre em dep', 'Moi 90%', 540000, 1, 1, '2025-09-25'),
(1, 1, 'Quan ao tre em 46', 'Quan ao tre em dep', 'Moi 90%', 550000, 1, 1, '2025-09-28'),
(1, 1, 'Quan ao tre em 47', 'Quan ao tre em dep', 'Moi 90%', 560000, 1, 1, '2025-09-30'),
(1, 1, 'Quan ao tre em 48', 'Quan ao tre em dep', 'Moi 90%', 570000, 1, 1, '2025-09-30');

-- 7. Tạo dữ liệu Posts_C2C - Tháng 10 (12 bài)
INSERT INTO Posts_C2C (UserID, CategoryID, Title, Content, Condition, Price, ListingType, Status, CreatedAt)
VALUES
(1, 1, 'Quan ao tre em 49', 'Quan ao tre em dep', 'Moi 90%', 580000, 1, 1, '2025-10-02'),
(1, 1, 'Quan ao tre em 50', 'Quan ao tre em dep', 'Moi 90%', 590000, 1, 1, '2025-10-05'),
(1, 1, 'Quan ao tre em 51', 'Quan ao tre em dep', 'Moi 90%', 600000, 1, 1, '2025-10-08'),
(1, 1, 'Quan ao tre em 52', 'Quan ao tre em dep', 'Moi 90%', 610000, 1, 1, '2025-10-12'),
(1, 1, 'Quan ao tre em 53', 'Quan ao tre em dep', 'Moi 90%', 620000, 1, 1, '2025-10-15'),
(1, 1, 'Quan ao tre em 54', 'Quan ao tre em dep', 'Moi 90%', 630000, 1, 1, '2025-10-18'),
(1, 1, 'Quan ao tre em 55', 'Quan ao tre em dep', 'Moi 90%', 640000, 1, 1, '2025-10-22'),
(1, 1, 'Quan ao tre em 56', 'Quan ao tre em dep', 'Moi 90%', 650000, 1, 1, '2025-10-25'),
(1, 1, 'Quan ao tre em 57', 'Quan ao tre em dep', 'Moi 90%', 660000, 1, 1, '2025-10-28'),
(1, 1, 'Quan ao tre em 58', 'Quan ao tre em dep', 'Moi 90%', 670000, 1, 1, '2025-10-30'),
(1, 1, 'Quan ao tre em 59', 'Quan ao tre em dep', 'Moi 90%', 680000, 1, 1, '2025-10-30'),
(1, 1, 'Quan ao tre em 60', 'Quan ao tre em dep', 'Moi 90%', 690000, 1, 1, '2025-10-30');

-- 8. Tạo dữ liệu MilkDonationPosts - Tháng 5 (5 bài)
INSERT INTO MilkDonationPosts (UserID, Title, Content, VerificationTier, Status, CreatedAt)
VALUES
(1, 'Su me du tang 1', 'Su me chat luong cao', 1, 1, '2025-05-10'),
(1, 'Su me du tang 2', 'Su me chat luong cao', 1, 1, '2025-05-15'),
(1, 'Su me du tang 3', 'Su me chat luong cao', 1, 1, '2025-05-20'),
(1, 'Su me du tang 4', 'Su me chat luong cao', 1, 1, '2025-05-25'),
(1, 'Su me du tang 5', 'Su me chat luong cao', 1, 1, '2025-05-30');

-- 9. Tạo dữ liệu MilkDonationPosts - Tháng 6 (6 bài)
INSERT INTO MilkDonationPosts (UserID, Title, Content, VerificationTier, Status, CreatedAt)
VALUES
(1, 'Su me du tang 6', 'Su me chat luong cao', 1, 1, '2025-06-05'),
(1, 'Su me du tang 7', 'Su me chat luong cao', 1, 1, '2025-06-10'),
(1, 'Su me du tang 8', 'Su me chat luong cao', 1, 1, '2025-06-15'),
(1, 'Su me du tang 9', 'Su me chat luong cao', 1, 1, '2025-06-20'),
(1, 'Su me du tang 10', 'Su me chat luong cao', 1, 1, '2025-06-25'),
(1, 'Su me du tang 11', 'Su me chat luong cao', 1, 1, '2025-06-30');

-- 10. Tạo dữ liệu MilkDonationPosts - Tháng 7 (4 bài)
INSERT INTO MilkDonationPosts (UserID, Title, Content, VerificationTier, Status, CreatedAt)
VALUES
(1, 'Su me du tang 12', 'Su me chat luong cao', 1, 1, '2025-07-08'),
(1, 'Su me du tang 13', 'Su me chat luong cao', 1, 1, '2025-07-18'),
(1, 'Su me du tang 14', 'Su me chat luong cao', 1, 1, '2025-07-28'),
(1, 'Su me du tang 15', 'Su me chat luong cao', 1, 1, '2025-07-31');

-- 11. Tạo dữ liệu MilkDonationPosts - Tháng 8 (6 bài)
INSERT INTO MilkDonationPosts (UserID, Title, Content, VerificationTier, Status, CreatedAt)
VALUES
(1, 'Su me du tang 16', 'Su me chat luong cao', 1, 1, '2025-08-05'),
(1, 'Su me du tang 17', 'Su me chat luong cao', 1, 1, '2025-08-10'),
(1, 'Su me du tang 18', 'Su me chat luong cao', 1, 1, '2025-08-15'),
(1, 'Su me du tang 19', 'Su me chat luong cao', 1, 1, '2025-08-20'),
(1, 'Su me du tang 20', 'Su me chat luong cao', 1, 1, '2025-08-25'),
(1, 'Su me du tang 21', 'Su me chat luong cao', 1, 1, '2025-08-30');

-- 12. Tạo dữ liệu MilkDonationPosts - Tháng 9 (5 bài)
INSERT INTO MilkDonationPosts (UserID, Title, Content, VerificationTier, Status, CreatedAt)
VALUES
(1, 'Su me du tang 22', 'Su me chat luong cao', 1, 1, '2025-09-05'),
(1, 'Su me du tang 23', 'Su me chat luong cao', 1, 1, '2025-09-12'),
(1, 'Su me du tang 24', 'Su me chat luong cao', 1, 1, '2025-09-18'),
(1, 'Su me du tang 25', 'Su me chat luong cao', 1, 1, '2025-09-25'),
(1, 'Su me du tang 26', 'Su me chat luong cao', 1, 1, '2025-09-30');

-- 13. Tạo dữ liệu MilkDonationPosts - Tháng 10 (4 bài)
INSERT INTO MilkDonationPosts (UserID, Title, Content, VerificationTier, Status, CreatedAt)
VALUES
(1, 'Su me du tang 27', 'Su me chat luong cao', 1, 1, '2025-10-05'),
(1, 'Su me du tang 28', 'Su me chat luong cao', 1, 1, '2025-10-15'),
(1, 'Su me du tang 29', 'Su me chat luong cao', 1, 1, '2025-10-25'),
(1, 'Su me du tang 30', 'Su me chat luong cao', 1, 1, '2025-10-30');

-- 14. Kiểm tra kết quả
SELECT 
    MONTH(CreatedAt) AS MonthNum,
    COUNT(*) AS PostCount,
    'Total Posts' AS PostType
FROM (
    SELECT CreatedAt FROM Posts_C2C WHERE CreatedAt >= '2025-05-01'
    UNION ALL
    SELECT CreatedAt FROM MilkDonationPosts WHERE CreatedAt >= '2025-05-01'
) AS AllPosts
GROUP BY MONTH(CreatedAt)
ORDER BY MonthNum;


