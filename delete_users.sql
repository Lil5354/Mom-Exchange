-- Script to delete all users except angelletta
-- Run this script in SQL Server Management Studio

-- First, let's check if angelletta exists
SELECT UserID, Email, UserName, Role 
FROM Users 
WHERE Email LIKE '%angelletta%' OR UserName LIKE '%angelletta%';

-- If angelletta exists, get their UserID (assuming it's in a variable or replace with actual ID)
-- Replace @KeepUserID with the actual UserID of angelletta after running the SELECT above

-- =============================================
-- DELETE ALL RELATED DATA FIRST (Foreign Keys)
-- =============================================

-- Delete from Messages where user is involved (if exists)
DELETE FROM Messages 
WHERE SenderID NOT IN (SELECT UserID FROM Users WHERE Email LIKE '%angelletta%' OR UserName LIKE '%angelletta%')
   OR RecipientID NOT IN (SELECT UserID FROM Users WHERE Email LIKE '%angelletta%' OR UserName LIKE '%angelletta%');

-- Delete from MilkDonationRequests (if exists)
DELETE FROM MilkDonationRequests 
WHERE UserID NOT IN (SELECT UserID FROM Users WHERE Email LIKE '%angelletta%' OR UserName LIKE '%angelletta%');

-- Delete from MilkDonationPosts (if exists)
DELETE FROM MilkDonationPosts 
WHERE UserID NOT IN (SELECT UserID FROM Users WHERE Email LIKE '%angelletta%' OR UserName LIKE '%angelletta%');

-- Delete from UserMedicalRecords (if exists)
DELETE FROM UserMedicalRecords 
WHERE UserID NOT IN (SELECT UserID FROM Users WHERE Email LIKE '%angelletta%' OR UserName LIKE '%angelletta%');

-- Delete from UserLifestyleSurveys (if exists)
DELETE FROM UserLifestyleSurveys 
WHERE UserID NOT IN (SELECT UserID FROM Users WHERE Email LIKE '%angelletta%' OR UserName LIKE '%angelletta%');

-- Delete from PostC2Cs (if exists)
DELETE FROM PostC2Cs 
WHERE UserID NOT IN (SELECT UserID FROM Users WHERE Email LIKE '%angelletta%' OR UserName LIKE '%angelletta%');

-- Delete from PostC2CImages (if exists)
DELETE FROM PostC2CImages 
WHERE PostID NOT IN (SELECT PostID FROM PostC2Cs);

-- Delete from PostC2CExchangePreferences (if exists)
DELETE FROM PostC2CExchangePreferences 
WHERE PostID NOT IN (SELECT PostID FROM PostC2Cs);

-- Delete from Ratings (if exists)
DELETE FROM Ratings 
WHERE RaterID NOT IN (SELECT UserID FROM Users WHERE Email LIKE '%angelletta%' OR UserName LIKE '%angelletta%')
   OR RatedUserID NOT IN (SELECT UserID FROM Users WHERE Email LIKE '%angelletta%' OR UserName LIKE '%angelletta%');

-- Delete from Notifications (if exists)
DELETE FROM Notifications 
WHERE UserID NOT IN (SELECT UserID FROM Users WHERE Email LIKE '%angelletta%' OR UserName LIKE '%angelletta%');

-- =============================================
-- DELETE USER DETAILS
-- =============================================
DELETE FROM UserDetails 
WHERE UserID NOT IN (SELECT UserID FROM Users WHERE Email LIKE '%angelletta%' OR UserName LIKE '%angelletta%');

-- =============================================
-- DELETE USERS (all except angelletta)
-- =============================================
DELETE FROM Users 
WHERE Email NOT LIKE '%angelletta%' AND UserName NOT LIKE '%angelletta%';

-- =============================================
-- VERIFY RESULT
-- =============================================
SELECT COUNT(*) AS RemainingUsers FROM Users;
SELECT UserID, Email, UserName, Role, IsActive, CreatedAt FROM Users;

-- Expected result: Only angelletta's account should remain


