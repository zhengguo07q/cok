using GameBase.Layer;
using GameBase.Loader;
using GameBase.Utility;
using System;
using System.Collections.Generic;
using TEngine;
using UnityEngine;

namespace GameBase.Scene
{
    [LayerBinding(LayerName.LoadingLayer)]
    public class SceneLoadingLayer :WindowLayerBase{
        
    }
    public  class SceneInstanceBase : MonoBehaviour
    {
        /// <summary>
        /// 场景资源定义
        /// </summary>
        public SceneMetaInfo SceneMetaInfo { get; set; }

        /// <summary>
        /// 场景资源的队列载入器
        /// </summary>
        private ListLoader sceneListLoader;
        
        /// <summary>
        /// 载入器
        /// </summary>

        private WindowLayerBase loader;

        /// <summary>
        /// 是否延迟销毁
        /// </summary>
        public bool DelayDestroyLoader { get; set; }

        protected virtual void Awake()
        {
            SceneMetaInfo = null;
            loader = null;
            sceneListLoader = new ListLoader();
            DelayDestroyLoader = false;
        }

        public virtual void Initialize()
        {

        }

        /// <summary>
        /// 开始场景，这个时候会构建场景遮罩层
        /// </summary>
        public virtual void StartScene()
        {
            Log.Debug($"SceneInstanceBase:startScene:{SceneMetaInfo.SceneSwitchType}");
            GL.Clear(false, true, Color.black);
            if (SceneMetaInfo.LoadingResource != null)
            {
                loader = WindowLayerManager.Instance.BuildSync<SceneLoadingLayer>(SceneMetaInfo.LoadingResource);
                GameObjectUtility.AddGameObject(WindowLayerManager.Instance.GetLayerRootObject(WindowLayerDefinition.LoadingLayer), loader.gameObject);
            }

            foreach (var asset in SceneMetaInfo.PreAssets)
            {
                sceneListLoader.PutWaitLoad(asset);
            }

            sceneListLoader.SetCallback(LoadPreAssetsComplete);
            sceneListLoader.Load();
        }


        /// <summary>
        // 载入场景必须资源完成
        /// </summary>
        private void LoadPreAssetsComplete()
        {
            if (SceneSwitchManager.Instance.DelayDestroy)
            {
                if (SceneSwitchManager.Instance.CurrentSceneInstance.SceneMetaInfo.SceneSwitchType == SceneSwitchType.MainUI )
                {
                    DelayDestroyLoader = true;
                }
                else
                {
                    DelayDestroyLoader = false;
                }
                SceneSwitchManager.Instance.DestroySceneDelay().Forget();
            }

            EnterScene();
            AfterEnterScene();
        }

        protected virtual void EnterScene()
        {

        }

        private void AfterEnterScene()
        {
            if (sceneListLoader != null)
            {
                sceneListLoader.Dispose();
                sceneListLoader = null;
            }
            if (loader != null) {
                loader.gameObject.SetActive(false);
            }

            SceneSwitchManager.Instance.Notify(SceneSwitchEvent.Into, this);

            Resources.UnloadUnusedAssets();
        }


        public virtual void ExitScene()
        {
            Debug.Log($"SceneInstanceBase:exitScene:{SceneMetaInfo.SceneType}");
        }

        public virtual void DestroyScene()
        {
            if (SceneMetaInfo.LoadingResource != null)
            {
                GameObjectUtility.ClearChildGameObject(WindowLayerManager.Instance.GetLayerRootObject(WindowLayerDefinition.LoadingLayer), true);
            }
        }

        public virtual void Dispose()
        {

        }

        private void Update()
        {
            GL.Clear(false, true, Color.black);

            if (sceneListLoader != null)
            {
                sceneListLoader.Update();
            }
            else
            {
                sceneListLoader = new ListLoader();
            }

            // WindowCamera.Instance.Update();
        }

        #region 预处理UI相关

        /// <summary>
        /// 载入预处理的UI
        /// </summary>
        /// <param name="callback"></param>
        protected virtual void LoadPerfabUI(Action callback)
        {
            var prepareUIList = GetShouldPrepareUI();
            if (prepareUIList != null)
            {
                int prepareUICount = prepareUIList.Count;
                LoadUI(prepareUIList[0], () => callback?.Invoke());
            }
            else
            {
                callback?.Invoke();
            }
        }

        protected List<UIWindow> PrepareUIList { get; set; }

        protected virtual void LoadUI(UIWindow panelData, Action callback)
        {
            if (PrepareUIList == null || PrepareUIList.Count == 0 || panelData == null)
            {
                callback?.Invoke();
                return;
            }

            //WindowStack.Instance.OpenWindow(panelData, null, false, panel =>
            //{
            //    panel.Close();
            //    PrepareUIList.RemoveAt(0);
            //    LoadUI(PrepareUIList.Count > 0 ? PrepareUIList[0] : null, callback);
            //}, true);
        }

        /// <summary>
        /// 获得所有的预处理UI， 子类需要重写这个类
        /// </summary>
        /// <returns></returns>
        protected virtual List<UIWindow> GetShouldPrepareUI()
        {
            var allPrepareUIList = PrepareUI();
            if (allPrepareUIList == null)
            {
                return null;
            }

            return allPrepareUIList;
        }

        protected virtual List<UIWindow> PrepareUI()
        {
            return null;
        }

        #endregion
    }
}