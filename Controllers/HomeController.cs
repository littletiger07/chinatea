using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using ChinaTea.Entities;
using ChinaTea.Models;
using System.Text.RegularExpressions;

namespace ChinaTea.Controllers
{


    public class HomeController : Controller
    {
        ChinaTeaEntities dbContext = new ChinaTeaEntities();
        public ActionResult Index()
        {            
            //Random rnd=new Random();
            //IEnumerable<Product> products = dbContext.Products.ToList().OrderBy(p=>rnd.Next()).Take(8);
            return View("Home");
        }

        public ActionResult Home()
        {
            ViewBag.currentCategory = 0;
            return View();
        }
        public ActionResult ProductDetails(int? id)
        {
            
            if(!id.HasValue)
            {
                return new HttpNotFoundResult("Invalid Product Id");
            }
            Product product = dbContext.Products.FirstOrDefault(p => p.Id == id.Value);
            if(product==null)
            {
                return new HttpNotFoundResult("Product Not Found!");
            }
            
            string returnUrl;
           
         
            if (Request.UrlReferrer!=null && Request.UrlReferrer.ToString().Length>0)
            {
                returnUrl = Request.UrlReferrer.ToString();
            }
            else
            {
                returnUrl = Url.Action("ShowProducts");
            }
           
         
         
            ViewBag.returnUrl = returnUrl;
            return View(product);
        }

       public ActionResult ShowProducts(string searchtext=null, int[] categories = null, int[] priceFilter = null, int orderby = 0,int page=1)
        {
            IEnumerable<Product> products = dbContext.Products;
            
            
            PageInfo pageInfo = new PageInfo() { PageSize = 9 };
            if(searchtext!=null && searchtext!=string.Empty)
            {
                ViewBag.searchtext = searchtext;
                products = products.Where(p => p.ProductName.ToLower().Contains(searchtext.ToLower()) 
                    || p.ProductCategory.ProductCategoryName.ToLower().Contains(searchtext.Trim().ToLower()) );
            }
            Dictionary<int, int> productsCountByCatetory = GetProductsCountByCategory(products);
            if (categories != null)
            {
                products = products.Where(p => (categories.Contains(p.ProductCategory.Id)));
            }
            Dictionary<int, int> productsCountByPriceRange = GetProductsCountByPriceRange(products);
            if (priceFilter != null)
            {
                for (int i = 0; i <= 3; i++)
                {
                    if (!Array.Exists(priceFilter, p => p == i))
                    {
                        products = products.Except(GetProductByPrice(products, i));
                    }
                }
            }
            if (orderby == 0)
            {
                products = products.OrderBy(p => p.CatalogNumber);
            }
           
            if (orderby == 1)
            {
                products = products.OrderBy(p => p.ProductName);
            }
            if (orderby == 2)
            {
                products = products.OrderBy(p => p.ProductUnitPrice);
            }
            if (orderby == 3)
            {
                products = products.OrderByDescending(p => p.ProductUnitPrice);
            }
            pageInfo.PageCount = (products.Count()-1)/pageInfo.PageSize +1;
            pageInfo.CurrentPage = page<=pageInfo.PageCount?page:1;
            if (Request.ServerVariables["QUERY_STRING"] == string.Empty)
            {
                pageInfo.Url = Request.Url + "?page=";
            }
            else
            {
                Match m=Regex.Match(Request.Url.ToString(), @"page=\d+");
                if (m.Success)
                {
                    pageInfo.Url = Regex.Replace(Request.Url.ToString(), @"page=\d+", "page=");
                }
                else
                {
                    pageInfo.Url = Request.Url.ToString() + "&page=";
                }
            }
            int count = products.Count();
            products = products.Skip((pageInfo.CurrentPage - 1) * pageInfo.PageSize).Take(pageInfo.PageSize);
            
            return View(new ProductsViewModel()
            {
                Products = products,
                Categories = dbContext.ProductCategories.ToList(),
                ProductsCountByCategory=productsCountByCatetory,
                ProductscountByPriceRange=productsCountByPriceRange,
                SelectedCategories = categories ?? (new int[]{}),
                SelectedPriceFilters = priceFilter ?? (new int[] { }),
                Orderby = orderby,
                PageInfo=pageInfo

            });
        } 

        private Dictionary<int,int> GetProductsCountByCategory(IEnumerable<Product> products)
       {
           IEnumerable<ProductCategory> categories = dbContext.ProductCategories;
           Dictionary<int, int> productsCountByCategory = new Dictionary<int, int>();
           foreach (var category in categories)
           {
               productsCountByCategory.Add(category.Id, products.Count(p => p.ProductCategory.Id == category.Id));
           }
           return productsCountByCategory;
       }

       private Dictionary<int,int> GetProductsCountByPriceRange(IEnumerable<Product> products)
        {
            return new Dictionary<int, int>()
            {
                {0,products.Count(p=>p.ProductUnitPrice<=50)},
                {1,products.Count(p=>p.ProductUnitPrice>50 && p.ProductUnitPrice<=100)},
                {2,products.Count(p=>p.ProductUnitPrice>100 && p.ProductUnitPrice<=200)},
                {3,products.Count(p=>p.ProductUnitPrice>200)}
            };

      

          
        }
        private IEnumerable<Product> GetProductByPrice(IEnumerable<Product> products, int i)
        {
            switch (i)
            {
                case 0:
                    return products.Where(p => p.ProductUnitPrice <= 50);
                    break;
                case 1:
                    return products.Where(p => p.ProductUnitPrice > 50 && p.ProductUnitPrice <= 100);
                    break;
                case 2:
                    return products.Where(p => p.ProductUnitPrice > 100 && p.ProductUnitPrice <= 200);
                    break;
                case 3:
                    return products.Where(p => p.ProductUnitPrice > 200);
                    break;
                default:
                    return products;
                    break;
            }
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult Category(string id)
        {
            int categoryId;
            try
            {
                categoryId = Int32.Parse(id);
            }
            catch{
                TempData["Message"]="Invalid Product Category";
                return RedirectToAction("home","home");
            }
            ProductCategory category = dbContext.ProductCategories.FirstOrDefault(c => c.Id == categoryId);
            ViewBag.currentCategory = category.Id;
            return View(category);
        }
    }
}