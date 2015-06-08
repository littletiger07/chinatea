using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ChinaTea.Entities
{
    [MetadataTypeAttribute(typeof(CustomerAddress.CustomerAddressMetadata))]
    public partial class CustomerAddress
    {
        internal sealed class CustomerAddressMetadata
        {
            [Required]
            public int UserName { get; set; }
            [Required]
            [Display(Name="First Name")]
            public string FirstName { get; set; }
            [Required]
            [Display(Name="Last Name")]
            public string LastName { get; set; }
            [Required]
            [Display(Name="First Line")]
            public string AddressFirstLine { get; set; }
            [Display(Name="Second Line")]
            public string AddressSecondLine { get; set; }
            [Required]
            [Display(Name="City")]
            public string AddressCity { get; set; }
            [Required]
            [Display(Name="State")]
            public string AddressState { get; set; }
            [Required]
            [Display(Name="Country")]
            public string AddressCountry { get; set; }
            [Required]
            [Display(Name="Zip Code")]
            public string AddressZipCode { get; set; }
        }
    }
}