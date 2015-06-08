using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ChinaTea.Entities;

namespace ChinaTea.Areas.Admin.Models
{
    public class OrderModel
    {
    }

    public class OrderInfo
    {
        public int Id { get; set; }
        [Required]
        public string  UserName { get; set; }
        [Required]
        public string OrderNumber { get; set; }
        [Required]
        [DisplayFormat(DataFormatString="{0:MM/dd/yyyy}",ApplyFormatInEditMode=true)]
        public DateTime OrderDate { get; set; }
        [Required]
        public string OrderStatus { get; set; }
        public string InvoiceNumber { get; set; }
    }

    public class EditAddressesViewModel
    {
        public Order Order { get; set; }
        public IEnumerable<CustomerAddress> Addresses { get; set; }
    }

    public class EditItemsViewModel
    {
        public Dictionary<int,string> Products { get; set; }
        public IEnumerable<OrderItem> OrderItems { get; set; }
    }
}