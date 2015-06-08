using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ChinaTea.Entities
{
    [MetadataTypeAttribute(typeof(OrderItem.OrderItemMetadata))]
    public partial class OrderItem
    {
        internal sealed class OrderItemMetadata
        {
            [Required]
            public int ProductId { get; set; }
            [Required]
            public int Quantities { get; set; }
            [Required]
            public int OrderId { get; set; }
        }
    }
}