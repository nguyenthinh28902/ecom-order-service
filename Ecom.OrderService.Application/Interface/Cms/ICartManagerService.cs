using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Application.Interface.Cms
{
    public interface ICartManagerService
    {
        Task<bool> ClearCartItemsAfterOrderAsync(int productId, int? variantId);
    }
}
