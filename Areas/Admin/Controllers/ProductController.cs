using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using ChinaTea.Models;
using ChinaTea.Entities;
using System.IO;
using ChinaTea.Areas.Admin.Models;

namespace ChinaTea.Areas.Admin.Controllers
{
    [Authorize(Roles = "Administrators")]
    public class ProductController : Controller
    {
        private ChinaTeaEntities dbContext = new ChinaTeaEntities();
        // GET: Admin/Product
        public ActionResult Index(string searchtext)
        {
            IEnumerable<Product> products = dbContext.Products;
            if(searchtext!=null && searchtext!=string.Empty)
            {
                products = products.Where(p => p.ProductName.ToLower().Contains(searchtext.ToLower()));
            }
            return View(products);
        }

        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Product product,HttpPostedFileBase picThumbnail, HttpPostedFileBase picDetail)
        {

            string picThumbailPath = Path.Combine(Server.MapPath("~/img"), product.PicPathThumbnail);
            string picDetailPath = Path.Combine(Server.MapPath("~/img"), product.PicPathDeatil);
            if (System.IO.File.Exists(picThumbailPath))
            {
                ModelState.AddModelError("", "File " + product.PicPathThumbnail + " already exists.");
            }
            if (System.IO.File.Exists(picDetailPath))
            {
                ModelState.AddModelError("", "File " + product.PicPathDeatil + " already exists.");
            }
            if (ModelState.IsValid)
            {
                
                
                try
                {
                    if (picThumbnail !=null && picThumbnail.ContentLength>0)
                    {
                        picThumbnail.SaveAs(picThumbailPath);
                        
                    }
                    if (picDetail != null && picDetail.ContentLength > 0)
                    {
                        picDetail.SaveAs(picDetailPath);

                    }
                    product.PicPathThumbnail = "/img/" + product.PicPathThumbnail;
                    product.PicPathDeatil = "/img/" + product.PicPathDeatil;
                    dbContext.Products.Add(product);
                    dbContext.SaveChanges();
                    TempData["message"] = "Successfully Added a product.";
                    return RedirectToAction("Index");

                }
                catch
                {
                    ModelState.AddModelError("", "Something is wrong, try again");
                }
            }
            return View(product);
        }
        [HttpGet]
        public ActionResult EditProduct(string id)
        {
            int productId;
            try
            {
                productId = Int32.Parse(id);
            }
            catch
            {
                TempData["Message"] = "Invalid product Id";
                return RedirectToAction("Index");
            }
            Product product = dbContext.Products.FirstOrDefault(p => p.Id == productId);
            ViewBag.a = product;
            return View(product);
        }

        [HttpPost]
        public ActionResult EditProduct(Product updatedProduct)
        {
           

            if (ModelState.IsValid)
            {
                Product product = dbContext.Products.FirstOrDefault(p => p.Id == updatedProduct.Id);
                if (product==null)
                {
                    TempData["Message"] = "Invalid Product Id";
                    return RedirectToAction("Index");
                }
                product.CatalogNumber = updatedProduct.CatalogNumber;
                product.ProductName = updatedProduct.ProductName;
                product.ProductCategoryId = updatedProduct.ProductCategoryId;
                product.ProductDescription = updatedProduct.ProductDescription;
                product.ProductPackageSize = updatedProduct.ProductPackageSize;
                product.ProductUnitPrice = updatedProduct.ProductUnitPrice;
                product.PackageUnit = updatedProduct.PackageUnit;
                product.Stock = updatedProduct.Stock;
                this.dbContext.Entry(product).State = EntityState.Modified;
                


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
            return View(updatedProduct);
        }

        public ActionResult ShowDetails(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("index");
            }
            Product product = dbContext.Products.FirstOrDefault(p => p.Id == id.Value);
            if (product == null)
            {
                return RedirectToAction("index");
            }
            return View(product);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult ConfirmDeleteProduct(int id)
        {
            Product product = dbContext.Products.FirstOrDefault(p => p.Id == id);
            if (!(product == null))
            {
                dbContext.Entry(product).State = EntityState.Deleted;
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

        [HttpGet]
        public ActionResult UpdateProductPicture(int id)
        {
            Product product = dbContext.Products.FirstOrDefault(p=>p.Id==id);
            return View(new UpdateProductPicture() { 
                Id=product.Id,
                ProductName=product.ProductName,
                PicPathThumbnail=product.PicPathThumbnail.Replace("/img/",""),
                PicPathDeatil=product.PicPathDeatil.Replace("/img/","")
            });
        }
        [HttpPost]
        public ActionResult UpdateProductPicture(UpdateProductPicture updatedProductPic, HttpPostedFileBase picThumbnail, HttpPostedFileBase picDetail)
        {
            Product product = dbContext.Products.FirstOrDefault(p=>p.Id==updatedProductPic.Id);
            if (product==null)
            {
                TempData["Message"] = "Invalid Entry";
                return RedirectToAction("Index");
            }
            if (picThumbnail != null && picThumbnail.ContentLength > 0)
            {
                string picThumbnailPath = Path.Combine(Server.MapPath("~/img"), updatedProductPic.PicPathThumbnail);
                try
                {
                    picThumbnail.SaveAs(picThumbnailPath);
                    product.PicPathThumbnail="/img/"+updatedProductPic.PicPathThumbnail;
                }
                catch
                {
                    ModelState.AddModelError("", "Fail to save file: " + updatedProductPic.PicPathThumbnail);
                }
            }
            if (picDetail!=null && picDetail.ContentLength>0)
            {
                string picDetailPath = Path.Combine(Server.MapPath("~/img"), updatedProductPic.PicPathDeatil);
                try
                {
                    picDetail.SaveAs(picDetailPath);
                    product.PicPathDeatil = "/img/" + updatedProductPic.PicPathDeatil;
                }
                catch
                {
                    ModelState.AddModelError("", "Fail to save file: " + updatedProductPic.PicPathDeatil);
                }
            }
            if (ModelState.IsValid)
            {
                dbContext.SaveChanges();
                TempData["Message"] = "Successfully updated product pictures";
                return RedirectToAction("Index");

            }
            
           
           
            return View(updatedProductPic);
            
        }
     

    

 

    }

}