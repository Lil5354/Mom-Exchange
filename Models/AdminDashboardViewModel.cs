using System.Collections.Generic;

namespace B_M.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int AdminUsers { get; set; }
        public int MomUsers { get; set; }
        public int ClientUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public List<User> RecentUsers { get; set; } = new List<User>();

        // Milk Donation Statistics
        public int TotalMilkPosts { get; set; }
        public int ActiveMilkPosts { get; set; }
        public int Tier1Users { get; set; }
        public int Tier2Users { get; set; }
        public int PendingMedicalRecords { get; set; }

        // Category Statistics
        public int TotalCategories { get; set; }
        public int ActiveCategories { get; set; }

        // C2C Post Statistics
        public int TotalC2CPosts { get; set; }
        public int ActiveC2CPosts { get; set; }
    }
}

