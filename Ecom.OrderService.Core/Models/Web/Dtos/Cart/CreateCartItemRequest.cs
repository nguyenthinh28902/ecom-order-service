using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ecom.OrderService.Core.Models.Web.Dtos.Cart
{
    public class CreateCartItemRequest
    {
        [Required(ErrorMessage = "ProductId không được để trống")]
        public int ProductId { get; set; } // Khớp với ProductId trong CartItem.cs

        [Required(ErrorMessage = "VariantId không được để trống")]
        public int VariantId { get; set; } // Khớp với VariantId trong CartItem.cs

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; } = 1; // Mặc định thêm 1 sản phẩm nếu không truyền số lượng
    }
}
