// File: Models/ApplicationDbContext.cs
using System.Data.Entity;

namespace B_M.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("MomExchangeDB")
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserDetails> UserDetails { get; set; }

        // Category Management
        public DbSet<Category> Categories { get; set; }

        // Module 3 - B2C Tables
        public DbSet<Brand> Brands { get; set; }
        public DbSet<ProductB2C> ProductB2Cs { get; set; }
        public DbSet<ProductB2CImage> ProductB2CImages { get; set; }
        public DbSet<BrandCategoryPermission> BrandCategoryPermissions { get; set; }

        // Module 3 - C2C Tables
        public DbSet<PostC2C> PostC2Cs { get; set; }
        public DbSet<PostC2CImage> PostC2CImages { get; set; }

        // Order Management
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        // Affiliate System
        public DbSet<AffiliateClick> AffiliateClicks { get; set; }
        public DbSet<AffiliateSale> AffiliateSales { get; set; }

        // Milk Donation System
        public DbSet<UserLifestyleSurvey> UserLifestyleSurveys { get; set; }
        public DbSet<UserMedicalRecord> UserMedicalRecords { get; set; }
        public DbSet<MilkDonationPost> MilkDonationPosts { get; set; }
        public DbSet<MilkDonationRequest> MilkDonationRequests { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        // Communication & Trading
        public DbSet<Message> Messages { get; set; }
        public DbSet<Rating> Ratings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // ====== USER CONFIGURATION ======
            modelBuilder.Entity<User>()
                .ToTable("Users")
                .HasKey(u => u.UserID);

            modelBuilder.Entity<User>()
                .Property(u => u.UserID)
                .HasColumnName("UserID");

            modelBuilder.Entity<User>()
                .Property(u => u.UserName)
                .HasMaxLength(50);

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .HasMaxLength(255);

            modelBuilder.Entity<User>()
                .Property(u => u.PhoneNumber)
                .HasMaxLength(20);

            modelBuilder.Entity<User>()
                .Property(u => u.PasswordHash)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.IsActive)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2");

            modelBuilder.Entity<User>()
                .Property(u => u.MilkDonationStatus)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.GoogleId)
                .HasMaxLength(255);

            // ====== USER DETAILS CONFIGURATION ======
            modelBuilder.Entity<UserDetails>()
                .ToTable("UserDetails")
                .HasKey(ud => ud.UserID);

            modelBuilder.Entity<UserDetails>()
                .Property(ud => ud.FullName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<UserDetails>()
                .Property(ud => ud.ProfilePictureURL)
                .HasMaxLength(500);

            modelBuilder.Entity<UserDetails>()
                .Property(ud => ud.Address)
                .HasMaxLength(500);

            modelBuilder.Entity<UserDetails>()
                .Property(ud => ud.ReputationScore)
                .IsRequired();

            // User -> UserDetails relationship (1:1)
            modelBuilder.Entity<User>()
                .HasOptional(u => u.UserDetails)
                .WithRequired(ud => ud.User)
                .WillCascadeOnDelete(true);

            // ====== CATEGORY CONFIGURATION ======
            modelBuilder.Entity<Category>()
                .ToTable("Categories")
                .HasKey(c => c.CategoryID);

            modelBuilder.Entity<Category>()
                .Property(c => c.CategoryName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Category>()
                .Property(c => c.Description)
                .HasMaxLength(500);

            // Self-referencing relationship for parent categories
            modelBuilder.Entity<Category>()
                .HasOptional(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryID)
                .WillCascadeOnDelete(false);


            // ====== BRAND CONFIGURATION ======
            modelBuilder.Entity<Brand>()
                .ToTable("Brands")
                .HasKey(b => b.BrandID);

            modelBuilder.Entity<Brand>()
                .Property(b => b.BrandName)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<Brand>()
                .Property(b => b.LogoUrl)
                .HasMaxLength(1024);

            modelBuilder.Entity<Brand>()
                .HasRequired(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserID)
                .WillCascadeOnDelete(false);

            // ====== POST C2C CONFIGURATION ======
            modelBuilder.Entity<PostC2C>()
                .ToTable("Posts_C2C")
                .HasKey(p => p.PostID);

            modelBuilder.Entity<PostC2C>()
                .Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<PostC2C>()
                .Property(p => p.Content)
                .IsRequired();

            modelBuilder.Entity<PostC2C>()
                .Property(p => p.Condition)
                .HasMaxLength(100);

            modelBuilder.Entity<PostC2C>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PostC2C>()
                .Property(p => p.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2");

            modelBuilder.Entity<PostC2C>()
                .HasRequired(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PostC2C>()
                .HasRequired(p => p.Category)
                .WithMany(c => c.PostC2Cs)
                .HasForeignKey(p => p.CategoryID)
                .WillCascadeOnDelete(false);

            // ====== POST C2C IMAGE CONFIGURATION ======
            modelBuilder.Entity<PostC2CImage>()
                .ToTable("Post_C2C_Images")
                .HasKey(pi => pi.ImageID);

            modelBuilder.Entity<PostC2CImage>()
                .Property(pi => pi.ImageUrl)
                .IsRequired()
                .HasMaxLength(1024);

            modelBuilder.Entity<PostC2CImage>()
                .HasRequired(pi => pi.Post)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.PostID)
                .WillCascadeOnDelete(true);

            // ====== PRODUCT B2C CONFIGURATION ======
            modelBuilder.Entity<ProductB2C>()
                .ToTable("Products_B2C")
                .HasKey(p => p.ProductID);

            modelBuilder.Entity<ProductB2C>()
                .Property(p => p.ProductName)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<ProductB2C>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ProductB2C>()
                .Property(p => p.AffiliateCommissionRate)
                .HasPrecision(5, 2);

            modelBuilder.Entity<ProductB2C>()
                .HasRequired(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ProductB2C>()
                .HasRequired(p => p.Category)
                .WithMany(c => c.ProductB2Cs)
                .HasForeignKey(p => p.CategoryID)
                .WillCascadeOnDelete(false);

            // ====== PRODUCT B2C IMAGE CONFIGURATION ======
            modelBuilder.Entity<ProductB2CImage>()
                .ToTable("Product_B2C_Images")
                .HasKey(pi => pi.ImageID);

            modelBuilder.Entity<ProductB2CImage>()
                .Property(pi => pi.ImageUrl)
                .IsRequired()
                .HasMaxLength(1024);

            modelBuilder.Entity<ProductB2CImage>()
                .HasRequired(pi => pi.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProductID)
                .WillCascadeOnDelete(true);

            // ====== BRAND CATEGORY PERMISSION CONFIGURATION ======
            modelBuilder.Entity<BrandCategoryPermission>()
                .ToTable("Brand_Category_Permissions")
                .HasKey(bcp => bcp.BrandCategoryPermissionID);

            modelBuilder.Entity<BrandCategoryPermission>()
                .HasRequired(bcp => bcp.Brand)
                .WithMany(b => b.CategoryPermissions)
                .HasForeignKey(bcp => bcp.BrandID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<BrandCategoryPermission>()
                .HasRequired(bcp => bcp.Category)
                .WithMany(c => c.BrandPermissions)
                .HasForeignKey(bcp => bcp.CategoryID)
                .WillCascadeOnDelete(false);

            // ====== ORDER CONFIGURATION ======
            modelBuilder.Entity<Order>()
                .ToTable("Orders")
                .HasKey(o => o.OrderID);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .HasRequired(o => o.BuyerUser)
                .WithMany()
                .HasForeignKey(o => o.BuyerUserID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Order>()
                .HasRequired(o => o.Brand)
                .WithMany()
                .HasForeignKey(o => o.BrandID)
                .WillCascadeOnDelete(false);

            // ====== ORDER DETAIL CONFIGURATION ======
            modelBuilder.Entity<OrderDetail>()
                .ToTable("OrderDetails")
                .HasKey(od => od.OrderDetailID);

            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.PriceAtPurchase)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderDetail>()
                .HasRequired(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderID)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<OrderDetail>()
                .HasRequired(od => od.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.ProductID)
                .WillCascadeOnDelete(false);

            // ====== AFFILIATE CLICK CONFIGURATION ======
            modelBuilder.Entity<AffiliateClick>()
                .ToTable("AffiliateClicks")
                .HasKey(ac => ac.ClickID);

            modelBuilder.Entity<AffiliateClick>()
                .HasRequired(ac => ac.AffiliatorUser)
                .WithMany()
                .HasForeignKey(ac => ac.AffiliatorUserID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AffiliateClick>()
                .HasRequired(ac => ac.Product)
                .WithMany(p => p.AffiliateClicks)
                .HasForeignKey(ac => ac.ProductID)
                .WillCascadeOnDelete(false);

            // ====== AFFILIATE SALE CONFIGURATION ======
            modelBuilder.Entity<AffiliateSale>()
                .ToTable("AffiliateSales")
                .HasKey(asale => asale.AffiliateSaleID);

            modelBuilder.Entity<AffiliateSale>()
                .Property(asale => asale.OrderTotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<AffiliateSale>()
                .Property(asale => asale.CommissionAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<AffiliateSale>()
                .HasRequired(asale => asale.AffiliatorUser)
                .WithMany()
                .HasForeignKey(asale => asale.AffiliatorUserID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AffiliateSale>()
                .HasRequired(asale => asale.BuyerUser)
                .WithMany()
                .HasForeignKey(asale => asale.BuyerUserID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AffiliateSale>()
                .HasRequired(asale => asale.Order)
                .WithMany()
                .HasForeignKey(asale => asale.OrderID)
                .WillCascadeOnDelete(false);

            // ====== USER LIFESTYLE SURVEY CONFIGURATION ======
            modelBuilder.Entity<UserLifestyleSurvey>()
                .ToTable("UserLifestyleSurveys")
                .HasKey(uls => uls.SurveyID);

            modelBuilder.Entity<UserLifestyleSurvey>()
                .HasRequired(uls => uls.User)
                .WithMany()
                .HasForeignKey(uls => uls.UserID)
                .WillCascadeOnDelete(false);

            // ====== USER MEDICAL RECORD CONFIGURATION ======
            modelBuilder.Entity<UserMedicalRecord>()
                .ToTable("UserMedicalRecords")
                .HasKey(umr => umr.RecordID);

            modelBuilder.Entity<UserMedicalRecord>()
                .HasRequired(umr => umr.User)
                .WithMany()
                .HasForeignKey(umr => umr.UserID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserMedicalRecord>()
                .HasOptional(umr => umr.AdminReviewer)
                .WithMany()
                .HasForeignKey(umr => umr.AdminReviewerID)
                .WillCascadeOnDelete(false);

            // ====== MILK DONATION POSTS CONFIGURATION ======
            modelBuilder.Entity<MilkDonationPost>()
                .ToTable("MilkDonationPosts")
                .HasKey(mdp => mdp.PostID);

            modelBuilder.Entity<MilkDonationPost>()
                .Property(mdp => mdp.Title)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<MilkDonationPost>()
                .Property(mdp => mdp.Content)
                .IsRequired();

            modelBuilder.Entity<MilkDonationPost>()
                .Property(mdp => mdp.VerificationTier)
                .IsRequired();

            modelBuilder.Entity<MilkDonationPost>()
                .Property(mdp => mdp.Status)
                .IsRequired();

            modelBuilder.Entity<MilkDonationPost>()
                .Property(mdp => mdp.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2");

            modelBuilder.Entity<MilkDonationPost>()
                .HasRequired(mdp => mdp.User)
                .WithMany()
                .HasForeignKey(mdp => mdp.UserID)
                .WillCascadeOnDelete(false);

            // ====== MILK DONATION REQUEST CONFIGURATION ======
            modelBuilder.Entity<MilkDonationRequest>()
                .ToTable("MilkDonationRequests")
                .HasKey(mdr => mdr.RequestID);

            modelBuilder.Entity<MilkDonationRequest>()
                .Property(mdr => mdr.PostID)
                .IsRequired();

            modelBuilder.Entity<MilkDonationRequest>()
                .Property(mdr => mdr.RecipientUserID)
                .IsRequired();

            modelBuilder.Entity<MilkDonationRequest>()
                .Property(mdr => mdr.DonorUserID)
                .IsRequired();

            modelBuilder.Entity<MilkDonationRequest>()
                .Property(mdr => mdr.Status)
                .IsRequired();

            modelBuilder.Entity<MilkDonationRequest>()
                .Property(mdr => mdr.RequestedAt)
                .IsRequired()
                .HasColumnType("datetime2");

            modelBuilder.Entity<MilkDonationRequest>()
                .Property(mdr => mdr.Note)
                .HasMaxLength(1000);

            // MilkDonationRequest relationships
            modelBuilder.Entity<MilkDonationRequest>()
                .HasRequired(mdr => mdr.Post)
                .WithMany()
                .HasForeignKey(mdr => mdr.PostID)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<MilkDonationRequest>()
                .HasRequired(mdr => mdr.RecipientUser)
                .WithMany()
                .HasForeignKey(mdr => mdr.RecipientUserID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<MilkDonationRequest>()
                .HasRequired(mdr => mdr.DonorUser)
                .WithMany()
                .HasForeignKey(mdr => mdr.DonorUserID)
                .WillCascadeOnDelete(false);

            // ====== NOTIFICATION CONFIGURATION ======
            modelBuilder.Entity<Notification>()
                .ToTable("Notifications")
                .HasKey(n => n.NotificationID);

            modelBuilder.Entity<Notification>()
                .Property(n => n.Title)
                .HasMaxLength(255)
                .IsRequired();

            modelBuilder.Entity<Notification>()
                .Property(n => n.Message)
                .IsRequired();

            modelBuilder.Entity<Notification>()
                .HasRequired(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserID)
                .WillCascadeOnDelete(true);

            // ====== MESSAGE CONFIGURATION ======
            modelBuilder.Entity<Message>()
                .ToTable("Messages")
                .HasKey(m => m.MessageID);

            modelBuilder.Entity<Message>()
                .HasRequired(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Message>()
                .HasRequired(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverID)
                .WillCascadeOnDelete(false);

            // ====== RATING CONFIGURATION ======
            modelBuilder.Entity<Rating>()
                .ToTable("Ratings")
                .HasKey(r => r.RatingID);

            modelBuilder.Entity<Rating>()
                .HasRequired(r => r.RaterUser)
                .WithMany()
                .HasForeignKey(r => r.RaterUserID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Rating>()
                .HasRequired(r => r.RatedUser)
                .WithMany()
                .HasForeignKey(r => r.RatedUserID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Rating>()
                .HasRequired(r => r.Post)
                .WithMany(p => p.Ratings)
                .HasForeignKey(r => r.PostID)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}