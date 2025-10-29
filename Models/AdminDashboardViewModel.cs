using System.Collections.Generic;

namespace B_M.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int AdminUsers { get; set; }
        public int ClientUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public List<User> RecentUsers { get; set; } = new List<User>();
    }
}

