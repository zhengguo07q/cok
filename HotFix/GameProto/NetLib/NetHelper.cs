using Grpc.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GameProto
{
    public static class NetHelper
    {
        /// <summary>
        /// 用来做错误转化和异常捕获，确保逻辑层不需要处理各种服务器错误和网络异常
        /// </summary>
        public static async Task CallStream<TResponse>(
            Func<AsyncServerStreamingCall<TResponse>> callFactory,
            Action<TResponse> responseHandler,
            CancellationToken cancellationToken = default)
        {

            var context = SynchronizationContext.Current;
            if (context == null)
            {
                TEngine.Log.Error("SynchronizationContext.Current is null. Ensure this method is called from the main thread.");
                return;
            }
            try
            {
                using var call = callFactory();

                while (await call.ResponseStream.MoveNext(cancellationToken).ConfigureAwait(false))
                {
                    var response = call.ResponseStream.Current;
                    context.Post(_ => responseHandler(response), null);
                }
            }
            catch (RpcException ex)
            {
                Status status = ex.Status;
                if (status.StatusCode != StatusCode.OK)
                {
                    // 如果不是 OK，则写日志
                    TEngine.Log.Error(string.Format("server err code: {0}, message: {1}", status.StatusCode, status.Detail));
                }
            }
            catch (OperationCanceledException)
            {
                TEngine.Log.Error("Operation was canceled.");
            }
            catch (Exception ex)
            {
                TEngine.Log.Error($"Unexpected error: {ex.Message}");
            }

        }

        /// <summary>
        /// 用来做错误转化和异常捕获，确保逻辑层不需要处理各种服务器错误和网络异常
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="call"></param>
        /// <returns></returns>
        public static async Task<(StatusCode StatusCode, TResponse Response)> Call<TResponse>(AsyncUnaryCall<TResponse> call)
        {
            var response = default(TResponse);
            var status = default(Status);
            try
            {
                response = await call.ResponseAsync.ConfigureAwait(false);

                status = call.GetStatus();
            }
            catch (Exception err)
            {
                if (err is RpcException rpcException)
                {
                    status = rpcException.Status;
                }
                else
                {
                    status = new Status(StatusCode.Unknown, err.Message);
                }
            }
            if (status.StatusCode != StatusCode.OK)
            {
                // 如果不是 OK，则写日志
                TEngine.Log.Error(string.Format("server err code: {0}, message: {1}", status.StatusCode, status.Detail));
            }
            return (status.StatusCode, response);
        }



        /// <summary>
        /// 判断是不是存在错误
        /// </summary>
        /// <param name="target">要检查的目标对象</param>
        /// <returns>如果目标为空则返回 true；否则返回 false。</returns>
        public static bool NotError(StatusCode code)
        {
            return !Error(code);
        }

        /// <summary>
        /// 判断是不是存在错误
        /// </summary>
        /// <param name="target">要检查的目标对象</param>
        /// <returns>如果目标为空则返回 true；否则返回 false。</returns>
        public static bool Error(StatusCode code)
        {
            if (code == StatusCode.OK)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
