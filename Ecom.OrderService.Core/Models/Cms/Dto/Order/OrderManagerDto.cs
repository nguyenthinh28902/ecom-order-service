using Ecom.OrderService.Core.Models.Dto.Cms;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Core.Models.Cms.Dtos.Order
{
    public class OrderManagerDto
    {
        public int Id { get; set; }
        public string OrderCode { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public string? Currency { get; set; }
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string ShippingAddress { get; set; } = null!;
        public byte? Status { get; set; }
        public string? StatusName { get; set; } // Map từ Enum OrderStatus
        public DateTime? CreatedAt { get; set; }

        public List<OrderItemManagerDto> OrderItems { get; set; } = new();

       
        // Chỉ comment dòng quan trọng: Chứa thông tin giao dịch lấy từ Payment Service qua gRPC
        public TransactionManagerDto? TransactionInfo   { get; set; }
    }
}
