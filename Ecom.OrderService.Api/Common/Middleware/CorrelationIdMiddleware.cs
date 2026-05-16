namespace Ecom.OrderService.Api.Common.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext context)
        {
            // Kiểm tra xem header có mã chưa, chưa có thì tạo mới
            if (!context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
            {
                // 2. Nếu không có trong Header, thử tìm trong Metadata (Trường hợp gRPC đặc thù)
                // Một số hệ thống dùng lowercase 'x-correlation-id' cho gRPC
                if (!context.Request.Headers.TryGetValue("x-correlation-id", out correlationId))
                {
                    // 3. Cuối cùng mới tạo mới nếu cả 2 đều không có
                    correlationId = Guid.NewGuid().ToString();
                }
            }

            // Đẩy vào LogContext để log của Gateway cũng có mã này
            using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId.ToString()))
            {
                // Quan trọng: Gắn lại vào Header để YARP tí nữa nó "bốc" đi theo
                context.Request.Headers["X-Correlation-ID"] = correlationId;

                // Trả về cho Client biết luôn (để ný debug trên trình duyệt)
                context.Response.Headers["X-Correlation-ID"] = correlationId;

                await _next(context);
            }
        }
    }
}
