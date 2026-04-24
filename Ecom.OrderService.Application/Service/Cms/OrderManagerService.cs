using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ecom.OrderService.Application.Interface.Auth;
using Ecom.OrderService.Application.Interface.Cms;
using Ecom.OrderService.Core.Abstractions.Persistence;
using Ecom.OrderService.Core.Entities;
using Ecom.OrderService.Core.Models;
using Ecom.OrderService.Core.Models.Auth;
using Ecom.OrderService.Core.Models.Cms.Dtos.Order;
using Ecom.OrderService.Core.Models.Cms.OrderMangerRequests;
using Ecom.OrderService.Core.Models.Dto.Cms;
using Ecom.PaymentService.Grpc;
using Ecom.Shared.Grpc;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Application.Service.CMS
{
    public class OrderManagerService : IOrderManagerService
    {
        private readonly ProductGrpc.ProductGrpcClient _productGrpcClient;
        private readonly PaymentGrpc.PaymentGrpcClient _paymentGrpcClient;
        private readonly ILogger<OrderManagerService> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IBaseService _baseService;

        public OrderManagerService(ILogger<OrderManagerService> logger, ICurrentUserService currentUserService, IUnitOfWork unitOfWork,
            ProductGrpc.ProductGrpcClient productGrpcClient,
            IMapper mapper,
            PaymentGrpc.PaymentGrpcClient paymentGrpcClient,
            IBaseService baseService)
        {
            _logger = logger;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
            _productGrpcClient = productGrpcClient;
            _mapper = mapper;
            _paymentGrpcClient = paymentGrpcClient;
            _baseService = baseService;
        }
        public async Task<Result<List<OrderSummaryManagerDto>>> GetOrderHistoryAsync()
        {
            _baseService.EnsurePermission(OrderPermission.OrderRead);
            try
            {
                // 1. Chỉ comment dòng quan trọng: Lấy ID từ dịch vụ context (giả sử là Id hoặc CustomerId tùy base của ông)
                var userId = _currentUserService.UserId;
                var workerId = _currentUserService.WorkplaceId; // Nếu có phân biệt worker và customer, tùy vào base của ông
                var workplaceType = _currentUserService.WorkplaceType;
                // 2. Chỉ comment dòng quan trọng: Query kèm Include và ProjectTo thẳng sang bản rút gọn (Summary)
                var query = _unitOfWork.Repository<Order>()
                    .Entities.AsNoTracking();
                if (workplaceType != WorkplaceType.Office.ToString())
                {
                    query = query.Where(o => o.WorkplaceId == workerId);
                }
                if (!_currentUserService.Roles.Contains(DepartmentCode.Manager.ToString()) && !_currentUserService.Roles.Contains(DepartmentCode.Accountant.ToString()))
                {
                    query = query.Where(o => o.UserId == userId);
                }

                var orders = await query
                    .OrderBy(x => x.CreatedAt)
                    .Include(o => o.OrderItems)
                    .ProjectTo<OrderSummaryManagerDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();
               
              
                return Result<List<OrderSummaryManagerDto>>.Success(orders, "Lấy lịch sử đơn hàng thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy lịch sử đơn hàng của khách hàng");
                return Result<List<OrderSummaryManagerDto>>.Failure("Không thể lấy lịch sử đơn hàng lúc này.");
            }
        }
        public async Task<Result<OrderManagerDto>> GetOrderManagerByOrderIdAsync(OrderDetailRequest request)
        {
            _baseService.EnsurePermission(OrderPermission.OrderRead);
            var userId = _currentUserService.UserId;
            var workerId = _currentUserService.WorkplaceId; // Nếu có phân biệt worker và customer, tùy vào base của ông
            var workplaceType = _currentUserService.WorkplaceType;
            // 2. Chỉ comment dòng quan trọng: Query kèm Include và ProjectTo thẳng sang bản rút gọn (Summary)
            var query = _unitOfWork.Repository<Order>()
                .Entities.AsNoTracking().Where(x => x.Id == request.Id);
            if (workplaceType != WorkplaceType.Office.ToString())
            {
                query = query.Where(o => o.WorkplaceId == workerId);
            }
            if (!_currentUserService.Roles.Contains(DepartmentCode.Manager.ToString()) && !_currentUserService.Roles.Contains(DepartmentCode.Accountant.ToString()))
            {
                query = query.Where(o => o.UserId == userId);
            }

            var order = await query
                .Include(o => o.OrderItems)
                .ProjectTo<OrderManagerDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
            if(order == null) return Result<OrderManagerDto>.Failure("Không tìm thấy đơn hàng");
            var requestGrpc = new OrderTransactionGrpcRequest { OrderId = request.Id };
            var transactionInfo = await _paymentGrpcClient.GetTransactionByOrderIdManagerAsync(requestGrpc);
            order.TransactionInfo = _mapper.Map<TransactionManagerDto>(transactionInfo);
            return Result<OrderManagerDto>.Success(order, "Lấy thông tin đơn hàng thành công.");
        }
    }
}
