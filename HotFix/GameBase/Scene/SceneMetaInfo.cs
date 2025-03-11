using System;
using System.Linq;
using TEngine;
using UnityEngine;

namespace GameBase.Scene
{
    /// <summary>
    /// 游戏物体上设置路径等属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class SceneBindingAttribute : Attribute
    {
        /// <summary>
        /// 切换的场景定义，不能使用T作为类型，因为一个场景类可能在多个场景中使用
        /// </summary>
        public SceneSwitchType SceneSwitchType { get; }

        /// <summary>
        /// 装载器资源
        /// </summary>
        public string LoadingResource { get; }

        /// <summary>
        /// 进入场景需要进行预处理资源的列表
        /// </summary>
        public string[] PreAssets { get; }


        public SceneBindingAttribute(SceneSwitchType sceneSwitchType, string loadingResource = null, string[] preAssets = null)
        {
            SceneSwitchType = sceneSwitchType;
            LoadingResource = loadingResource;
            PreAssets = preAssets;
        }
    }
    /// <summary>
    /// 场景资源定义
    /// </summary>
    public class SceneMetaInfo
    {
        /// <summary>
        /// 切换类型
        /// </summary>
        public SceneSwitchType SceneSwitchType { get; internal set; }

        /// <summary>
        ///  脚本类型
        /// </summary>
        public Type SceneType { get; internal set; }

        /// <summary>
        /// 场景载入器
        /// </summary>
        public string LoadingResource { get; internal set; }

        /// <summary>
        /// 预加载资源列表
        /// </summary>
        public string[] PreAssets { get; }


        public SceneMetaInfo(SceneSwitchType sceneSwitchType, Type sceneType, string loadingResource = null, string[] preAssets = null)
        {
            SceneSwitchType = sceneSwitchType;
            SceneType = sceneType;
            LoadingResource = loadingResource;
            PreAssets = preAssets??new string[0];
        }

        /// <summary>
        /// 获得绑定的场景资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static SceneMetaInfo GetBindSceneMetaInfo<T>()
        {
            SceneMetaInfo sceneRes = null;
            Type type = typeof(T);
            var attributes = type.GetCustomAttributes(typeof(SceneBindingAttribute), false);

            foreach (SceneBindingAttribute attr in attributes.Cast<SceneBindingAttribute>())
            {
                sceneRes = new SceneMetaInfo(attr.SceneSwitchType, type, attr.LoadingResource, attr.PreAssets);
            }
            return sceneRes;
        }
    }

}