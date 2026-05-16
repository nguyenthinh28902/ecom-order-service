using Ecom.OrderService.Application.Interface.Auth;
using Ecom.OrderService.Application.Interface.Web;
using Ecom.OrderService.Core.Abstractions.Persistence;
using Ecom.OrderService.Core.Entities;
using Ecom.OrderService.Core.Exceptions;
using Ecom.OrderService.Core.Models;
using Ecom.OrderService.Core.Models.Web.Dtos.Cart;
using Ecom.Shared.Grpc;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ecom.OrderService.Application.Service.Web
{
    public class CartWebService : ICartWebService
    {
        private readonly ProductGrpc.ProductGrpcClient _productGrpcClient;
        private readonly ILogger<CartWebService> _logger;
        private readonly ICurrentCustomerService _currentCustomerService;
        private readonly IUnitOfWork _unitOfWork;
        public CartWebService(ILogger<CartWebService> logger, ICurrentCustomerService currentCustomerService, IUnitOfWork unitOfWork,
            ProductGrpc.ProductGrpcClient productGrpcClient)
        {
            _logger = logger;
            _currentCustomerService = currentCustomerService;
            _unitOfWork = unitOfWork;
            _productGrpcClient = productGrpcClient;
        }


        public async Task<Result<bool>> AddToCartAsync(CreateCartItemRequest request)
        {
            var customerId = _currentCustomerService.Id;
            _logger.LogInformation("Adding product {ProductId} with quantity {Quantity} to cart for customer {CustomerId}",
                request.ProductId, request.Quantity, customerId);

            try
            {
                // 1. Lấy hoặc khởi tạo giỏ hàng cho khách hàng
                var cart = await _unitOfWork.Repository<Cart>()
                    .Entities.Where(x => x.CustomerId == customerId)
                    .Include(x => x.CartItems)
                    .FirstOrDefaultAsync();

                if (cart == null)
                {
                    // Nếu chưa có giỏ hàng thì tạo mới
                    cart = new Cart
                    {
                        CustomerId = customerId,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Repository<Cart>().AddAsync(cart);
                    await _unitOfWork.SaveChangesAsync();
                    if (cart.Id == 0) return Result<bool>.Failure("Có lỗi xảy ra khi thêm vào giỏ hàng");
                    // Lưu để có ID giỏ hàng trước khi thêm item (tùy thuộc vào thiết kế DB)
                    // Hoặc để EF Core tự xử lý quan hệ nếu CartId là FK
                }

                await _unitOfWork.BeginTransactionAsync();
                // 2. Kiểm tra sản phẩm (với variant cụ thể) đã tồn tại trong giỏ chưa
                var existingItem = cart.CartItems
                    .FirstOrDefault(x => x.ProductId == request.ProductId && x.VariantId == request.VariantId);
                // Chỉ comment dòng quan trọng: Log trạng thái giỏ hàng để kiểm soát luồng thêm mới hoặc cập nhật
                _logger.LogInformation("Check cart customer: {CustomerId} | Status: {CartStatus}",
                    customerId,
                    existingItem != null ? "Existing Cart" : "New Cart");
                if (existingItem != null)
                {
                    // Nếu đã có: Cập nhật thêm số lượng
                    existingItem.Quantity += request.Quantity;
                    existingItem.AddedAt = DateTime.UtcNow; // Cập nhật lại thời gian tương tác gần nhất

                    _unitOfWork.Repository<CartItem>().Update(existingItem);
                }
                else
                {
                    // Nếu chưa có: Thêm mới item vào giỏ
                    var newItem = new CartItem
                    {
                        CartId = cart.Id,
                        ProductId = request.ProductId,
                        VariantId = request.VariantId,
                        Quantity = request.Quantity,
                        AddedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Repository<CartItem>().AddAsync(newItem);
                }

                // 3. Cập nhật thời gian thay đổi của giỏ hàng
                cart.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<Cart>().Update(cart);

                // 4. Xác nhận lưu toàn bộ thay đổi xuống Database
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Product {ProductId} added to cart successfully for customer {CustomerId}",
                    request.ProductId, customerId);

                return Result<bool>.Success(true, "Đã thêm sản phẩm vào giỏ hàng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product to cart for customer {CustomerId}", customerId);
                return Result<bool>.Failure("Có lỗi xảy ra khi thêm vào giỏ hàng");
            }
        }
        public async Task<Result<CartDto>> GetCartAsync()
        {
            _logger.LogInformation($"{nameof(GetCartAsync)} start");
            // 1. Lấy UserId của khách hàng hiện tại
            var customerId = _currentCustomerService.Id;

            // 2. Lấy giỏ hàng từ Database nội bộ của Order Service
            var cart = await _unitOfWork.Repository<Cart>()
                .Entities.Where(x => x.CustomerId == customerId)
                .Include(x => x.CartItems)
                .FirstOrDefaultAsync();

            if (cart == null || !cart.CartItems.Any())
                return Result<CartDto>.Failure("Giỏ hàng không tồn tại hoặc trống");

            // 3. Tạo Request gRPC theo cấu trúc mới (repeated ProductItem)
            var grpcRequest = new ProductRequest();

            // Chỉ comment dòng quan trọng: Map danh sách CartItems sang ProductItems của gRPC
            var itemsForGrpc = cart.CartItems.Select(x => new ProductItem
            {
                Id = x.ProductId,
                VariantId = x.VariantId
            });

            // Chỉ comment dòng quan trọng: Dùng AddRange để đổ dữ liệu vào RepeatedField của gRPC
            grpcRequest.Items.AddRange(itemsForGrpc);

            // 4. Gọi gRPC sang Product Service để lấy thông tin hiển thị (Name, Price, Image)
            // var productResponse = await _productGrpcClient.GetProductDisplayInfosAsync(grpcRequest);

            // 1. Tách biệt việc gọi gRPC để kiểm soát lỗi kết nối / lỗi mạng
            ProductResponse grpcResponse;
            try
            {
                _logger.LogInformation("gRPC Request: Sending {Count} product IDs to ProductService", itemsForGrpc.Count());

               grpcResponse = await _productGrpcClient.GetProductDisplayInfosAsync(grpcRequest);

                // Kiểm tra nếu response hoặc danh sách products bị null hệ thống
                if (grpcResponse?.Products == null)
                {
                    _logger.LogWarning("gRPC Response thành công nhưng danh sách Products bị null");
                    return Result<CartDto>.Failure("Dữ liệu sản phẩm trả về không hợp lệ");
                }

                _logger.LogInformation("gRPC Response: Received {Count} product details from ProductService", grpcResponse.Products.Count);
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
            {
                _logger.LogWarning("Không tìm thấy thông tin sản phẩm từ gRPC: {Detail}", ex.Status.Detail);
                throw new NotFoundException("Sản phẩm không còn tồn tại trong hệ thống ný ơi!");
            }
            catch (RpcException ex)
            {
                // Bắt các lỗi gRPC khác (mất kết nối, timeout, unavailable...)
                _logger.LogError(ex, "Lỗi kết nối gRPC đến Product Service. Status: {Status}", ex.Status);
                return Result<CartDto>.Failure("Lỗi kết nối hệ thống khi lấy thông tin sản phẩm");
            }

            // 2. Logic xử lý và Map dữ liệu (Nếu lỗi ở đây sẽ là lỗi Logic/Data chứ không phải lỗi gRPC)
            try
            {
                if (cart.CartItems == null)
                {
                    return Result<CartDto>.Success(new CartDto { Id = cart.Id, Items = new List<CartItemDto>() }, "Giỏ hàng trống.");
                }

                var cartDto = new CartDto
                {
                    Id = cart.Id,
                    Items = cart.CartItems.Select(item =>
                    {
                        var pInfo = grpcResponse.Products.FirstOrDefault(p => p.Id == item.ProductId);
                        var productName = pInfo?.Name ?? "Sản phẩm không xác định";
                        var variantName = pInfo?.VariantName ?? "Phiên bản không xác định";

                        // An toàn hơn khi ép kiểu từ double/float (của gRPC) sang decimal
                        decimal unitPrice = 0;
                        if (pInfo != null)
                        {
                            unitPrice = Convert.ToDecimal(pInfo.Price);
                        }

                        return new CartItemDto
                        {
                            ProductId = item.ProductId,
                            VariantId = item.VariantId,
                            Quantity = item.Quantity ?? 0,
                            VariantName = variantName,
                            ProductName = productName,
                            ProductDisplayName = $"{productName} - {variantName}",
                            UnitPrice = unitPrice,
                            MainImage = pInfo?.ImageUrl ?? string.Empty,
                            CurrencyUnit = pInfo?.CurrencyUnit ?? "VNĐ",
                        };
                    }).ToList()
                };

                return Result<CartDto>.Success(cartDto, "Thành công.");
            }
            catch (Exception ex)
            {
                // Log chính xác lỗi xuất hiện khi xử lý mapping dữ liệu
                _logger.LogError(ex, "Lỗi logic khi map dữ liệu Cart sang CartDto");
                return Result<CartDto>.Failure("Lỗi xử lý dữ liệu giỏ hàng");
            }
            finally
            {
                _logger.LogInformation($"{nameof(GetCartAsync)} end");
            }
        }
        public async Task<Result<bool>> CleanCartAsync()
        {
            _logger.LogInformation($"{nameof(GetCartAsync)} start");
            // 1. Lấy UserId từ thông tin đăng nhập (Gateway chuyển xuống)
            var userId = _currentCustomerService.Id;

            // 2. Tìm giỏ hàng của User này
            var cart = await _unitOfWork.Repository<Cart>()
                .Entities.Where(x => x.CustomerId == userId)
                .Include(x => x.CartItems)
                .FirstOrDefaultAsync();

            if (cart == null)
            {
                return Result<bool>.Failure("Không tìm thấy giỏ hàng để xóa");
            }

            try
            {
                // Chỉ comment dòng quan trọng: Xóa danh sách sản phẩm trong giỏ hàng thay vì xóa luôn cả bản ghi Cart
                _unitOfWork.Repository<CartItem>().RemoveRange(cart.CartItems);

                // Chỉ comment dòng quan trọng: Lưu thay đổi vào Database
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("User {UserId} đã xóa sạch giỏ hàng", userId);

                return Result<bool>.Success(true, "Thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa giỏ hàng của User {UserId}", userId);
                return Result<bool>.Failure("Lỗi hệ thống khi làm sạch giỏ hàng");
            }
            finally
            {
                _logger.LogInformation($"{nameof(GetCartAsync)} end");
            }
        }
    }
}
