
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering.Universal;

namespace GameBase.Utility
{
    public class CameraUtility
    {
        public static Camera UICamera;
        /// 设置UI相机为主相机
        public static void SetUICameraAsMainCamera()
        {
            var cameraObject = GameObject.Find("UIRoot/UICamera");
            if (cameraObject == null)
            {
                Debug.LogError("UIRoot/UICamera not found");
                return;
            }
            UICamera = cameraObject.EnsureComponent<Camera>();
            // 配置UI相机参数
            UICamera.orthographic = true;
            UICamera.nearClipPlane = 0.1f;
            UICamera.farClipPlane = 1000f;

            UICamera.clearFlags = CameraClearFlags.Depth;

            // 确保UI相机没有PixelPerfect组件
            var ppc = UICamera.GetComponent<PixelPerfectCamera>();
            if (ppc != null)
            {
                ppc.enabled = false;
            }
        }

        /// <summary>
        /// 调整相机渲染类型， 切换场景的时候可能需要
        /// </summary>
        /// <param name="sceneCamera"></param>
        /// <param name="uiCamera"></param>
        public static void SetRenderTypeAndStack(Camera sceneCamera=null)
        {
            if (sceneCamera == null)
            {
                var uiCameraData = UICamera.GetUniversalAdditionalCameraData();
                uiCameraData.renderType = CameraRenderType.Base;
                uiCameraData.cameraStack.Clear();
            }
            else if (sceneCamera != null) 
            {
                var sceneCameraData = sceneCamera.GetUniversalAdditionalCameraData();

                sceneCameraData.renderType = CameraRenderType.Base;
                sceneCameraData.cameraStack.Clear();

                var uiCameraData = UICamera.GetUniversalAdditionalCameraData();

                // 配置UI相机为Overlay
                uiCameraData.renderType = CameraRenderType.Overlay;

                // 添加前检查是否已存在
                if (!sceneCameraData.cameraStack.Contains(UICamera))
                {
                    sceneCameraData.cameraStack.Add(UICamera);
                }

                UICamera.depth = sceneCamera.depth + 1;
            }
        }
    }
}
