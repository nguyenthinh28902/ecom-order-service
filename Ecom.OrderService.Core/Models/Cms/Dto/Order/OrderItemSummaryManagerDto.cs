using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Core.Models.Cms.Dtos.Order
{
    public class OrderItemSummaryManagerDto
    {
        public string ProductName { get; set; } = null!;
        public string ProductMainImage { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
