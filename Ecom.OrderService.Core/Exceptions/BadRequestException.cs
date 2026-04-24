using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Core.Exceptions
{
    public class BadRequestException : Exception
    {
        public int StatusCode { get; }

        public BadRequestException()
            : base("Yêu cầu không hợp lệ.")
        {
            StatusCode = StatusCodes.Status400BadRequest;
        }

        public BadRequestException(string message)
            : base(message)
        {
            StatusCode = StatusCodes.Status400BadRequest;
        }

        public BadRequestException(string message, Exception innerException)
            : base(message, innerException)
        {
            StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}
