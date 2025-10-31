namespace B_M.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPayment : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OrderItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrderId = c.Int(nullable: false),
                        ProductId = c.Int(),
                        ProductName = c.String(maxLength: 200),
                        ProductPrice = c.String(maxLength: 100),
                        ProductImageUrl = c.String(maxLength: 500),
                        Quantity = c.Int(nullable: false),
                        UnitPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Orders", t => t.OrderId, cascadeDelete: true)
                .ForeignKey("dbo.Products", t => t.ProductId)
                .Index(t => t.OrderId)
                .Index(t => t.ProductId);
            
            CreateTable(
                "dbo.Orders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrderCode = c.String(nullable: false, maxLength: 20),
                        CustomerId = c.Int(nullable: false),
                        BrandId = c.Int(nullable: false),
                        Status = c.Byte(nullable: false),
                        ShippingName = c.String(nullable: false, maxLength: 200),
                        ShippingPhone = c.String(nullable: false, maxLength: 15),
                        ShippingAddress = c.String(nullable: false, maxLength: 500),
                        SubTotal = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Commission = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreatedAt = c.DateTime(nullable: false),
                        PaidAt = c.DateTime(),
                        ConfirmedAt = c.DateTime(),
                        ShippedAt = c.DateTime(),
                        DeliveredAt = c.DateTime(),
                        Note = c.String(maxLength: 500),
                        PaymentMethod = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Brands", t => t.BrandId)
                .ForeignKey("dbo.Users", t => t.CustomerId)
                .Index(t => t.CustomerId)
                .Index(t => t.BrandId);
            
            CreateTable(
                "dbo.PayOSPaymentLinks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PayOSLinkId = c.String(maxLength: 50),
                        OrderCode = c.String(nullable: false, maxLength: 20),
                        CheckoutUrl = c.String(maxLength: 500),
                        QrCode = c.String(maxLength: 2000),
                        Status = c.Byte(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreatedAt = c.DateTime(nullable: false),
                        PaidAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.OrderItems", "ProductId", "dbo.Products");
            DropForeignKey("dbo.OrderItems", "OrderId", "dbo.Orders");
            DropForeignKey("dbo.Orders", "CustomerId", "dbo.Users");
            DropForeignKey("dbo.Orders", "BrandId", "dbo.Brands");
            DropIndex("dbo.Orders", new[] { "BrandId" });
            DropIndex("dbo.Orders", new[] { "CustomerId" });
            DropIndex("dbo.OrderItems", new[] { "ProductId" });
            DropIndex("dbo.OrderItems", new[] { "OrderId" });
            DropTable("dbo.PayOSPaymentLinks");
            DropTable("dbo.Orders");
            DropTable("dbo.OrderItems");
        }
    }
}
