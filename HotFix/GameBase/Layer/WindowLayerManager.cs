using GameBase.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;
using TEngine;
using UnityEngine;
using UnityEngine.UI;
using static GameBase.Layer.LayerIndexInfo;

namespace GameBase.Layer
{
    /// <summary>
    /// UGUI支持了2D显示的图层定义
    /// TENGINE支持了绑定窗口的几个基础图层定义
    /// 此管理器定义了除开窗口外，其他的图层，如UI、特效、特效等。还定义了多相机等
    /// 兼容了TENGINE的图层定义
    /// </summary>
    [GameObjectBinding(path: "[GameModule]/Root/WindowLayer")]
    public class WindowLayerManager : BehaviourSingletonGameObject<WindowLayerManager>
    {
        private readonly Dictionary<LayerIndexInfo, LayerHolder> _layerDict = new();

        /// <summary>
        /// 构建层对象。此方法用于加载游戏资源并应用特定的图层定义到加载的游戏对象。
        /// 由于这些对象是游戏专用的，因此不能直接使用 GameObject 的 API。
        /// </summary>
        /// <param name="onCompletedBuilder">当构建完成时调用的回调。</param>
        public void BuildAsync<T>(Action<GameObject> onCompletedBuilder) where T : WindowLayerBase
        {
            var layerMetaInfo = LayerMetaInfo.GetLayerBindingMetaInfo<T>();

            if (onCompletedBuilder == null)
                throw new ArgumentNullException(nameof(onCompletedBuilder));

            // 定义一个局部函数来处理资产加载成功的情况
            void OnAssetLoaded(string assetName, object asset, float duration, object userdata)
            {
                var gameObject = asset as GameObject;
                if (gameObject != null)
                {
                    T script = GameObjectUtility.BindComponentToGameObject<T>(gameObject, typeof(T));

                    script.LayerMetaInfo = layerMetaInfo;
                    script.Create();

                    Apply(gameObject, layerMetaInfo.LayerIndexInfo);
                    onCompletedBuilder(gameObject);
                }
                else
                {
                    Log.Error($"Failed to cast loaded asset to GameObject: [ {assetName} ]");
                }
            }

            // 定义一个局部函数来处理资产加载失败的情况
            void OnLoadFailed(string assetName, LoadResourceStatus status, string errorMessage, object userData)
            {
                Log.Error($"LoadAssetAsync failed for resource: [ {layerMetaInfo.Location} ]. Status: {status}, Error: {errorMessage}");
            }

            // 开始异步加载资源
            GameModule.Resource.LoadAssetAsync(
                layerMetaInfo.Location,
                typeof(UnityEngine.GameObject),
                new LoadAssetCallbacks(OnAssetLoaded,  OnLoadFailed)
            );
        }

        /// <summary>
        /// 同步载入并应用层级
        /// </summary>
        /// <param name="location"></param>
        public T BuildSync<T>(string location = null) where T : WindowLayerBase
        {
            var layerMetaInfo = LayerMetaInfo.GetLayerBindingMetaInfo<T>();
            if (location != null)
            {
                layerMetaInfo.Location = location;
            }
            GameObject gameObject = GameModule.Resource.LoadGameObject(layerMetaInfo.Location);
            T script = GameObjectUtility.EnsureComponent<T>(gameObject);

            script.LayerMetaInfo = layerMetaInfo;
            script.Create();

            Apply(gameObject, layerMetaInfo.LayerIndexInfo);
            return script;
        }

        /// <summary>
        /// 构建空的层级对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T BuildNull<T>() where T: WindowLayerBase
        {
            var layerMetaInfo = LayerMetaInfo.GetLayerBindingMetaInfo<T>();
            string name = typeof(T).Name;
            GameObject gameObject = new(name);
            T script = GameObjectUtility.BindComponentToGameObject<T>(gameObject, typeof(T));
            script.LayerMetaInfo = layerMetaInfo;
            script.Create();
            Apply(gameObject, layerMetaInfo.LayerIndexInfo);
            return script;
        }


        /// <summary>
        /// 应用索引
        /// </summary>
        /// <param name="go"></param>
        /// <param name="indexInfo"></param>
        public void Apply(GameObject go, LayerIndexInfo indexInfo)
        {
            LayerUtility.SetCameraIndex(go, indexInfo.CameraIndex);
            LayerUtility.SetLayerIndexInCanvas(go, indexInfo.LayerIndex);
            LayerUtility.SetLayerIndexInRender(go, indexInfo.LayerIndex);
        }

