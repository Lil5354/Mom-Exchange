namespace B_M.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Products", "BrandId", "dbo.Brands");
            AddForeignKey("dbo.Products", "BrandId", "dbo.Brands", "BrandID", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Products", "BrandId", "dbo.Brands");
            AddForeignKey("dbo.Products", "BrandId", "dbo.Brands", "BrandID");
        }
    }
}
