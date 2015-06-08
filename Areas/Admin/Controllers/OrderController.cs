using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ChinaTea.Entities;
using ChinaTea.Areas.Admin.Models;
using System.Data.Entity;

namespace ChinaTea.Areas.Admin.Controllers
{
    [Authorize(Roles = "Administrators")]
    public class OrderController : Controller
    {
        private ChinaTeaEntities dbContext = new ChinaTeaEntities();
        // GET: Admin/Order
        public ActionResult Index(string searchtext)
        {
            IEnumerable<Order> orders = dbContext.Orders;
            if (searchtext!=null && searchtext!=string.Empty)
            {
                orders = orders.Where(o => o.OrderNumber.ToLower().Contains(searchtext.ToLower()));
            }
            return View(orders);
        }  
        public ActionResult ShowOrderDetail(string id)
        {
            int orderId;
            try
            {
                orderId = Int32.Parse(id);
            }
            catch
            {
                TempData["Message"] = "Invalid Order Id!";
                return RedirectToAction("Index");
            }
            Order order = dbContext.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order==null)
            {
                TempData["Message"] = "Order Not Found";
                return RedirectToAction("Index");
            }
            return View("OrderDetail",order);
        }

        public ActionResult EditOrderInfo(string id)
        {
            int orderId;
            try
            {
                orderId = Int32.Parse(id);
            }
            catch
            {
                TempData["Message"] = "Invalid Order Id!";
                return RedirectToAction("Index");
            }
            Order order = dbContext.Orders.FirstOrDefault(o => o.Id == orderId);
            return View(order);
        }
        [HttpPost]
        public ActionResult EditOrderInfo(OrderInfo orderInfo)
        {
            Order order = dbContext.Orders.FirstOrDefault(o => o.Id == orderInfo.Id);
            if(ModelState.IsValid)
            {
                order.UserName = orderInfo.UserName;
                order.OrderNumber = orderInfo.OrderNumber;
                order.OrderDate = orderInfo.OrderDate;
                order.OrderStatus = orderInfo.OrderStatus;
                order.InvoiceNumber = orderInfo.InvoiceNumber;
                dbContext.Entry(order).State = EntityState.Modified;
                dbContext.SaveChanges();
                return RedirectToAction("ShowOrderDetail", new { controller = "order", area = "admin", id = orderInfo.Id });
            }
            return View(order);
        }

