using GameBase;
using GameLogic.Country.View;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using GameBase.Utility;

namespace GameLogic.Country.Manager
{
    [GameObjectBinding(path: "[GameModule]/Root/SceneReferenceManager")]
    public class SceneReferenceManager : BehaviourSingletonGameObject<SceneReferenceManager>
    {
        public CountryScene Scene { get; set; }
        public Camera Camera { get; private set; }
        public PixelPerfectCamera PixelPerfectCamera { get; private set; }

        public UniversalAdditionalCameraData CameraData;

        public Transform CameraTs { get; private set; }
        public Transform MapTs { get; private set; }


        public void Initialize(CountryScene scene)
        {
            Scene = scene;
            string cameraPath = "SceneCamera";
            Camera = TransformUtility.FindChildComponent<Camera>(Scene.SceneGameObject.transform, cameraPath);
            CameraTs = Camera.transform;
            CameraData = Camera.GetUniversalAdditionalCameraData();
            PixelPerfectCamera = TransformUtility.FindChildComponent<PixelPerfectCamera>(Scene.SceneGameObject.transform, cameraPath);
            MapTs = TransformUtility.FindChild(Scene.SceneGameObject.transform, "SceneMap");
        }



        public void Dispose()
        {
            Scene = null;
            Camera = null;
            CameraData = null;
            PixelPerfectCamera = null;
            MapTs = null;
        }
    }
}