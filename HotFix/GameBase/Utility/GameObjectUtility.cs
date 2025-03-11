using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameBase.Utility
{
    /// <summary>
    /// 游戏对象工具
    /// </summary>
    public static class GameObjectUtility
    {
        /// <summary>
        /// 在父GameObject上添加子GameObject，并相应地调整变换属性。
        /// </summary>
        /// <param name="parentGo">父游戏对象</param>
        /// <param name="childGo">需要添加的子游戏对象</param>
        /// <returns>添加的子GameObject。</returns>
        public static GameObject AddGameObject(this GameObject parentGo, GameObject childGo)
        {
            if (parentGo == null || childGo == null)
            {
                Debug.LogError("Parent or child GameObject is null.");
                return null;
            }

            Transform childTransform = childGo.transform;
            childTransform.SetParent(parentGo.transform, false); 
            
            childTransform.localPosition = Vector3.zero;

            childTransform.localScale = Vector3.one;
            childTransform.localRotation = Quaternion.identity;

            return childGo;
        }

        /// <summary>
        /// 实例化一个预制件作为指定父GameObject的子对象。
        /// </summary>
        /// <param name="parent">父游戏对象</param>
        /// <param name="prefab">要实例化的预制件。</param>
        /// <returns>实例化的GameObject。</returns>
        public static GameObject AddGameObjectPrefab(this Transform parent, GameObject prefab)
        {
            if (prefab == null)
            {
                Debug.LogError("Provided prefab is null.");
                return null;
            }

            GameObject go;

            // Instantiate based on the active state of the prefab
            if (prefab.activeSelf)
            {
                go = Instantiate(prefab);
            }
            else
            {
                prefab.SetActive(true);
                go = Instantiate(prefab);
                prefab.SetActive(false);
            }

            if (go != null && parent != null)
            {
                var t = go.transform;
                t.SetParent(parent, false); // World position stays the same
                t.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                t.localScale = Vector3.one;
                go.layer = parent.gameObject.layer;
            }

            return go;
        }

        /// <summary>
        /// 根据给定的路径查找或创建 GameObject，并附加上指定类型的组件。
        /// </summary>
        /// <typeparam name="T">要附加的组件类型</typeparam>
        /// <param name="path">GameObject 的层级路径（例如 "Canvas/Panel/Button"）</param>
        /// <returns>带有指定组件的 GameObject</returns>
        public static T FindOrCreateGameObjectWithComponent<T>(string path) where T : Component
        {
            // 尝试通过路径查找 GameObject
            Transform parent = null;
            string[] parts = path.Split('/');
            GameObject currentObject = null;

            foreach (string part in parts)
            {
                if (parent == null)
                {
                    currentObject = GameObject.Find(part);
                }
                else
                {
                    currentObject = parent.Find(part)?.gameObject;
                }

                if (currentObject == null)
                {
                    // 如果未找到则创建新的 GameObject
                    currentObject = new GameObject(part);
                    if (parent != null)
                    {
                        currentObject.transform.SetParent(parent, false);
                    }
                }

                parent = currentObject.transform;
            }

            // 确保返回的对象包含指定的组件
            if (!currentObject.TryGetComponent<T>(out var component))
            {
                component = currentObject.AddComponent<T>();
            }

            return component;
        }


        /// <summary>
        /// 清除指定游戏对象下的所有子游戏对象。
        /// </summary>
        /// <param name="gameObject">父级游戏对象。</param>
        /// <param name="clearImmediate">如果为 true，则立即清除子对象；否则，在下一帧或更新周期中清除。</param>
        internal static void ClearChildGameObjects(GameObject gameObject, bool clearImmediate)
        {
            if (gameObject == null)
            {
                Debug.LogWarning("The provided GameObject is null.");
                return;
            }

            // 获取所有子对象
            Transform[] childTransforms = new Transform[gameObject.transform.childCount];
            int i = 0;
            foreach (Transform child in gameObject.transform)
            {
                childTransforms[i++] = child;
                
            }

            // 根据提供的标志决定如何销毁子对象
            if (clearImmediate)
            {
                foreach (Transform child in childTransforms)
                {
                    GameObject.DestroyImmediate(child.gameObject);
                }
            }
            else
            {
                foreach (Transform child in childTransforms)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// 创建一个新的空 GameObject，并可选地设置其为给定父级 GameObject 的子对象。
        /// </summary>
        /// <param name="parent">可选的父级 GameObject。</param>
        /// <param name="name">新创建的 GameObject 的名称。</param>
        /// <returns>新创建的空 GameObject。</returns>
        internal static GameObject CreateNullGameObject(GameObject parent, string name)
        {
            // 创建新的空 GameObject 并命名
            GameObject newGameObject = new(name);

            // 如果提供了父级 GameObject，则设置新 GameObject 为它的子对象
            if (parent != null)
            {
                newGameObject.transform.SetParent(parent.transform, false);
            }

            return newGameObject;
        }

        /// <summary>
        /// 销毁给定的游戏对象。
        /// </summary>
        /// <param name="gameObject">要销毁的游戏对象。</param>
        /// <param name="immediate">如果为 true，则立即销毁；否则，在当前帧结束时销毁。</param>
        public static void DestroyGameObject(object gameObject, bool immediate)
        {
            // 检查传入的对象是否为 null 或不是 Unity 的 Object 类型
            if (gameObject == null || gameObject is not UnityEngine.Object unityObject)
            {
                Debug.LogWarning("提供的对象为空或不是 Unity 的 Object 类型。");
                return;
            }

            // 根据提供的标志决定如何销毁对象
            if (immediate)
            {
                // 立即销毁对象
                UnityEngine.Object.DestroyImmediate(unityObject);
            }
            else
            {
                // 在当前帧结束后销毁对象
                UnityEngine.Object.Destroy(unityObject);
            }
        }

        /// <summary>
        /// 克隆给定的游戏对象。
        /// </summary>
        /// <param name="original">要克隆的原始游戏对象。</param>
        /// <returns>克隆后的新游戏对象。</returns>
        public static GameObject Instantiate(GameObject original)
        {
            if (original == null)
            {
                Debug.LogWarning("尝试实例化一个空的游戏对象。");
                return null;
            }

            // 使用 Unity 提供的针对 GameObject 的 Instantiate 方法
            return UnityEngine.Object.Instantiate(original);
        }

        /// <summary>
        /// 清除指定父级 GameObject 下的所有子 GameObject。
        /// </summary>
        /// <param name="parent">父级 GameObject。</param>
        /// <param name="includeInactive">如果为 true，则包括未激活的子对象；否则只清除激活的子对象。</param>
        public static void ClearChildGameObject(GameObject parent, bool includeInactive)
        {
            if (parent == null)
            {
                Debug.LogWarning("提供的父级 GameObject 为空。");
                return;
            }

            // 获取所有子对象的副本（防止在迭代过程中修改集合）
            IList<Transform> children = parent.GetComponentsInChildren<Transform>();

            foreach (Transform child in children)
            {
                // 根据 includeInactive 决定是否销毁未激活的对象
                if ((includeInactive || child.gameObject.activeInHierarchy) && child != parent.transform)
                {
                    UnityEngine.Object.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// 给定一个继承自 MonoBehaviour 的 Type 和一个 GameObject，创建该类型的组件并绑定到 GameObject 上。
        /// </summary>
        /// <param name="gameObject">要绑定组件的目标 GameObject。</param>
        /// <param name="componentType">要创建的组件类型。</param>
        /// <returns>新创建的组件实例。</returns>
        /// <exception cref="ArgumentException">如果提供的类型不是继承自 MonoBehaviour 或者是 null。</exception>
        public static T BindComponentToGameObject<T>(GameObject gameObject, Type componentType) where T: Component
        {
            // 确保提供的类型不是 null 并且是继承自 MonoBehaviour 的
            if (componentType == null)
                throw new ArgumentException("The provided type is null.", nameof(componentType));

            if (!typeof(Behaviour).IsAssignableFrom(componentType))
                throw new ArgumentException("The provided type must inherit from Behaviour.", nameof(componentType));

            // 创建并返回新的组件实例
            return gameObject.AddComponent(componentType) as T;
        }

        /// 确保游戏对象上存在给定类型T的组件，如果没有， 则添加一个
        /// <typeparam name="T">要添加的组件的类型</typeparam>
        /// <param name="gameObject">要检查的或添加组件的游戏对象</param>
        /// <returns>游戏对象上的组件实例</returns>
        public static T EnsureComponent<T>(this GameObject gameObject) where T : Component
        {
            // 检查游戏对象是否为空
            if (gameObject == null)
                throw new ArgumentNullException(nameof(gameObject), "The provided game object is null.");

            // 检查游戏对象是否已经存在给定类型的组件
            T component = gameObject.GetComponent<T>();
            if (component != null)
                return component;

            // 如果游戏对象上不存在给定类型的组件，则添加一个
            return gameObject.AddComponent<T>();
        }



    }
}
