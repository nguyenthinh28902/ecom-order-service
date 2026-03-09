using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Core.Models.Auth
{
    public static class PolicyNames
    {
        public const string OrderRead = "OrderReadPolicy";
        public const string OrderReadWeb = "OrderReadWebPolicy";
        public const string OrderWriteWeb = "OrderWriteWebPolicy";
        public const string OrderWrite = "OrderWritePolicy";
        public const string Internal = "InternalPolicy";
    }
}
