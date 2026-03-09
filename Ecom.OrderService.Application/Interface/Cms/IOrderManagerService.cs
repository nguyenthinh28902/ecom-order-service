using Ecom.OrderService.Core.Models;
using Ecom.OrderService.Core.Models.Cms.Dtos.Order;
using Ecom.OrderService.Core.Models.Cms.OrderMangerRequests;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Application.Interface.Cms
{
    public interface IOrderManagerService
    {
        Task<Result<List<OrderSummaryManagerDto>>> GetOrderHistoryAsync();

        Task<Result<OrderManagerDto>> GetOrderManagerByOrderIdAsync(OrderDetailRequest request);
    }
}
