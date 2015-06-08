using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ChinaTea.Entities;

namespace ChinaTea.Models
{
    public class ShoppingCartViewModel
    {
        public List<Cart> CartItems { get; set; }
        public decimal CartTotal { get; set; }
    }

    public class EditCartItem
    {
        public int RecordId { get; set; }
        public int quantity { get; set; }
    }

    public class EditCartModel
    {
        public ICollection<EditCartItem> EditCartItems { get; set; }
        public int[] ItemsToBeDeleted { get; set; }
    }
}