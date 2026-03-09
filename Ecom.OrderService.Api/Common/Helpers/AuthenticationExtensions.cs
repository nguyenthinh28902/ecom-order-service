using Ecom.OrderService.Common.Helpers;
using Ecom.OrderService.Common.Requirement;
using Ecom.OrderService.Core.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace Ecom.orderService.Common.Helpers
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddAuthenticationExtensions(this IServiceCollection services, IConfiguration configuration)
        {
            var _internalAuth = configuration
                 .GetSection("InternalAuth")
                 .Get<InternalAuthConfig>()
                 ?? throw new InvalidOperationException("JwtSettings missing");
            var _internalAuthWeb = configuration
                 .GetSection("InternalAuthWeb")
                 .Get<InternalAuthConfig>()
                 ?? throw new InvalidOperationException("JwtSettings missing");
            services.AddAuthentication(options =>
            {
                // Sử dụng DefaultAuthenticateScheme chung để Middleware tự động kiểm tra cả hai
                options.DefaultAuthenticateScheme = "Bearer";
                options.DefaultChallengeScheme = "WebScheme";
            }).AddJwtBearer("Bearer", options =>
                {
                    options.Authority = _internalAuth.Issuer;

                    // Chỉ để false khi đang ở môi trường Dev/Local không có SSL thật
                    options.RequireHttpsMetadata = false;

                    options.TokenValidationParameters = new TokenValidationParameters {
                        ValidateIssuer = true,
                        ValidIssuer = _internalAuth.Issuer,
                        ValidateAudience = true,
                        ValidAudience = _internalAuth.Audience,
                        ValidateLifetime = false,
                        ClockSkew = TimeSpan.FromSeconds(20),
                        ValidateIssuerSigningKey = true,
                    };
                }).AddJwtBearer("WebScheme", options => // Scheme cho nguồn Web (localhost:7109)
                {
                    options.Authority = _internalAuthWeb.Issuer;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = _internalAuthWeb.Issuer,
                        ValidateAudience = true,
                        ValidAudience = _internalAuthWeb.Audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(20),
                        ValidateIssuerSigningKey = true,
                    };
                });
            services.AddSingleton<IAuthorizationHandler, InternalOrPermissionHandler>();
            services.AddAuthorization(options =>
            {
                // 1. Quyền Web: Chỉ dành cho WebScheme
                options.AddPolicy(PolicyNames.OrderReadWeb, policy =>
                {
                    policy.AddAuthenticationSchemes("WebScheme");
                    policy.AddRequirements(new InternalOrPermissionRequirement("order.read.web"));
                });
                options.AddPolicy(PolicyNames.OrderWriteWeb, policy =>
                {
                    policy.AddAuthenticationSchemes("WebScheme");
                    policy.AddRequirements(new InternalOrPermissionRequirement("order.write.web"));
                });

                // 2. Các quyền hệ thống: Chỉ dành cho Bearer (Internal)
                options.AddPolicy(PolicyNames.OrderRead, policy =>
                {
                    policy.AddAuthenticationSchemes("Bearer");
                    policy.AddRequirements(new InternalOrPermissionRequirement("order.read"));
                });

                options.AddPolicy(PolicyNames.OrderWrite, policy =>
                {
                    policy.AddAuthenticationSchemes("Bearer");
                    policy.AddRequirements(new InternalOrPermissionRequirement("order.write"));
                });

                // 3. Policy Internal: Bao gồm tất cả các nguồn và tất cả các quyền
                options.AddPolicy(PolicyNames.Internal, policy =>
                {
                    // Cho phép cả 2 Scheme để Admin từ nguồn nào cũng có thể truy cập nếu đủ quyền
                    policy.AddAuthenticationSchemes("Bearer");

                    // Yêu cầu đầy đủ các quyền (bao gồm cả quyền .web như bạn mong muốn)
                    policy.AddRequirements(new InternalOrPermissionRequirement("order.internal"));
                    policy.AddRequirements(new InternalOrPermissionRequirement("order.write"));
                    policy.AddRequirements(new InternalOrPermissionRequirement("order.read"));
                    policy.AddRequirements(new InternalOrPermissionRequirement("order.read.web"));
                    policy.AddRequirements(new InternalOrPermissionRequirement("order.write.web"));
                });
            });
            return services;
        }
    }
}
