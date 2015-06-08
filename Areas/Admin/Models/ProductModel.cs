using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ChinaTea.Areas.Admin.Models
{
    public class ProductModel
    {
    }
    public class UpdateProductPicture
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string ProductName { get; set; }
        public string PicPathThumbnail { get; set; }
        public string PicPathDeatil { get; set; }
    }
}