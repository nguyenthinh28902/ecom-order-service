using AutoMapper;
using Ecom.OrderService.Application.Interface.Web;
using Ecom.OrderService.Core.Models;
using Ecom.OrderService.Core.Models.Auth;
using Ecom.OrderService.Core.Models.Cms.Dtos.Order;
using Ecom.OrderService.Core.Models.Web.Dtos.Checkout;
using Ecom.OrderService.Core.Models.Web.Dtos.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecom.OrderService.Api.Controllers.Web
{
    [Route("api/don-hang")]
    [ApiController]
    [Authorize(PolicyNames.OrderReadWeb)]
    public class OrderWebController : ControllerBase
    {
        private readonly IOrderWebService _orderService;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderWebController> _logger;

        public OrderWebController(
            IOrderWebService orderService,
            IMapper mapper,
            ILogger<OrderWebController> logger)
        {
            _orderService = orderService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet("thong-tin-don-hang")]
        public async Task<IActionResult> CheckoutInforAsync()
        {
            _logger.LogInformation("Đang chuẩn bị dữ liệu cho trang Checkout");

            // Chỉ comment dòng quan trọng: Lấy dữ liệu Checkout đã được làm giàu từ gRPC Product Service
            var result = await _orderService.GetCheckoutDetailsAsync();
           
            return Ok(result);
        }

        [HttpPost("dat-hang")]
        public async Task<IActionResult> PlaceOrderAsync([FromBody] CheckoutRequestDto request)
        {
            _logger.LogInformation("Bắt đầu xử lý đặt hàng cho khách hàng");

            // Chỉ comment dòng quan trọng: Gọi service thực hiện lưu Order, OrderItem và xóa Cart trong 1 Transaction
            var result = await _orderService.PlaceOrderAsync(request);

            if (result == null || result.Data == null || !result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [Authorize(PolicyNames.OrderWrite)]
        [HttpPatch("cap-nhat-trang-thai")]
        public async Task<IActionResult> UpdateStatusAsync([FromBody] UpdateStatusRequest request)
        {
            _logger.LogInformation("Cập nhật trạng thái đơn hàng {OrderId} sang {Status}", request.Id, request.Status);

            // Chỉ comment dòng quan trọng: Cập nhật trạng thái sử dụng Enum OrderStatus (byte) và lưu log lịch sử
            var result = await _orderService.UpdateOrderStatusAsync(request);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        /// <summary>
        /// Lấy chi tiết đơn hàng bao gồm cả thông tin thanh toán từ Payment Service
        /// </summary>
        /// <param name="id">ID của đơn hàng</param>
        [HttpPost("thong-tin-don-hang")]
        [ProducesResponseType(typeof(Result<OrderManagerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderDetailsAsync([FromBody] OrderDetailsRequest request)
        {
            _logger.LogInformation("Truy vấn chi tiết đơn hàng ID: {OrderCode}", request.OrderCode);

            // 1. Chỉ comment dòng quan trọng: Gọi service lấy Order kèm thông tin Transaction qua gRPC
            var result = await _orderService.GetOrderDetailsAsync(request);

            if (!result.IsSuccess)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        /// <summary>
        /// Lấy danh sách lịch sử đơn hàng của khách hàng hiện tại (Không kèm thông tin thanh toán chi tiết)
        /// </summary>
        /// <returns>Danh sách đơn hàng của khách hàng</returns>
        [HttpGet("lich-su-dat-hang")]
        [ProducesResponseType(typeof(Result<List<OrderManagerDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrderHistoryAsync()
        {
            _logger.LogInformation("Khách hàng đang truy cập lịch sử đơn hàng");

            // 1. Chỉ comment dòng quan trọng: Gọi service lấy danh sách đơn hàng đã được map qua Profile (bao gồm cả StatusName và Items)
            var result = await _orderService.GetOrderHistoryAsync();

            return Ok(result);
        }
    }
}
