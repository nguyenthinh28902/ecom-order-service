using Ecom.OrderService.Core.Exceptions;
using System.Net;
using System.Text.Json;

namespace Ecom.OrderService.Common.Extensions
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Một lỗi không mong đợi đã xảy ra: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            // Mặc định là lỗi 500
            var statusCode = (int)HttpStatusCode.InternalServerError;
            var message = "Đã có lỗi hệ thống xảy ra. Vui lòng thử lại sau.";

            // Phân loại Exception
            switch (exception)
            {
                case UnauthorizedException authEx:
                    statusCode = authEx.StatusCode;
                    message = authEx.Message;
                    break;
                case ForbiddenException forbEx: // Lỗi 403 (Mới thêm)
                    statusCode = forbEx.StatusCode;
                    message = forbEx.Message;
                    break;
            }

            context.Response.StatusCode = statusCode;

            var response = new
            {
                isSuccess = false,
                statusCode = statusCode,
                error = message,
                // Chỉ hiển thị chi tiết lỗi (StackTrace) khi ở môi trường Development
                detail = _env.IsDevelopment() ? exception.ToString() : null
            };

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(response, options);

            await context.Response.WriteAsync(json);
        }
    }
}
