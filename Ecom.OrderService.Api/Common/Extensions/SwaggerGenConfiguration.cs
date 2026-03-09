
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Ecom.OrderService.Common.Extensions
{
    public static class SwaggerGenConfiguration
    {
        public static IServiceCollection AddSwaggerGenConfiguration(this IServiceCollection services,
          IConfiguration configuration)
        {

            //thêm cấu hình Swagger Auth (Bearer JWT)
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() {
                    Title = "Customer Identity API",
                    Version = "v1"
                });

                // 🔐 JWT Bearer config
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Nhập token dạng: Bearer {your JWT token}"
                });
                c.OperationFilter<AllowAnonymousOperationFilter>();
            });
            return services;
        }
    }

    public class AllowAnonymousOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasAuthorize =
            context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>().Any() == true
            || context.MethodInfo.GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>().Any();

            var hasAllowAnonymous =
                context.MethodInfo.GetCustomAttributes(true)
                    .OfType<AllowAnonymousAttribute>().Any();

            if (hasAuthorize && !hasAllowAnonymous)
            {
                operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                }
            };
            }
        }
    }
}
