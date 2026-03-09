using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ecom.OrderService.Core.Models.Cms.OrderMangerRequests
{
    public class OrderDetailRequest
    {
        [Required(ErrorMessage = "Order id không được bỏ trống")]
        public int Id { get; set; }
    }
}
