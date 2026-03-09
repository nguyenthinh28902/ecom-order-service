using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Core.Models.Web.Dtos.Checkout
{
    public class PaymentResultDto
    {
        public int OrderId { get; set; }
        // Chỉ comment dòng quan trọng: Trạng thái xử lý từ phía Payment Service
        public bool IsSuccess { get; set; }

        public string Message { get; set; } = null!;

        // Chỉ comment dòng quan trọng: URL thanh toán (PayPal, VNPAY...) nếu có
        public string? ApprovalUrl { get; set; }

        public string OrderCode { get; set; } = null!;
    }
}