        public ActionResult EditBillingAddress(string id)
        {
            int orderId;
            try
            {
                orderId = Int32.Parse(id);
            }
            catch
            {
                TempData["Message"] = "Invalid Order Id!";
                return RedirectToAction("Index");
            }
            Order order = dbContext.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order==null)
            {
                TempData["Message"] = "Order Not Found!";
                return RedirectToAction("Index");
                    
            }
            IEnumerable < CustomerAddress > addresses= dbContext.CustomerAddresses.Where(a => a.UserName == order.UserName);
            return View(new EditAddressesViewModel() { 
            Order=order,
            Addresses=addresses
            });
        }

        [HttpPost]
        public ActionResult EditBillingAddress(int orderId,int billingAddressId)
        {
            Order order = dbContext.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order.CustomerBillingAddressId!=billingAddressId)
            {
                order.CustomerBillingAddressId = billingAddressId;
                dbContext.Entry(order).State = EntityState.Modified;
                dbContext.SaveChanges();
                TempData["Message"] = "Billing Address Updated!";
            }
            
            return RedirectToAction("ShowOrderDetail", new{controller="order", area="admin", id=orderId});
        }

        public ActionResult EditShippingAddress(string id)
        {
            int orderId;
            try
            {
                orderId = Int32.Parse(id);
            }
            catch
            {
                TempData["Message"] = "Invalid Order Id!";
                return RedirectToAction("Index");
            }
            Order order = dbContext.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order == null)
            {
                TempData["Message"] = "Order Not Found!";
                return RedirectToAction("Index");

            }
            IEnumerable<CustomerAddress> addresses = dbContext.CustomerAddresses.Where(a => a.UserName == order.UserName);
            return View(new EditAddressesViewModel()
            {
                Order = order,
                Addresses = addresses
            });
        }

        [HttpPost]
        public ActionResult EditShippingAddress(int orderId, int shippingAddressId)
        {
            Order order = dbContext.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order.CustomerShippingAddressId != shippingAddressId)
            {
                order.CustomerShippingAddressId = shippingAddressId;
                dbContext.Entry(order).State = EntityState.Modified;
                dbContext.SaveChanges();
                TempData["Message"] = "Shipping Address Updated!";
            }

            return RedirectToAction("ShowOrderDetail", new { controller = "order", area = "admin", id = orderId });
        }
        public ActionResult EditPaymentInfo(string id)
        {
            int orderId;
            try
            {
                orderId = Int32.Parse(id);
            }
            catch
            {
                TempData["Message"] = "Invalid Order Id!";
                return RedirectToAction("Index");
            }
            Order order = dbContext.Orders.FirstOrDefault(o => o.Id == orderId);
            PaymentInfo paymentInfo = order.PaymentInfoes.First();
            return View(paymentInfo);
        }
        [HttpPost]
        public ActionResult EditPaymentInfo(PaymentInfo revisedPaymentInfo)
        {
            Order order = dbContext.Orders.FirstOrDefault(o => o.Id == revisedPaymentInfo.OrderId);
            if (order == null)
            {
                TempData["Message"] = "Invalid Order Id!";
                return RedirectToAction("Index");
            }
            if (ModelState.IsValid)
            {
                dbContext.PaymentInfoes.Attach(revisedPaymentInfo);
                dbContext.Entry(revisedPaymentInfo).State = EntityState.Modified;
                dbContext.SaveChanges();
                TempData["Message"] = "Payment Information Updated";
                return RedirectToAction("ShowOrderDetail", new { controller = "order", area = "admin", id = revisedPaymentInfo.OrderId });
            }
            return View(order.PaymentInfoes.First());
        }

        public ActionResult EditItems(string id)
        {
            int orderId;
            try
            {
                orderId = Int32.Parse(id);
            }
            catch
            {
                TempData["Message"] = "Invalid Order Id!";
                return RedirectToAction("Index");
            }
            Order order = dbContext.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order==null)
            {
                TempData["Message"] = "Order Not Found!";
                return RedirectToAction("Index");
            }
            Dictionary<int, string> products = dbContext.Products.Select(p => new { p.Id, p.ProductName}).AsEnumerable().ToDictionary(k=>k.Id, k=>k.ProductName);
            return View(new EditItemsViewModel() { OrderItems=order.OrderItems, Products=products});
        }
        [HttpPost]
        public ActionResult EditItems(IEnumerable<OrderItem> updateditems)
        {
            if (updateditems==null || updateditems.Count()==0)
            {
                Dictionary<int, string> products = dbContext.Products.Select(p => new { p.Id, p.ProductName }).AsEnumerable().ToDictionary(k => k.Id, k => k.ProductName);
                return View(new EditItemsViewModel() { OrderItems =new List<OrderItem>(), Products = products });
            }
            if (ModelState.IsValid)
            {
                List<OrderItem> itemsToBeAdded = new List<OrderItem>();
                foreach(var item in updateditems)
                {
                    if(itemsToBeAdded.Count()==0 || itemsToBeAdded.FirstOrDefault(i=>i.ProductId==item.ProductId)==null)
                    {
                        itemsToBeAdded.Add(item);
                    }
                    else
                    {
                        itemsToBeAdded.Single(i => i.ProductId == item.ProductId).Quantities += item.Quantities;
                    }
                }
                OrderItem firstItem = itemsToBeAdded[0];
                Order order = dbContext.Orders.FirstOrDefault(o => o.Id == firstItem.OrderId);
                if (UpdateOrderCheckStock(itemsToBeAdded))
                {
                    //dbContext.DeleteOrderItems(itemsToBeAdded[0].OrderId);
                    foreach(var item in order.OrderItems.ToList())
                    {
                        dbContext.Entry(item).State = EntityState.Deleted;
                        dbContext.Products.FirstOrDefault(p => p.Id == item.ProductId).Stock += item.Quantities;
                    }
                    dbContext.SaveChanges();
                    foreach (var item in itemsToBeAdded)
                    {
                        dbContext.OrderItems.Add(item);
                        dbContext.Products.FirstOrDefault(p => p.Id == item.ProductId).Stock -= item.Quantities;
                    }
                    try
                    {
                        dbContext.SaveChanges();
                        TempData["Message"] = "Order Items successfully updated";
                        return RedirectToAction("ShowOrderDetail", new { controller = "order", area = "admin", id = itemsToBeAdded[0].OrderId });
                    }
                    catch
                    {
                        return View("error", new string[] { "Database Error!" });
                    }
                }
            }
            
            return View(new EditItemsViewModel(){ OrderItems=updateditems, Products = dbContext.Products.Select(p => new { p.Id, p.ProductName }).AsEnumerable().ToDictionary(k => k.Id, k => k.ProductName) });
        }

        [HttpPost]
        public ActionResult Cancel(string id)
        {
            int orderId;
            try
            {
                orderId = Int32.Parse(id);
            }
            catch
            {
                TempData["Message"] = "Invalid Order";
                return RedirectToAction("Index");
            }
            Order order = dbContext.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order == null)
            {
                TempData["Message"] = "Order Not Found!";
                return RedirectToAction("Index");
            }
            if (order.OrderStatus!="Received")
            {
                TempData["Message"] = "This order CANNOT be cancelled!";
                return RedirectToAction("Index");

            }
            order.OrderStatus = "Cancelled";
            foreach (var item in order.OrderItems)
            {
                Product product = item.Product;
                product.Stock += item.Quantities;
                dbContext.Products.Attach(product);
                dbContext.Entry(product).State = EntityState.Modified;
            }
            dbContext.SaveChanges();
            TempData["Message"] = "Order successfully cancelled";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            int orderId;
            try
            {
                orderId = Int32.Parse(id);
            }
            catch
            {
                TempData["Message"] = "Invalid Order";
                return RedirectToAction("Index");
            }
            Order order = dbContext.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order == null)
            {
                TempData["Message"] = "Order Not Found!";
                return RedirectToAction("Index");
            }
            if (order.OrderStatus != "Cancelled")
            {
                TempData["Message"] = "Only Cancelled Order can be deleted!";
                return RedirectToAction("Index");

            }
            try
            {
                dbContext.DeleteOrder(orderId);
                TempData["Message"] = "Order Successfully Deleted!";
                return RedirectToAction("Index", new { controller = "order", area = "admin" });
            }
            catch
            {
                return View("Error", new string[] { "Database Error, Please Try Again!" });
            }
           
        }

        private bool UpdateOrderCheckStock(List<OrderItem> itemsToBeAdded)
        {
            bool inStock = true;
            OrderItem firstItem = itemsToBeAdded[0];
            Order order = dbContext.Orders.FirstOrDefault(o => o.Id == firstItem.OrderId);
            foreach(var item in itemsToBeAdded)
            {
                Product product=dbContext.Products.FirstOrDefault(p=>p.Id==item.ProductId);
                if(order.OrderItems.Where(i=>i.ProductId==item.ProductId).Sum(i=>i.Quantities)+product.Stock<item.Quantities)
                {
                    ModelState.AddModelError("", "The Requested Quantity of Product " + product.ProductName + " is NOT Available!");
                    inStock = false;
                }
            }
            return inStock;
        }
    }

 
}