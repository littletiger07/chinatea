using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using ChinaTea.Models;
using ChinaTea.Entities;
using System.Collections.Generic;
using System.Data.Entity;
using ChinaTea.Areas.Admin.Models;

namespace ChinaTea.Controllers
{
    [Authorize]
    
    public class ManageController : Controller
    {
        private ChinaTeaEntities dbContext = new ChinaTeaEntities();
        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            //ViewBag.StatusMessage =
            //    message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
            //    : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
            //    : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
            //    : message == ManageMessageId.Error ? "An error has occurred."
            //    : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
            //    : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
            //    : "";

            //var model = new IndexViewModel
            //{
            //    HasPassword = HasPassword(),
            //    PhoneNumber = await UserManager.GetPhoneNumberAsync(User.Identity.GetUserId()),
            //    TwoFactor = await UserManager.GetTwoFactorEnabledAsync(User.Identity.GetUserId()),
            //    Logins = await UserManager.GetLoginsAsync(User.Identity.GetUserId()),
            //    BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(User.Identity.GetUserId())
            //};
            ApplicationUser user = await UserManager.FindByNameAsync(User.Identity.Name);
            ViewBag.currentTab = "DashBoard";
            return View(user);
        }

        //
        // GET: /Manage/RemoveLogin
        public ActionResult RemoveLogin()
        {
            var linkedAccounts = UserManager.GetLogins(User.Identity.GetUserId());
            ViewBag.ShowRemoveButton = HasPassword() || linkedAccounts.Count > 1;
            return View(linkedAccounts);
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInAsync(user, isPersistent: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        //
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInAsync(user, isPersistent: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to verify phone");
            return View(model);
        }

        //
        // GET: /Manage/RemovePhoneNumber
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        public async Task<ActionResult> EditProfile()
        {
            ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                EditProfileViewModel vm = new EditProfileViewModel()
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email
                };
                ViewBag.currentTab = "EditProfile";
                return View(vm);
            }
            return View("Error", new string[] { "Current User Not Valid" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditProfile(EditUserViewModel userVm)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await UserManager.FindByIdAsync(userVm.Id);
                if (user!=null)
                {
                    user.Email = userVm.Email;
                    user.UserName = userVm.Email;
                    user.FirstName = userVm.FirstName;
                    user.LastName = userVm.LastName;
                    IdentityResult validEmail = await UserManager.UserValidator.ValidateAsync(user);
                    if (!validEmail.Succeeded)
                    {
                        foreach(var error in validEmail.Errors)
                        {
                            ModelState.AddModelError("",error);
                        }
                    }
                    
                    IdentityResult result = await UserManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("index");
                    }
                    else
                    {
                        foreach(var error in result.Errors)
                        {
                            ModelState.AddModelError("",error);
                        }
                    }
                    
                }
                else
                {
                    ModelState.AddModelError("","Current User Not Valid");
                }
            }
            return View(userVm);
        }
        


        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            ViewBag.currentTab = "ChangePassword";
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInAsync(user, isPersistent: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInAsync(user, isPersistent: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }
        public ActionResult ShowAddresses()
        {
            string userName = User.Identity.Name;
            IEnumerable<CustomerAddress> addresses= dbContext.CustomerAddresses.Where(a => a.UserName == userName && a.Status != "Deleted").ToList();
            ViewBag.currentTab = "ShowAddresses";
            return View(addresses);
           
            
        }
        public ActionResult CreateAddress(string returnUrl)
        {
            ViewBag.returnUrl = returnUrl;
            ViewBag.currentTab = "ShowAddresses";
            return View(new CustomerAddress() { UserName=User.Identity.Name,AddressCountry="US"});
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAddress(CustomerAddress address,string returnUrl)
        {
            if (ModelState.IsValid)
            {
                dbContext.CustomerAddresses.Add(address);
                
                try
                {
                    dbContext.SaveChanges();
                    if (returnUrl != null && returnUrl!="")
                        return Redirect(returnUrl);
                    else
                        return RedirectToAction("Index");
                }
                catch
                {
                    return View("error", new string[] { "Database Error!" });
                }
            }
            ViewBag.returnUrl = returnUrl;
            return View(address);
        }

        public ActionResult DeleteAddress(string id)
        {
            int addressId;
            try
            {
                addressId = int.Parse(id);

            }
            catch
            {
                return RedirectToAction("Index");
            }
            CustomerAddress address = dbContext.CustomerAddresses.FirstOrDefault(a => a.Id == addressId);
            if(address.UserName!=User.Identity.Name)
            {
                return View("Error", new string[] { "You are NOT authorized!" });
            }
            if (dbContext.Orders.Select(o => o.CustomerBillingAddressId).Any(i => i == addressId) || dbContext.Orders.Select(o => o.CustomerShippingAddressId).Any(i => i == addressId))
            {
                address.Status = "Deleted";
                dbContext.Entry(address).State = EntityState.Modified;
            }
            else
            {
                dbContext.Entry(address).State = EntityState.Deleted;
            }
            try
            {
                dbContext.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return View("error", new string[] { "Database Error!" });
            }

           
        }
        public ActionResult EditAddress(string id, string returnUrl)
        {
            int addressId;
            try
            {
                addressId = int.Parse(id);

            }
            catch
            {
                return RedirectToAction("Index");
            }
            CustomerAddress address = dbContext.CustomerAddresses.FirstOrDefault(a => a.Id == addressId);
            if(address.UserName!=User.Identity.Name)
            {
                return View("Error", new string[] { "You are NOT authorized!" });
            }
            ViewBag.returnUrl = returnUrl;
            return View(address);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAddress(CustomerAddress address, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if(address.UserName!=User.Identity.Name)
                {
                    return View("Error", new string[] { "You are NOT authorized!" });
                }
                if (dbContext.Orders.Select(o => o.CustomerBillingAddressId).Any(i => i == address.Id) || dbContext.Orders.Select(o => o.CustomerShippingAddressId).Any(i => i == address.Id))
                {
                    dbContext.CustomerAddresses.FirstOrDefault(a => a.Id == address.Id).Status = "Deleted";
                    dbContext.CustomerAddresses.Add(address);
                }
                else
                {
                    dbContext.CustomerAddresses.Attach(address);
                    dbContext.Entry(address).State = EntityState.Modified;
                }
                try
                {
                    dbContext.SaveChanges();
                    if (returnUrl == null || returnUrl == string.Empty)
                    {
                        return RedirectToAction("ShowAddresses");
                    }
                    else
                    {
                        return Redirect(returnUrl);
                    }
                }
                catch
                {
                    return View("error", new string[] { "Database Error!" });
                }
            }
            return View(address);
        }
        [Authorize]
        public ActionResult OrderHistory()
        {
            IEnumerable<Order> orders = dbContext.Orders.Where(o => o.UserName == (User.Identity.Name));
            ViewBag.currentTab = "OrderHistory";
            return View(orders);
        }
        [Authorize]
        public ActionResult OrderDetails(string id)
        {
            int orderId;
            try
            {
                orderId = int.Parse(id);
            }
            catch
            {
                return RedirectToAction("OrderHistory");
            }
            Order order = dbContext.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order==null)
            {
                return RedirectToAction("OrderHistory");
            }
            if (order.UserName != User.Identity.Name)
            {
                return View("Error", new string[] { "You are NOT authorized!" });
            }
            return View(order);
        }
        [HttpGet]
        public ActionResult EditOrderBillingAddress(string id)
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
            if (order.UserName!=User.Identity.Name)
            {
                return View("Error", new string[] { "You are NOT authorized!" });
            }
            if (order.OrderStatus!="Received")
            {
                return View("Error", new string[] { "This order CANNOT be changed!" });
            }
            IEnumerable < CustomerAddress > addresses= dbContext.CustomerAddresses.Where(a => a.UserName == order.UserName);
            return View(new EditAddressesViewModel() { 
            Order=order,
            Addresses=addresses
            });
        }

