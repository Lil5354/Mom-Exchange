namespace B_M.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Brands",
                c => new
                    {
                        BrandID = c.Int(nullable: false, identity: true),
                        BrandName = c.String(nullable: false, maxLength: 255),
                        LogoUrl = c.String(maxLength: 1024),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.BrandID);
            
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        CategoryID = c.Int(nullable: false, identity: true),
                        CategoryName = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 500),
                        ParentCategoryID = c.Int(),
                        IsB2CEnabled = c.Boolean(nullable: false),
                        IsC2CEnabled = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.CategoryID)
                .ForeignKey("dbo.Categories", t => t.ParentCategoryID)
                .Index(t => t.ParentCategoryID);
            
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
                "dbo.Post_C2C_ExchangePreferences",
                c => new
                    {
                        ExchangePreferenceID = c.Long(nullable: false, identity: true),
                        PostID = c.Long(nullable: false),
                        CategoryID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ExchangePreferenceID)
                .ForeignKey("dbo.Categories", t => t.CategoryID)
                .ForeignKey("dbo.Posts_C2C", t => t.PostID, cascadeDelete: true)
                .Index(t => t.PostID)
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
                "dbo.Ratings",
                c => new
                    {
                        RatingID = c.Long(nullable: false, identity: true),
                        RaterUserID = c.Int(nullable: false),
                        RatedUserID = c.Int(nullable: false),
                        PostID = c.Long(nullable: false),
                        Score = c.Byte(nullable: false),
                        Comment = c.String(maxLength: 1000),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.RatingID)
                .ForeignKey("dbo.Posts_C2C", t => t.PostID)
                .ForeignKey("dbo.Users", t => t.RatedUserID)
                .ForeignKey("dbo.Users", t => t.RaterUserID)
                .Index(t => t.RaterUserID)
                .Index(t => t.RatedUserID)
                .Index(t => t.PostID);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UserID = c.Int(nullable: false, identity: true),
                        UserName = c.String(maxLength: 50),
                        Email = c.String(maxLength: 255),
                        PhoneNumber = c.String(maxLength: 20),
                        PasswordHash = c.String(nullable: false),
                        Role = c.Byte(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        MilkDonationStatus = c.Int(nullable: false),
                        GoogleId = c.String(maxLength: 255),
                    })
                .PrimaryKey(t => t.UserID);
            
            CreateTable(
                "dbo.UserDetails",
                c => new
                    {
                        UserID = c.Int(nullable: false),
                        FullName = c.String(nullable: false, maxLength: 100),
                        ProfilePictureURL = c.String(maxLength: 500),
                        Address = c.String(maxLength: 500),
                        ReputationScore = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.UserID)
                .ForeignKey("dbo.Users", t => t.UserID, cascadeDelete: true)
                .Index(t => t.UserID);
            
            CreateTable(
                "dbo.Messages",
                c => new
                    {
                        MessageID = c.Long(nullable: false, identity: true),
                        SenderID = c.Int(nullable: false),
                        ReceiverID = c.Int(nullable: false),
                        Content = c.String(nullable: false),
                        SentAt = c.DateTime(nullable: false),
                        IsRead = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.MessageID)
                .ForeignKey("dbo.Users", t => t.ReceiverID)
                .ForeignKey("dbo.Users", t => t.SenderID)
                .Index(t => t.SenderID)
                .Index(t => t.ReceiverID);
            
            CreateTable(
                "dbo.MilkDonationPosts",
                c => new
                    {
                        PostID = c.Long(nullable: false, identity: true),
                        UserID = c.Int(nullable: false),
                        Title = c.String(nullable: false, maxLength: 255),
                        Content = c.String(nullable: false),
                        VerificationTier = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.PostID)
                .ForeignKey("dbo.Users", t => t.UserID)
                .Index(t => t.UserID);
            
            CreateTable(
                "dbo.MilkDonationRequests",
                c => new
                    {
                        RequestID = c.Long(nullable: false, identity: true),
                        PostID = c.Long(nullable: false),
                        RecipientUserID = c.Int(nullable: false),
                        DonorUserID = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        RequestedAt = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        Note = c.String(maxLength: 1000),
                    })
                .PrimaryKey(t => t.RequestID)
                .ForeignKey("dbo.Users", t => t.DonorUserID)
                .ForeignKey("dbo.MilkDonationPosts", t => t.PostID, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.RecipientUserID)
                .Index(t => t.PostID)
                .Index(t => t.RecipientUserID)
                .Index(t => t.DonorUserID);
            
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        NotificationID = c.Long(nullable: false, identity: true),
                        UserID = c.Int(nullable: false),
                        Title = c.String(nullable: false, maxLength: 255),
                        Message = c.String(nullable: false),
                        Type = c.Int(nullable: false),
                        IsRead = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        ReadAt = c.DateTime(),
                        RelatedPostID = c.Long(),
                        RelatedRequestID = c.Long(),
                    })
                .PrimaryKey(t => t.NotificationID)
                .ForeignKey("dbo.Users", t => t.UserID, cascadeDelete: true)
                .Index(t => t.UserID);
            
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
            
            CreateTable(
                "dbo.UserLifestyleSurveys",
                c => new
                    {
                        SurveyID = c.Long(nullable: false, identity: true),
                        UserID = c.Int(nullable: false),
                        IsSmoker = c.Boolean(nullable: false),
                        UsesAlcohol = c.Boolean(nullable: false),
                        UsesMedication = c.Boolean(nullable: false),
                        MedicationDetails = c.String(),
                        CommitNoDrugs = c.Boolean(nullable: false),
                        CommitNoInfectiousDiseases = c.Boolean(nullable: false),
                        SubmittedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.SurveyID)
                .ForeignKey("dbo.Users", t => t.UserID)
                .Index(t => t.UserID);
            
            CreateTable(
                "dbo.UserMedicalRecords",
                c => new
                    {
                        RecordID = c.Long(nullable: false, identity: true),
                        UserID = c.Int(nullable: false),
                        FileName = c.String(nullable: false, maxLength: 255),
                        FileUrl = c.String(nullable: false, maxLength: 1024),
                        VerificationStatus = c.Int(nullable: false),
                        AdminReviewerID = c.Int(),
                        ReviewNotes = c.String(maxLength: 500),
                        UploadedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.RecordID)
                .ForeignKey("dbo.Users", t => t.AdminReviewerID)
                .ForeignKey("dbo.Users", t => t.UserID)
                .Index(t => t.UserID)
                .Index(t => t.AdminReviewerID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserMedicalRecords", "UserID", "dbo.Users");
            DropForeignKey("dbo.UserMedicalRecords", "AdminReviewerID", "dbo.Users");
            DropForeignKey("dbo.UserLifestyleSurveys", "UserID", "dbo.Users");
            DropForeignKey("dbo.Notifications", "UserID", "dbo.Users");
            DropForeignKey("dbo.MilkDonationRequests", "RecipientUserID", "dbo.Users");
            DropForeignKey("dbo.MilkDonationRequests", "PostID", "dbo.MilkDonationPosts");
            DropForeignKey("dbo.MilkDonationRequests", "DonorUserID", "dbo.Users");
            DropForeignKey("dbo.MilkDonationPosts", "UserID", "dbo.Users");
            DropForeignKey("dbo.Messages", "SenderID", "dbo.Users");
            DropForeignKey("dbo.Messages", "ReceiverID", "dbo.Users");
            DropForeignKey("dbo.Posts_C2C", "UserID", "dbo.Users");
            DropForeignKey("dbo.Ratings", "RaterUserID", "dbo.Users");
            DropForeignKey("dbo.Ratings", "RatedUserID", "dbo.Users");
            DropForeignKey("dbo.UserDetails", "UserID", "dbo.Users");
            DropForeignKey("dbo.Ratings", "PostID", "dbo.Posts_C2C");
            DropForeignKey("dbo.Post_C2C_Images", "PostID", "dbo.Posts_C2C");
            DropForeignKey("dbo.Post_C2C_ExchangePreferences", "PostID", "dbo.Posts_C2C");
            DropForeignKey("dbo.Post_C2C_ExchangePreferences", "CategoryID", "dbo.Categories");
            DropForeignKey("dbo.Posts_C2C", "CategoryID", "dbo.Categories");
            DropForeignKey("dbo.Categories", "ParentCategoryID", "dbo.Categories");
            DropIndex("dbo.UserMedicalRecords", new[] { "AdminReviewerID" });
            DropIndex("dbo.UserMedicalRecords", new[] { "UserID" });
            DropIndex("dbo.UserLifestyleSurveys", new[] { "UserID" });
            DropIndex("dbo.Notifications", new[] { "UserID" });
            DropIndex("dbo.MilkDonationRequests", new[] { "DonorUserID" });
            DropIndex("dbo.MilkDonationRequests", new[] { "RecipientUserID" });
            DropIndex("dbo.MilkDonationRequests", new[] { "PostID" });
            DropIndex("dbo.MilkDonationPosts", new[] { "UserID" });
            DropIndex("dbo.Messages", new[] { "ReceiverID" });
            DropIndex("dbo.Messages", new[] { "SenderID" });
            DropIndex("dbo.UserDetails", new[] { "UserID" });
            DropIndex("dbo.Ratings", new[] { "PostID" });
            DropIndex("dbo.Ratings", new[] { "RatedUserID" });
            DropIndex("dbo.Ratings", new[] { "RaterUserID" });
            DropIndex("dbo.Post_C2C_Images", new[] { "PostID" });
            DropIndex("dbo.Post_C2C_ExchangePreferences", new[] { "CategoryID" });
            DropIndex("dbo.Post_C2C_ExchangePreferences", new[] { "PostID" });
            DropIndex("dbo.Posts_C2C", new[] { "CategoryID" });
            DropIndex("dbo.Posts_C2C", new[] { "UserID" });
            DropIndex("dbo.Categories", new[] { "ParentCategoryID" });
            DropTable("dbo.UserMedicalRecords");
            DropTable("dbo.UserLifestyleSurveys");
            DropTable("dbo.SystemSettings");
            DropTable("dbo.Notifications");
            DropTable("dbo.MilkDonationRequests");
            DropTable("dbo.MilkDonationPosts");
            DropTable("dbo.Messages");
            DropTable("dbo.UserDetails");
            DropTable("dbo.Users");
            DropTable("dbo.Ratings");
            DropTable("dbo.Post_C2C_Images");
            DropTable("dbo.Post_C2C_ExchangePreferences");
            DropTable("dbo.Posts_C2C");
            DropTable("dbo.Categories");
            DropTable("dbo.Brands");
        }
    }
}
