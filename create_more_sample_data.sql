-- Script tạo thêm dữ liệu mẫu để biểu đồ đẹp hơn
-- Thêm thêm bài posts và users với CreatedAt phù hợp

-- 1. Xóa tất cả data cũ từ 6 tháng trước
DELETE FROM Posts_C2C WHERE CreatedAt >= DATEADD(MONTH, -6, GETDATE());
DELETE FROM MilkDonationPosts WHERE CreatedAt >= DATEADD(MONTH, -6, GETDATE());

-- 2. Tạo Posts_C2C đẹp hơn (phân bổ đều theo tháng)
-- Tháng 5 (May 2025)
INSERT INTO Posts_C2C (UserID, CategoryID, Title, Content, Condition, Price, ListingType, Status, CreatedAt) VALUES
(1, 1, 'Ao so mi tre em 1', 'Ao so mi dep cho be', 'Moi 100%', 150000, 1, 1, '2025-05-03'),
(1, 1, 'Quan short tre em 1', 'Quan short thoang mat', 'Moi 100%', 100000, 1, 1, '2025-05-08'),
(1, 1, 'Giay dep tre em 1', 'Giay dep chat luong', 'Moi 90%', 120000, 1, 1, '2025-05-12'),
(1, 1, 'Mu len tre em 1', 'Mu len am ap', 'Moi 100%', 80000, 1, 1, '2025-05-15'),
(1, 1, 'Ao khoac tre em 1', 'Ao khoac chong nang', 'Moi 95%', 180000, 1, 1, '2025-05-18'),
(1, 1, 'Quan au tre em 1', 'Quan au mau sac', 'Moi 90%', 130000, 1, 1, '2025-05-22'),
(1, 1, 'Ao thun tre em 1', 'Ao thun de chiu', 'Moi 100%', 90000, 1, 1, '2025-05-25'),
(1, 1, 'Vay lien tre em 1', 'Vay lien xinh xan', 'Moi 95%', 140000, 1, 1, '2025-05-28');

-- Tháng 6 (June 2025)
INSERT INTO Posts_C2C (UserID, CategoryID, Title, Content, Condition, Price, ListingType, Status, CreatedAt) VALUES
(1, 1, 'Ao so mi tre em 2', 'Ao so mi dep cho be', 'Moi 100%', 155000, 1, 1, '2025-06-02'),
(1, 1, 'Quan short tre em 2', 'Quan short thoang mat', 'Moi 100%', 105000, 1, 1, '2025-06-05'),
(1, 1, 'Giay dep tre em 2', 'Giay dep chat luong', 'Moi 90%', 125000, 1, 1, '2025-06-09'),
(1, 1, 'Mu len tre em 2', 'Mu len am ap', 'Moi 100%', 85000, 1, 1, '2025-06-12'),
(1, 1, 'Ao khoac tre em 2', 'Ao khoac chong nang', 'Moi 95%', 185000, 1, 1, '2025-06-15'),
(1, 1, 'Quan au tre em 2', 'Quan au mau sac', 'Moi 90%', 135000, 1, 1, '2025-06-18'),
(1, 1, 'Ao thun tre em 2', 'Ao thun de chiu', 'Moi 100%', 95000, 1, 1, '2025-06-22'),
(1, 1, 'Vay lien tre em 2', 'Vay lien xinh xan', 'Moi 95%', 145000, 1, 1, '2025-06-26'),
(1, 1, 'Ao dam tre em 1', 'Ao dam mat me', 'Moi 100%', 110000, 1, 1, '2025-06-29'),
(1, 1, 'Quan legging tre em 1', 'Quan legging co gian', 'Moi 95%', 115000, 1, 1, '2025-06-30');