        [HttpGet]
        public ActionResult EditOrderShippingAddress(string id)
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
            if (order.UserName != User.Identity.Name)
            {
                return View("Error", new string[] { "You are NOT authorized!" });
            }
            if (order.OrderStatus != "Received")
            {
                return View("Error", new string[] { "This order CANNOT be changed!" });
            }
            IEnumerable<CustomerAddress> addresses = dbContext.CustomerAddresses.Where(a => a.UserName == order.UserName);
            return View(new EditAddressesViewModel()
            {
                Order = order,
                Addresses = addresses
            });
        }

        [HttpPost]
        
        public ActionResult EditOrderBillingAddress(int orderId, int billingAddressId)
        {
            Order order = dbContext.Orders.FirstOrDefault(o => o.Id == orderId);
            if(order.UserName!=User.Identity.Name)
            {
                return View("Error", new string[] { "You are NOT authorized!" });
            }
            if(order.OrderStatus!="Received")
            {
                return View("Error", new string[]{"This order CANNOT be changed!"});
            }
            if (order.CustomerBillingAddressId != billingAddressId)
            {
                order.CustomerBillingAddressId = billingAddressId;
                dbContext.Entry(order).State = EntityState.Modified;
                dbContext.SaveChanges();
                TempData["Message"] = "Billing Address Updated!";
            }

            return RedirectToAction("OrderDetails", new { controller = "Manage", area = "", id = orderId });
        }

        [HttpPost]

        public ActionResult EditOrderShippingAddress(int orderId, int shippingAddressId)
        {
            Order order = dbContext.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order.UserName != User.Identity.Name)
            {
                return View("Error", new string[] { "You are NOT authorized!" });
            }
            if (order.OrderStatus != "Received")
            {
                return View("Error", new string[] { "This order CANNOT be changed!" });
            }
            if (order.CustomerShippingAddressId != shippingAddressId)
            {
                order.CustomerShippingAddressId = shippingAddressId;
                dbContext.Entry(order).State = EntityState.Modified;
                dbContext.SaveChanges();
                TempData["Message"] = "Shipping Address Updated!";
            }

            return RedirectToAction("OrderDetails", new { controller = "Manage", area = "", id = orderId });
        }

        public ActionResult EditOrderPaymentInfo(string id)
        {
            int orderId;
            try
            {
                orderId = Int32.Parse(id);
            }
            catch
            {
                TempData["Message"] = "Invalid Order Id!";
                return RedirectToAction("OrderHistory");
            }
            Order order = dbContext.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order == null)
            {
                TempData["Message"] = "Order Not Found!";
                return RedirectToAction("OrderHistory");

            }
            if (order.UserName != User.Identity.Name)
            {
                return View("Error", new string[] { "You are NOT authorized!" });
            }
            if (order.OrderStatus != "Received")
            {
                return View("Error", new string[] { "This order CANNOT be changed!" });
            }
            PaymentInfo paymentInfo = dbContext.PaymentInfoes.FirstOrDefault(p => p.OrderId == orderId);
            return View(paymentInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditOrderPaymentInfo(PaymentInfo revisedPaymentInfo)
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
                return RedirectToAction("OrderDetails", new { controller = "Manage", area = "", id = revisedPaymentInfo.OrderId });
            }
            return View(revisedPaymentInfo);
        }

        public ActionResult EditOrderItems(string id)
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
            if (order.UserName != User.Identity.Name)
            {
                return View("Error", new string[] { "You are NOT authorized!" });
            }
            if (order.OrderStatus != "Received")
            {
                return View("Error", new string[] { "This order CANNOT be changed!" });
            }
            Dictionary<int, string> products = dbContext.Products.Select(p => new { p.Id, p.ProductName }).AsEnumerable().ToDictionary(k => k.Id, k => k.ProductName);
            return View(new EditItemsViewModel() { OrderItems = order.OrderItems, Products = products });
        }
