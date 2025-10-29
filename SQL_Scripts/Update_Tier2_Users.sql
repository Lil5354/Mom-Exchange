-- ===================================================
-- Update users with >= 2 approved medical records to Tier 2
-- AND update their posts to Tier 2
-- ===================================================

USE MomExchangeDB;
GO

PRINT N'[1/3] Đang cập nhật Users có >= 2 hồ sơ duyệt lên Tầng 2...';

-- Update users who have >= 2 approved medical records to Tier 2
UPDATE Users
SET MilkDonationStatus = 3  -- Tier 2 - HealthVerified
WHERE UserID IN (
    SELECT UserID
    FROM UserMedicalRecords
    WHERE VerificationStatus = 1  -- Approved
    GROUP BY UserID
    HAVING COUNT(*) >= 2
)
AND MilkDonationStatus != 3  -- Chưa phải Tầng 2
AND MilkDonationStatus != 4;  -- Không bị từ chối

GO

PRINT N'[2/3] Đang cập nhật Users có 1 hồ sơ duyệt sang trạng thái chờ duyệt...';

-- Update users who have exactly 1 approved medical record to PendingVerification
UPDATE Users
SET MilkDonationStatus = 2  -- PendingVerification
WHERE UserID IN (
    SELECT UserID
    FROM UserMedicalRecords
    WHERE VerificationStatus = 1  -- Approved
    GROUP BY UserID
    HAVING COUNT(*) = 1
)
AND MilkDonationStatus = 1;  -- Chỉ update từ BasicDeclared

GO

PRINT N'[3/3] Đang cập nhật VerificationTier cho các posts...';

-- Update VerificationTier cho TẤT CẢ posts của users đã lên Tầng 2
UPDATE MilkDonationPosts
SET VerificationTier = 3  -- Tầng 2 - HealthVerified
WHERE UserID IN (
    SELECT UserID FROM Users WHERE MilkDonationStatus = 3
)
AND VerificationTier = 1;  -- Chỉ update các post ở Tầng 1 (posts cũ)

GO

-- Show updated users and their posts
PRINT N'';
PRINT N'📊 THỐNG KÊ:';
PRINT N'----------------------------------------';

SELECT 
    u.UserID,
    u.Email,
    ud.FullName,
    u.MilkDonationStatus,
    COUNT(DISTINCT umr.RecordID) as ApprovedRecordsCount,
    COUNT(DISTINCT p.PostID) as TotalPosts,
    COUNT(DISTINCT CASE WHEN p.VerificationTier = 3 THEN p.PostID END) as Tier2Posts
FROM Users u
LEFT JOIN UserDetails ud ON u.UserID = ud.UserID
LEFT JOIN UserMedicalRecords umr ON u.UserID = umr.UserID AND umr.VerificationStatus = 1
LEFT JOIN MilkDonationPosts p ON u.UserID = p.UserID
WHERE u.MilkDonationStatus IN (2, 3)  -- Cả PendingVerification và HealthVerified
GROUP BY u.UserID, u.Email, ud.FullName, u.MilkDonationStatus;

GO

PRINT N'';
PRINT N'✅ Đã cập nhật users và posts theo luồng mới thành công!';

