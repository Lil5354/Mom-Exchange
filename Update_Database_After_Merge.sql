-- Update Database After Merge
-- This script updates the database to match the new ApplicationDbContext

-- Add ReadAt column to Notifications table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND name = 'ReadAt')
BEGIN
    ALTER TABLE [dbo].[Notifications] ADD [ReadAt] [datetime2](7) NULL
END

-- Create SystemSettings table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SystemSettings]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SystemSettings] (
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SettingKey] [nvarchar](100) NOT NULL,
        [SettingValue] [nvarchar](1000) NULL,
        [SettingType] [nvarchar](50) NOT NULL,
        [Description] [nvarchar](500) NULL,
        [LastModified] [datetime2](7) NOT NULL,
        [ModifiedBy] [nvarchar](100) NULL,
        [IsActive] [bit] NOT NULL,
        CONSTRAINT [PK_dbo.SystemSettings] PRIMARY KEY ([Id])
    )
END

-- Drop foreign key constraints first, then drop tables that were removed in the merge

-- Drop foreign keys for Orders table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_dbo.AffiliateSales_dbo.Orders_OrderID')
BEGIN
    ALTER TABLE [dbo].[AffiliateSales] DROP CONSTRAINT [FK_dbo.AffiliateSales_dbo.Orders_OrderID]
END

-- Drop foreign keys for OrderDetails table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_dbo.OrderDetails_dbo.Orders_OrderID')
BEGIN
    ALTER TABLE [dbo].[OrderDetails] DROP CONSTRAINT [FK_dbo.OrderDetails_dbo.Orders_OrderID]
END

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_dbo.OrderDetails_dbo.Products_B2C_ProductID')
BEGIN
    ALTER TABLE [dbo].[OrderDetails] DROP CONSTRAINT [FK_dbo.OrderDetails_dbo.Products_B2C_ProductID]
END

-- Drop foreign keys for Products_B2C table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_dbo.AffiliateClicks_dbo.Products_B2C_ProductID')
BEGIN
    ALTER TABLE [dbo].[AffiliateClicks] DROP CONSTRAINT [FK_dbo.AffiliateClicks_dbo.Products_B2C_ProductID]
END

-- Drop foreign keys for AffiliateSales table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_dbo.AffiliateSales_dbo.Users_BuyerUserID')
BEGIN
    ALTER TABLE [dbo].[AffiliateSales] DROP CONSTRAINT [FK_dbo.AffiliateSales_dbo.Users_BuyerUserID]
END

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_dbo.AffiliateSales_dbo.Users_AffiliatorUserID')
BEGIN
    ALTER TABLE [dbo].[AffiliateSales] DROP CONSTRAINT [FK_dbo.AffiliateSales_dbo.Users_AffiliatorUserID]
END

-- Drop foreign keys for AffiliateClicks table
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_dbo.AffiliateClicks_dbo.Users_AffiliatorUserID')
BEGIN
    ALTER TABLE [dbo].[AffiliateClicks] DROP CONSTRAINT [FK_dbo.AffiliateClicks_dbo.Users_AffiliatorUserID]
END

-- Now drop the tables
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AffiliateSales]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[AffiliateSales]
END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AffiliateClicks]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[AffiliateClicks]
END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrderDetails]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[OrderDetails]
END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[Orders]
END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProductB2CImages]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[ProductB2CImages]
END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Products_B2C]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[Products_B2C]
END

-- Update __MigrationHistory to reflect current state
-- This is a simplified approach - in production you'd want to be more careful
DELETE FROM [dbo].[__MigrationHistory] WHERE [Model] IS NOT NULL

PRINT 'Database updated successfully after merge!'
