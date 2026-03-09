using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Core.Models.Web.Dtos.Cart
{
    public class CartItemDto
    {      
        public int ProductId { get; set; }
        public int VariantId { get; set; }
        public int Quantity { get; set; }

        // --- Các trường mở rộng từ Product Service để hiển thị ---
        public string ProductDisplayName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string VariantName { get; set; } = string.Empty; // VD: "Xanh Titan"
        public string? MainImage { get; set; }
        public decimal UnitPrice { get; set; }
        public string CurrencyUnit { get; set; } = "VNĐ";

        // Thuộc tính tính toán
        public decimal TotalPrice => UnitPrice * Quantity;
    }
}
