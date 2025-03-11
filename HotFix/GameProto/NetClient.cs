using System.Threading;
using System;
using UnityEngine;
using Cysharp.Net.Http;
using Grpc.Net.Client;
using Grpc.Core;
using Grpc.Core.Interceptors;
using GameBase;
using User.V1;
using Cysharp.Threading.Tasks;

namespace GameProto
{
    [DisallowMultipleComponent]
    public sealed class NetClient : Singleton<NetClient>
    {
        [SerializeField]
        String Ip = "123.56.0.25";
        // String Ip = "192.168.1.71";

        [SerializeField] 
        int Port = 9090;

        [SerializeField]
        int Timeout = 3600;

        private string _accessToken;
        /// <summary>
        /// token被设置后新的请求都会带上token
        /// </summary>
        public string AccessToken
        {
            private get => _accessToken;
            set
            {
                _accessToken = value;
                AuthCallInvoker = GrpcChannel
                 .Intercept(new LogInterceptor()).Intercept(new AuthInterceptor(AccessToken));
            }
        }

        [SerializeField]
        bool Http2Only = true;
        public CallInvoker CallInvoker { get; set; }

        public CallInvoker AuthCallInvoker { get; set; }

        public CallOptions CallOptions{ get; set; }

        public GrpcChannel GrpcChannel { get; set; }

        /// <summary>
        /// 创建基础连接
        /// </summary>
        public void Connect()
        {
            string uriAddress = string.Format("http://{0}:{1}", this.Ip, this.Port);
            TEngine.Log.Info(string.Format("connect uri : {0}", uriAddress));
            var handler = new YetAnotherHttpHandler { Http2Only = Http2Only };
            GrpcChannel = GrpcChannel.ForAddress(uriAddress, new GrpcChannelOptions() { HttpHandler = handler });


            // 设置超时时间为 5 秒钟
            var timeout = TimeSpan.FromSeconds(5);
            var cancellationToken = new CancellationTokenSource(timeout).Token;
            CallOptions = new CallOptions(deadline: System.DateTime.UtcNow.Add(timeout), cancellationToken: cancellationToken);
            CallInvoker = GrpcChannel.Intercept(new LogInterceptor());
        }



        /// <summary>
        /// 释放资源
        /// </summary>
       public override void Release() {
            GrpcChannel?.Dispose();
        }

    }
}
