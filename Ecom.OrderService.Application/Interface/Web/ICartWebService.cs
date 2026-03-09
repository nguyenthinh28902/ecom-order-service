using Ecom.OrderService.Core.Models;
using Ecom.OrderService.Core.Models.Web.Dtos.Cart;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Application.Interface.Web
{
    public interface ICartWebService
    {
        public Task<Result<bool>> AddToCartAsync(CreateCartItemRequest request);
        public Task<Result<CartDto>> GetCartAsync();
        public Task<Result<bool>> CleanCartAsync();
    }
}
