namespace B_M.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using B_M.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<B_M.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "B_M.Models.ApplicationDbContext";
        }

        protected override void Seed(B_M.Models.ApplicationDbContext context)
        {
            // Seed initial accounts (Admin and User) and related UserDetails
            SeedUsers(context);
        }

        private void SeedUsers(B_M.Models.ApplicationDbContext context)
        {
            // Use PasswordHelper to generate stable hashes for known default passwords
            var adminPasswordHash = B_M.Helpers.PasswordHelper.HashPassword("Admin@123");
            var userPasswordHash = B_M.Helpers.PasswordHelper.HashPassword("User@123");

            // Create or update users by Email key
            context.Users.AddOrUpdate(
                u => u.Email,
                new User
                {
                    UserName = "admin",
                    Email = "admin@momexchange.local",
                    PhoneNumber = "",
                    PasswordHash = adminPasswordHash,
                    Role = 1, // Admin
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    MilkDonationStatus = 0,
                    GoogleId = null
                },
                new User
                {
                    UserName = "user",
                    Email = "user@momexchange.local",
                    PhoneNumber = "",
                    PasswordHash = userPasswordHash,
                    Role = 2, // Mom/User
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    MilkDonationStatus = 0,
                    GoogleId = null
                }
            );

            context.SaveChanges();

            // Ensure UserDetails exist for the users
            var admin = context.Users.FirstOrDefault(x => x.Email == "admin@momexchange.local");
            var normal = context.Users.FirstOrDefault(x => x.Email == "user@momexchange.local");

            if (admin != null)
            {
                context.UserDetails.AddOrUpdate(
                    ud => ud.UserID,
                    new UserDetails
                    {
                        UserID = admin.UserID,
                        FullName = "System Administrator",
                        ProfilePictureURL = "/images/avatar-default.jpg",
                        Address = null,
                        ReputationScore = 5.0
                    }
                );
            }

            if (normal != null)
            {
                context.UserDetails.AddOrUpdate(
                    ud => ud.UserID,
                    new UserDetails
                    {
                        UserID = normal.UserID,
                        FullName = "Demo User",
                        ProfilePictureURL = "/images/avatar-default.jpg",
                        Address = null,
                        ReputationScore = 0
                    }
                );
            }

            context.SaveChanges();
        }
    }
}

