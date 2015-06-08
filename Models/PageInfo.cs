using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChinaTea.Models
{
    public class PageInfo
    {
        public int PageSize { get; set; }
        public int PageCount { get; set; }
        public int CurrentPage { get; set; }
        public string Url { get; set; }
    }
    //public class PagingInfo
    //{
    //    public int TotalItems { get; set; }
    //    public int ItemsPerPage { get; set; }
    //    public int CurrentPage { get; set; }
    //    public int TotalPages { get { return (int)Math.Ceiling((decimal)TotalItems / ItemsPerPage); } }
    //}

}