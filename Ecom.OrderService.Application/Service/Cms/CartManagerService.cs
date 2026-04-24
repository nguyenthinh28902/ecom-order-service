using Ecom.OrderService.Application.Interface.Cms;
using Ecom.OrderService.Core.Abstractions.Persistence;
using Ecom.OrderService.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Application.Service.Cms
{
    public class CartManagerService : ICartManagerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CartManagerService> _logger;

        public CartManagerService(IUnitOfWork unitOfWork, ILogger<CartManagerService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Xóa sản phẩm khỏi giỏ hàng dựa trên Event từ RabbitMQ
        /// </summary>
        public async Task<bool> ClearCartItemsAfterOrderAsync(int productId, int? variantId)
        {
            _logger.LogInformation("Bắt đầu dọn dẹp giỏ hàng cho ProductId: {ProductId}, VariantId: {VariantId}", productId, variantId);

            try
            {
                // 1. Tạo query cơ bản theo ProductId
                var cartItemQuery = _unitOfWork.Repository<CartItem>().Entities
                    .Where(x => x.ProductId == productId);

                // 2. Logic check: Nếu có VariantId cụ thể thì lọc thêm, không thì lấy hết theo ProductId
                if (variantId.HasValue && variantId.Value > 0)
                {
                    cartItemQuery = cartItemQuery.Where(x => x.VariantId == variantId.Value);
                }

                var itemsToRemove = await cartItemQuery.ToListAsync();

                if (itemsToRemove.Any())
                {
                    // 3. Xóa danh sách item đã lọc
                    _unitOfWork.Repository<CartItem>().RemoveRange(itemsToRemove);

                    // 4. Lưu thay đổi xuống Database
                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInformation("Đã xóa {Count} dòng dữ liệu trong giỏ hàng", itemsToRemove.Count);
                    return true;
                }

                _logger.LogInformation("Không tìm thấy sản phẩm nào khớp trong giỏ hàng để xóa");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý ClearCartItems cho Product: {ProductId}", productId);
                await _unitOfWork.RollbackAsync();
                throw; // Throw để Consumer có thể Retry nếu cần
            }
        }
    }
}
