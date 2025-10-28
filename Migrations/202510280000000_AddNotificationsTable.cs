namespace B_M.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNotificationsTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RecipientUserId = c.Int(nullable: false),
                        Title = c.String(nullable: false, maxLength: 255),
                        Message = c.String(nullable: false, maxLength: 1000),
                        IsRead = c.Boolean(nullable: false),
                        Link = c.String(maxLength: 500),
                        CreatedAt = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.RecipientUserId, cascadeDelete: false)
                .Index(t => t.RecipientUserId)
                .Index(t => t.IsRead)
                .Index(t => t.CreatedAt);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Notifications", "RecipientUserId", "dbo.Users");
            DropIndex("dbo.Notifications", new[] { "CreatedAt" });
            DropIndex("dbo.Notifications", new[] { "IsRead" });
            DropIndex("dbo.Notifications", new[] { "RecipientUserId" });
            DropTable("dbo.Notifications");
        }
    }
}


