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
            
        }
    }
}

