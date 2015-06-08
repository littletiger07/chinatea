using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ChinaTea.Entities
{
    [MetadataTypeAttribute(typeof(Product.ProductMetadata))]
    public partial class Product
    {
        internal sealed class ProductMetadata
        {
            [Required]
            public string CatalogNumber { get; set; }
            [Required]
            public string ProductName { get; set; }
            [Required]
            public int ProductCategoryId { get; set; }
            [Required]
            public decimal ProductPackageSize { get; set; }
            [Required]
            public decimal ProductUnitPrice { get; set; }
            [Required]
            public string ProductDescription { get; set; }
            [Required]
            public decimal Stock { get; set; }
            
            public string PicPathThumbnail { get; set; }
            
            public string PicPathDeatil { get; set; }
            }
    }
}