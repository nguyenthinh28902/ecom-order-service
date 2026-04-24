using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Core.Exceptions
{
    public class ConflictException : Exception
    {
        public int StatusCode { get; }

        public ConflictException()
            : base("Dữ liệu đã tồn tại trong hệ thống.")
        {
            StatusCode = StatusCodes.Status409Conflict;
        }

        public ConflictException(string message)
            : base(message)
        {
            StatusCode = StatusCodes.Status409Conflict;
        }

        public ConflictException(string message, Exception innerException)
            : base(message, innerException)
        {
            StatusCode = StatusCodes.Status409Conflict;
        }
    }
}
