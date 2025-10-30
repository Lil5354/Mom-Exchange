-- Script to delete all users except angelletta (UserID = 1)
-- CORRECTED VERSION with proper column names

USE [MomExchange]
GO

PRINT '========================================='
PRINT 'BAT DAU XOA TAT CA TAI KHOAN NGOAI TRU angelletta'
PRINT '========================================='
PRINT ''

-- Display current user count
DECLARE @TotalUsers INT
SELECT @TotalUsers = COUNT(*) FROM Users
PRINT 'Tong so tai khoan hien tai: ' + CAST(@TotalUsers AS VARCHAR)
PRINT ''

-- Check angelletta exists
DECLARE @KeepUserID INT = 1
DECLARE @KeepUserEmail VARCHAR(255)
SELECT @KeepUserEmail = Email FROM Users WHERE UserID = @KeepUserID

IF @KeepUserEmail IS NULL
BEGIN
    PRINT 'LOI: Khong tim thay tai khoan angelletta (UserID = 1)!'
    PRINT 'DUNG CHAY SCRIPT!'
    RETURN
END

PRINT 'Giu lai tai khoan: ' + @KeepUserEmail + ' (UserID = ' + CAST(@KeepUserID AS VARCHAR) + ')'
PRINT ''

-- Start transaction
BEGIN TRANSACTION
BEGIN TRY

    -- =============================================
    -- DELETE RELATED DATA FIRST (Foreign Keys)
    -- =============================================
    
    PRINT 'Dang xoa du lieu lien quan...'
    
    -- 1. Delete Messages (corrected column: ReceiverID)
    IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Messages')
    BEGIN
        DECLARE @DeletedMessages INT = 0
        DELETE FROM Messages 
        WHERE SenderID IN (SELECT UserID FROM Users WHERE UserID <> @KeepUserID)
           OR ReceiverID IN (SELECT UserID FROM Users WHERE UserID <> @KeepUserID)
        SET @DeletedMessages = @@ROWCOUNT
        PRINT '  - Da xoa ' + CAST(@DeletedMessages AS VARCHAR) + ' Messages'
    END
    
    -- 2. Delete MilkDonationRequests (corrected columns: RecipientUserID, DonorUserID)
    IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MilkDonationRequests')
    BEGIN
        DECLARE @DeletedRequests INT = 0
        DELETE FROM MilkDonationRequests 
        WHERE RecipientUserID IN (SELECT UserID FROM Users WHERE UserID <> @KeepUserID)
           OR DonorUserID IN (SELECT UserID FROM Users WHERE UserID <> @KeepUserID)
        SET @DeletedRequests = @@ROWCOUNT
        PRINT '  - Da xoa ' + CAST(@DeletedRequests AS VARCHAR) + ' MilkDonationRequests'
    END
    
    -- 3. Delete MilkDonationPosts
    IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MilkDonationPosts')
    BEGIN
        DECLARE @DeletedPosts INT = 0
        DELETE FROM MilkDonationPosts 
        WHERE UserID IN (SELECT UserID FROM Users WHERE UserID <> @KeepUserID)
        SET @DeletedPosts = @@ROWCOUNT
        PRINT '  - Da xoa ' + CAST(@DeletedPosts AS VARCHAR) + ' MilkDonationPosts'
    END
    
    -- 4. Delete UserMedicalRecords
    IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UserMedicalRecords')
    BEGIN
        DECLARE @DeletedRecords INT = 0
        DELETE FROM UserMedicalRecords 
        WHERE UserID IN (SELECT UserID FROM Users WHERE UserID <> @KeepUserID)
        SET @DeletedRecords = @@ROWCOUNT
        PRINT '  - Da xoa ' + CAST(@DeletedRecords AS VARCHAR) + ' UserMedicalRecords'
    END
    
    -- 5. Delete UserLifestyleSurveys
    IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UserLifestyleSurveys')
    BEGIN
        DECLARE @DeletedSurveys INT = 0
        DELETE FROM UserLifestyleSurveys 
        WHERE UserID IN (SELECT UserID FROM Users WHERE UserID <> @KeepUserID)
        SET @DeletedSurveys = @@ROWCOUNT
        PRINT '  - Da xoa ' + CAST(@DeletedSurveys AS VARCHAR) + ' UserLifestyleSurveys'
    END
    
    -- 6. Delete Posts_C2C and related data (corrected table name)
    IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Posts_C2C')
    BEGIN
        DECLARE @DeletedC2C INT = 0
        
        -- Delete from Post_C2C_Images (corrected table name)
        DELETE FROM Post_C2C_Images 
        WHERE PostID IN (
            SELECT PostID FROM Posts_C2C 
            WHERE UserID IN (SELECT UserID FROM Users WHERE UserID <> @KeepUserID)
        )
        
        DELETE FROM Posts_C2C 
        WHERE UserID IN (SELECT UserID FROM Users WHERE UserID <> @KeepUserID)
        SET @DeletedC2C = @@ROWCOUNT
        PRINT '  - Da xoa ' + CAST(@DeletedC2C AS VARCHAR) + ' Posts_C2C va du lieu lien quan'
    END
    
    -- 7. Delete Ratings (corrected columns: RaterUserID, RatedUserID)
    IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Ratings')
    BEGIN
        DECLARE @DeletedRatings INT = 0
        DELETE FROM Ratings 
        WHERE RaterUserID IN (SELECT UserID FROM Users WHERE UserID <> @KeepUserID)
           OR RatedUserID IN (SELECT UserID FROM Users WHERE UserID <> @KeepUserID)
        SET @DeletedRatings = @@ROWCOUNT
        PRINT '  - Da xoa ' + CAST(@DeletedRatings AS VARCHAR) + ' Ratings'
    END
    
    -- 8. Delete Notifications
    IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Notifications')
    BEGIN
        DECLARE @DeletedNotifications INT = 0
        DELETE FROM Notifications 
        WHERE UserID IN (SELECT UserID FROM Users WHERE UserID <> @KeepUserID)
        SET @DeletedNotifications = @@ROWCOUNT
        PRINT '  - Da xoa ' + CAST(@DeletedNotifications AS VARCHAR) + ' Notifications'
    END
    
    PRINT ''
    PRINT 'Dang xoa UserDetails...'
    
    -- 9. Delete UserDetails
    DECLARE @DeletedDetails INT = 0
    DELETE FROM UserDetails 
    WHERE UserID IN (SELECT UserID FROM Users WHERE UserID <> @KeepUserID)
    SET @DeletedDetails = @@ROWCOUNT
    PRINT '  - Da xoa ' + CAST(@DeletedDetails AS VARCHAR) + ' UserDetails'
    
    PRINT ''
    PRINT 'Dang xoa Users...'
    
    -- 10. Delete Users (all except angelletta)
    DECLARE @DeletedUsers INT = 0
    DELETE FROM Users 
    WHERE UserID <> @KeepUserID
    SET @DeletedUsers = @@ROWCOUNT
    PRINT '  - Da xoa ' + CAST(@DeletedUsers AS VARCHAR) + ' Users'
    
    -- Commit transaction
    COMMIT TRANSACTION
    
    PRINT ''
    PRINT '========================================='
    PRINT 'HOAN THANH! GIAO DICH DA DUOC COMMIT.'
    PRINT '========================================='
    PRINT ''
    
    -- Verify result
    DECLARE @RemainingUsers INT
    SELECT @RemainingUsers = COUNT(*) FROM Users
    PRINT 'SO TAI KHOAN CON LAI: ' + CAST(@RemainingUsers AS VARCHAR)
    PRINT ''
    
    PRINT 'TAI KHOAN CON LAI:'
    SELECT UserID, Email, UserName, Role, IsActive, CreatedAt 
    FROM Users
    ORDER BY CreatedAt DESC
    
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION
    PRINT ''
    PRINT '========================================='
    PRINT 'LOI XAY RA! GIAO DICH DA DUOC ROLLBACK.'
    PRINT '========================================='
    PRINT 'LOI: ' + ERROR_MESSAGE()
    PRINT 'Dong ma: ' + CAST(ERROR_LINE() AS VARCHAR)
    PRINT ''
END CATCH

GO

