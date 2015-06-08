using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ChinaTea.Entities
{
    [MetadataTypeAttribute(typeof(Order.OrderMetadata))]
    public partial class Order
    {
        internal sealed class OrderMetadata
        {
            [Required]
            public string UserName { get; set; }
            [Required]
            [DisplayFormat(DataFormatString="{0:MM/dd/yyyy}", ApplyFormatInEditMode=true)]
            public string OrderDate { get; set; }
            [Required]
            public string OrderStatus { get; set; }
            public string InvoiceNumber { get; set; }
            [Required]
            public int CustomerBillingAddressId { get; set; }
            [Required]
            public int CustomerShippingAddressId { get; set; }
        }

    }
}