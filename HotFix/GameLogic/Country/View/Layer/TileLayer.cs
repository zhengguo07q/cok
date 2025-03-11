using GameBase.Layer;
using GameConfig.Country;
using GameLogic.Country.Model;
using TEngine;
using UnityEngine;
using UnityEngine.Tilemaps;
using GameLogic.Country.Manager;
using GameLogic.Country.View.Object;

namespace GameLogic.Country.View.Layer
{
    /// <summary>
    /// 地图层，使用Tilemap来实现基础地图显示
    /// </summary>
    [LayerBinding(layerName: LayerName.TileLayer)]
    public class TileLayer : WindowLayerBase
    {
        private SceneReferenceManager SceneRef => SceneReferenceManager.Instance;
        private MapLODManager MapLODManager => MapLODManager.Instance;
        private CameraAnimationManager CameraAnimationManager => CameraAnimationManager.Instance;

        [Header("References")]
        [SerializeField] private Grid grid;
        [SerializeField] private Tilemap tilemap;
        [SerializeField] private GameObject highlightPrefab;

        [Header("Highlight Settings")]
        [SerializeField] private string highlightPrefabPath = "Effects_EffectClickTile";
        [SerializeField] private Vector3 highlightPrefabScaleUnit = Vector3.zero;

        public Tilemap Tilemap => tilemap;

        public override void Initialize()
        {
            SetupReferences();
            LoadHighlightEffect();
        }

        private void SetupReferences()
        {
            grid = FindChildComponent<Grid>(SceneRef.Scene.SceneGameObject.transform, "SceneMap/TileLayer/Grid");
            tilemap = FindChildComponent<Tilemap>(SceneRef.Scene.SceneGameObject.transform, "SceneMap/TileLayer/Grid/Tilemap");

            ApplyCameraIndex(tilemap.transform);
            ApplyLayerIndex(tilemap.transform);
        }

        private void LoadHighlightEffect()
        {
            GameModule.Resource.LoadAsset<GameObject>(highlightPrefabPath, LoadEffectClickTileComplete);
        }

        private void LoadEffectClickTileComplete(GameObject go)
        {
            highlightPrefab = Instantiate(go);
            highlightPrefab.name = "EffectClickTile";
            highlightPrefab.SetActive(false);
            highlightPrefab.transform.SetParent(SceneRef.MapTs);
            
            // 确保高亮预制体的大小正确匹配一个格子
            var spriteRenderer = highlightPrefab.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                // 获取一个格子的实际大小
                Vector3 cellWorldSize = grid.GetComponent<Grid>().cellSize;
                
                // 获取Sprite的实际大小
                Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
                
                // 计算缩放因子，考虑等距视图的投影
                float scaleX = cellWorldSize.x / spriteSize.x; // 移除 0.5f 因子
                float scaleY = cellWorldSize.y / spriteSize.y;

                // 应用缩放
                highlightPrefabScaleUnit = new Vector3(scaleX, scaleY, 1f);
                highlightPrefab.transform.localScale = highlightPrefabScaleUnit;
                
                // 确保Sprite渲染器的设置正确
                spriteRenderer.drawMode = SpriteDrawMode.Simple;
                spriteRenderer.adaptiveModeThreshold = 0.5f;
                spriteRenderer.tileMode = SpriteTileMode.Continuous;
            }
            
            ApplyCameraIndex(highlightPrefab.transform);
        }

        /// <summary>
        /// 点击场景
        /// </summary>
        public void HandleClick(Vector3 mousePosition)
        {
            Vector3Int cellPosition = tilemap.WorldToCell(mousePosition);

            // 首先检查是否点击到场景对象
            var sceneObject = CheckSceneObjectClick(mousePosition);
            if (sceneObject != null)
            {
                var sceneCellPosition = tilemap.WorldToCell(sceneObject.SceneObjectInfo.Position);
                // 如果点击到场景对象，触发场景对象的点击事件
                OnClickSceneObject(sceneCellPosition, sceneObject.SceneObjectInfo);
                return;
            }

            TileBase clickedTile = tilemap.GetTile(cellPosition);

            // 如果没有点击到场景对象，则处理tile的点击
            if (clickedTile != null)
            {
                OnClickEditArea(cellPosition, clickedTile);
            }
            else
            {
                OnClickNullArea(cellPosition);
            }
        }

