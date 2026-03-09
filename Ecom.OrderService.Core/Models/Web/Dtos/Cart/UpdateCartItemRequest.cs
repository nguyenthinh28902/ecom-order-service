using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ecom.OrderService.Core.Models.Web.Dtos.Cart
{
    public class UpdateCartItemRequest
    {
        [Required]
        public int CartItemId { get; set; }

        [Range(1, 100)]
        public int Quantity { get; set; }
    }
}
