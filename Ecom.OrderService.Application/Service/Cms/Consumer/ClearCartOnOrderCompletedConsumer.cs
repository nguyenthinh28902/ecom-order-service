using Ecom.Contracts.ProductService;
using Ecom.OrderService.Application.Interface.Cms;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Application.Service.Cms.Consumer
{
    public class ClearCartOnOrderCompletedConsumer : IConsumer<OrderCompletedEvent>
    {
        private readonly ICartManagerService _cartManagerService;
        private readonly ILogger<ClearCartOnOrderCompletedConsumer> _logger;

        public ClearCartOnOrderCompletedConsumer(
            ICartManagerService cartManagerService,
            ILogger<ClearCartOnOrderCompletedConsumer> logger)
        {
            _cartManagerService = cartManagerService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
        {
            var message = context.Message;

            // Chỉ comment dòng quan trọng: Log để trace hành trình tin nhắn từ RabbitMQ
            _logger.LogInformation("Consumer nhận message xóa giỏ hàng: ProductId {PId}, VariantId {VId}",
                message.ProductId, message.ProductVariantId);

            try
            {
                // Gọi sang Service xử lý logic check Product/Variant như ný yêu cầu
                var isSuccess = await _cartManagerService.ClearCartItemsAfterOrderAsync(message.ProductId, message.ProductVariantId);

                if (isSuccess)
                {
                    _logger.LogInformation("Xử lý xóa giỏ hàng thành công cho ProductId {PId}", message.ProductId);
                }
            }
            catch (Exception ex)
            {
                // Chỉ comment dòng quan trọng: Log lỗi để MassTransit kích hoạt cơ chế Retry
                _logger.LogError(ex, "Lỗi khi xử lý xóa giỏ hàng qua Consumer cho Product {PId}", message.ProductId);
                throw;
            }
        }
    }
}
