using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Core.Exceptions
{
    public class ForbiddenException : Exception
    {
        public int StatusCode { get; }

        public ForbiddenException()
            : base("Bạn không có quyền truy cập vào tài nguyên này.")
        {
            StatusCode = StatusCodes.Status403Forbidden;
        }

        public ForbiddenException(string message)
            : base(message)
        {
            StatusCode = StatusCodes.Status403Forbidden;
        }

        public ForbiddenException(string message, Exception innerException)
            : base(message, innerException)
        {
            StatusCode = StatusCodes.Status403Forbidden;
        }
    }
}
