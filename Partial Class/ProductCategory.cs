using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ChinaTea.Entities
{
    [MetadataTypeAttribute(typeof(ProductCategory.ProductCategoryMetadata))]
    public partial class ProductCategory
    {
        internal sealed class ProductCategoryMetadata
        {
            [Required]
            public string ProductCategoryName { get; set; }
            [Required]
            public string ProductCategoryDescription { get; set; }
        }
    }
}