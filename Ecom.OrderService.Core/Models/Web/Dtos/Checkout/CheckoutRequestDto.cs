using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Core.Models.Web.Dtos.Checkout
{
    public class CheckoutRequestDto
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string ShippingAddress { get; set; }
        public string PaymentMethodCode { get; set; }
        public string Note { get; set; }
    }
}
