using Ecom.OrderService.Core.Enum;
using Ecom.OrderService.Core.Models;
using Ecom.OrderService.Core.Models.Web.Dtos.Checkout;
using Ecom.OrderService.Core.Models.Web.Dtos.Order;
using Ecom.PaymentService.Grpc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Application.Interface.Web
{
    public interface IOrderWebService
    {
        Task<Result<CheckoutDto>> GetCheckoutDetailsAsync();
        Task<Result<PaymentResultDto>> PlaceOrderAsync(CheckoutRequestDto request);
        Task<Result<bool>> UpdateOrderStatusAsync(UpdateStatusRequest request);
        Task<Result<OrderDto>> GetOrderDetailsAsync(OrderDetailsRequest request);
        Task<Result<List<OrderSummaryDto>>> GetOrderHistoryAsync();
    }
}
