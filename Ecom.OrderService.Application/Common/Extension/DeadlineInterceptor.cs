using Grpc.Core;
using Grpc.Core.Interceptors;
using System;
using System.Collections.Generic;
using System.Text;
using static Grpc.Core.Interceptors.Interceptor;

namespace Ecom.OrderService.Application.Common.Extension
{
    public class DeadlineInterceptor : Interceptor
    {
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            // Tự động gắn Deadline 2s cho mọi request đi qua Interceptor này
            var callOptions = context.Options.WithDeadline(DateTime.UtcNow.AddSeconds(7));

            var newContext = new ClientInterceptorContext<TRequest, TResponse>(
                context.Method,
                context.Host,
                callOptions);

            return continuation(request, newContext);
        }
    }
}
