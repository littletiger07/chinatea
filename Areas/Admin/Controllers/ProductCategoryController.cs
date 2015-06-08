using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ChinaTea.Models;
using ChinaTea.Entities;
using System.Data.Entity;

namespace ChinaTea.Areas.Admin.Controllers
{
    [Authorize(Roles = "Administrators")]
    public class ProductCategoryController : Controller
    {
        private ChinaTeaEntities dbContext = new ChinaTeaEntities();
        // GET: Admin/ProductCategory
        public ActionResult Index(string searchtext)
        {
            IEnumerable<ProductCategory> categories= dbContext.ProductCategories;
            if (searchtext!=null && searchtext!=string.Empty)
            {
                categories = categories.Where(c => c.ProductCategoryName.ToLower().Contains(searchtext.ToLower()));
            }
            return View(categories);
        }

        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(ProductCategory category)
        {
            if (ModelState.IsValid)
            {
                dbContext.ProductCategories.Add(category);
                try
                {
                    dbContext.SaveChanges();
                     return RedirectToAction("Index");
                }
                catch
                {
                    return View("Error", new string[] { "Database Error, something is wrong....." });
                }
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            ProductCategory category = dbContext.ProductCategories.FirstOrDefault(c => c.Id == id);
            if(category.Products!=null && category.Products.Count()>0)
            {
                return View("Error", new string[] { "Only Empty Category can be deleted!" });
            }
            if (!(category==null))
            {
                dbContext.Entry(category).State = EntityState.Deleted;
                dbContext.SaveChanges();
                return RedirectToAction("Index");
            }
            return View("Error", new string[]{"Category Not Found"});
        }

        public ActionResult GenerateCategoryList(int selectedId=-1)
        {
            IEnumerable<ProductCategory> categories = dbContext.ProductCategories;
            ViewBag.selectedId = selectedId;
            return PartialView("_CategoryList",categories);
        }
        public ActionResult EditProductCategory(int id)
        {
            ProductCategory pCategory = dbContext.ProductCategories.FirstOrDefault(p=>p.Id==id);
            return View(pCategory);
        }
        [HttpPost]
        public ActionResult EditProductCategory(ProductCategory pCategory)
        {
            //this.dbContext.ProductCategories.Attach(pCategory);
            //this.dbContext.Entry(pCategory).State = System.Data.Entity.EntityState.Modified;
            //dbContext.SaveChanges();
            //return RedirectToAction("index");

            if (ModelState.IsValid)
            {
                this.dbContext.ProductCategories.Attach(pCategory);
                this.dbContext.Entry(pCategory).State = EntityState.Modified;
                try
                {
                    dbContext.SaveChanges();
                    return RedirectToAction("Index");

                }
                catch
                {
                    ModelState.AddModelError("", "Something is wrong, try again");
                }
            }
            return View();
            
        }
    }
}