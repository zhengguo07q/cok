using System;
using Grpc.Core;
using Grpc.Core.Interceptors;
using System.Threading.Tasks;

namespace GameProto
{
    public class AuthInterceptor : Interceptor
    {
        private readonly string _accessToken;

        public AuthInterceptor(string accessToken)
        {
            _accessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
        }

        // 同步一元流调用
        public override TResponse BlockingUnaryCall<TRequest, TResponse>(
            TRequest request, 
            ClientInterceptorContext<TRequest, TResponse> context, 
            BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var updatedHeaders = AddAuthorizationHeader(context.Options.Headers);

            var newContext = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, new CallOptions(updatedHeaders));

            return continuation(request, newContext);
        }

        // 异步一元流调用
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request, 
            ClientInterceptorContext<TRequest, TResponse> context, 
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var updatedHeaders = AddAuthorizationHeader(context.Options.Headers);

            var newContext = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, new CallOptions(updatedHeaders));

            return continuation(request, newContext);
        }

        // 异步服务器流式调用
        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
            TRequest request, 
            ClientInterceptorContext<TRequest, TResponse> context, 
            AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            var updatedHeaders = AddAuthorizationHeader(context.Options.Headers);

            var newContext = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, new CallOptions(updatedHeaders));

            return continuation(request, newContext);
        }

        // 异步客户端流式调用
        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context, 
            AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation) 
        {
            var updatedHeaders = AddAuthorizationHeader(context.Options.Headers);

            var newContext = new ClientInterceptorContext<TRequest, TResponse>(
                context.Method,
                context.Host,
                context.Options.WithHeaders(updatedHeaders)
            );
            return continuation(newContext);
        }

        // 异步双工流式调用
        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context, 
            AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            var updatedHeaders = AddAuthorizationHeader(context.Options.Headers);

            var newContext = new ClientInterceptorContext<TRequest, TResponse>(
                context.Method,
                context.Host,
                context.Options.WithHeaders(updatedHeaders)
            );
            return continuation(newContext);
        }


        private Metadata AddAuthorizationHeader(Metadata headers)
        {
            headers ??= new Metadata();
            headers.Add("authorization", $"Bearer {_accessToken}");
            return headers;
        }
    }
}
