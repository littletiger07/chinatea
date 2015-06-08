using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ChinaTea.Entities;

namespace ChinaTea.Models
{
    public partial class ShoppingCart
    {
        ChinaTeaEntities dbContext = new ChinaTeaEntities();
        string shoppingCartId { get; set; }
        public const string CartSessionKey = "CartId";

        public static ShoppingCart GetCart(HttpContextBase context)
        {
            var cart = new ShoppingCart();
            cart.shoppingCartId = cart.GetCartId(context);
            return cart;
        }

        public void AddToCart(Product product, int count=1)
        {
            var cartItem = dbContext.Carts.SingleOrDefault(
                c => c.CartId == shoppingCartId && c.ProductId == product.Id);
            
            if(cartItem==null)
            {
                //Create a new cart item
                cartItem = new Cart
                {
                    ProductId = product.Id,
                    CartId = shoppingCartId,
                    Count = count,
                    DateCreated = DateTime.Now
                };
                dbContext.Carts.Add(cartItem);                
            }

            else
            {
                //Addd one to the quantities
                cartItem.Count+=count;
            }

            dbContext.SaveChanges();
        }


        public void RemoveFromCart(int id)
        {
            //Get the cart
            var cartItem = dbContext.Carts.Single(
                c => c.CartId == shoppingCartId && c.RecordId == id);
            if(cartItem!=null)
            {
                if(cartItem.Count>1)
                {
                    cartItem.Count--;
                }
                else
                {
                    dbContext.Carts.Remove(cartItem);
                }
                dbContext.SaveChanges();
                
            }
        }

        public int UpdateCartCount(int id, int cartCount)
        {
            // Get the cart 
            var cartItem = dbContext.Carts.Single(
                cart => cart.CartId == shoppingCartId && cart.RecordId == id);

            int itemCount = 0;

            if (cartItem != null)
            {
                if (cartCount > 0)
                {
                    cartItem.Count = cartCount;
                    itemCount = cartItem.Count;
                }
                else
                {
                    dbContext.Carts.Remove(cartItem);
                }
               
                dbContext.SaveChanges();
            }
            return itemCount;
        }

        public void EmptyCart()
        {
            var cartItems = dbContext.Carts.Where(c => c.CartId == shoppingCartId);

            foreach (var cartItem in cartItems)
            {
                dbContext.Carts.Remove(cartItem);
            }
            dbContext.SaveChanges();
        }

        public List<Cart> GetCartItems()
        {
            var cartItems = (from cart in dbContext.Carts where cart.CartId == shoppingCartId select cart).ToList();
            return cartItems;
        }

        public int GetCount()
        {
            int? count = (from cartItems in dbContext.Carts where cartItems.CartId == shoppingCartId select (int?)cartItems.Count).Sum();

            return count ?? 0;
        }

        public decimal GetTotal()
        {
            decimal? total = (from cartItems in dbContext.Carts where cartItems.CartId == shoppingCartId select (int?)cartItems.Count * cartItems.Product.ProductUnitPrice).Sum();

            return total ?? decimal.Zero;
        }

        public int CreateOrder (Order order)
        {
            decimal orderTotal = 0;
            var cartItems = GetCartItems();

            //Iterate the items in the cart

            foreach (var cartItem in cartItems)
            {
                OrderItem orderItem = new OrderItem()
                {
                    ProductId = cartItem.ProductId,
                    OrderId = order.Id,
                    Quantities=cartItem.Count
                };
                Product product = dbContext.Products.FirstOrDefault(p => p.Id == cartItem.ProductId);
                product.Stock -= cartItem.Count;
                
                dbContext.OrderItems.Add(orderItem);

                orderTotal += (cartItem.Count * cartItem.Product.ProductUnitPrice);
            }

            dbContext.SaveChanges();

            //Empty the shopping cart
            EmptyCart();

            return order.Id;
        }

        //to allow access to cookies
        public string GetCartId(HttpContextBase context)
        {
            if (context.Session[CartSessionKey] == null)
            {
                if (!string.IsNullOrWhiteSpace(context.User.Identity.Name))
                {
                    // User is logged in, associate the cart with there username
                    context.Session[CartSessionKey] = context.User.Identity.Name;
                }
                else
                {
                    // Generate a new random GUID using System.Guid Class
                    Guid tempCartId = Guid.NewGuid();
                    // Send tempCartId back to client as a cookie
                    context.Session[CartSessionKey] = tempCartId.ToString();
                }
            }
            return context.Session[CartSessionKey].ToString();
        }

        // When a user has logged in, migrate their shopping cart to
        // be associated with their username
        public void MigrateCart(string username)
        {
            var shoppingCart = dbContext.Carts.Where(c => c.CartId == shoppingCartId);
            foreach (Cart item in shoppingCart)
            {
                item.CartId = username;
            }
            dbContext.SaveChanges();
        }

       
    } 
   
}