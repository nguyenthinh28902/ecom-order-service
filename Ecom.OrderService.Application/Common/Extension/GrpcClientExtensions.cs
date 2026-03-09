using Ecom.OrderService.Application.Interface.Auth;
using Ecom.OrderService.Application.Service.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Application.Common.Extension
{
    public static class GrpcClientExtensions
    {
        public static IHttpClientBuilder AddCommonCallCredentials(this IHttpClientBuilder builder, IConfiguration configuration)
        {
            return builder.AddCallCredentials(async (context, metadata, serviceProvider) =>
            {
                var currentCustomer = serviceProvider.GetRequiredService<ICurrentCustomerService>();
                var currentUserService = serviceProvider.GetRequiredService<ICurrentUserService>();

                // 1. Chỉ comment dòng quan trọng: Tự động đính kèm thông tin user nếu đã login
                if (currentUserService.IsAuthenticated || currentCustomer.IsAuthenticated)
                {

                    if (!currentUserService.IsAuthenticated)
                    {
                        metadata.Add("X-User-Id", currentCustomer.Id.ToString());
                        if (!string.IsNullOrEmpty(currentCustomer.Email))
                            metadata.Add("X-User-Email", currentCustomer.Email);
                        if (!string.IsNullOrEmpty(currentCustomer.PhoneNumber))
                            metadata.Add("X-User-Phone", currentCustomer.PhoneNumber);
                    }
                    else
                    {
                        metadata.Add("X-User-Id", currentUserService.UserId.ToString());
                        metadata.Add("X-User-WorkplaceId", currentUserService.WorkplaceId.ToString());
                        if (!string.IsNullOrEmpty(currentUserService.Email))
                        metadata.Add("X-User-Email", currentUserService.Email.ToString());
                        if (!string.IsNullOrEmpty(currentUserService.WorkplaceType))
                        metadata.Add("X-User-WorkplaceType", currentUserService.WorkplaceType.ToString());
                        if (currentUserService.Roles.Count() > 0)
                        {
                            var rolesString = string.Join(",", currentUserService.Roles);
                            metadata.Add("X-User-Roles", rolesString);
                        }
                        if(!string.IsNullOrEmpty(currentUserService.WorkplaceType)) metadata.Add("X-User-WorkplaceType", currentUserService.WorkplaceType.ToString());
                        if (currentUserService.Scopes.Count() > 0)
                        {
                            var scopesString = string.Join(",", currentUserService.Scopes);
                            metadata.Add("X-User-Scopes", scopesString);
                        }
                    }
                }
                // 2. Chỉ comment dòng quan trọng: Đính kèm API Key nội bộ lấy từ file cấu hình
                var internalKey = configuration["InternalGrpcApiKey"] ?? string.Empty;
                metadata.Add("x-internal-key", internalKey);

                await Task.CompletedTask;
            });
        }
    }
}
