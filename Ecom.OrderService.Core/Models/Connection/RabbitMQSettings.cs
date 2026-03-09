using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Core.Models.Connection
{
    //"Host": "localhost",
    //"Port": 5672,
    //"UserName": "guest",
    //"Password": "guest"
    public class RabbitMQSettings
    {
        public string Host { get; set; } = null!;
        public int Port { get; set; }
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
