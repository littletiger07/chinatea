using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using ChinaTea.Entities;
using ChinaTea.Models;

namespace ChinaTea.Controllers
{
    public class ShoppingCartController : Controller
    {
        ChinaTeaEntities dbContext = new ChinaTeaEntities();
        // GET: ShoppingCart
        public ActionResult Index(string returnUrl="/home/showproducts")
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);

                //setup ViewModel
                var viewModel =new ShoppingCartViewModel{
                    CartItems=cart.GetCartItems(),
                    CartTotal=cart.GetTotal()
                };

            ViewBag.ReturnUrl = returnUrl;
            return View(viewModel);
        }

        public ActionResult AddToCart(int id, int quantity=1, string returnUrl="/home/showproducts")
        {
            //retrieve
            var addedProduct = dbContext.Products.Single(p => p.Id == id);
            var cart = ShoppingCart.GetCart(this.HttpContext);
            
            if(!CheckStock(id,quantity))
            {
                TempData["Message"] = "Not enough stock, please select another product or reduce the quantity.";
                return Redirect(returnUrl);
            }
            cart.AddToCart(addedProduct, quantity);
            return RedirectToAction("Index", new { returnUrl=returnUrl});
        }

        private bool CheckStock(int id, int quantity)
        {
            bool instock = false;
            Product product = dbContext.Products.FirstOrDefault(p => p.Id == id);
            if (product!=null&&product.Stock>=quantity)
            {
                instock = true;
            }

            return instock;
        }

     /*
        [HttpPost]
        public ActionResult RemoveFromCart(int id)
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);
            string productName = dbContext.Carts.Single(item => item.RecordId == id).Product.ProductName;
            cart.RemoveFromCart(id);

            //Display the confirmation message
           var results = new ShoppingCartRemoveViewModel
            {
                Message = Server.HtmlEncode(productName) + "has been removed from your cart.",
                CartCount = cart.GetCount(),
                CartTotal = cart.GetTotal(),
                DeleteId = id
            };
           return View("ProductRemoved", results);
        }
      */


        public ActionResult ConfirmRemoveFromCart(int id)
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);
            string productName = dbContext.Carts.Single(item => item.RecordId == id).Product.ProductName;
        
                        
            if (cart != null)
            {
                cart.RemoveFromCart(id);
                //dbContext.Entry(cart).State = EntityState.Deleted;
                try
                {
                    dbContext.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch
                {
                    return View("Error", new string[] { "Database Error" });
                }
            }

            return View("Error", new string[] { "Product Not Found" });

        }

        [HttpPost]
        public ActionResult UpdateCart(EditCartModel model)
        {
           if (ModelState.IsValid)
           {
               if (model.ItemsToBeDeleted != null)
               {
                   foreach (var id in model.ItemsToBeDeleted)
                   {
                       Cart itemToDelete = dbContext.Carts.FirstOrDefault(c => c.RecordId == id);
                       if (itemToDelete != null)
                       {
                           dbContext.Entry(itemToDelete).State = EntityState.Deleted;
                       }
                   }
               }
               dbContext.SaveChanges();
               foreach (var item in model.EditCartItems)
               {

                   Cart itemToEdit = dbContext.Carts.FirstOrDefault(c => c.RecordId == item.RecordId);
                   if (itemToEdit==null)
                   {
                       continue;
                   }
                   if(item.quantity==0)
                   {
                       dbContext.Entry(itemToEdit).State = EntityState.Deleted;
                   }
                   else
                   {
                       if (itemToEdit.Count==item.quantity)
                       {
                           continue;
                       }
                       if (!CheckStock(itemToEdit.ProductId, item.quantity))
                       {
                           TempData["message"] = "Requested quantity not available for Item: " + itemToEdit.Product.ProductName;
                           return RedirectToAction("Index");
                       }
                       itemToEdit.Count = item.quantity;
                       dbContext.Entry(itemToEdit).State = EntityState.Modified;
                   }
               }
               dbContext.SaveChanges();
               return RedirectToAction("Index");
           }
            return View();
        }

        //Empty the cart
        public ActionResult EmptyCart()
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);
            cart.EmptyCart();
            return RedirectToAction("Index");
        }


        [HttpPost]
        public ActionResult UpdateCartCount(int id, int cartCount)
        {
            
            //int Count = Convert.ToInt32(cartCount);
            var cart = ShoppingCart.GetCart(this.HttpContext);           
            string product = dbContext.Carts.Single(item => item.RecordId == id).Product.ProductName;
            int itemCount = cart.UpdateCartCount(id,cartCount);

            return RedirectToAction("Index");
        }




        [ChildActionOnly]
        public ActionResult CartSummary()
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);
            ViewData["CartCount"] = cart.GetCount();          
            return PartialView("CartSummary");
        }
        private bool IsCartEmpty()
        {
            return (ShoppingCart.GetCart(this.HttpContext).GetCount() == 0);
        }
        [Authorize]
       
        public ActionResult CheckoutStep1()
        {
            if (IsCartEmpty())
            {
                return View("CartEmpty");
            }
            var cart = ShoppingCart.GetCart(this.HttpContext);
            foreach(var cartItem in cart.GetCartItems())
            {
                if (!CheckStock(cartItem.ProductId, cartItem.Count))
                {
                    TempData["message"] = "Requested quantity not available for Item: " + cartItem.Product.ProductName;
                    return RedirectToAction("Index");
                }
            }
            CheckoutModel checkout = GetCheckoutModel();
            
            return View(new CheckoutViewModelStep1() { Addresses = GetAddresses(),  Step1=checkout.Step1 });
        }
        [Authorize]
        [HttpPost]
      
        public ActionResult CheckoutStep1(CheckoutModelStep1 step1, string BtnNext, string BtnPrevious)
        {

            CheckoutModel checkout = GetCheckoutModel();
            if(BtnNext!=null)
            {
                if(ModelState.IsValid)
                {
                    
                    checkout.Step1 = step1;
                    //return View("CheckoutStep2", new CheckoutViewModelStep2() { Addresses=GetAddresses(),Step2=checkout.Step2});
                    return RedirectToAction("CheckoutStep2");
                }
            }
            return View(new CheckoutViewModelStep1() { Addresses = GetAddresses(), Step1 = checkout.Step1 });
        }
        [Authorize]
      
        public ActionResult CheckoutStep2()
        {
            if (IsCartEmpty())
            {
                return View("CartEmpty");
            }
            CheckoutModel checkout = GetCheckoutModel();
            
            if (checkout.Step1==null)
            {
                return RedirectToAction("CheckoutStep1");
            }
            return View(new CheckoutViewModelStep2() { Addresses = GetAddresses(),  Step2=checkout.Step2 });
        
          
        }
        [Authorize]
        [HttpPost]
       
        public ActionResult CheckoutStep2(CheckoutModelStep2 step2, string BtnNext, string BtnPrevious)
        {
            CheckoutModel checkout = GetCheckoutModel();
            if(BtnPrevious!=null)
            {
                //return View("CheckoutStep1", new CheckoutViewModelStep1(){Addresses = GetAddresses(),  Step1=checkout.Step1 });
                return RedirectToAction("CheckoutStep1");
            }
            if(BtnNext!=null)
            {
                if(ModelState.IsValid)
                {
                   
                    checkout.Step2 = step2;
                    return RedirectToAction("CheckoutStep3");
                }
            }
            return View(new CheckoutViewModelStep2() { Addresses=GetAddresses(),Step2=checkout.Step2});
        }
        [Authorize]
      
        public ActionResult CheckoutStep3()
        {
            if (IsCartEmpty())
            {
                return View("CartEmpty");
            }
            CheckoutModel checkout = GetCheckoutModel();
            return View(checkout.Step3);
        }
        [Authorize]
        [HttpPost]
       
        public ActionResult CheckoutStep3(CheckoutViewModelStep3 step3, string BtnPrevious, string BtnNext)
        {
            CheckoutModel checkout = GetCheckoutModel();
            if (BtnPrevious!=null)
            {
                return RedirectToAction("CheckoutStep2");
            }
            if (BtnNext!=null)
            {
                if (ModelState.IsValid)
                {
                    checkout.Step3 = step3;
                    return RedirectToAction("ReviewOrder");
                }
            }
            return View(step3);
        }
        [Authorize]
      
        public ActionResult ReviewOrder()
        {
            CheckoutModel checkoutModel = GetCheckoutModel();
            if (checkoutModel.Step1==null)
            {
                return RedirectToAction("CheckoutStep1");
            }
            if (checkoutModel.Step2==null)
            {
                return RedirectToAction("CheckoutStep2");
            }
            if(checkoutModel.Step3==null)
            {
                return RedirectToAction("CheckoutStep3");
            }
            ReviewOrderViewModel vm = new ReviewOrderViewModel()
            {
                MyCarts = ShoppingCart.GetCart(this.HttpContext).GetCartItems(),
                BillingAddress=GetAddresses().FirstOrDefault(a=>a.Id==checkoutModel.Step1.BillingAddressId),
                ShippingAddress = GetAddresses().FirstOrDefault(a => a.Id == checkoutModel.Step2.ShippingAddressId),
                CreditCardInfo=checkoutModel.Step3
            };
            return View(vm);
        }
        [Authorize]
        [HttpPost]
     
        public ActionResult PlaceOrder()
        {
            CheckoutModel checkoutModel = GetCheckoutModel();
            ShoppingCart  cart = ShoppingCart.GetCart(this.HttpContext);
            Order order = new Order();
            if (checkoutModel.Step1 == null)
            {
                return RedirectToAction("CheckoutStep1");
            }
            if (checkoutModel.Step2 == null)
            {
                return RedirectToAction("CheckoutStep2");
            }
            if (checkoutModel.Step3 == null)
            {
                return RedirectToAction("CheckoutStep3");
            }
            order.UserName = User.Identity.Name;
            order.OrderDate = DateTime.Now.Date;
            order.OrderNumber = CreateOrderNumber();
            order.OrderStatus = "Received";
            order.CustomerBillingAddress = dbContext.CustomerAddresses.FirstOrDefault(a => a.Id == checkoutModel.Step1.BillingAddressId);
            order.CustomerShippingAddress = dbContext.CustomerAddresses.FirstOrDefault(a => a.Id == checkoutModel.Step2.ShippingAddressId);
            dbContext.Orders.Add(order);
            foreach (var cartItem in cart.GetCartItems())
            {
                if (!CheckStock(cartItem.ProductId, cartItem.Count))
                {
                    TempData["message"] = "Requested quantity not available for Item: " + cartItem.Product.ProductName;
                    return RedirectToAction("Index");
                }
            }
            try
            {
                dbContext.SaveChanges();
            }
            catch
            {
                 return View("error", new string[] { "Database Error!" });
            }
            Order orderCreated = dbContext.Orders.FirstOrDefault(o => o.OrderNumber == order.OrderNumber);

               
            PaymentInfo paymentInfo = new PaymentInfo() {
                Order=orderCreated,
                    
                Name=checkoutModel.Step3.Name,
                CreditCardNumber=checkoutModel.Step3.CreditCardNumber,
                ExpiredDate=checkoutModel.Step3.ExpiredDate,
                SecurityCode=checkoutModel.Step3.SecurityCode
            };
            dbContext.PaymentInfoes.Add(paymentInfo);
            var cartItems = cart.GetCartItems();


            foreach (var cartItem in cartItems)
            {

                OrderItem orderItem = new OrderItem()
                {
                    ProductId = cartItem.ProductId,
                    OrderId = orderCreated.Id,
                    Quantities = cartItem.Count
                };

                Product product = dbContext.Products.FirstOrDefault(p => p.Id == cartItem.ProductId);
                product.Stock -= cartItem.Count;
                dbContext.Entry(product).State = EntityState.Modified;
                dbContext.OrderItems.Add(orderItem);
                   
            }
            try
            {
                dbContext.SaveChanges();
            }

           catch
            {
                
                dbContext.DeleteOrder(orderCreated.Id);
                return View("error", new string[] { "Database Error!" });
            }
            cart.EmptyCart();
                
            Session.Remove("Checkout");
            try
            {
                SendConfirmationEmail(orderCreated);
            }
            catch
            {
                return View("error", new string[] { "Sorry, we have problem to send confirmation email to you, please log into My Account to Track your order." });
            }
            return RedirectToAction("OrderConfirmation", new { id = orderCreated.Id });
            
         
            
            
        }

        public ActionResult CheckoutAsGuest()
        {
            if (IsCartEmpty())
            {
                return View("CartEmpty");
            }
            var cart = ShoppingCart.GetCart(this.HttpContext);
            foreach (var cartItem in cart.GetCartItems())
            {
                if (!CheckStock(cartItem.ProductId, cartItem.Count))
                {
                    TempData["message"] = "Requested quantity not available for Item: " + cartItem.Product.ProductName;
                    return RedirectToAction("Index");
                }
            }
            ViewBag.Cart = cart;
            return View(new CheckoutAsGuest());
        }

        [HttpPost]
        public ActionResult CheckoutAsGuest(CheckoutAsGuest checkoutModel)
        {
            return View();
        }
 
        public ActionResult OrderConfirmation()
        {
            return View();
        }

        private void SendConfirmationEmail(Order order)
        {
            var callbackUrl = Url.Action("OrderHistory", "Manage", new{area=""}, protocol: Request.Url.Scheme);
            string body = "Thank you for your order! you can view order details by click <a href=\"" + callbackUrl + "\">here</a>";
            string subject = "Order Confirmation: " + order.OrderNumber;
            string recipient = User.Identity.Name;
            string mailFrom = "mvc-001-team-4-2@outlook.com";
            string mailPass = "P@$$w0rdwaspec";
            var mailService = new OutlookMail(mailFrom, mailPass);
            mailService.SendMail(recipient, subject, body);
        }
        private CheckoutModel GetCheckoutModel()
        {
            if (Session["Checkout"]==null)
            {
                CheckoutModel checkout = new CheckoutModel();
                Session["Checkout"] = checkout;
            }
            return (CheckoutModel)Session["Checkout"];
        }
        private List<CustomerAddress> GetAddresses()
        {
            return dbContext.CustomerAddresses.Where(a => a.UserName == User.Identity.Name && a.Status!="Deleted").ToList();
        }

        private string CreateOrderNumber()
        {
            int lastId = dbContext.Orders.OrderByDescending(o => o.Id).First().Id;
            return "CT" + DateTime.Now.ToString("yyyyMMdd") + (lastId + 1);
        }

    }
}