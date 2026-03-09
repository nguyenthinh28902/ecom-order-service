using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.Contracts.Requests
{
    public class NotificationRequestDto
    {
        public int ReceiverId { get; set; }
        public string ReceiverRole { get; set; } = "CUSTOMER";
        public string ReceiverEmail { get; set; } = null!;
        public string TypeCode { get; set; } = null!;
        public string? LanguageCode { get; set; } = "vi-VN";
        public string Channel { get; set; } = "WEB_PUSH";
        public string? Message { get; set; }

        // Các tham số đơn (customer_name, order_code, total_amount...)
        public Dictionary<string, string> Parameters { get; set; } = new();

        // Mảng dữ liệu cho phần lặp lại (Danh sách sản phẩm)
        // Mỗi Dictionary trong List là 1 dòng sản phẩm (product_name, quantity, sub_total)
        public List<Dictionary<string, string>> Items { get; set; } = new();
    }
}
