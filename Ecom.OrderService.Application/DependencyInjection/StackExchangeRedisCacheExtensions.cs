using Ecom.OrderService.Application.Interface;
using Ecom.OrderService.Application.Service;
using Ecom.OrderService.Core.Models.Connection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecom.OrderService.Application.DependencyInjection
{
    public static class StackExchangeRedisCacheExtensions
    {
        public static IServiceCollection AddStackExchangeRedis(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Mapping từ Section "RedisConnection" trong appsettings vào Model
            var redisSettings = configuration.GetSection(nameof(RedisConnection)).Get<RedisConnection>()
                ?? throw new InvalidOperationException("RedisConfig configuration is missing.");

            // 2. Đăng ký StackExchangeRedis với thông số từ Model
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisSettings.RedisConnectionString;
                options.InstanceName = redisSettings.InstanceName;
            });

            // Đăng ký Service xử lý cache user (như đã bàn ở bước trước)
            services.AddScoped<ICacheService, CacheService>();
            return services;
        }
    }
}
