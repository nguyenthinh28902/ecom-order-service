using Ecom.OrderService.Application.Interface.Cms;
using Ecom.OrderService.Application.Service.Cms;
using Ecom.OrderService.Application.Service.CMS;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Application.DependencyInjection
{
    public static class DependencyInjectionCmsApplication
    {
        public static IServiceCollection AddCmsApplication(this IServiceCollection services)
        {
            services.AddScoped<IOrderManagerService, OrderManagerService>();
            services.AddScoped<ICartManagerService, CartManagerService>();
            return services;
        }
    }
}