        /// <summary>
        /// 获得给定层级根对象
        /// </summary>
        /// <param name="layerIndexInfo"></param>
        /// <returns></returns>
        public GameObject GetLayerRootObject(LayerIndexInfo layerIndexInfo)
        {
            if (_layerDict.TryGetValue(layerIndexInfo, out LayerHolder holder))
            {
                return holder.GameObject;
            }
            return null;
        }

        /// <summary>
        /// 清除给定层级里的对象
        /// </summary>
        /// <param name="layerIndexInfo"></param>
        public void ClearLayerObject(LayerIndexInfo layerIndexInfo)
        {
            if (_layerDict.TryGetValue(layerIndexInfo, out LayerHolder holder))
            {
                GameObjectUtility.ClearChildGameObject(holder.GameObject, false);
            }
        }


        /// <summary>
        /// 初始化图层
        /// </summary>
        public void Initialize()
        {
            InitializeHolder();
            InitializeScript();
        }
        /// <summary>
        /// 构建层级对象
        /// </summary>
        /// <param name="rootGo"></param>
        private void InitializeHolder( )
        {
            var gameObject = GameObject.Find("Scene2d");
            Transform scene2d = gameObject != null ?  gameObject.transform: CreateScene2D().transform;

            foreach (var layer in typeof(WindowLayerDefinition).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                var indexInfo = (LayerIndexInfo)layer.GetValue(null);
                string layerName = layer.Name;

                Transform tr = scene2d.Find(layerName);
                GameObject layerGameObject = tr != null ? tr.gameObject : new GameObject(layerName);

                if (tr == null)
                {
                    GameObjectUtility.AddGameObject(scene2d.gameObject, layerGameObject);
                }

                LayerHolder layerHolder = new ()
                {
                    GameObject = layerGameObject,
                    LayerName = layerName,
                    LayerIndexInfo = indexInfo
                };

                _layerDict[indexInfo] = layerHolder;
            }
        }

        /// <summary>
        ///  为所有相机为5的对象绑定一个canvas脚本, 并设置其正确的索引层级
        /// </summary>
        /// <returns></returns>
        private void InitializeScript()
        {
            // 目前只有一个相机
            var uiCamera = InitializeCamera();   
            foreach (var layer in typeof(WindowLayerDefinition).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                var layerIndexInfo = (LayerIndexInfo)layer.GetValue(null);
                if (layerIndexInfo.CameraIndex == 5)
                {
                    GameObject layerGo = GetLayerRootObject(layerIndexInfo);
                    if (layerGo != null)
                    {
                        var canvas = GameObjectUtility.EnsureComponent<Canvas>(layerGo);
                        canvas.renderMode = RenderMode.ScreenSpaceCamera;
                        canvas.worldCamera = uiCamera;
                        GameObjectUtility.EnsureComponent<CanvasScaler>(layerGo);
                        GameObjectUtility.EnsureComponent<GraphicRaycaster>(layerGo);

                        LayerUtility.SetCameraIndex(layerGo, layerIndexInfo.CameraIndex);
                        LayerUtility.SetLayerIndexInCanvas(layerGo, layerIndexInfo.LayerIndex);
                    }
                }
            }
        }


        /// <summary>
        /// 构建摄像机，默认从场景中获取UI摄像机，如果没有，则自己构建对象并绑定摄像机
        /// </summary>
        private Camera InitializeCamera() { 
            GameObject uiCameraGo = GameObject.Find("UICamera");
            if (uiCameraGo == null)
            {
                uiCameraGo = new GameObject("UICamera");
                GameObjectUtility.AddGameObject(GameObject.Find("Root"), uiCameraGo);
            }
            return GameObjectUtility.EnsureComponent<Camera>(uiCameraGo);
        }

        /// <summary>
        /// 创建一个场景根对象
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        private GameObject CreateScene2D( )
        {
            GameObject scene2d = new("Scene2d");
            DontDestroyOnLoad(scene2d);
            return scene2d;
        }

        public void SwapLayer(LayerIndexInfo srcLayerDef, LayerIndexInfo destLayerDef)
        {
            GameObject destGo = GetLayerRootObject(destLayerDef);
            if (destGo != null && destGo.transform.childCount > 0)
            {
                Debug.LogError($"Destination layer {destLayerDef} is not empty.");
                return;
            }

            GameObject srcGo = GetLayerRootObject(srcLayerDef);
            if (srcGo != null)
            {
                foreach (Transform child in srcGo.transform)
                {
                    Apply(child.gameObject, destLayerDef);
                    child.SetParent(GetLayerRootObject(destLayerDef).transform);
                }
            }
        }

        private class LayerHolder
        {
            public GameObject GameObject { get; set; }
            public string LayerName { get; set; }
            public LayerIndexInfo LayerIndexInfo { get; set; }
        }
    }


}
