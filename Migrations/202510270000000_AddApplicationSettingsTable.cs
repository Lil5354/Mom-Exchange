namespace B_M.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddApplicationSettingsTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ApplicationSettings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Category = c.String(nullable: false, maxLength: 50),
                        Key = c.String(nullable: false, maxLength: 100),
                        Value = c.String(),
                        DataType = c.String(maxLength: 20),
                        Description = c.String(maxLength: 500),
                        IsEncrypted = c.Boolean(nullable: false),
                        LastUpdated = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        UpdatedBy = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => new { t.Category, t.Key }, unique: true, name: "IX_Category_Key");
            
            // Insert default settings
            InsertDefaultSettings();
        }
        
        public override void Down()
        {
            DropIndex("dbo.ApplicationSettings", "IX_Category_Key");
            DropTable("dbo.ApplicationSettings");
        }
        
        private void InsertDefaultSettings()
        {
            // Email Settings - Load from Web.config
            Sql(@"INSERT INTO ApplicationSettings (Category, [Key], Value, DataType, Description, IsEncrypted, LastUpdated) 
                  VALUES ('Email', 'SmtpHost', 'smtp.gmail.com', 'String', 'SMTP Server Host', 0, GETDATE())");
            
            Sql(@"INSERT INTO ApplicationSettings (Category, [Key], Value, DataType, Description, IsEncrypted, LastUpdated) 
                  VALUES ('Email', 'SmtpPort', '587', 'Int', 'SMTP Server Port', 0, GETDATE())");
            
            Sql(@"INSERT INTO ApplicationSettings (Category, [Key], Value, DataType, Description, IsEncrypted, LastUpdated) 
                  VALUES ('Email', 'EnableSSL', 'true', 'Bool', 'Enable SSL/TLS', 0, GETDATE())");
            
            Sql(@"INSERT INTO ApplicationSettings (Category, [Key], Value, DataType, Description, IsEncrypted, LastUpdated) 
                  VALUES ('Email', 'Username', '', 'String', 'SMTP Username', 0, GETDATE())");
            
            Sql(@"INSERT INTO ApplicationSettings (Category, [Key], Value, DataType, Description, IsEncrypted, LastUpdated) 
                  VALUES ('Email', 'Password', '', 'String', 'SMTP Password', 1, GETDATE())");
            
            Sql(@"INSERT INTO ApplicationSettings (Category, [Key], Value, DataType, Description, IsEncrypted, LastUpdated) 
                  VALUES ('Email', 'FromEmail', 'noreply@momexchange.com', 'String', 'From Email Address', 0, GETDATE())");
            
            Sql(@"INSERT INTO ApplicationSettings (Category, [Key], Value, DataType, Description, IsEncrypted, LastUpdated) 
                  VALUES ('Email', 'FromName', 'MomExchange System', 'String', 'From Name', 0, GETDATE())");
            
            Sql(@"INSERT INTO ApplicationSettings (Category, [Key], Value, DataType, Description, IsEncrypted, LastUpdated) 
                  VALUES ('Email', 'IsEnabled', 'true', 'Bool', 'Email Service Enabled', 0, GETDATE())");

            // Security Settings
            Sql(@"INSERT INTO ApplicationSettings (Category, [Key], Value, DataType, Description, IsEncrypted, LastUpdated) 
                  VALUES ('Security', 'MinPasswordLength', '8', 'Int', 'Minimum Password Length', 0, GETDATE())");
            
            Sql(@"INSERT INTO ApplicationSettings (Category, [Key], Value, DataType, Description, IsEncrypted, LastUpdated) 
                  VALUES ('Security', 'RequireSpecialChars', 'true', 'Bool', 'Require Special Characters', 0, GETDATE())");
            
            Sql(@"INSERT INTO ApplicationSettings (Category, [Key], Value, DataType, Description, IsEncrypted, LastUpdated) 
                  VALUES ('Security', 'RequireNumbers', 'true', 'Bool', 'Require Numbers', 0, GETDATE())");
            
            Sql(@"INSERT INTO ApplicationSettings (Category, [Key], Value, DataType, Description, IsEncrypted, LastUpdated) 
                  VALUES ('Security', 'RequireUppercase', 'true', 'Bool', 'Require Uppercase Letters', 0, GETDATE())");
            
            Sql(@"INSERT INTO ApplicationSettings (Category, [Key], Value, DataType, Description, IsEncrypted, LastUpdated) 
                  VALUES ('Security', 'SessionTimeoutMinutes', '30', 'Int', 'Session Timeout in Minutes', 0, GETDATE())");
            
            Sql(@"INSERT INTO ApplicationSettings (Category, [Key], Value, DataType, Description, IsEncrypted, LastUpdated) 
                  VALUES ('Security', 'MaxLoginAttempts', '5', 'Int', 'Max Login Attempts', 0, GETDATE())");
            
            Sql(@"INSERT INTO ApplicationSettings (Category, [Key], Value, DataType, Description, IsEncrypted, LastUpdated) 
                  VALUES ('Security', 'EnableTwoFactor', 'false', 'Bool', 'Enable Two-Factor Authentication', 0, GETDATE())");
            
            Sql(@"INSERT INTO ApplicationSettings (Category, [Key], Value, DataType, Description, IsEncrypted, LastUpdated) 
                  VALUES ('Security', 'AccountLockoutMinutes', '15', 'Int', 'Account Lockout Duration', 0, GETDATE())");
            
            Sql(@"INSERT INTO ApplicationSettings (Category, [Key], Value, DataType, Description, IsEncrypted, LastUpdated) 
                  VALUES ('Security', 'PasswordChangeDays', '90', 'Int', 'Password Change Requirement', 0, GETDATE())");

            // System Configuration
            Sql(@"INSERT INTO ApplicationSettings (Category, [Key], Value, DataType, Description, IsEncrypted, LastUpdated) 
                  VALUES ('System', 'SiteName', 'MomExchange', 'String', 'Website Name', 0, GETDATE())");
            
            Sql(@"INSERT INTO ApplicationSettings (Category, [Key], Value, DataType, Description, IsEncrypted, LastUpdated) 
                  VALUES ('System', 'SiteUrl', 'https://localhost:44300', 'String', 'Website URL', 0, GETDATE())");
            
            Sql(@"INSERT INTO ApplicationSettings (Category, [Key], Value, DataType, Description, IsEncrypted, LastUpdated) 
                  VALUES ('System', 'ContactEmail', 'contact@momexchange.com', 'String', 'Contact Email', 0, GETDATE())");
            
            Sql(@"INSERT INTO ApplicationSettings (Category, [Key], Value, DataType, Description, IsEncrypted, LastUpdated) 
                  VALUES ('System', 'MaxFileUploadSizeMB', '10', 'Int', 'Max File Upload Size in MB', 0, GETDATE())");
            
            Sql(@"INSERT INTO ApplicationSettings (Category, [Key], Value, DataType, Description, IsEncrypted, LastUpdated) 
                  VALUES ('System', 'MaintenanceMode', 'false', 'Bool', 'Maintenance Mode Enabled', 0, GETDATE())");
            
            Sql(@"INSERT INTO ApplicationSettings (Category, [Key], Value, DataType, Description, IsEncrypted, LastUpdated) 
                  VALUES ('System', 'EnableCaching', 'true', 'Bool', 'Enable Caching', 0, GETDATE())");
        }
    }
}

