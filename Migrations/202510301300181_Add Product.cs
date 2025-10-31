namespace B_M.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddProduct : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Products",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Category = c.String(),
                        Price = c.String(),
                        StockQuantity = c.Int(nullable: false),
                        ShortDescription = c.String(),
                        DetailedDescription = c.String(),
                        Condition = c.String(),
                        BrandId = c.Int(nullable: false),
                        Location = c.String(),
                        SellerName = c.String(),
                        SellerAvatarUrl = c.String(),
                        SellerRating = c.Double(nullable: false),
                        SellerReviewCount = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Brands", t => t.BrandId, cascadeDelete: true)
                .Index(t => t.BrandId);
            
            CreateTable(
                "dbo.Product_Images",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProductId = c.Int(nullable: false),
                        ImageUrl = c.String(nullable: false, maxLength: 500),
                        SortOrder = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Products", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.ProductId);
            
            AlterColumn("dbo.Brands", "Description", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Products", "BrandId", "dbo.Brands");
            DropForeignKey("dbo.Product_Images", "ProductId", "dbo.Products");
            DropIndex("dbo.Product_Images", new[] { "ProductId" });
            DropIndex("dbo.Products", new[] { "BrandId" });
            AlterColumn("dbo.Brands", "Description", c => c.String());
            DropTable("dbo.Product_Images");
            DropTable("dbo.Products");
        }
    }
}
