using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ChinaTea.Entities;

namespace ChinaTea.Models
{
    public class ProductsViewModel
    {
        public IEnumerable<Product> Products { get; set; }
        public Dictionary<int,int> ProductsCountByCategory { get; set; }
        public Dictionary<int,int> ProductscountByPriceRange { get; set; }
        public IEnumerable<ProductCategory> Categories { get; set; }
        public int[] SelectedCategories { get; set; }
        public int[] SelectedPriceFilters { get; set; }
        public int Orderby { get; set; }
        public PageInfo PageInfo { get; set; }
    }
}