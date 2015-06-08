using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ChinaTea.Entities;

namespace ChinaTea.Models
{
    
    public class CheckoutViewModelStep1
    {
        public List<CustomerAddress> Addresses { set; get; } 
        public CheckoutModelStep1 Step1 { get; set; }
    }
    public class CheckoutModelStep1
    {
        [Required]
        public int BillingAddressId { get; set; }
    }

    public class CheckoutViewModelStep2
    {
        public List<CustomerAddress> Addresses { get; set; }
        public CheckoutModelStep2 Step2{ get; set; }
    }
    public class CheckoutModelStep2
    {
        [Required]
        public int ShippingAddressId { get; set; }
    }

    public class CheckoutViewModelStep3
    {
        [Required]
        
        public string Name { set; get; }
        [Required]
        public string CreditCardNumber { set; get; }
        [Required]
        public string ExpiredDate { set; get; }
        [Required]
        public string SecurityCode { set; get; }

    }
    public class CheckoutModel
    {

        public CheckoutModelStep1 Step1 { get; set; }
        public CheckoutModelStep2 Step2 { get; set; }
        public CheckoutViewModelStep3 Step3 { get; set; }
    }

    public class ReviewOrderViewModel
    {
        public List<Cart> MyCarts { get; set; }
        public CustomerAddress BillingAddress { get; set; }
        public CustomerAddress ShippingAddress { get; set; }
        public CheckoutViewModelStep3 CreditCardInfo { get; set; }
    }

    public class CheckoutAsGuest
    {
        public CheckoutAsGuest()
        {
            this.BillingAddress = new CustomerAddress();
            this.ShippingAddress = new CustomerAddress();
            this.Payment = new PaymentInfo();
        }
        public string UserEmail { get; set; }
        public CustomerAddress BillingAddress { get; set; }
        public string UseBillingAddress { get; set; }
        public CustomerAddress ShippingAddress { get; set; }
        public PaymentInfo Payment { get; set; }
    }
}