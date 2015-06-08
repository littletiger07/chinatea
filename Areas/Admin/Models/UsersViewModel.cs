using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ChinaTea.Models;
using ChinaTea;

namespace ChinaTea.Areas.Admin.Models
{
    public class UsersViewModel
    {
        public List<ApplicationUser> Users { get; set; }
        public PageInfo PageInfo { get; set; }
    }
}