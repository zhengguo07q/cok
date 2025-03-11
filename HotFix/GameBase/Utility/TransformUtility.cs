using System.Collections.Generic;
using UnityEngine;


namespace GameBase.Utility
{
    public static class TransformUtility
    {
        /// <summary>
        ///  缩放父对象时保持子对象视觉大小不变
        /// </summary>
        /// <param name="parentScale"></param>
        public static void ScaleParentWithChildren(Transform transform, Vector3 parentScale)
        {
            Transform parent = transform;

            // 1. 解除所有子对象的父子关系
            List<Transform> children = new List<Transform>();
            foreach (Transform child in parent)
            {
                children.Add(child);
                child.SetParent(null);
            }

            // 2. 缩放父对象
            parent.localScale = parentScale;

            // 3. 重新设置父子关系并恢复子对象原始本地缩放
            foreach (Transform child in children)
            {
                child.SetParent(parent);
                child.localScale = Vector3.one; // 保持子对象本地缩放为1
            }
        }

        /// <summary>
        /// 根据给定路径查找子对象上的组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T FindChildComponent<T>(Transform parent, string path) where T : Component
        {
            Transform child = parent.Find(path);
            return child != null ? child.GetComponent<T>() : null;
        }


        /// <summary>
        /// 根据给定路径查找子对象
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Transform FindChild(Transform parent, string path)
        {
            return parent.Find(path);
        }
    }
}
