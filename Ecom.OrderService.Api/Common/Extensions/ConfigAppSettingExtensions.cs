

using Ecom.OrderService.Core.Models.Auth;
using Ecom.OrderService.Core.Models.Connection;

namespace Ecom.OrderService.Common.Extensions
{
    public static class ConfigAppSettingExtensions
    {
        public static IServiceCollection AddConfigAppSettingExtensions(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RedisConnection>(configuration.GetSection("RedisConnection"));
            return services;
        }
    }
}
