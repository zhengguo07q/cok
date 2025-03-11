using GameBase;
using GameBase.Layer;
using GameBase.Scene;
using GameBase.Utility;
using GameLogic.Country.Manager;
using GameLogic.Country.View.Layer;
using TEngine;
using UnityEngine;

namespace GameLogic.Country.View
{
    public class MapConfig : Singleton<MapConfig>
    {
        public Rect MapSize; // 地图大小 

        protected override void Init() 
        {
            MapSize = new Rect(-60000f, -60000f, 120000f, 120000f);
        }
    }
    [SceneBinding(SceneSwitchType.Country)]
    public class CountryScene : SceneInstanceBase
    {
        public GameObject SceneGameObject { get; set; }

        public BackgroundLayer BackgroundLayer { get; set; }
        public TileLayer TileLayer { get; set; }
        public GridLayer GridLayer { get; set; }
        public PathLayer PathLayer { get; set; }
        public SceneObjectLayer SceneObjectLayer { get; set; }
        public MainUILayer MainUILayer { get; set; }



        public override void Initialize() {
            LoadResource();

            BackgroundLayer = WindowLayerManager.Instance.BuildNull<BackgroundLayer>();
            BackgroundLayer.Initialize();
            GameObjectUtility.AddGameObject(WindowLayerManager.Instance.GetLayerRootObject(WindowLayerDefinition.BackgroundLayer), BackgroundLayer.gameObject);

            TileLayer = WindowLayerManager.Instance.BuildNull<TileLayer>();
            TileLayer.Initialize();
            GameObjectUtility.AddGameObject(WindowLayerManager.Instance.GetLayerRootObject(WindowLayerDefinition.TileLayer), TileLayer.gameObject);

            GridLayer = WindowLayerManager.Instance.BuildNull<GridLayer>();
            GridLayer.Initialize();
            GameObjectUtility.AddGameObject(WindowLayerManager.Instance.GetLayerRootObject(WindowLayerDefinition.GridLayer), GridLayer.gameObject);

            PathLayer = WindowLayerManager.Instance.BuildNull<PathLayer>();
            PathLayer.Initialize();
            GameObjectUtility.AddGameObject(WindowLayerManager.Instance.GetLayerRootObject(WindowLayerDefinition.PathLayer), PathLayer.gameObject);

            SceneObjectLayer = WindowLayerManager.Instance.BuildNull<SceneObjectLayer>();
            SceneObjectLayer.Initialize();
            GameObjectUtility.AddGameObject(WindowLayerManager.Instance.GetLayerRootObject(WindowLayerDefinition.SceneObjectLayer), SceneObjectLayer.gameObject);

            MainUILayer = WindowLayerManager.Instance.BuildSync<MainUILayer>();
            MainUILayer.Initialize();
            GameObjectUtility.AddGameObject(WindowLayerManager.Instance.GetLayerRootObject(WindowLayerDefinition.MainUILayer), MainUILayer.gameObject);


            CameraUtility.SetRenderTypeAndStack(SceneReferenceManager.Instance.Camera);

            // 同步当前王国的对象
            SceneObjectManager.Instance.CurrentKingdomId = 1;
            SceneObjectManager.Instance.SyncCurrentKingdomObjects();
            CountryRepo.Instance.ListMapObjects(SceneObjectManager.Instance.CurrentKingdomId);
            ViewportManager.Instance.InitializeViewport(new Vector3(0, 0, -10));
        }

        private void LoadResource()
        {
            var sceneMap = GameModule.Resource.LoadGameObject("Scenes_Country");
            sceneMap.name = "Country";
            SceneGameObject = sceneMap;

            SceneReferenceManager.Instance.Initialize(this);
            ViewportManager.Instance.Initialize();
           // MapLODManager.Instance.Initialize();
           // CameraAnimationManager.Instance.Initialize();
            VisibilityManager.Instance.Initialize();
        }


        public override void Dispose()
        {
            ViewportManager.Instance.Dispose();
          //  MapLODManager.Instance.Dispose();
          //  CameraAnimationManager.Instance.Dispose();
            VisibilityManager.Instance.Dispose();
            SceneReferenceManager.Instance.Dispose(); // 最后才销毁资源

            WindowLayerManager.Instance.ClearLayerObject(WindowLayerDefinition.BackgroundLayer);
            WindowLayerManager.Instance.ClearLayerObject(WindowLayerDefinition.GridLayer);
            WindowLayerManager.Instance.ClearLayerObject(WindowLayerDefinition.PathLayer);
            WindowLayerManager.Instance.ClearLayerObject(WindowLayerDefinition.SceneObjectLayer);
            WindowLayerManager.Instance.ClearLayerObject(WindowLayerDefinition.MainUILayer);

            CameraUtility.SetRenderTypeAndStack();

            if (SceneGameObject != null)
            {
                GameObject.Destroy(SceneGameObject);
                SceneGameObject = null;
            }
        }

        /// <summary>
        /// 更新所有层的显示状态
        /// </summary>
        public void UpdateLayersByScaleLevel(MapLODLevel level)
        {
            // 更新背景层
            if (BackgroundLayer != null)
            {
            //    BackgroundLayer.UpdateByScaleLevel(level);
            }
            // 更新Tile层
            if (TileLayer != null)
            {
                TileLayer.UpdateByScaleLevel(level);
            }
            // 更新场景对象层
            if (SceneObjectLayer != null)
            {
                SceneObjectLayer.UpdateObjectsByScaleLevel(level);
            }
        }
    }
}
