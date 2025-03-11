using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GameBase.Utility
{
    /// <summary>
    /// 层工具
    /// </summary>
    public static class LayerUtility
    {
        /// <summary>
        /// 递归地将游戏对象及其所有子对象的图层设置为指定的图层索引。
        /// 默认UI图层为5，
        /// </summary>
        /// <param name="go">要设置图层的游戏对象。</param>
        /// <param name="index">目标图层的索引。</param>
        public static void SetCameraIndex(GameObject go, int index)
        {
            if (go == null)
            {
                Debug.LogWarning("提供的游戏对象为空。");
                return;
            }

            // 设置当前游戏对象的图层
            go.layer = index;

            // 递归设置所有子对象的图层
            foreach (Transform child in go.transform)
            {
                SetCameraIndex(child.gameObject, index);
            }
        }

        /// <summary>
        /// 调整面板（Canvas）的深度。
        /// </summary>
        /// <param name="go">要调整深度的游戏对象。</param>
        /// <param name=")">目标深度值（Sorting Order）。</param>
        public static void SetLayerIndexInCanvas(GameObject go, int index)
        {
            if (go == null)
            {
                Debug.LogWarning("提供的游戏对象为空。");
                return;
            }

            // 尝试获取 Canvas 组件
            Canvas canvas = go.GetComponent<Canvas>();
            if (canvas != null)
            {
                // 如果存在 Canvas，则设置其 Sorting Order
                canvas.sortingOrder = index;
            }
            else
            {
                // 如果没有 Canvas，则尝试调整 RectTransform 的兄弟顺序
                Transform transform = go.transform;
                if (transform.parent != null)
                {
                    transform.SetSiblingIndex(index);
                }
                else
                {
                    Debug.LogWarning("该游戏对象既不是 Canvas 也没有父级 RectTransform，无法调整深度。");
                }
            }

            // 递归调整所有子对象的深度
            foreach (Transform child in go.transform)
            {
                SetLayerIndexInCanvas(child.gameObject, index);
            }
        }

        /// <summary>
        /// 调整面板（Canvas）的深度。
        /// </summary>
        /// <param name="go">要调整深度的游戏对象。</param>
        /// <param name="index">目标深度值（Sorting Order）。</param>
        public static void SetLayerIndexInRender(GameObject go, int index)
        {
            if (go == null)
            {
                Debug.LogWarning("提供的游戏对象为空。");
                return;
            }

            // 尝试获取 Renderer 组件
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                // 如果存在 Renderer，则设置其 Sorting Order
                renderer.sortingOrder = index;
            }
            // 递归调整所有子对象的深度
            foreach (Transform child in go.transform)
            {
                SetLayerIndexInRender(child.gameObject, index);
            }
        }
    }
}
