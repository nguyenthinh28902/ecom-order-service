using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Application.Interface.Auth
{
    public interface ICurrentCustomerService
    {
        int Id { get; }
        string? Email { get; }
        string? PhoneNumber { get; }
        string? EmailOrPhone { get; }
        bool IsAuthenticated { get; }
    }
}
