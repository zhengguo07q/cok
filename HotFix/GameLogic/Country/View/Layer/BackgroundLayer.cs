using GameBase.Layer;
using GameBase.Utility;
using GameLogic.Country.Manager;
using TEngine;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GameLogic.Country.View.Layer
{
    [LayerBinding(layerName: LayerName.BackgroundLayer)]
    public class BackgroundLayer : WindowLayerBase
    {
        private SceneReferenceManager SceneRef => SceneReferenceManager.Instance;
        private ViewportManager ViewportManager => ViewportManager.Instance;

        [Header("References")]
        [SerializeField] private Transform backgroundLayerTs;
        [SerializeField] private Grid grid;
        [SerializeField] private Tilemap tilemap;

        private Tile backgroundTile;
        private bool isInitialized = false;

        public override void Initialize()
        {
            ViewportManager.OnViewportInitialized += HandleViewportInitialized;
            
            SetupGrid();
            LoadBackgroundTile();
        }

        private void HandleViewportInitialized()
        {
            if (!isInitialized && backgroundTile != null)
            {
                isInitialized = true;
                SetupBackground();
            }
        }

        private void SetupGrid()
        {
            backgroundLayerTs = FindChild(SceneReferenceManager.Instance.Scene.SceneGameObject.transform, "SceneMap/BackgroundLayer");
            GameObject gridObj = new GameObject("Grid");
            grid = gridObj.AddComponent<Grid>();
            grid.cellLayout = GridLayout.CellLayout.Isometric;

            GameObject tilemapObj = new GameObject("Tilemap");
            tilemapObj.transform.SetParent(gridObj.transform);
            tilemap = tilemapObj.AddComponent<Tilemap>();
            tilemapObj.AddComponent<TilemapRenderer>();
            gridObj.transform.SetParent(backgroundLayerTs);

            TransformUtility.ScaleParentWithChildren(backgroundLayerTs, new Vector3(10, 10, 1));
            ApplyCameraIndex(backgroundLayerTs);
            ApplyLayerIndex(backgroundLayerTs);
        }

        private void LoadBackgroundTile()
        {
            GameModule.Resource.LoadAsset<Sprite>("BackgroudLayer_background_1", (sprite) =>
            {
                if (sprite == null)
                {
                    Debug.LogError("Background sprite not found");
                    return;
                }

                backgroundTile = ScriptableObject.CreateInstance<Tile>();
                backgroundTile.sprite = sprite;

                if (ViewportManager.IsViewportInitialized && !isInitialized)
                {
                    HandleViewportInitialized();
                }
            });
        }

        private void SetupBackground()
        {
            tilemap.SetTile(new Vector3Int(0, 0, 0), backgroundTile);
        }

        protected void OnDestroy()
        {
            if (ViewportManager != null)
            {
                ViewportManager.OnViewportInitialized -= HandleViewportInitialized;
            }
            if (backgroundTile != null)
            {
                Destroy(backgroundTile);
            }
        }
    }
}