using System;
using System.Collections.Generic;

namespace B_M.Models
{
    public class AdminPostsViewModel
    {
        public List<PostItem> Posts { get; set; } = new List<PostItem>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalPosts { get; set; }
        
        // Basic search
        public string SearchTerm { get; set; }
        public string TypeFilter { get; set; } // "all", "community", "trade", "social", "milk"
        public string StatusFilter { get; set; } // "all", "active", "pending", "rejected"
        
        // Advanced search
        public string AuthorSearch { get; set; }
        public string TitleSearch { get; set; }
        public string ContentSearch { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public string SortBy { get; set; } // "created", "title", "author", "type", "status"
        public string SortOrder { get; set; } // "asc", "desc"
        
        // Search options
        public bool ShowAdvancedSearch { get; set; }
        public bool CaseSensitive { get; set; }
        public bool ExactMatch { get; set; }
        
        // Statistics
        public int TotalCommunityPosts { get; set; }
        public int TotalTradePosts { get; set; }
        public int TotalSocialPosts { get; set; }
        public int TotalMilkPosts { get; set; }
        public int PendingPosts { get; set; }
        public int RejectedPosts { get; set; }
    }

    public class PostItem
    {
        public int Id { get; set; }
        public string Type { get; set; } // "Community", "Trade", "Social", "Milk"
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public string AuthorEmail { get; set; }
        public string Content { get; set; }
        public string Excerpt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Status { get; set; } // "Active", "Pending", "Rejected"
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public int ViewCount { get; set; }
        public string ImageUrl { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public string Location { get; set; }
        public bool IsHealthVerified { get; set; } // For milk posts
        public string RejectionReason { get; set; }
    }
}





