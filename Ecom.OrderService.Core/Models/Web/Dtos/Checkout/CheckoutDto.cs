using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Core.Models.Web.Dtos.Checkout
{
    public class CheckoutDto
    {
        public List<CheckoutItemDto> Items { get; set; } = new();
        public decimal SubTotal { get; set; } // Tổng tiền hàng
        public decimal ShippingFee { get; set; } // Phí ship (nếu có)
        public decimal TotalAmount { get; set; } // Tổng thanh toán cuối cùng
    }
}
