using System;
using System.Collections.Generic;
using System.Linq;
using B_M.Models.Entities;

namespace B_M.Helpers
{
    // Cart item for session storage
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
    }
    
    public class CartHelper
    {
        private const string SessionCartKey = "ShoppingCart";
        
        /// <summary>
        /// Get current cart from session
        /// </summary>
        public static List<CartItem> GetCart()
        {
            if (System.Web.HttpContext.Current.Session[SessionCartKey] == null)
            {
                System.Web.HttpContext.Current.Session[SessionCartKey] = new List<CartItem>();
            }
            return (List<CartItem>)System.Web.HttpContext.Current.Session[SessionCartKey];
        }
        
        /// <summary>
        /// Add item to cart
        /// </summary>
        public static void AddToCart(CartItem item)
        {
            var cart = GetCart();
            var existingItem = cart.FirstOrDefault(x => x.ProductId == item.ProductId);
            
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                cart.Add(item);
            }
            
            System.Web.HttpContext.Current.Session[SessionCartKey] = cart;
        }
        
        /// <summary>
        /// Update item quantity in cart
        /// </summary>
        public static void UpdateCartItem(int productId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);
            
            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
            }
            
            System.Web.HttpContext.Current.Session[SessionCartKey] = cart;
        }
        
        /// <summary>
        /// Remove item from cart
        /// </summary>
        public static void RemoveFromCart(int productId)
        {
            var cart = GetCart();
            cart.RemoveAll(x => x.ProductId == productId);
            System.Web.HttpContext.Current.Session[SessionCartKey] = cart;
        }
        
        /// <summary>
        /// Clear entire cart
        /// </summary>
        public static void ClearCart()
        {
            System.Web.HttpContext.Current.Session[SessionCartKey] = new List<CartItem>();
        }
        
        /// <summary>
        /// Get total items in cart
        /// </summary>
        public static int GetCartItemCount()
        {
            return GetCart().Sum(x => x.Quantity);
        }
        
        /// <summary>
        /// Get cart total amount
        /// </summary>
        public static decimal GetCartTotal()
        {
            return GetCart().Sum(x => x.TotalPrice);
        }
        
        /// <summary>
        /// Check if cart is empty
        /// </summary>
        public static bool IsCartEmpty()
        {
            return GetCart().Count == 0;
        }
    }
}

