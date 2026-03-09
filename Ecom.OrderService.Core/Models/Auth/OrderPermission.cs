using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Core.Models.Auth
{
    public class OrderPermission
    {
        public const string OrderRead = "order.read";
        public const string OrderCreate = "order.create";
        public const string OrderUpdate = "order.update";
        public const string OrderDelete = "order.delete";
    }
}
