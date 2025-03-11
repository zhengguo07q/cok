using System;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace GameProto
{
    internal class LogInterceptor : Interceptor
    {
        // 同步一元流调用
        public override TResponse BlockingUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            TEngine.Log.Debug($"Request {context.Method} with: {request}");
            var newcontinuation = continuation(request, context);
            TEngine.Log.Debug($"Received response from {context.Method}: {newcontinuation}");
            return newcontinuation;
        }

        // 异步一元流调用
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            TEngine.Log.Debug($"Request {context.Method} with: {request}");
            var newcontinuation = continuation(request, context);
            TEngine.Log.Debug($"Received response from {context.Method}: {newcontinuation}");
            return newcontinuation;
        }

        // 异步服务器流式调用
        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            TEngine.Log.Debug($"Request {context.Method} with: {request}");
            var newcontinuation = continuation(request, context);
            TEngine.Log.Debug($"Received response from {context.Method}: {newcontinuation}");
            return newcontinuation;
        }

        // 异步客户端流式调用
        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            TEngine.Log.Debug($"Request {context.Method} ");
            return continuation(context);
        }

        // 异步双工流式调用
        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            TEngine.Log.Debug($"Request {context.Method} ");
            return continuation(context);
        }
    }
}
