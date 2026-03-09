using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Core.Models.Web.Dtos.Checkout
{
    public class CheckoutItemDto
    {
        public int ProductId { get; set; }
        public int VariantId { get; set; }
        public string ProductName { get; set; }
        public string VariantName { get; set; }
        public string ImageUrl { get; set; }
        public string Sku { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalLine { get; set; }
        public bool IsAvailable { get; set; } // Trạng thái để báo cho khách nếu hàng bị ngưng bán
    }
}
