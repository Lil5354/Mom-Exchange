-- ===================================================
-- Create tables for Category Management
-- ===================================================

USE MomExchangeDB;
GO

-- Create Categories table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categories')
BEGIN
    CREATE TABLE [dbo].[Categories](
        [CategoryID] [int] IDENTITY(1,1) NOT NULL,
        [CategoryName] [nvarchar](100) NOT NULL,
        [Description] [nvarchar](500) NULL,
        [ParentCategoryID] [int] NULL,
        [IsB2CEnabled] [bit] NOT NULL DEFAULT 0,
        [IsC2CEnabled] [bit] NOT NULL DEFAULT 0,
        CONSTRAINT [PK_dbo.Categories] PRIMARY KEY CLUSTERED ([CategoryID] ASC)
    );

    -- Add foreign key for self-referencing
    ALTER TABLE [dbo].[Categories]
    ADD CONSTRAINT [FK_dbo.Categories_dbo.Categories_ParentCategoryID] 
    FOREIGN KEY([ParentCategoryID]) REFERENCES [dbo].[Categories] ([CategoryID]);

    -- Add index
    CREATE NONCLUSTERED INDEX [IX_ParentCategoryID] ON [dbo].[Categories]([ParentCategoryID] ASC);

    PRINT '✅ Created table: Categories';
END
ELSE
    PRINT '⚠️  Table Categories already exists';
GO

-- Create Brands table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Brands')
BEGIN
    CREATE TABLE [dbo].[Brands](
        [BrandID] [int] IDENTITY(1,1) NOT NULL,
        [BrandName] [nvarchar](255) NOT NULL,
        [LogoUrl] [nvarchar](1024) NULL,
        [Description] [nvarchar](max) NULL,
        [UserID] [int] NOT NULL,
        CONSTRAINT [PK_dbo.Brands] PRIMARY KEY CLUSTERED ([BrandID] ASC),
        CONSTRAINT [FK_dbo.Brands_dbo.Users_UserID] FOREIGN KEY([UserID]) REFERENCES [dbo].[Users] ([UserID])
    );

    CREATE NONCLUSTERED INDEX [IX_UserID] ON [dbo].[Brands]([UserID] ASC);

    PRINT '✅ Created table: Brands';
END
ELSE
    PRINT '⚠️  Table Brands already exists';
GO

-- Create Brand_Category_Permissions table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Brand_Category_Permissions')
BEGIN
    CREATE TABLE [dbo].[Brand_Category_Permissions](
        [BrandCategoryPermissionID] [bigint] IDENTITY(1,1) NOT NULL,
        [BrandID] [int] NOT NULL,
        [CategoryID] [int] NOT NULL,
        CONSTRAINT [PK_dbo.Brand_Category_Permissions] PRIMARY KEY CLUSTERED ([BrandCategoryPermissionID] ASC),
        CONSTRAINT [FK_dbo.Brand_Category_Permissions_dbo.Brands_BrandID] FOREIGN KEY([BrandID]) REFERENCES [dbo].[Brands] ([BrandID]),
        CONSTRAINT [FK_dbo.Brand_Category_Permissions_dbo.Categories_CategoryID] FOREIGN KEY([CategoryID]) REFERENCES [dbo].[Categories] ([CategoryID])
    );

    CREATE NONCLUSTERED INDEX [IX_BrandID] ON [dbo].[Brand_Category_Permissions]([BrandID] ASC);
    CREATE NONCLUSTERED INDEX [IX_CategoryID] ON [dbo].[Brand_Category_Permissions]([CategoryID] ASC);

    PRINT '✅ Created table: Brand_Category_Permissions';
END
ELSE
    PRINT '⚠️  Table Brand_Category_Permissions already exists';
GO

-- Create Products_B2C table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products_B2C')
BEGIN
    CREATE TABLE [dbo].[Products_B2C](
        [ProductID] [bigint] IDENTITY(1,1) NOT NULL,
        [BrandID] [int] NOT NULL,
        [CategoryID] [int] NOT NULL,
        [ProductName] [nvarchar](255) NOT NULL,
        [Description] [nvarchar](max) NULL,
        [Price] [decimal](18, 2) NOT NULL,
        [StockQuantity] [int] NOT NULL,
        [IsAffiliateEnabled] [bit] NOT NULL DEFAULT 0,
        [AffiliateCommissionRate] [decimal](5, 2) NULL,
        CONSTRAINT [PK_dbo.Products_B2C] PRIMARY KEY CLUSTERED ([ProductID] ASC),
        CONSTRAINT [FK_dbo.Products_B2C_dbo.Brands_BrandID] FOREIGN KEY([BrandID]) REFERENCES [dbo].[Brands] ([BrandID]),
        CONSTRAINT [FK_dbo.Products_B2C_dbo.Categories_CategoryID] FOREIGN KEY([CategoryID]) REFERENCES [dbo].[Categories] ([CategoryID])
    );

    CREATE NONCLUSTERED INDEX [IX_BrandID] ON [dbo].[Products_B2C]([BrandID] ASC);
    CREATE NONCLUSTERED INDEX [IX_CategoryID] ON [dbo].[Products_B2C]([CategoryID] ASC);

    PRINT '✅ Created table: Products_B2C';
END
ELSE
    PRINT '⚠️  Table Products_B2C already exists';
GO

-- Create Posts_C2C table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Posts_C2C')
BEGIN
    CREATE TABLE [dbo].[Posts_C2C](
        [PostID] [bigint] IDENTITY(1,1) NOT NULL,
        [SellerID] [int] NOT NULL,
        [CategoryID] [int] NOT NULL,
        [Title] [nvarchar](255) NOT NULL,
        [Description] [nvarchar](max) NULL,
        [Price] [decimal](18, 2) NOT NULL,
        [Condition] [nvarchar](50) NULL,
        [Status] [nvarchar](20) NOT NULL DEFAULT 'Available',
        [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_dbo.Posts_C2C] PRIMARY KEY CLUSTERED ([PostID] ASC),
        CONSTRAINT [FK_dbo.Posts_C2C_dbo.Users_SellerID] FOREIGN KEY([SellerID]) REFERENCES [dbo].[Users] ([UserID]),
        CONSTRAINT [FK_dbo.Posts_C2C_dbo.Categories_CategoryID] FOREIGN KEY([CategoryID]) REFERENCES [dbo].[Categories] ([CategoryID])
    );

    CREATE NONCLUSTERED INDEX [IX_SellerID] ON [dbo].[Posts_C2C]([SellerID] ASC);
    CREATE NONCLUSTERED INDEX [IX_CategoryID] ON [dbo].[Posts_C2C]([CategoryID] ASC);

    PRINT '✅ Created table: Posts_C2C';
END
ELSE
    PRINT '⚠️  Table Posts_C2C already exists';
GO

PRINT '';
PRINT '=================================================';
PRINT '✅ Database schema created successfully!';
PRINT '=================================================';
GO





