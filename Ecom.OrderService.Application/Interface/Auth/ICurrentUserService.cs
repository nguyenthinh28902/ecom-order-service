using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Application.Interface.Auth
{
    public interface ICurrentUserService
    {
        int UserId { get; }
        string? Email { get; }
        string? Role { get; }
        int WorkplaceId { get; }
        string? WorkplaceType { get; }
        List<string> Scopes { get; }
        List<string> Roles { get; }
        bool IsAuthenticated { get; }
    }
}
