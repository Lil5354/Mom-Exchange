namespace B_M.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DBModule3 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.MilkProductDetails", "ProductID", "dbo.Products");
            DropForeignKey("dbo.Products", "MilkProductDetails_ProductID", "dbo.MilkProductDetails");
            DropForeignKey("dbo.BarterOffers", "OfferedProductID", "dbo.Products");
            DropForeignKey("dbo.BarterOffers", "RecipientID", "dbo.Users");
            DropForeignKey("dbo.BarterOffers", "RequestedProductID", "dbo.Products");
            DropForeignKey("dbo.BarterOffers", "RequesterID", "dbo.Users");
            DropForeignKey("dbo.Ratings", "ProductID", "dbo.Products");
            DropForeignKey("dbo.AffiliateClicks", "ProductID", "dbo.Products");
            DropForeignKey("dbo.OrderDetails", "ProductID", "dbo.Products");
            DropForeignKey("dbo.ProductImages", "ProductID", "dbo.Products");
            DropIndex("dbo.AffiliateClicks", new[] { "ProductID" });
            DropIndex("dbo.OrderDetails", new[] { "ProductID" });
            DropIndex("dbo.ProductImages", new[] { "ProductID" });
            DropIndex("dbo.Products", new[] { "MilkProductDetails_ProductID" });
            DropIndex("dbo.MilkProductDetails", new[] { "ProductID" });
            DropIndex("dbo.BarterOffers", new[] { "RequesterID" });
            DropIndex("dbo.BarterOffers", new[] { "RecipientID" });
            DropIndex("dbo.BarterOffers", new[] { "RequestedProductID" });
            DropIndex("dbo.BarterOffers", new[] { "OfferedProductID" });
            DropIndex("dbo.Ratings", new[] { "ProductID" });
            CreateTable(
                "dbo.Products_B2C",
                c => new
                    {
                        ProductID = c.Long(nullable: false, identity: true),
                        BrandID = c.Int(nullable: false),
                        CategoryID = c.Int(nullable: false),
                        ProductName = c.String(nullable: false, maxLength: 255),
                        Description = c.String(),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        StockQuantity = c.Int(nullable: false),
                        IsAffiliateEnabled = c.Boolean(nullable: false),
                        AffiliateCommissionRate = c.Decimal(precision: 5, scale: 2),
                    })
                .PrimaryKey(t => t.ProductID)
                .ForeignKey("dbo.Brands", t => t.BrandID)
                .ForeignKey("dbo.Categories", t => t.CategoryID)
                .Index(t => t.BrandID)
                .Index(t => t.CategoryID);
            
            CreateTable(
                "dbo.Brands",
                c => new
                    {
                        BrandID = c.Int(nullable: false, identity: true),
                        BrandName = c.String(nullable: false, maxLength: 255),
                        LogoUrl = c.String(maxLength: 1024),
                        Description = c.String(),
                        UserID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.BrandID)
                .ForeignKey("dbo.Users", t => t.UserID)
                .Index(t => t.UserID);
            
            CreateTable(
                "dbo.Brand_Category_Permissions",
                c => new
                    {
                        BrandCategoryPermissionID = c.Long(nullable: false, identity: true),
                        BrandID = c.Int(nullable: false),
                        CategoryID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.BrandCategoryPermissionID)
                .ForeignKey("dbo.Brands", t => t.BrandID)
                .ForeignKey("dbo.Categories", t => t.CategoryID)
                .Index(t => t.BrandID)
                .Index(t => t.CategoryID);
            
            CreateTable(
                "dbo.Posts_C2C",
                c => new
                    {
                        PostID = c.Long(nullable: false, identity: true),
                        UserID = c.Int(nullable: false),
                        CategoryID = c.Int(nullable: false),
                        Title = c.String(nullable: false, maxLength: 255),
                        Content = c.String(nullable: false),
                        Condition = c.String(maxLength: 100),
                        Price = c.Decimal(precision: 18, scale: 2),
                        ListingType = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.PostID)
                .ForeignKey("dbo.Categories", t => t.CategoryID)
                .ForeignKey("dbo.Users", t => t.UserID)
                .Index(t => t.UserID)
                .Index(t => t.CategoryID);
            
            CreateTable(
                "dbo.Post_C2C_Images",
                c => new
                    {
                        ImageID = c.Long(nullable: false, identity: true),
                        PostID = c.Long(nullable: false),
                        ImageUrl = c.String(nullable: false, maxLength: 1024),
                        IsPrimary = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ImageID)
                .ForeignKey("dbo.Posts_C2C", t => t.PostID, cascadeDelete: true)
                .Index(t => t.PostID);
            
            CreateTable(
                "dbo.Product_B2C_Images",
                c => new
                    {
                        ImageID = c.Long(nullable: false, identity: true),
                        ProductID = c.Long(nullable: false),
                        ImageUrl = c.String(nullable: false, maxLength: 1024),
                        IsPrimary = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ImageID)
                .ForeignKey("dbo.Products_B2C", t => t.ProductID, cascadeDelete: true)
                .Index(t => t.ProductID);
            
            AddColumn("dbo.Categories", "ParentCategoryID", c => c.Int());
            AddColumn("dbo.Categories", "IsB2CEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.Categories", "IsC2CEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.Orders", "BrandID", c => c.Int());
            AddColumn("dbo.OrderDetails", "ProductB2C_ProductID", c => c.Long());
            AddColumn("dbo.Ratings", "PostID", c => c.Long());
            AlterColumn("dbo.Products", "ProductID", c => c.Long(nullable: false, identity: true));
            AlterColumn("dbo.AffiliateClicks", "ProductID", c => c.Long(nullable: false));
            AlterColumn("dbo.OrderDetails", "ProductID", c => c.Long(nullable: false));
            AlterColumn("dbo.ProductImages", "ProductID", c => c.Long(nullable: false));
            CreateIndex("dbo.AffiliateClicks", "ProductID");
            CreateIndex("dbo.OrderDetails", "ProductID");
            CreateIndex("dbo.ProductImages", "ProductID");
            AddForeignKey("dbo.AffiliateClicks", "ProductID", "dbo.Products", "ProductID");
            AddForeignKey("dbo.OrderDetails", "ProductID", "dbo.Products", "ProductID");
            AddForeignKey("dbo.ProductImages", "ProductID", "dbo.Products", "ProductID", cascadeDelete: true);
            CreateIndex("dbo.Categories", "ParentCategoryID");
            CreateIndex("dbo.Ratings", "PostID");
            CreateIndex("dbo.OrderDetails", "ProductB2C_ProductID");
            CreateIndex("dbo.Orders", "BrandID");
            AddForeignKey("dbo.Categories", "ParentCategoryID", "dbo.Categories", "CategoryID");
            AddForeignKey("dbo.Ratings", "PostID", "dbo.Posts_C2C", "PostID");
            AddForeignKey("dbo.Orders", "BrandID", "dbo.Brands", "BrandID");
            AddForeignKey("dbo.OrderDetails", "ProductB2C_ProductID", "dbo.Products_B2C", "ProductID");
            DropColumn("dbo.Products", "MilkProductDetails_ProductID");
            DropColumn("dbo.Categories", "IsForMilkDonation");
            DropColumn("dbo.Ratings", "ProductID");
            DropTable("dbo.MilkProductDetails");
            DropTable("dbo.BarterOffers");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.BarterOffers",
                c => new
                    {
                        OfferID = c.Int(nullable: false, identity: true),
                        RequesterID = c.Int(nullable: false),
                        RecipientID = c.Int(nullable: false),
                        RequestedProductID = c.Int(nullable: false),
                        OfferedProductID = c.Int(nullable: false),
                        Status = c.String(maxLength: 20),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.OfferID);
            
            CreateTable(
                "dbo.MilkProductDetails",
                c => new
                    {
                        ProductID = c.Int(nullable: false),
                        CollectionDate = c.DateTime(nullable: false),
                        MotherDietInfo = c.String(),
                        StorageMethod = c.String(maxLength: 500),
                    })
                .PrimaryKey(t => t.ProductID);
            
            AddColumn("dbo.Ratings", "ProductID", c => c.Int());
            AddColumn("dbo.Categories", "IsForMilkDonation", c => c.Boolean(nullable: false));
            AddColumn("dbo.Products", "MilkProductDetails_ProductID", c => c.Int());
            DropForeignKey("dbo.OrderDetails", "ProductB2C_ProductID", "dbo.Products_B2C");
            DropForeignKey("dbo.Orders", "BrandID", "dbo.Brands");
            DropForeignKey("dbo.Product_B2C_Images", "ProductID", "dbo.Products_B2C");
            DropForeignKey("dbo.Products_B2C", "CategoryID", "dbo.Categories");
            DropForeignKey("dbo.Products_B2C", "BrandID", "dbo.Brands");
            DropForeignKey("dbo.Brands", "UserID", "dbo.Users");
            DropForeignKey("dbo.Brand_Category_Permissions", "CategoryID", "dbo.Categories");
            DropForeignKey("dbo.Posts_C2C", "UserID", "dbo.Users");
            DropForeignKey("dbo.Ratings", "PostID", "dbo.Posts_C2C");
            DropForeignKey("dbo.Post_C2C_Images", "PostID", "dbo.Posts_C2C");
            DropForeignKey("dbo.Posts_C2C", "CategoryID", "dbo.Categories");
            DropForeignKey("dbo.Categories", "ParentCategoryID", "dbo.Categories");
            DropForeignKey("dbo.Brand_Category_Permissions", "BrandID", "dbo.Brands");
            DropIndex("dbo.Orders", new[] { "BrandID" });
            DropIndex("dbo.OrderDetails", new[] { "ProductB2C_ProductID" });
            DropIndex("dbo.Product_B2C_Images", new[] { "ProductID" });
            DropIndex("dbo.Ratings", new[] { "PostID" });
            DropIndex("dbo.Post_C2C_Images", new[] { "PostID" });
            DropIndex("dbo.Posts_C2C", new[] { "CategoryID" });
            DropIndex("dbo.Posts_C2C", new[] { "UserID" });
            DropIndex("dbo.Categories", new[] { "ParentCategoryID" });
            DropIndex("dbo.Brand_Category_Permissions", new[] { "CategoryID" });
            DropIndex("dbo.Brand_Category_Permissions", new[] { "BrandID" });
            DropIndex("dbo.Brands", new[] { "UserID" });
            DropIndex("dbo.Products_B2C", new[] { "CategoryID" });
            DropIndex("dbo.Products_B2C", new[] { "BrandID" });
            DropIndex("dbo.AffiliateClicks", new[] { "ProductID" });
            DropIndex("dbo.OrderDetails", new[] { "ProductID" });
            DropIndex("dbo.ProductImages", new[] { "ProductID" });
            DropForeignKey("dbo.AffiliateClicks", "ProductID", "dbo.Products");
            DropForeignKey("dbo.OrderDetails", "ProductID", "dbo.Products");
            DropForeignKey("dbo.ProductImages", "ProductID", "dbo.Products");
            AlterColumn("dbo.Products", "ProductID", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.AffiliateClicks", "ProductID", c => c.Int(nullable: false));
            AlterColumn("dbo.OrderDetails", "ProductID", c => c.Int(nullable: false));
            AlterColumn("dbo.ProductImages", "ProductID", c => c.Int(nullable: false));
            DropColumn("dbo.Ratings", "PostID");
            DropColumn("dbo.OrderDetails", "ProductB2C_ProductID");
            DropColumn("dbo.Orders", "BrandID");
            DropColumn("dbo.Categories", "IsC2CEnabled");
            DropColumn("dbo.Categories", "IsB2CEnabled");
            DropColumn("dbo.Categories", "ParentCategoryID");
            DropTable("dbo.Product_B2C_Images");
            DropTable("dbo.Post_C2C_Images");
            DropTable("dbo.Posts_C2C");
            DropTable("dbo.Brand_Category_Permissions");
            DropTable("dbo.Brands");
            DropTable("dbo.Products_B2C");
            CreateIndex("dbo.Ratings", "ProductID");
            CreateIndex("dbo.BarterOffers", "OfferedProductID");
            CreateIndex("dbo.BarterOffers", "RequestedProductID");
            CreateIndex("dbo.BarterOffers", "RecipientID");
            CreateIndex("dbo.BarterOffers", "RequesterID");
            CreateIndex("dbo.MilkProductDetails", "ProductID");
            CreateIndex("dbo.Products", "MilkProductDetails_ProductID");
            CreateIndex("dbo.AffiliateClicks", "ProductID");
            CreateIndex("dbo.OrderDetails", "ProductID");
            CreateIndex("dbo.ProductImages", "ProductID");
            AddForeignKey("dbo.AffiliateClicks", "ProductID", "dbo.Products", "ProductID");
            AddForeignKey("dbo.OrderDetails", "ProductID", "dbo.Products", "ProductID");
            AddForeignKey("dbo.ProductImages", "ProductID", "dbo.Products", "ProductID", cascadeDelete: true);
            AddForeignKey("dbo.Ratings", "ProductID", "dbo.Products", "ProductID");
            AddForeignKey("dbo.BarterOffers", "RequesterID", "dbo.Users", "UserID");
            AddForeignKey("dbo.BarterOffers", "RequestedProductID", "dbo.Products", "ProductID");
            AddForeignKey("dbo.BarterOffers", "RecipientID", "dbo.Users", "UserID");
            AddForeignKey("dbo.BarterOffers", "OfferedProductID", "dbo.Products", "ProductID");
            AddForeignKey("dbo.Products", "MilkProductDetails_ProductID", "dbo.MilkProductDetails", "ProductID");
            AddForeignKey("dbo.MilkProductDetails", "ProductID", "dbo.Products", "ProductID", cascadeDelete: true);
        }
    }
}
