using Ecom.OrderService.Application.Common.Extension;
using Ecom.OrderService.Application.Interface.Auth;
using Ecom.OrderService.Application.Interface.Web;
using Ecom.OrderService.Application.Service.Web;
using Ecom.PaymentService.Grpc;
using Ecom.Shared.Grpc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Application.DependencyInjection
{
    public static class DependencyInjectionWebApplication
    {
        public static IServiceCollection AddWebApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ICartWebService, CartWebService>();
            services.AddScoped<IOrderWebService, OrderWebService>();

            var productUrl = configuration["ServiceUrl:ProductServiceUrl"] ?? string.Empty;
            var paymentUrl = configuration["ServiceUrl:PaymentServiceUrl"] ?? string.Empty;

            // Đăng ký gRPC Client
            services.AddGrpcClient<ProductGrpc.ProductGrpcClient>(o => o.Address = new Uri(productUrl))
         .AddCommonCallCredentials(configuration);

            // Đăng ký Payment Service Client
            services.AddGrpcClient<PaymentGrpc.PaymentGrpcClient>(o => o.Address = new Uri(paymentUrl))
                    .AddCommonCallCredentials(configuration);
            return services;
        }
    }
}
