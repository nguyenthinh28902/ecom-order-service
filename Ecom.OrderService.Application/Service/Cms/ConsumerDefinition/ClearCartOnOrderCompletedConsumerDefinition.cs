using Ecom.OrderService.Application.Service.Cms.Consumer;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Application.Service.Cms.ConsumerDefinition
{
    public class ClearCartOnOrderCompletedConsumerDefinition : ConsumerDefinition<ClearCartOnOrderCompletedConsumer>
    {
        public ClearCartOnOrderCompletedConsumerDefinition()
        {
            // Chỉ comment dòng quan trọng: Đặt tên Queue hiển thị trên Dashboard RabbitMQ cho dễ quản lý
            EndpointName = "order-completed-clear-cart-queue";
        }

        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<ClearCartOnOrderCompletedConsumer> consumerConfigurator, IRegistrationContext context)
        {
            // --- Config MassTransit start ----
            // Cấu hình mặc định đã được MassTransit cung cấp.
            //ghi xuống ổ đĩa để đảm bảo rằng khi RabbitMQ restart thì Queue vẫn tồn tại và không bị mất dữ liệu
            if (endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rabbit)
            {
                // 3. Đảm bảo Queue không bị mất khi RabbitMQ restart
                rabbit.Durable = true;

                // Đảm bảo Queue tồn tại kể cả khi Service ClearCartOnOrderCompletedConsumer đang offline
                rabbit.AutoDelete = false;
            }
            // --- Config MassTransit end ----

            endpointConfigurator.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
            // nếu sau 3 lần retry mà vẫn lỗi thì sẽ chuyển sang hàng đợi redelivery để xử lý lại sau
            endpointConfigurator.UseScheduledRedelivery(r => r.Intervals(
                TimeSpan.FromMinutes(5),
                TimeSpan.FromMinutes(15),
                TimeSpan.FromMinutes(30))
            );
        }
    }
}
