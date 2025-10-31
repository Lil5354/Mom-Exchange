namespace B_M.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddImgUrlForMilk : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MilkDonationPosts", "ImageUrl", c => c.String(maxLength: 500));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MilkDonationPosts", "ImageUrl");
        }
    }
}