-- Tháng 7 (July 2025)
INSERT INTO Posts_C2C (UserID, CategoryID, Title, Content, Condition, Price, ListingType, Status, CreatedAt) VALUES
(1, 1, 'Ao so mi tre em 3', 'Ao so mi dep cho be', 'Moi 100%', 160000, 1, 1, '2025-07-04'),
(1, 1, 'Quan short tre em 3', 'Quan short thoang mat', 'Moi 100%', 110000, 1, 1, '2025-07-08'),
(1, 1, 'Giay dep tre em 3', 'Giay dep chat luong', 'Moi 90%', 130000, 1, 1, '2025-07-12'),
(1, 1, 'Mu len tre em 3', 'Mu len am ap', 'Moi 100%', 90000, 1, 1, '2025-07-15'),
(1, 1, 'Ao khoac tre em 3', 'Ao khoac chong nang', 'Moi 95%', 190000, 1, 1, '2025-07-18'),
(1, 1, 'Quan au tre em 3', 'Quan au mau sac', 'Moi 90%', 140000, 1, 1, '2025-07-22'),
(1, 1, 'Ao thun tre em 3', 'Ao thun de chiu', 'Moi 100%', 100000, 1, 1, '2025-07-28'),
(1, 1, 'Vay lien tre em 3', 'Vay lien xinh xan', 'Moi 95%', 150000, 1, 1, '2025-07-30');

-- Tháng 8 (August 2025)
INSERT INTO Posts_C2C (UserID, CategoryID, Title, Content, Condition, Price, ListingType, Status, CreatedAt) VALUES
(1, 1, 'Ao so mi tre em 4', 'Ao so mi dep cho be', 'Moi 100%', 165000, 1, 1, '2025-08-02'),
(1, 1, 'Quan short tre em 4', 'Quan short thoang mat', 'Moi 100%', 115000, 1, 1, '2025-08-05'),
(1, 1, 'Giay dep tre em 4', 'Giay dep chat luong', 'Moi 90%', 135000, 1, 1, '2025-08-09'),
(1, 1, 'Mu len tre em 4', 'Mu len am ap', 'Moi 100%', 95000, 1, 1, '2025-08-12'),
(1, 1, 'Ao khoac tre em 4', 'Ao khoac chong nang', 'Moi 95%', 195000, 1, 1, '2025-08-16'),
(1, 1, 'Quan au tre em 4', 'Quan au mau sac', 'Moi 90%', 145000, 1, 1, '2025-08-20'),
(1, 1, 'Ao thun tre em 4', 'Ao thun de chiu', 'Moi 100%', 105000, 1, 1, '2025-08-24'),
(1, 1, 'Vay lien tre em 4', 'Vay lien xinh xan', 'Moi 95%', 155000, 1, 1, '2025-08-28'),
(1, 1, 'Ao dam tre em 2', 'Ao dam mat me', 'Moi 100%', 120000, 1, 1, '2025-08-30'),
(1, 1, 'Quan legging tre em 2', 'Quan legging co gian', 'Moi 95%', 125000, 1, 1, '2025-08-30'),
(1, 1, 'Set do choi tre em 1', 'Set do choi gia tre', 'Moi 100%', 200000, 1, 1, '2025-08-31'),
(1, 1, 'Bo do ngu tre em 1', 'Bo do ngu thoai mai', 'Moi 90%', 170000, 1, 1, '2025-08-31');

-- Tháng 9 (September 2025)
INSERT INTO Posts_C2C (UserID, CategoryID, Title, Content, Condition, Price, ListingType, Status, CreatedAt) VALUES
(1, 1, 'Ao so mi tre em 5', 'Ao so mi dep cho be', 'Moi 100%', 170000, 1, 1, '2025-09-04'),
(1, 1, 'Quan short tre em 5', 'Quan short thoang mat', 'Moi 100%', 120000, 1, 1, '2025-09-08'),
(1, 1, 'Giay dep tre em 5', 'Giay dep chat luong', 'Moi 90%', 140000, 1, 1, '2025-09-12'),
(1, 1, 'Mu len tre em 5', 'Mu len am ap', 'Moi 100%', 100000, 1, 1, '2025-09-15'),
(1, 1, 'Ao khoac tre em 5', 'Ao khoac chong nang', 'Moi 95%', 200000, 1, 1, '2025-09-19'),
(1, 1, 'Quan au tre em 5', 'Quan au mau sac', 'Moi 90%', 150000, 1, 1, '2025-09-22'),
(1, 1, 'Ao thun tre em 5', 'Ao thun de chiu', 'Moi 100%', 110000, 1, 1, '2025-09-26'),
(1, 1, 'Vay lien tre em 5', 'Vay lien xinh xan', 'Moi 95%', 160000, 1, 1, '2025-09-28'),
(1, 1, 'Ao dam tre em 3', 'Ao dam mat me', 'Moi 100%', 125000, 1, 1, '2025-09-30'),
(1, 1, 'Quan legging tre em 3', 'Quan legging co gian', 'Moi 95%', 130000, 1, 1, '2025-09-30');

