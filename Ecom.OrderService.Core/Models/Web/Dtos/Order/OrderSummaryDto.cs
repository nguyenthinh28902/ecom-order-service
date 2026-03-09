using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Core.Models.Web.Dtos.Order
{
    public class OrderSummaryDto
    {
        public int Id { get; set; }
        public string OrderCode { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public string? Currency { get; set; }

        // Chỉ comment dòng quan trọng: Tên trạng thái để hiển thị màu sắc/label trên UI
        public string? StatusName { get; set; }

        public DateTime? CreatedAt { get; set; }

        // Chỉ comment dòng quan trọng: Trả về danh sách sản phẩm nhưng chỉ lấy thông tin hiển thị cơ bản
        public List<OrderItemSummaryDto> OrderItems { get; set; } = new();

        // Chỉ comment dòng quan trọng: Tính nhanh tổng số lượng món hàng để hiển thị "X sản phẩm"
        public int TotalItems => OrderItems.Sum(x => x.Quantity);
    }
}
