using System;
using System.Collections.Generic;

namespace B_M.Models
{
    public class AdminUsersViewModel
    {
        public List<User> Users { get; set; } = new List<User>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalUsers { get; set; }
        
        // Basic search
        public string SearchTerm { get; set; }
        public string RoleFilter { get; set; }
        
        // Advanced search
        public string EmailSearch { get; set; }
        public string UsernameSearch { get; set; }
        public string FullNameSearch { get; set; }
        public string PhoneSearch { get; set; }
        public string AddressSearch { get; set; }
        public string StatusFilter { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public string SortBy { get; set; }
        public string SortOrder { get; set; }
        
        // Search options
        public bool ShowAdvancedSearch { get; set; }
        public bool CaseSensitive { get; set; }
        public bool ExactMatch { get; set; }
    }
}

