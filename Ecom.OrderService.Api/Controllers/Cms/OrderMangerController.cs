using Ecom.OrderService.Application.Interface.Cms;
using Ecom.OrderService.Core.Models;
using Ecom.OrderService.Core.Models.Auth;
using Ecom.OrderService.Core.Models.Cms.Dtos.Order;
using Ecom.OrderService.Core.Models.Cms.OrderMangerRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecom.OrderService.Api.Controllers.Cms
{
    [Route("api/don-hang/quan-ly")]
    [ApiController]
    [Authorize(PolicyNames.OrderRead)]
    public class OrderMangerController : ControllerBase
    {
        private readonly ILogger<OrderMangerController> _logger;
        private readonly IOrderManagerService _orderManagerService;
        public OrderMangerController(ILogger<OrderMangerController> logger
            , IOrderManagerService orderManagerService)
        {
            _logger = logger;
            _orderManagerService = orderManagerService;
        }

        [HttpGet("danh-sach")]
        [ProducesResponseType(typeof(Result<List<OrderSummaryManagerDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> OrderManagerAsync()
        {
            var result = await _orderManagerService.GetOrderHistoryAsync();
            return Ok(result);
        }


        [HttpPost("chi-tiet-don-hang")]
        [ProducesResponseType(typeof(Result<List<OrderSummaryManagerDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> OrderDetailManagerAsync([FromBody]OrderDetailRequest request)
        {
            var result = await _orderManagerService.GetOrderManagerByOrderIdAsync(request);

            return Ok(result);
        }
    }
}
