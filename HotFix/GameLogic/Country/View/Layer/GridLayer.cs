using GameBase.Layer;
using UnityEngine;
using UnityEngine.Tilemaps;
using GameLogic.Country.Manager;

namespace GameLogic.Country.View.Layer
{
    [LayerBinding(layerName: LayerName.GridLayer)]
    public class GridLayer : WindowLayerBase
    {
        private SceneReferenceManager SceneRef => SceneReferenceManager.Instance;
        private ViewportManager ViewportManager => ViewportManager.Instance;

        [Header("Grid Settings")]
        [SerializeField] private Color gridColor = new (0.7f, 0.7f, 0.7f, 0.5f);
        [SerializeField] private float lineWidth = 0.01f;
        [SerializeField] private Transform gridLayerTs;
        private LineRenderer[] horizontalLines;
        private LineRenderer[] verticalLines;
        private Material gridMaterial;

        public override void Initialize()
        {
            gridLayerTs = FindChild(SceneReferenceManager.Instance.Scene.SceneGameObject.transform, "SceneMap/GridLayer");
            // 获取TileLayer的引用
            var tileLayer = SceneRef.Scene.TileLayer;
            if (tileLayer == null || tileLayer.Tilemap == null) return;

            var tilemap = tileLayer.Tilemap;
            
            // 获取Tilemap的边界
            tilemap.CompressBounds();
            BoundsInt bounds = tilemap.cellBounds;
            
            // 创建网格材质
            CreateGridMaterial();
            
            // 创建水平线
            CreateHorizontalLines(bounds, tilemap);
            
            // 创建垂直线
            CreateVerticalLines(bounds, tilemap);

            // 应用相机和层级设置
            ApplyCameraIndex(gridLayerTs);
            ApplyLayerIndex(gridLayerTs);

            // 订阅相机移动事件以更新网格可见性
            if (ViewportManager != null)
            {
                ViewportManager.OnCameraMoved += UpdateGridVisibility;
            }
        }

        private void CreateGridMaterial()
        {
            // 创建网格线材质
            gridMaterial = new Material(Shader.Find("Sprites/Default"));
            gridMaterial.color = gridColor;
        }

        private void CreateHorizontalLines(BoundsInt bounds, Tilemap tilemap)
        {
            int rowCount = bounds.size.y + 1;
            horizontalLines = new LineRenderer[rowCount];

            for (int y = 0; y < rowCount; y++)
            {
                var line = CreateLineRenderer($"HorizontalLine_{y}");
                
                // 设置线的起点和终点
                Vector3 startPos = tilemap.CellToWorld(new Vector3Int(bounds.xMin, bounds.yMin + y, 0));
                Vector3 endPos = tilemap.CellToWorld(new Vector3Int(bounds.xMax, bounds.yMin + y, 0));
                
                line.positionCount = 2;
                line.SetPosition(0, startPos);
                line.SetPosition(1, endPos);
                
                horizontalLines[y] = line;
            }
        }

        private void CreateVerticalLines(BoundsInt bounds, Tilemap tilemap)
        {
            int columnCount = bounds.size.x + 1;
            verticalLines = new LineRenderer[columnCount];

            for (int x = 0; x < columnCount; x++)
            {
                var line = CreateLineRenderer($"VerticalLine_{x}");
                
                // 设置线的起点和终点
                Vector3 startPos = tilemap.CellToWorld(new Vector3Int(bounds.xMin + x, bounds.yMin, 0));
                Vector3 endPos = tilemap.CellToWorld(new Vector3Int(bounds.xMin + x, bounds.yMax, 0));
                
                line.positionCount = 2;
                line.SetPosition(0, startPos);
                line.SetPosition(1, endPos);
                
                verticalLines[x] = line;
            }
        }

        private LineRenderer CreateLineRenderer(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(gridLayerTs);
            
            var line = go.AddComponent<LineRenderer>();
            line.material = gridMaterial;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            line.startColor = gridColor;
            line.endColor = gridColor;
            
            return line;
        }

        
        private void UpdateGridVisibility()
        {
            if (SceneRef.Camera == null) return;

            var camera = SceneRef.Camera;
            float height = 2f * camera.orthographicSize;
            float width = height * camera.aspect;
            
            Vector2 cameraPos2D = new Vector2(
                camera.transform.position.x,
                camera.transform.position.y
            );
            
            Rect viewportRect = new Rect(
                cameraPos2D.x - width * 0.5f,
                cameraPos2D.y - height * 0.5f,
                width,
                height
            );

            // 更新水平线可见性
            if (horizontalLines != null)
            {
                foreach (var line in horizontalLines)
                {
                    if (line != null)
                    {
                        Vector2 start = line.GetPosition(0);
                        Vector2 end = line.GetPosition(1);
                        
                        // 检查线段是否与视口相交
                        bool isVisible = IsLineVisibleInViewport(start, end, viewportRect);
                        line.gameObject.SetActive(isVisible);
                    }
                }
            }

            // 更新垂直线可见性
            if (verticalLines != null)
            {
                foreach (var line in verticalLines)
                {
                    if (line != null)
                    {
                        Vector2 start = line.GetPosition(0);
                        Vector2 end = line.GetPosition(1);
                        
                        // 检查线段是否与视口相交
                        bool isVisible = IsLineVisibleInViewport(start, end, viewportRect);
                        line.gameObject.SetActive(isVisible);
                    }
                }
            }
        }

        private bool IsLineVisibleInViewport(Vector2 start, Vector2 end, Rect viewport)
        {
            // 1. 如果任一端点在视口内，线段可见
            if (viewport.Contains(start) || viewport.Contains(end))
                return true;

            // 2. 检查线段是否与视口相交
            // 计算线段的斜率
            float dx = end.x - start.x;
            float dy = end.y - start.y;
            
            // 检查线段是否与视口的四条边相交
            // 视口的四条边：左、右、上、下
            Vector2[] viewportCorners = new Vector2[]
            {
                new Vector2(viewport.xMin, viewport.yMin), // 左下
                new Vector2(viewport.xMax, viewport.yMin), // 右下
                new Vector2(viewport.xMax, viewport.yMax), // 右上
                new Vector2(viewport.xMin, viewport.yMax)  // 左上
            };

            // 检查线段是否与视口的任意一边相交
            for (int i = 0; i < 4; i++)
            {
                Vector2 edgeStart = viewportCorners[i];
                Vector2 edgeEnd = viewportCorners[(i + 1) % 4];

                if (LineSegmentsIntersect(start, end, edgeStart, edgeEnd))
                {
                    return true;
                }
            }

            return false;
        }

        private bool LineSegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            float denominator = (p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y);

            // 如果分母为0，则线段平行
            if (Mathf.Approximately(denominator, 0))
                return false;

            float ua = ((p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x)) / denominator;
            float ub = ((p2.x - p1.x) * (p1.y - p3.y) - (p2.y - p1.y) * (p1.x - p3.x)) / denominator;

            // 检查交点是否在两条线段上
            return ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1;
        }


        private void OnDestroy()
        {
            if (ViewportManager != null)
            {
                ViewportManager.OnCameraMoved -= UpdateGridVisibility;
            }

            if (gridMaterial != null)
            {
                Destroy(gridMaterial);
            }
        }
    }
}