using System.Collections.Generic;
using B_M.Models.Entities;

namespace B_M.Models.ViewModels
{
    public class OrderUserIndexViewModel
    {
        public List<Order> AllOrders { get; set; }
        public List<Order> PendingOrders { get; set; }
        public List<Order> PaidOrders { get; set; }
        public List<Order> ProcessingOrders { get; set; }
        public List<Order> ShippedOrders { get; set; }
        public List<Order> CompletedOrders { get; set; }
        public List<Order> CancelledOrders { get; set; }
        
        public int TotalCount { get; set; }
        public int PendingCount { get; set; }
        public int PaidCount { get; set; }
        public int ProcessingCount { get; set; }
        public int ShippedCount { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
    }
}

