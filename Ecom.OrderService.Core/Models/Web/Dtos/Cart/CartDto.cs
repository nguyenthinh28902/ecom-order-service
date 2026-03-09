using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Core.Models.Web.Dtos.Cart
{
    public class CartDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public List<CartItemDto> Items { get; set; } = new();

        // Các thông tin tổng hợp cho Frontend
        public int TotalItems => Items.Sum(i => i.Quantity);
        public decimal GrandTotal => Items.Sum(i => i.TotalPrice);
        public DateTime? UpdatedAt { get; set; }
    }
}