        /// <summary>
        /// 检查是否点击到场景对象
        /// </summary>
        private SceneObject CheckSceneObjectClick(Vector3 mouseWorldPosition)
        {
            // 使用 OverlapPoint 检测点击位置的碰撞体
            Collider2D hitCollider = Physics2D.OverlapPoint(mouseWorldPosition);
            if (hitCollider != null)
            {
                // 获取碰撞体所属的场景对象
                return hitCollider.GetComponent<SceneObject>();
            }

            return null;
        }

        /// <summary>
        /// 点击空区域
        /// </summary>
        private void OnClickNullArea(Vector3Int cellPosition)
        {
            int scope = 1;
            ShowHighlight(cellPosition, scope);
        }

        /// <summary>
        /// 点击编辑区域
        /// </summary>
        private void OnClickEditArea(Vector3Int cellPosition, TileBase tile)
        {
            int scope = 1;
            ShowHighlight(cellPosition, scope);
        }

        /// <summary>
        /// 点击的是场景对象
        /// </summary>
        private void OnClickSceneObject(Vector3Int position, SceneObjectInfo sceneObjectInfo)
        {
            var scope = sceneObjectInfo.MapObjectEntity.Scope;
            int area = 5;  // 如果对象有不同大小，可以从sceneObjectInfo获取
            ShowHighlight(position, area);
            
            // 这里可以触发场景对象的点击事件
            // 例如：发送事件、显示UI等
            Log.Debug($"点击场景对象：{sceneObjectInfo.MapObjectEntity.ObjectType}");
        }

        /// <summary>
        /// 显示点击的高亮效果
        /// </summary>
        private void ShowHighlight(Vector3Int cellPosition, int area)
        {
            HideHighlight();

            // 获取格子的世界坐标
            Vector3 worldPos = tilemap.CellToWorld(cellPosition);
            
            // 在等距视图中，需要调整Y轴位置以对齐格子中心
            worldPos.y += grid.cellSize.y * 0.5f;
            
            // 设置位置和旋转，但保持缩放不变
            highlightPrefab.transform.position = worldPos;
            highlightPrefab.transform.rotation = Quaternion.identity;
            highlightPrefab.transform.localScale = highlightPrefabScaleUnit * area;
            highlightPrefab.SetActive(true);
        }

        /// <summary>
        /// 隐藏高亮
        /// </summary>
        public void HideHighlight()
        {
            highlightPrefab.SetActive(false);
        }

        /// <summary>
        /// 移动的偏移向量
        /// </summary>
        public void MovePosition(Vector3 moveVector)
        {
            SceneRef.MapTs.position = moveVector;
            Log.Debug($"移动到{SceneRef.MapTs.position}");
        }

        /// <summary>
        /// 更新缩放级别
        /// </summary>
        public void UpdateByScaleLevel(MapLODLevel level)
        {
            switch (level)
            {
                case MapLODLevel.Highest:
                    // 最详细视图
                    break;
                case MapLODLevel.High:
                    // 显示箭头标记
                    break;
                case MapLODLevel.Medium:
                    // 简化显示
                    break;
                case MapLODLevel.Low:
                    // 最简化视图
                    break;
            }
        }
    }

    /// <summary>
    /// 场景对象Tile
    /// </summary>
    public class SceneObjectTile : TileBase
    {
        public long Id { get; set; }
        public Vector3Int Position { get; set; }
        public SceneObjectType SceneObjectType { get; set; }

        public SceneObjectTile(long id, SceneObjectType sceneObjectType, Vector3Int position)
        {
            Id = id;
            SceneObjectType = sceneObjectType;
            Position = position;
        }
    }
}