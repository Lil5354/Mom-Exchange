namespace B_M.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPasswordResetCodes : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PasswordResetCodes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserID = c.Int(nullable: false),
                        Code = c.String(nullable: false, maxLength: 6),
                        Token = c.String(maxLength: 200),
                        ExpiresAt = c.DateTime(nullable: false),
                        Attempts = c.Int(nullable: false),
                        UsedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserID, cascadeDelete: true)
                .Index(t => t.UserID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PasswordResetCodes", "UserID", "dbo.Users");
            DropIndex("dbo.PasswordResetCodes", new[] { "UserID" });
            DropTable("dbo.PasswordResetCodes");
        }
    }
}
