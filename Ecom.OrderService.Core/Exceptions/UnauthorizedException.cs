using Microsoft.AspNetCore.Http;

namespace Ecom.OrderService.Core.Exceptions
{
    public class UnauthorizedException : Exception
    {
        public int StatusCode { get; }

        public UnauthorizedException()
            : base("Yêu cầu không được xác thực.")
        {
            StatusCode = StatusCodes.Status401Unauthorized;
        }

        public UnauthorizedException(string message)
            : base(message)
        {
            StatusCode = StatusCodes.Status401Unauthorized;
        }

        public UnauthorizedException(string message, Exception innerException)
            : base(message, innerException)
        {
            StatusCode = StatusCodes.Status401Unauthorized;
        }
    }
}
