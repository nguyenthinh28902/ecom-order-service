using Ecom.OrderService.Application.Interface.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Application.Service.Dev
{
    public class CurrentCustomerServiceDev : ICurrentCustomerService
    {
        // Trả về ID cố định (ví dụ: 101) để ný test logic liên quan đến CustomerId
        public int Id => 3;

        // Trả về email hoặc số điện thoại mock
        public string? EmailOrPhone => "customer.test.local@gmail.com";

        public string? Email => "customer.test.local@gmail.com";

        public string? PhoneNumber => "0901234567";

        // Trong môi trường Dev, mặc định coi như đã xác thực thành công
        public bool IsAuthenticated => true;
    }
}
