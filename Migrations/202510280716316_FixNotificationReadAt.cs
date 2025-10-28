namespace B_M.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixNotificationReadAt : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SystemSettings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SettingKey = c.String(nullable: false, maxLength: 100),
                        SettingValue = c.String(maxLength: 1000),
                        SettingType = c.String(nullable: false, maxLength: 50),
                        Description = c.String(maxLength: 500),
                        LastModified = c.DateTime(nullable: false),
                        ModifiedBy = c.String(maxLength: 100),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Notifications", "ReadAt", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Notifications", "ReadAt");
            DropTable("dbo.SystemSettings");
        }
    }
}
