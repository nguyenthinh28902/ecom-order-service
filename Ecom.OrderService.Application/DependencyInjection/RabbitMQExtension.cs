using Ecom.OrderService.Application.Interface;
using Ecom.OrderService.Application.Service;
using Ecom.OrderService.Core.Models.Auth;
using Ecom.OrderService.Core.Models.Connection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Application.DependencyInjection
{
    public static class RabbitMQExtension
    {
        public static IServiceCollection AddRabbitMQExtension(this IServiceCollection services, IConfiguration configuration)
        {
            var rabbitSettings = configuration
        .GetSection(nameof(RabbitMQSettings))
        .Get<RabbitMQSettings>()
        ?? throw new InvalidOperationException("RabbitMQSettings missing");

            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    // 1. Chỉ comment dòng quan trọng: Cấu hình Host kèm Port (đổi sang ushort)
                    cfg.Host(rabbitSettings.Host, (ushort)rabbitSettings.Port, "/", h =>
                    {
                        h.Username(rabbitSettings.UserName);
                        h.Password(rabbitSettings.Password);
                    });
                });
            });
            return services;
        }
    }
}