#region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie, DefaultAuthenticationTypes.TwoFactorCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, await user.GenerateUserIdentityAsync(UserManager));
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

#endregion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditOrderItems(IEnumerable<OrderItem> updateditems)
        {
            if (updateditems == null || updateditems.Count() == 0)
            {
                Dictionary<int, string> products = dbContext.Products.Select(p => new { p.Id, p.ProductName }).AsEnumerable().ToDictionary(k => k.Id, k => k.ProductName);
                return View(new EditItemsViewModel() { OrderItems = new List<OrderItem>(), Products = products });
            }
            if (ModelState.IsValid)
            {
                List<OrderItem> itemsToBeAdded = new List<OrderItem>();
                foreach (var item in updateditems)
                {
                    if (itemsToBeAdded.Count() == 0 || itemsToBeAdded.FirstOrDefault(i => i.ProductId == item.ProductId) == null)
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
                    foreach (var item in order.OrderItems.ToList())
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
                        return RedirectToAction("OrderDetails", new { controller = "Manage", area = "", id = itemsToBeAdded[0].OrderId });
                    }
                    catch
                    {
                        return View("error", new string[] { "Database Error!" });
                    }
                }
            }

            return View(new EditItemsViewModel() { OrderItems = updateditems, Products = dbContext.Products.Select(p => new { p.Id, p.ProductName }).AsEnumerable().ToDictionary(k => k.Id, k => k.ProductName) });
        }

        private bool UpdateOrderCheckStock(List<OrderItem> itemsToBeAdded)
        {
            bool inStock = true;
            OrderItem firstItem = itemsToBeAdded[0];
            Order order = dbContext.Orders.FirstOrDefault(o => o.Id == firstItem.OrderId);
            foreach (var item in itemsToBeAdded)
            {
                Product product = dbContext.Products.FirstOrDefault(p => p.Id == item.ProductId);
                if (order.OrderItems.Where(i => i.ProductId == item.ProductId).Sum(i => i.Quantities) + product.Stock < item.Quantities)
                {
                    ModelState.AddModelError("", "The Requested Quantity of Product " + product.ProductName + " is NOT Available!");
                    inStock = false;
                }
            }
            return inStock;
        }

        [HttpPost]
        public ActionResult CancelOrder(string id)
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
            if (order.UserName != User.Identity.Name)
            {
                return View("Error", new string[] { "You are NOT authorized!" });
            }
            if (order.OrderStatus != "Received")
            {
                return View("Error", new string[] { "This order CANNOT be changed!" });
            }
            order.OrderStatus = "Cancelled";
            dbContext.SaveChanges();
            TempData["Message"] = "Order Successfully Cancelled";
            return RedirectToAction("OrderHistory",new{controller="Manage",area=""});
        }
    }
}