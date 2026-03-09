using Ecom.OrderService.Application.Interface.Web;
using Ecom.OrderService.Core.Entities;
using Ecom.OrderService.Core.Models.Auth;
using Ecom.OrderService.Core.Models.Web.Dtos.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Ecom.OrderService.Api.Controllers.Web
{

    [Authorize(PolicyNames.OrderWriteWeb)]
    [ApiController]
    // Định nghĩa route thuần Việt cho Controller
    [Route("api/gio-hang")]
    public class CartWebController : ControllerBase
    {
        private readonly ILogger<CartWebController> _logger;
        private readonly ICartWebService _cartService;
        public CartWebController(ILogger<CartWebController> logger, ICartWebService cartWebService)
        {
            _logger = logger;
            _cartService = cartWebService;
        }

        [HttpGet("khach-hang")]
        public async Task<IActionResult> GetCart()
        {
            _logger.LogInformation("API: Đang lấy chi tiết giỏ hàng");

            var result = await _cartService.GetCartAsync();
            _logger.LogInformation("thong tin cho gio hang: {cart}", JsonSerializer.Serialize(result));
            if (!result.IsSuccess) return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("them-moi")]
        public async Task<IActionResult> AddToCart([FromBody] CreateCartItemRequest request)
        {
            _logger.LogInformation("CreateCartItemRequest in AddToCart: {requestString}", JsonSerializer.Serialize(request));
            // Thực hiện gọi hàm nghiệp vụ xử lý UnitOfWork đã viết
            var result = await _cartService.AddToCartAsync(request);
            _logger.LogInformation("Result in AddToCart: {resultString}", result);
            return Ok(result);
        }

        // 3. API Xóa sạch toàn bộ giỏ hàng
        [HttpDelete("lam-moi-gio-hang")]
        public async Task<IActionResult> CleanCart()
        {
            _logger.LogInformation("API: Đang thực hiện làm sạch giỏ hàng");

            var result = await _cartService.CleanCartAsync();
            _logger.LogInformation("Result in CleanCart: {resultString}", result);
            if (!result.IsSuccess) return BadRequest(result);

            return Ok(result);
        }
    }
}
