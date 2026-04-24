using AutoMapper;
using AutoMapper.QueryableExtensions;
using Azure;
using Ecom.Contracts.Requests;
using Ecom.OrderService.Application.Interface.Auth;
using Ecom.OrderService.Application.Interface.Web;
using Ecom.OrderService.Core.Abstractions.Persistence;
using Ecom.OrderService.Core.Entities;
using Ecom.OrderService.Core.Enum;
using Ecom.OrderService.Core.Exceptions;
using Ecom.OrderService.Core.Models;
using Ecom.OrderService.Core.Models.Web.Dtos.Checkout;
using Ecom.OrderService.Core.Models.Web.Dtos.Order;
using Ecom.PaymentService.Grpc;
using Ecom.Shared.Grpc;
using Grpc.Core;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Application.Service.Web
{
    public class OrderWebService : IOrderWebService
    {
        private readonly ProductGrpc.ProductGrpcClient _productGrpcClient;
        private readonly PaymentGrpc.PaymentGrpcClient _paymentGrpcClient;
        private readonly ILogger<OrderWebService> _logger;
        private readonly ICurrentCustomerService _currentCustomerService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;
        public OrderWebService(ILogger<OrderWebService> logger, ICurrentCustomerService currentCustomerService, IUnitOfWork unitOfWork,
            ProductGrpc.ProductGrpcClient productGrpcClient,
            IMapper mapper,
            PaymentGrpc.PaymentGrpcClient paymentGrpcClient,
            IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _currentCustomerService = currentCustomerService;
            _unitOfWork = unitOfWork;
            _productGrpcClient = productGrpcClient;
            _mapper = mapper;
            _paymentGrpcClient = paymentGrpcClient;
            _publishEndpoint = publishEndpoint;
        }
        public async Task<Result<CheckoutDto>> GetCheckoutDetailsAsync()
        {
            var customerId = _currentCustomerService.Id;
            _logger.LogInformation("Bắt đầu lấy chi tiết checkout cho Customer: {CustomerId}", customerId);

            try
            {
                // 1. Lấy dữ liệu cơ bản từ Cart
                var checkoutDto = await _unitOfWork.Repository<Cart>()
                    .Entities.AsNoTracking() // Thêm AsNoTracking vì đây là tác vụ Read
                    .Where(x => x.CustomerId == customerId)
                    .ProjectTo<CheckoutDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();

                if (checkoutDto == null || !checkoutDto.Items.Any())
                {
                    return Result<CheckoutDto>.Failure("Giỏ hàng của ný đang trống.");
                }

                // 2. Gọi gRPC với cơ chế bọc lỗi
                var productData = await CallProductGrpcWithRetry(checkoutDto.Items);

                if (productData == null)
                {
                    // Thay vì throw Exception, ta trả về lỗi nghiệp vụ để UI hiển thị thông báo nhẹ nhàng
                    return Result<CheckoutDto>.Failure("Hệ thống kiểm tra giá đang bảo trì, ný vui lòng thử lại sau giây lát!");
                }

                // 3. Enrich dữ liệu (Sử dụng Dictionary để tối ưu O(n) thay vì vòng lặp lồng O(n^2))
                var productDict = productData.Products.ToDictionary(p => p.VariantId, p => p);

                foreach (var item in checkoutDto.Items)
                {
                    if (productDict.TryGetValue(item.VariantId, out var pInfo))
                    {
                        item.ProductName = pInfo.Name;
                        item.VariantName = pInfo.VariantName;
                        item.ImageUrl = pInfo.ImageUrl;
                        item.Sku = pInfo.Sku;
                        item.UnitPrice = (decimal)pInfo.Price;
                        item.IsAvailable = pInfo.IsAvailable;
                        item.TotalLine = item.UnitPrice * item.Quantity;
                    }
                    else
                    {
                        item.IsAvailable = false; // Đánh dấu không khả dụng nếu gRPC không trả về thông tin
                        _logger.LogWarning("Sản phẩm VariantId {Id} không tìm thấy thông tin mới", item.VariantId);
                    }
                }

                // 4. Tính toán tổng tiền
                checkoutDto.SubTotal = checkoutDto.Items.Where(x => x.IsAvailable).Sum(x => x.TotalLine);
                checkoutDto.TotalAmount = checkoutDto.SubTotal + checkoutDto.ShippingFee;

                return Result<CheckoutDto>.Success(checkoutDto, "");
            }
            catch (RpcException rpcEx)
            {
                _logger.LogError(rpcEx, "Lỗi gRPC khi checkout cho Customer {Id}", customerId);
                return Result<CheckoutDto>.Failure("Không thể kết nối với kho hàng, ný đợi xíu nhé.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi không xác định trong tiến trình Checkout của {Id}", customerId);
                return Result<CheckoutDto>.Failure("Đã xảy ra lỗi trong quá trình chuẩn bị đơn hàng.");
            }
        }
        public async Task<Result<PaymentResultDto>> PlaceOrderAsync(CheckoutRequestDto request)
        {
            // 1. Chỉ comment dòng quan trọng: Lấy lại thông tin Checkout để tính toán giá thực tế từ Server
            var checkoutDataResult = await GetCheckoutDetailsAsync();
            if (checkoutDataResult == null || !checkoutDataResult.IsSuccess || checkoutDataResult.Data == null) return Result<PaymentResultDto>.Failure(checkoutDataResult?.Noti ?? "Lỗi không thể thực hiện đặt hàng ngay lúc này");

            var checkoutDto = checkoutDataResult.Data;
            var customerId = _currentCustomerService.Id;

            // Chỉ comment dòng quan trọng: Mở Transaction để đảm bảo lưu Order và xóa Cart đồng bộ
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 2. Chỉ comment dòng quan trọng: Khởi tạo thực thể Order với các thông tin mặc định
                var order = new Order
                {
                    OrderCode = $"ORD-{DateTime.Now.Ticks}", // Tạo mã đơn hàng duy nhất
                    CustomerId = customerId,
                    UserId = null,
                    WorkplaceId = 2, // Mặc định theo yêu cầu
                    SubTotal = checkoutDto.SubTotal,
                    ShippingFee = checkoutDto.ShippingFee,
                    TotalAmount = checkoutDto.TotalAmount,
                    Currency = "VNĐ",
                    FullName = request.FullName,
                    PhoneNumber = request.PhoneNumber,
                    ShippingAddress = request.ShippingAddress,
                    Status = (byte)OrderStatus.Pending, // Dùng Enum vừa tạo
                    CreatedAt = DateTime.Now
                };



                // 3. Chỉ comment dòng quan trọng: Chuyển đổi từ CheckoutItem sang OrderItem để lưu DB
                var orderItems = new List<OrderItem>();
                foreach (var item in checkoutDto.Items)
                {
                    // 2. Chỉ comment dòng quan trọng: Khai báo các loại giảm giá (Sau này tính toán dựa trên logic Voucher/Promotion)
                    decimal promotionDiscount = 0;
                    decimal memberDiscount = 0;
                    decimal couponDiscount = 0;
                    

                    // 3. Chỉ comment dòng quan trọng: Tính giá chốt (UnitPrice) khách phải trả cho 1 sản phẩm
                    decimal unitPrice = item.UnitPrice - (promotionDiscount + memberDiscount + couponDiscount);

                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        VariantId = item.VariantId,
                        ProductName = item.ProductName,
                        ProductMainImage = item.ImageUrl,
                        Sku = item.Sku,

                        // Phân tách các loại giá theo DB mới
                        BasePrice = item.UnitPrice,       // Giá gốc/niêm yết lúc chưa giảm
                        CurrentPrice = item.UnitPrice,    // Giá đang hiển thị trên web

                        PromotionDiscount = promotionDiscount,
                        MemberDiscount = memberDiscount,
                        CouponDiscount = couponDiscount,

                        UnitPrice = unitPrice,            // Giá thực tế khách trả
                        Quantity = item.Quantity

                        // TotalLineAmount không gán ở đây vì là cột Computed trong SQL Server
                    };

                    orderItems.Add(orderItem);
                }
                
                order.OrderItems = orderItems;
                await _unitOfWork.Repository<Order>().AddAsync(order);
           
                // 4. Chỉ comment dòng quan trọng: Xóa giỏ hàng của khách sau khi đặt hàng thành công
                var cart = await _unitOfWork.Repository<Cart>()
                    .Entities.Where(x => x.CustomerId == customerId)
                    .Include(x => x.CartItems)
                    .FirstOrDefaultAsync();

                if (cart != null)
                {
                    _unitOfWork.Repository<CartItem>().RemoveRange(cart.CartItems); // Xóa luôn Cart hoặc chỉ xóa CartItems tùy logic của ông
                }

                await _unitOfWork.CommitAsync();
                //tạo thanh toán
                var paymentGrpcRequest = new PaymentGrpcRequest();
                paymentGrpcRequest.Amount = (double)order.TotalAmount;
                paymentGrpcRequest.Currency = order.Currency;
                paymentGrpcRequest.OrderId = order.Id;
                paymentGrpcRequest.OrderCode = order.OrderCode;
               
                paymentGrpcRequest.Description = $"Thanh toán cho đơn hàng {order.OrderCode}";
                paymentGrpcRequest.PaymentMethodCode = request.PaymentMethodCode;
                var paymentResult = await _paymentGrpcClient.ProcessPaymentAsync(paymentGrpcRequest);
                var paymentSuccess = paymentResult.IsSuccess;
                _logger.LogInformation("Đơn hàng {OrderCode} đã được tạo thành công.", order.OrderCode);
                var resultDto = _mapper.Map<PaymentResultDto>(paymentResult);
                resultDto.OrderId = order.Id;
                if (resultDto.IsSuccess && !string.IsNullOrEmpty(_currentCustomerService.Email))
                {
                    
                    var notificationEvent = new NotificationRequestDto
                    {
                        ReceiverId = customerId,
                        ReceiverRole = "CUSTOMER",
                        ReceiverEmail = _currentCustomerService.Email,
                        TypeCode = "ORDER_SUCCESS_CUSTOMER",
                        Channel = "EMAIL",
                        Message = $"Thông báo đơn hàng {order.OrderCode}",
                        Parameters = new Dictionary<string, string>
                        {
                            { "customer_name", order.FullName },
                            { "order_code", order.OrderCode },
                            { "total_amount", $"{order.TotalAmount:N0} {order.Currency}" },
                            
                        },
                                Items = order.OrderItems.Select(x => new Dictionary<string, string>
                        {
                            { "product_name", x.ProductName },
                            { "quantity", x.Quantity.ToString() },
                            { "sub_total", $"{(x.UnitPrice * x.Quantity):N0} {order.Currency}" }
                        }).ToList()
                    };

                    await _publishEndpoint.Publish(notificationEvent);
                    _logger.LogInformation("Đã đẩy thông báo đơn hàng {OrderCode} vào RabbitMQ", order.OrderCode);
                }
                return Result<PaymentResultDto>.Success(resultDto, "Đặt hàng thành công.");
            }
            catch (Exception ex)
            {
                
                _logger.LogError(ex, "Lỗi xảy ra khi PlaceOrder cho Customer: {CustomerId}", customerId);
                return Result<PaymentResultDto>.Failure("Có lỗi xảy ra trong quá trình xử lý đơn hàng.");
            }
        }


        public async Task<Result<bool>> UpdateOrderStatusAsync(UpdateStatusRequest request)
        {
            // 1. Chỉ comment dòng quan trọng: Bắt đầu Transaction để cập nhật trạng thái và ghi log lịch sử đồng bộ
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var orderRepo = _unitOfWork.Repository<Order>();

                // 2. Chỉ comment dòng quan trọng: Tìm đơn hàng bằng ID, nếu không thấy thì trả về lỗi ngay
                var order = await orderRepo.Entities.Where(x => x.Id == request.Id).FirstOrDefaultAsync();
                if (order == null) return Result<bool>.Failure("Không tìm thấy đơn hàng.");

                // Kiểm tra logic nghiệp vụ (Ví dụ: Đã Hoàn thành thì không cho Hủy)
                if (order.Status == (byte)OrderStatus.Completed && request.Status == OrderStatus.Cancelled)
                {
                    return Result<bool>.Failure("Đơn hàng đã hoàn thành, không thể hủy.");
                }

                var oldStatus = order.Status;

                // 3. Chỉ comment dòng quan trọng: Cập nhật giá trị byte từ Enum vào cột Status của đơn hàng
                order.Status = (byte)request.Status;
                order.UpdatedAt = DateTime.Now;

                // 4. Chỉ comment dòng quan trọng: Nếu hệ thống có bảng Log, ghi lại vết thay đổi để đối soát sau này
                // (Giả sử ông đã tạo bảng OrderStatusLogs hoặc dùng bảng TransactionLogs tôi đưa lúc nãy)

                var statusLog = new OrderStatusLog
                {
                    OrderId = request.Id,
                    OldStatus = oldStatus,
                    NewStatus = (byte)request.Status,
                    Note =  request.Note,
                    CreatedAt = DateTime.Now
                };
                await _unitOfWork.Repository<OrderStatusLog>().AddAsync(statusLog);
                await _unitOfWork.CommitAsync();



                return Result<bool>.Success(true, "Cập nhật trạng thái thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái đơn hàng {OrderId}", request.Id);
                return Result<bool>.Failure("Lỗi hệ thống khi cập nhật trạng thái.");
            }
        }

        public async Task<Result<OrderDto>> GetOrderDetailsAsync(OrderDetailsRequest request)
        {
            try
            {
                // 1. Chỉ comment dòng quan trọng: Lấy thông tin đơn hàng và item từ DB của Order Service
                var order = await _unitOfWork.Repository<Order>()
                    .Entities.Where(x => x.OrderCode.ToLower() == request.OrderCode.ToLower())
                    .ProjectTo<OrderDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();

                if (order == null) return Result<OrderDto>.Failure("Không tìm thấy đơn hàng.");

                // Map tên trạng thái đơn hàng
                order.StatusName = ((OrderStatus)order.Status!).ToString();

                // 2. Chỉ comment dòng quan trọng: Gọi gRPC sang Payment Service để lấy thông tin giao dịch (Transaction)
                try
                {
                    var paymentResult = await _paymentGrpcClient.GetTransactionByOrderIdAsync(new GetTransactionRequest
                    {
                        OrderId = order.Id
                    });

                    if (paymentResult != null && paymentResult.IsSuccess)
                    {
                        // 2. Chỉ comment dòng quan trọng: Map thủ công hoặc dùng AutoMapper từ gRPC sang Model C#
                        order.TransactionInfo = new TransactionInfoDto
                        {
                            Id = paymentResult.Id,
                            OrderId = paymentResult.OrderId,
                            OrderCode = paymentResult.OrderCode,
                            Amount = (decimal)paymentResult.Amount,
                            Currency = paymentResult.Currency,
                            PaymentMethodName = paymentResult.PaymentMethodName,
                            ExternalTransactionId = paymentResult.ExternalTransactionId,
                            StatusName = paymentResult.StatusName
                        };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Không thể lấy thông tin thanh toán qua gRPC cho OrderId: {OrderId}", order.Id);
                    // Vẫn trả về Order, thông tin Transaction sẽ bị null
                }

                return Result<OrderDto>.Success(order, "Thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi lấy chi tiết đơn hàng: {OrderId}", request.OrderCode);
                return Result<OrderDto>.Failure("Lỗi hệ thống khi lấy đơn hàng.");
            }
        }

        public async Task<Result<List<OrderSummaryDto>>> GetOrderHistoryAsync()
        {
            try
            {
                // 1. Chỉ comment dòng quan trọng: Lấy ID từ dịch vụ context (giả sử là Id hoặc CustomerId tùy base của ông)
                var customerId = _currentCustomerService.Id;

                // 2. Chỉ comment dòng quan trọng: Query kèm Include và ProjectTo thẳng sang bản rút gọn (Summary)
                var orders = await _unitOfWork.Repository<Order>()
                    .Entities.Where(x => x.CustomerId == customerId)
                    .Include(x => x.OrderItems) // Load sản phẩm để map vào list summary
                    .OrderByDescending(x => x.CreatedAt)
                    .ProjectTo<OrderSummaryDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                return Result<List<OrderSummaryDto>>.Success(orders, "Lấy lịch sử đơn hàng thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy lịch sử đơn hàng của khách hàng");
                return Result<List<OrderSummaryDto>>.Failure("Không thể lấy lịch sử đơn hàng lúc này.");
            }
        }

        private async Task<ProductResponse?> CallProductGrpcWithRetry(List<CheckoutItemDto> items)
        {
            var grpcRequest = new ProductRequest();
            grpcRequest.Items.AddRange(items.Select(i => new ProductItem { Id = i.ProductId, VariantId = i.VariantId }));

            try
            {
                // Chỉ comment dòng quan trọng: Ép gRPC phải trả lời trong vòng 5 giây, quá hạn là ngắt
                var deadline = DateTime.UtcNow.AddSeconds(5);
                return await _productGrpcClient.GetProductCheckoutDetailsAsync(grpcRequest, deadline: deadline);
            }
            catch (Exception ex)
            {
                _logger.LogError("Gọi gRPC thất bại: {Msg}", ex.Message);
                return null;
            }
        }
    }
}
