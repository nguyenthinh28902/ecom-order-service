using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Core.Models.Web.Dtos.Order
{
    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string ProductMainImage { get; set; } = null!;
        public string Sku { get; set; } = null!;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal? TotalLineAmount { get; set; }
    }
}
