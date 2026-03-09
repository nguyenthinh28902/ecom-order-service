using Ecom.OrderService.Core.Abstractions.Persistence;
using Ecom.OrderService.Core.Models.Auth;
using Ecom.OrderService.Infrastructure.DbContexts;
using Ecom.OrderService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecom.OrderService.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependencyInjectionInfrastructure(this IServiceCollection services,
         IConfiguration configuration)
        {
            ConnectionStrings.EcomOrderConnectionString = configuration.GetConnectionString("EcomOrderDb") ?? string.Empty;
            // Đăng ký DbContext sử dụng SQL Server
            services.AddDbContext<EcomOrderDbContext>(options =>
                options.UseSqlServer(ConnectionStrings.EcomOrderConnectionString));

            //add kiến trúc repo and UoW
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            return services;
        }
    }
}
