using Ecom.OrderService.Core.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Core.Models.Web.Dtos.Checkout
{
    public class UpdateStatusRequest
    {
        public int Id { get; set; }
        public OrderStatus Status { get; set; }
        public string Note { get; set; }
    }
}
