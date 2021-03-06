﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WingtipToys.Models;


namespace WingtipToys.Logic
{
    public class ShoppingCartActions : IDisposable
    {
        public string ShoppingCartId { get; set; }
        private ProductContext _db = new ProductContext();
        public const string CartSessionKey = "CartId";

        // Recieve call from 'usersShoppingCart' located in AddToCart page
        // that passes the rawId (Original ProductID query string) parameter
        public void AddToCart(int id)
        {
            // Store return from GetCartId method below in var 'ShoppingCartId'
            ShoppingCartId = GetCartId();
            var cartItem = _db.ShoppingCartItems.SingleOrDefault(c => c.CartId == ShoppingCartId 
                && c.ProductId == id);

            if (cartItem == null)
            {
                // Create a new cart if none exits
                cartItem = new CartItem
                {
                    ItemId = Guid.NewGuid().ToString(),
                    ProductId = id,
                    CartId = ShoppingCartId,
                    Product = _db.Products.SingleOrDefault(
                    p => p.ProductID == id),
                    Quantity = 1,
                    DateCreated = DateTime.Now
                };

                // Add cartItem to database
                _db.ShoppingCartItems.Add(cartItem);
            }
            else
            {
                // If the item does exist in the cart, 
                // then add one to the quantity. 
                cartItem.Quantity++;
            }
            _db.SaveChanges();

        }

        public void Dispose()
        {
            if (_db != null)
            {
                _db.Dispose();
                _db = null;
            }
        }

        // Called by 'AddToCart' method above
        public string GetCartId()
        {
            if(HttpContext.Current.Session[CartSessionKey] == null)
            {
                if (!string.IsNullOrWhiteSpace(HttpContext.Current.User.Identity.Name))
                {
                   HttpContext.Current.Session[CartSessionKey] = HttpContext.Current.User.Identity.Name;
                }
                else
                {
                    // Generate a new random GUID using System.Guid class. 
                    Guid tempCartId = Guid.NewGuid();
                    HttpContext.Current.Session[CartSessionKey] = tempCartId.ToString();
                }
            }
            return HttpContext.Current.Session[CartSessionKey].ToString();
        }

        public List<CartItem> GetCartItems()
        {
            ShoppingCartId = GetCartId();

            return _db.ShoppingCartItems.Where(
            c => c.CartId == ShoppingCartId).ToList();
        }

        public decimal GetTotal()
        {
            ShoppingCartId = GetCartId();
            // Multiply product price by quantity of that product to get 
            // the current price for each of those products in the cart. 
            // Sum all product price totals to get the cart total. 
            decimal? total = decimal.Zero;            total = (decimal?)(from cartItems in _db.ShoppingCartItems                               where cartItems.CartId == ShoppingCartId                                select(int?) cartItems.Quantity * cartItems.Product.UnitPrice).Sum();
            // IF total !null return total, else return Decimal.Zero            return total ?? decimal.Zero;                                
        }
    }
}
  

    
