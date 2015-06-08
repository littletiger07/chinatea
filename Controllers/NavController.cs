using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using ChinaTea.Models;
using System.Web.Security;
using ChinaTea.Entities;

namespace ChinaTea.Controllers
{
    public class NavController : Controller
    {
        private ChinaTeaEntities dbContext = new ChinaTeaEntities();
       
        public ActionResult GenerateTopNavBar()
        {
          
            IEnumerable<ProductCategory> categories = dbContext.ProductCategories;
        
            
            return PartialView("_TopNavBar", categories);
        }

        public ActionResult MyAccountNavBar(string currentTab=null)
        {
            ViewBag.currentTab = currentTab;
            return PartialView("_MyAccountNavBar");
        }

        public ActionResult LoginPartial()
        {
            ApplicationUserManager userManager= HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            ApplicationUser user = userManager.FindByName(HttpContext.User.Identity.GetUserName());
            
            return PartialView(user);
        }

        public ActionResult ProductCategoryNavBar(int currentCategory=0)
        {
            IEnumerable<ProductCategory> categories = dbContext.ProductCategories;
            ViewBag.currentCategory = currentCategory;
            return PartialView(categories);
        }
    }
}