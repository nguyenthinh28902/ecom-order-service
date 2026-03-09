using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Core.Models
{
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public string? Noti { get; }
        public T? Data { get; }

        protected Result(bool isSuccess, T? data, string? error)
        {
            IsSuccess = isSuccess;
            Data = data;
            Noti = error;
        }
        public static Result<T> Success(T data, string mess)
       => new(true, data, mess);

        public static Result<T> Failure(string error)
            => new(false, default, error);
    }
}
