using Ecom.OrderService.Common.Requirement;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Ecom.OrderService.Common.Helpers
{
    public class InternalOrPermissionHandler : AuthorizationHandler<InternalOrPermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, InternalOrPermissionRequirement requirement)
        {
            // Kiểm tra nếu Token có claim 'client_id' nhưng KHÔNG có 'sub' (User ID)
            // Đây là dấu hiệu của Client Credentials Flow (System-to-System)
            var isSystemToken = context.User.HasClaim(c => c.Type == "client_id")
                        && !context.User.HasClaim(c => c.Type == "sub");
            // Kiểm tra quyền cụ thể được truyền vào policy (ví dụ: user.internal)
            var hasPermission = context.User.HasClaim(c => c.Value == requirement.RequiredPermission);

            if (isSystemToken || hasPermission)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
