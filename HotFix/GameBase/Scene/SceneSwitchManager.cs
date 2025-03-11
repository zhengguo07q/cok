using Cysharp.Threading.Tasks;
using GameBase.Layer;
using GameBase.Utility;
using System;
using System.Collections.Generic;
using TEngine;
using UnityEngine;

namespace GameBase.Scene
{
    public enum SceneSwitchEvent
    {
        Null = 1,
        Create = 2,
        Exit = 3,
        Destroy = 4,
        Into = 5,
    }

    public enum SceneSwitchType
    {
        Login = 1,       // 登录
        MainUI = 2,      // 主界面
        Country = 3,    // 国家
        Battle = 4,         // 战斗
    }

    /// <summary>
    /// U3D的场景API支持了事件，路径与对象映射等基础行为
    /// TENGINE支持了YooAssets资源异步载入，YooAssets场景切换资源卸载，子场景附件。
    /// 此模块支持了场景切换事件，场景切换资源预载入，过度载入器，场景定义等业务基础逻辑
    /// </summary>
    [GameObjectBinding(path: "[GameModule]/Root/SceneSwitch")]
    public class SceneSwitchManager : BehaviourSingletonGameObject<SceneSwitchManager>
    {
        private List<Action<SceneSwitchEvent, SceneInstanceBase>> listeners = new List<Action<SceneSwitchEvent, SceneInstanceBase>>();

        /// <summary>
        /// 上一次的场景引用，会用于延迟销毁等机制
        /// </summary>
        private SceneMetaInfo LastSceneRes { get; set; }
        /// <summary>
        /// 上一次的场景引用，会用于延迟销毁等机制
        /// </summary>
        public SceneInstanceBase LastSceneInstance { get; set; }
        /// <summary>
        /// 当前正在进行的场景
        /// </summary>
        public SceneInstanceBase CurrentSceneInstance { get; set; }

        private GameObject LastSceneWindow;
        public bool DelayDestroy { get; set; }

        public void Awake()
        {
            listeners = new List<Action<SceneSwitchEvent, SceneInstanceBase>>();
            LastSceneRes = null;
            LastSceneInstance = null;
            CurrentSceneInstance = null;
            LastSceneWindow = null;
            DelayDestroy = false;
        }

        /// <summary>
        ///  添加场景监听器
        /// </summary>
        /// <param name="listener"></param>
        public void AddListener(Action<SceneSwitchEvent, SceneInstanceBase> listener)
        {
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }

        /// <summary>
        /// 删除场景监听器
        /// </summary>
        /// <param name="listener"></param>
        public void RemoveListener(Action<SceneSwitchEvent, SceneInstanceBase> listener)
        {
            listeners.Remove(listener);
        }

        /// <summary>
        /// 通知场景监听器
        /// </summary>
        /// <param name="evtType"></param>
        /// <param name="sceneInstance"></param>
        public void Notify(SceneSwitchEvent evtType, SceneInstanceBase sceneInstance)
        {
            foreach (var listener in listeners)
            {
                listener?.Invoke(evtType, sceneInstance);
            }
        }

        /// <summary>
        /// 进入场景
        /// </summary>
        /// <param name="sceneRes"></param>
        public void EnterScene<T>()
        {
            SceneMetaInfo sceneRes = SceneMetaInfo.GetBindSceneMetaInfo<T>();
            Debug.Log($"SceneSwitchManager:enterScene:{sceneRes.SceneSwitchType}");

            if (CurrentSceneInstance != null)
            {
                if (CurrentSceneInstance.SceneMetaInfo.SceneSwitchType == sceneRes.SceneSwitchType)
                {
                    DelayDestroy = false;
                }
                else
                {
                    DelayDestroy = true;
                }

                Notify(SceneSwitchEvent.Exit, CurrentSceneInstance);
                CurrentSceneInstance.ExitScene();
                LastSceneInstance = CurrentSceneInstance;

                if (!DelayDestroy)
                {
                    DestroySceneDelay().Forget(); // 需要延迟销毁的场景
                }
                DestroySceneNow();
            }

            CreateScene(sceneRes);
        }

        /// <summary>
        /// 创建场景
        /// </summary>
        /// <param name="sceneRes"></param>
        private void CreateScene(SceneMetaInfo sceneRes)
        {
            Debug.Log($"SceneSwitchManager:createScene:{sceneRes.SceneSwitchType}");

            GameObjectUtility.ClearChildGameObjects(gameObject, true);
            GameObject sceneGo = GameObjectUtility.CreateNullGameObject(gameObject, sceneRes.SceneSwitchType.ToString());
            
            CurrentSceneInstance = GameObjectUtility.BindComponentToGameObject<SceneInstanceBase>(sceneGo, sceneRes.SceneType);
            Notify(SceneSwitchEvent.Create, CurrentSceneInstance);
            CurrentSceneInstance.Initialize();
            CurrentSceneInstance.SceneMetaInfo = sceneRes;

            CurrentSceneInstance.StartScene();

            if (LastSceneInstance != null)
            {
                LastSceneRes = LastSceneInstance.SceneMetaInfo;
            }
        }

        /// <summary>
        /// 立刻销毁场景, 进入场景后需要销毁上一个场景
        /// </summary>
        private void DestroySceneNow()
        {
            if (LastSceneInstance != null)
            {
                DestroySceneImpl();
            }
        }

        /// <summary>
        /// 延迟销毁场景, 退出场景后需要延迟销毁上一个场景， 
        /// </summary>
        /// <returns></returns>
        public async UniTaskVoid DestroySceneDelay() {
            await UniTask.DelayFrame(3);
            if (LastSceneInstance != null)
            {
                DestroySceneImpl();
            }
            GameModule.Resource.UnloadUnusedAssets(true);
            GC.Collect();
        }

        /// <summary>
        /// 销毁场景
        /// </summary>
        private void DestroySceneImpl()
        {
            Debug.Log($"SceneSwitchManager:delayDestroyScene:{LastSceneInstance.SceneMetaInfo.SceneType}");
            Notify(SceneSwitchEvent.Destroy, LastSceneInstance);
            LastSceneInstance.DestroyScene();
            LastSceneInstance.Dispose();
            GameObjectUtility.DestroyGameObject(LastSceneInstance.gameObject, false);
        }

        /// <summary>
        /// 获得需要的特定的场景对象, 如果转换失败，则返回null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetCurrentScene<T>() where T : SceneInstanceBase
        {
            return CurrentSceneInstance as T;
        }
    }

}