-- Tháng 10 (October 2025)
INSERT INTO Posts_C2C (UserID, CategoryID, Title, Content, Condition, Price, ListingType, Status, CreatedAt) VALUES
(1, 1, 'Ao so mi tre em 6', 'Ao so mi dep cho be', 'Moi 100%', 175000, 1, 1, '2025-10-03'),
(1, 1, 'Quan short tre em 6', 'Quan short thoang mat', 'Moi 100%', 125000, 1, 1, '2025-10-06'),
(1, 1, 'Giay dep tre em 6', 'Giay dep chat luong', 'Moi 90%', 145000, 1, 1, '2025-10-10'),
(1, 1, 'Mu len tre em 6', 'Mu len am ap', 'Moi 100%', 105000, 1, 1, '2025-10-14'),
(1, 1, 'Ao khoac tre em 6', 'Ao khoac chong nang', 'Moi 95%', 205000, 1, 1, '2025-10-18'),
(1, 1, 'Quan au tre em 6', 'Quan au mau sac', 'Moi 90%', 155000, 1, 1, '2025-10-22'),
(1, 1, 'Ao thun tre em 6', 'Ao thun de chiu', 'Moi 100%', 115000, 1, 1, '2025-10-26'),
(1, 1, 'Vay lien tre em 6', 'Vay lien xinh xan', 'Moi 95%', 165000, 1, 1, '2025-10-28'),
(1, 1, 'Ao dam tre em 4', 'Ao dam mat me', 'Moi 100%', 130000, 1, 1, '2025-10-30'),
(1, 1, 'Quan legging tre em 4', 'Quan legging co gian', 'Moi 95%', 135000, 1, 1, '2025-10-30'),
(1, 1, 'Set do choi tre em 2', 'Set do choi gia tre', 'Moi 100%', 210000, 1, 1, '2025-10-30'),
(1, 1, 'Bo do ngu tre em 2', 'Bo do ngu thoai mai', 'Moi 90%', 180000, 1, 1, '2025-10-30');

-- 3. Tạo MilkDonationPosts với CreatedAt đẹp hơn
-- Tháng 5 (May 2025)
INSERT INTO MilkDonationPosts (UserID, Title, Content, VerificationTier, Status, CreatedAt) VALUES
(1, 'Su me tu van - 1', 'Su me tu van chat luong cao, dam bao an toan', 1, 1, '2025-05-10'),
(1, 'Su me tu van - 2', 'Su me tu van chat luong cao, dam bao an toan', 1, 1, '2025-05-15'),
(1, 'Su me tu van - 3', 'Su me tu van chat luong cao, dam bao an toan', 1, 1, '2025-05-20'),
(1, 'Su me tu van - 4', 'Su me tu van chat luong cao, dam bao an toan', 1, 1, '2025-05-25'),
(1, 'Su me tu van - 5', 'Su me tu van chat luong cao, dam bao an toan', 1, 1, '2025-05-30');

