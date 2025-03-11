using GameBase.Utility;
using System;
using System.Linq;
using TEngine;
using UnityEngine;

namespace GameBase
{
    /// <summary>
    /// 游戏物体上设置路径等属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class GameObjectBindingAttribute : Attribute
    {
        public string Path { get; }
        public string Description { get; }

        public GameObjectBindingAttribute(string path,  string description=null)
        {
            Path = path;
            Description = description;
        }
    }

    /// <summary>
    /// 游戏物体上绑定单例对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BehaviourSingletonGameObject<T> : MonoBehaviour where T : BehaviourSingletonGameObject<T>, new()
    {
        private static T _instance;
        protected static string GameObjectPath;
        // 防止多次初始化
        private static bool _applicationIsQuitting = false;
        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                        "' already destroyed on application quit." +
                        " Won't create again - returning null.");
                    return null;
                }

                if (_instance == null)
                {
                    // 尝试从场景中找到已经存在的对象
                    _instance = FindObjectOfType<T>();

                    if (FindObjectsOfType<T>().Length > 1)
                    {
                        Debug.LogError("[Singleton] Something went really wrong " +
                            " - there should never be more than 1 singleton!" +
                            " Reopening the scene might fix it.");
                        return _instance;
                    }

                    if (_instance == null)
                    {
                        string gameObjectPath = GetGameObjectBindingPath<T>();
                        _instance = GameObjectUtility.FindOrCreateGameObjectWithComponent<T>(gameObjectPath);
                        Log.Assert(_instance != null, $"Failed to create GameObject for singleton {typeof(T).Name}");

                        // 确保单例不会被意外销毁
                        DontDestroyOnLoad(_instance.gameObject);
                    }

                    // 确保单例不会被意外销毁
                    DontDestroyOnLoad(_instance.gameObject);
                }

                return _instance;
            }
        }

        /// <summary>
        /// 获得单例绑定的物体路径
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static string GetGameObjectBindingPath<R>() {
            Type type = typeof(R);
            var attributes = type.GetCustomAttributes(typeof(GameObjectBindingAttribute), false);

            foreach (GameObjectBindingAttribute attr in attributes.Cast<GameObjectBindingAttribute>())
            {
                return attr.Path;
            }
            return "";
        }

        /// <summary>
        /// 当应用程序退出时调用，防止再次创建单例。
        /// </summary>
        private void OnDestroy()
        {
       //     _applicationIsQuitting = true;
        }
    }
}
