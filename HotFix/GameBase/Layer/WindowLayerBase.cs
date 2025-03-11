using GameBase.Scene;
using GameBase.Utility;
using System;
using TEngine;
using UnityEngine;
using static GameBase.Layer.LayerIndexInfo;

namespace GameBase.Layer
{
    public class WindowLayerBase : MonoBehaviour
    {
        /// <summary>
        /// 层的基本属性
        /// </summary>
        public LayerMetaInfo LayerMetaInfo { set; get; }
        /// <summary>
        /// 窗口矩阵位置组件。
        /// </summary>
        public virtual RectTransform rectTransform { protected set; get; }

        public void Create()
        {
            rectTransform = GetComponent<RectTransform>();
            ScriptGenerator();
        }

        /// <summary>
        /// 创建
        /// </summary>
        public virtual void Initialize()
        {
            
        }


        /// <summary>
        /// 代码自动生成绑定。
        /// </summary>
        protected virtual void ScriptGenerator()
        {
        }

        void OnDestroy() {
            Dispose();
        }

        public virtual void Dispose()
        {
            
        }

        public string LayerNameStr 
        { 
            get 
            {
                return LayerMetaInfo.LayerIndexInfo.LayerName.ToString();
            }
        }

        /// <summary>
        /// 对给定对象应用层索引
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="offset"></param>
        /// <param name="layerMetaInfo"></param> 特殊情况别的层可能嵌入到这个层里
        protected void ApplyLayerIndex(Transform transform, int offset=0, LayerMetaInfo layerMetaInfo=null) 
        {
            layerMetaInfo ??= LayerMetaInfo;

            LayerUtility.SetLayerIndexInRender(transform.gameObject, layerMetaInfo.LayerIndexInfo.LayerIndex + offset);
            LayerUtility.SetLayerIndexInCanvas(transform.gameObject, layerMetaInfo.LayerIndexInfo.LayerIndex + offset);
        }

        /// <summary>
        /// 应用相机索引
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="offset"></param>
        /// <param name="layerMetaInfo"></param> 特殊情况别的层可能嵌入到这个层里
        protected void ApplyCameraIndex(Transform transform, int offset = 0, LayerMetaInfo layerMetaInfo = null)
        {
            layerMetaInfo ??= LayerMetaInfo;

            var cameraIndex = layerMetaInfo.LayerIndexInfo.CameraIndex + offset;
            if(cameraIndex > 31 || cameraIndex < 0){ 
                throw new Exception("CameraIndex out of range");
            }
            LayerUtility.SetCameraIndex(transform.gameObject, cameraIndex);
        }



        #region FindChildComponent

        public Transform FindChild(string path)
        {
            return UnityExtension.FindChild(rectTransform, path);
        }

        public Transform FindChild(Transform trans, string path)
        {
            return UnityExtension.FindChild(trans, path);
        }

        public T FindChildComponent<T>(string path) where T : Component
        {
            return UnityExtension.FindChildComponent<T>(rectTransform, path);
        }

        public T FindChildComponent<T>(Transform trans, string path) where T : Component
        {
            return UnityExtension.FindChildComponent<T>(trans, path);
        }

        #endregion
    }
}
