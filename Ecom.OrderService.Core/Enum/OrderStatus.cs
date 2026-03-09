using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Core.Enum
{
    public enum OrderStatus : byte
    {
        Pending = 0,        // Chờ xử lý
        Confirmed = 1,      // Đã xác nhận
        Processing = 2,     // Đang đóng gói
        Shipping = 3,       // Đang giao hàng
        Completed = 4,      // Hoàn thành
        Cancelled = 5,      // Đã hủy
        Refunded = 6        // Đã hoàn tiền
    }
}
