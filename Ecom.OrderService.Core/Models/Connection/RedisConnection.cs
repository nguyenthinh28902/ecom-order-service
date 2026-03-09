using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Core.Models.Connection
{
    public class RedisConnection
    {
        public string RedisConnectionString { get; set; } = string.Empty;
        public string InstanceName { get; set; } = string.Empty;
    }
}
