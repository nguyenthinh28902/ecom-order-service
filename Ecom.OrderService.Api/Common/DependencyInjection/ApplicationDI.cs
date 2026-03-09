using Ecom.OrderService.Application.DependencyInjection;

namespace Ecom.OrderService.Common.DependencyInjection
{
    public static class ApplicationDI
    {
        public static IServiceCollection AddApplicationDI(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDependencyInjectionApplication(configuration);
            return services;
        }
    }
}