-- Tháng 6 (June 2025)
INSERT INTO MilkDonationPosts (UserID, Title, Content, VerificationTier, Status, CreatedAt) VALUES
(1, 'Su me tu van - 6', 'Su me tu van chat luong cao, dam bao an toan', 1, 1, '2025-06-05'),
(1, 'Su me tu van - 7', 'Su me tu van chat luong cao, dam bao an toan', 1, 1, '2025-06-10'),
(1, 'Su me tu van - 8', 'Su me tu van chat luong cao, dam bao an toan', 1, 1, '2025-06-15'),
(1, 'Su me tu van - 9', 'Su me tu van chat luong cao, dam bao an toan', 1, 1, '2025-06-20'),
(1, 'Su me tu van - 10', 'Su me tu van chat luong cao, dam bao an toan', 1, 1, '2025-06-25'),
(1, 'Su me tu van - 11', 'Su me tu van chat luong cao, dam bao an toan', 1, 1, '2025-06-30');

-- Tháng 7 (July 2025)
INSERT INTO MilkDonationPosts (UserID, Title, Content, VerificationTier, Status, CreatedAt) VALUES
(1, 'Su me tu van - 12', 'Su me tu van chat luong cao, dam bao an toan', 1, 1, '2025-07-08'),
(1, 'Su me tu van - 13', 'Su me tu van chat luong cao, dam bao an toan', 1, 1, '2025-07-15'),
(1, 'Su me tu van - 14', 'Su me tu van chat luong cao, dam bao an toan', 1, 1, '2025-07-22'),
(1, 'Su me tu van - 15', 'Su me tu van chat luong cao, dam bao an toan', 1, 1, '2025-07-28');

-- Tháng 8 (August 2025)
INSERT INTO MilkDonationPosts (UserID, Title, Content, VerificationTier, Status, CreatedAt) VALUES
(1, 'Su me tu van - 16', 'Su me tu van chat luong cao, dam bao an toan', 1, 1, '2025-08-05'),
(1, 'Su me tu van - 17', 'Su me tu van chat luong cao, dam bao an toan', 1, 1, '2025-08-10'),
(1, 'Su me tu van - 18', 'Su me tu van chat luong cao, dam bao an toan', 1, 1, '2025-08-15'),
(1, 'Su me tu van - 19', 'Su me tu van chat luong cao, dam bao an toan', 1, 1, '2025-08-22'),
(1, 'Su me tu van - 20', 'Su me tu van chat luong cao, dam bao an toan', 1, 1, '2025-08-28'),
(1, 'Su me tu van - 21', 'Su me tu van chat luong cao, dam bao an toan', 1, 1, '2025-08-30');

-- Tháng 9 (September 2025)
INSERT INTO MilkDonationPosts (UserID, Title, Content, VerificationTier, Status, CreatedAt) VALUES
(1, 'Su me tu van - 22', 'Su me tu van chat luong cao, dam bao an toan', 1, 1, '2025-09-05'),
(1, 'Su me tu van - 23', 'Su me tu van chat luong cao, dam bao an toan', 1, 1, '2025-09-12'),
(1, 'Su me tu van - 24', 'Su me tu van chat luong cao, dam bao an toan', 1, 1, '2025-09-18'),
(1, 'Su me tu van - 25', 'Su me tu van chat luong cao, dam bao an toan', 1, 1, '2025-09-25'),
(1, 'Su me tu van - 26', 'Su me tu van chat luong cao, dam bao an toan', 1, 1, '2025-09-30');

-- Tháng 10 (October 2025)
INSERT INTO MilkDonationPosts (UserID, Title, Content, VerificationTier, Status, CreatedAt) VALUES
(1, 'Su me tu van - 27', 'Su me tu van chat luong cao, dam bao an toan', 1, 1, '2025-10-06'),
(1, 'Su me tu van - 28', 'Su me tu van chat luong cao, dam bao an toan', 1, 1, '2025-10-14'),
(1, 'Su me tu van - 29', 'Su me tu van chat luong cao, dam bao an toan', 1, 1, '2025-10-22'),
(1, 'Su me tu van - 30', 'Su me tu van chat luong cao, dam bao an toan', 1, 1, '2025-10-30');

-- 4. Kiểm tra kết quả
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


