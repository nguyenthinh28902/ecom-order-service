using Ecom.OrderService.Application.AutoMappings;
using Ecom.OrderService.Application.Interface.Auth;
using Ecom.OrderService.Application.Service.Auth;
using Ecom.OrderService.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecom.OrderService.Application.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependencyInjectionApplication(this IServiceCollection services,
         IConfiguration configuration)
        {
            services.AddDependencyInjectionInfrastructure(configuration);
            services.AddStackExchangeRedis(configuration);
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<ApplicatinoOrderCmsProfile>();
                cfg.AddProfile<ApplicatinoOrderWebProfile>();
            });
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<ICurrentCustomerService, CurrentCustomerService>();
            services.AddScoped<IBaseService, BaseService>();
            services.AddRabbitMQExtension(configuration);
            services.AddCmsApplication();
            services.AddWebApplication(configuration);
            return services;
        }
    }
}
