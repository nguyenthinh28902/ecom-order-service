using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.Contracts.ProductService
{
    public class OrderCompletedEvent
    {
        public int ProductId { get; set; }
        public int? ProductVariantId { get; set; }
    }
}
