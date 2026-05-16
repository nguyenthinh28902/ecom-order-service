using Ecom.OrderService.Application.Interface.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Application.Service.Dev
{
    public class CurrentUserServiceDev : ICurrentUserService
    {
        // Trả về ID 1 cố định để ný test logic ghi log/permission cho sướng
        public int UserId => 1;

        public string? Email => "dev.test.local@ecom.com";

        // Cho ný full quyền để không bị chặn bởi EnsurePermission
        public List<string> Roles => new List<string> { "Content" };

        public string? Role => "Content";

        public List<string> Scopes => new List<string> { "product.read", "product.create", "product.update", "product.delete" };

        public int WorkplaceId => 1;

        // Mock thông tin môi trường
        public string? IpAddress => "127.0.0.1";

        public string? UserAgent => "Development-Test-Mode (No-JWT)";

        public string? WorkplaceType => Core.Models.Auth.WorkplaceType.Office.ToString();

        public bool IsAuthenticated => true;
    }
}
