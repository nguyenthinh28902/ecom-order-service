using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Core.Models.Web.Dtos.Order
{
    public class TransactionInfoDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public string PaymentMethodName { get; set; } = null!;
        public string ExternalTransactionId { get; set; } = null!;
        public string StatusName { get; set; } = null!;
    }
}
