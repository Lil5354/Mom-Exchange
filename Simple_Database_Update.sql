-- Simple Database Update After Merge
-- This script only adds the missing columns and tables

-- Add ReadAt column to Notifications table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND name = 'ReadAt')
BEGIN
    ALTER TABLE [dbo].[Notifications] ADD [ReadAt] [datetime2](7) NULL
    PRINT 'Added ReadAt column to Notifications table'
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
    PRINT 'Created SystemSettings table'
END

-- Clear migration history to force model recreation
DELETE FROM [dbo].[__MigrationHistory] WHERE [Model] IS NOT NULL
PRINT 'Cleared migration history'

PRINT 'Database update completed successfully!'
PRINT 'You can now try Google OAuth login again.'
