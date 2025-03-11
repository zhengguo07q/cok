using System;
using System.Collections.Generic;
using TEngine;


namespace GameBase.Loader
{
    public enum LoadState
    {
        Success,
        Failure
    }
    /// <summary>
    /// 资源加载任务，在这里保存资源信息，用于当退出的时候进行释放资源
    /// </summary>
    public class LoadTask
    {
        /// <summary>
        ///  资源名字
        /// </summary>
        public string Name { get; set; } 

        /// <summary>
        /// 载入状态，如果载入成功，需要释放资源
        /// </summary>
        public bool LoadState { get; set; }

        public LoadResourceStatus Status { get; set; }

        public string FailReason { get; set; }
    }


    /// <summary>
    /// 关于场景和资源加载卸载的逻辑
    /// 资源装载的最底层目前是yooassets
    /// Tengine增加了对象池化和引用功能，缓存功能
    /// ResourceModule里面有一套列表资源下载，是给启动流程专用的。
    /// 这里增加的是一个下载资源的队列列表
    /// </summary>
    public class ListLoader : BehaviourSingleton<ListLoader>
    {
        private readonly List<string> waitList = new();
        private readonly List<UnityEngine.Object> assetListT = new();
        private readonly List<LoadTask> loadTaskList = new ();
        private Action callbackLoadComplete;
        private readonly Dictionary<string, bool> _loadedFlag = new Dictionary<string, bool>();

        private int loadTotal = 0; // 总下载队列数量
        private int loadCount = 0; // 剩余还在下载的数量
        private float combProgress = 0;
        private bool isStart = false;
        private readonly HashSet<LoadTask> removeTaskSet = new ();
        LoadAssetCallbacks m_PreLoadAssetCallbacks;
        public void PutWaitLoad(string waitLoadResource)
        {
            waitList.Add(waitLoadResource);
        }

        public override void Start()
        {
            m_PreLoadAssetCallbacks = new LoadAssetCallbacks(OnPreLoadAssetSuccess, OnPreLoadAssetFailure);
        }

        /// <summary>
        ///  资源载入失败标记为false
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="status"></param>
        /// <param name="errormessage"></param>
        /// <param name="userdata"></param>
        private void OnPreLoadAssetFailure(string assetName, LoadResourceStatus status, string errormessage, object userdata)
        {
            Log.Warning("Can not preload asset from '{0}' with error message '{1}'.", assetName, errormessage);
            _loadedFlag[assetName] = false;
        }

        /// <summary>
        /// 资源载入成功标记为true
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="asset"></param>
        /// <param name="duration"></param>
        /// <param name="userdata"></param>
        private void OnPreLoadAssetSuccess(string assetName, object asset, float duration, object userdata)
        {
            Log.Debug("Success preload asset from '{0}' duration '{1}'.", assetName, duration);
            _loadedFlag[assetName] = true;
        }

        // 启动装载
        public void Load()
        {
            if (waitList.Count == 0 && callbackLoadComplete != null) // 检查是否有需要装载的，没有直接回调
            {
                callbackLoadComplete?.Invoke();
                return;
            }

            loadTotal = waitList.Count;
            loadCount = waitList.Count;

            foreach (var resource in waitList)
            {
                GameModule.Resource.LoadAssetAsync(resource, typeof(UnityEngine.Object), m_PreLoadAssetCallbacks);
            }

            waitList.Clear();
            isStart = true;
        }

        private float _progress = 0f;

        public override void Update()
        {
            if (!isStart) // 没有开始返回
            {
                return;
            }
            var totalCount = _loadedFlag.Count <= 0 ? 1 : _loadedFlag.Count;

            var loadCount = _loadedFlag.Count <= 0 ? 1 : 0;

            foreach (KeyValuePair<string, bool> loadedFlag in _loadedFlag)
            {
                if (!loadedFlag.Value)
                {
                    break;
                }
                else
                {
                    loadCount++;
                }
            }

            if (_loadedFlag.Count != 0)
            {
            }
            else
            {

                string progressStr = $"{_progress * 100:f1}";

                if (Math.Abs(_progress - 1f) < 0.001f)
                {
                }
                else
                {
                }
            }

            if (loadCount < totalCount)
            {
                return;
            }
            if (loadCount == 0 && callbackLoadComplete != null)
            {
                isStart = false;
                callbackLoadComplete();
                combProgress = 1;
            }

        }


        public void SetCallback(Action callbackFun)
        {
            callbackLoadComplete = callbackFun;
        }

        public void Dispose()
        {
            foreach(var assetName in _loadedFlag.Keys) {
                GameModule.Resource.UnloadAsset(assetName);
            }
            callbackLoadComplete = null;
        }
    }

}