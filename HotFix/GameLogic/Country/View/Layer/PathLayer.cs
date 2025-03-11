using GameBase.Layer;
using GameLogic.Country.Manager;
using System.Collections.Generic;
using TEngine;
using UnityEngine;

namespace GameLogic.Country.View.Layer
{
    [LayerBinding(layerName: LayerName.PathLayer)]
    public class PathLayer : WindowLayerBase
    {
        private SceneReferenceManager SceneRef => SceneReferenceManager.Instance;

        [SerializeField] private float lineWidth = 0.1f;
        [SerializeField] private float moveSpeed = 1.50f;
        [SerializeField] private Color lineColor = Color.white;
        [SerializeField] private float arrowDensity = 5.0f; // 每个单位长度显示的箭头数量

        [SerializeField] private string pathArrowPrefabPath = "Effects_EffectPathArrow";
        [SerializeField] private GameObject pathArrowPrefab;
        [SerializeField] private Transform PathLayerTs;

        private readonly Dictionary<string, LineRenderer> pathLines = new ();

        public override void Initialize()
        {
            PathLayerTs = FindChildComponent<Transform>(SceneRef.Scene.SceneGameObject.transform, "SceneMap/PathLayer");
            // 加载箭头贴图
            GameModule.Resource.LoadAsset<GameObject>(pathArrowPrefabPath, go =>
            {
                pathArrowPrefab = Instantiate(go);
                pathArrowPrefab.name = "path";
                pathArrowPrefab.SetActive(false);
                pathArrowPrefab.transform.SetParent(PathLayerTs);
                var pathLineRenderer = pathArrowPrefab.GetComponent<LineRenderer>();
                var pathMaterial = pathLineRenderer.material;
                // 创建共享材质
                pathMaterial.SetFloat("_Speed", moveSpeed);
                pathMaterial.SetFloat("_Tiling", 2);
                pathMaterial.SetColor("_Color", lineColor);
                // 设置UV模式以正确显示贴图
                pathLineRenderer.textureMode = LineTextureMode.Tile;
                ApplyCameraIndex(PathLayerTs);
                ApplyLayerIndex(PathLayerTs);
            });
        }

        /// <summary>
        /// 创建路径线
        /// </summary>
        /// <param name="pathId">路径唯一标识</param>
        /// <param name="startPos">起点位置</param>
        /// <param name="endPos">终点位置</param>
        public void CreatePath(string pathId, Vector3 startPos, Vector3 endPos)
        {
            if (pathLines.ContainsKey(pathId))
            {
                UpdatePath(pathId, startPos, endPos);
                return;
            }

            // 创建新的LineRenderer
            var clonePathArrow = Instantiate(pathArrowPrefab);
            clonePathArrow.name = $"Path_{pathId}";
            clonePathArrow.transform.SetParent(PathLayerTs);
            clonePathArrow.SetActive(true);

            var lineRenderer = clonePathArrow.GetComponent<LineRenderer>();
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);
            
            // 计算并设置tiling
            UpdatePathTiling(lineRenderer, startPos, endPos);
            
            pathLines.Add(pathId, lineRenderer);
        }

        /// <summary>
        /// 更新路径线位置
        /// </summary>
        public void UpdatePath(string pathId, Vector3 startPos, Vector3 endPos)
        {
            if (pathLines.TryGetValue(pathId, out var lineRenderer))
            {
                lineRenderer.SetPosition(0, startPos);
                lineRenderer.SetPosition(1, endPos);

                // 更新tiling
                UpdatePathTiling(lineRenderer, startPos, endPos);
            }
        }

        /// <summary>
        /// 根据路径长度更新tiling值
        /// </summary>
        private void UpdatePathTiling(LineRenderer lineRenderer, Vector3 startPos, Vector3 endPos)
        {
            lineRenderer.material.SetFloat("_Tiling", arrowDensity);
        }


        /// <summary>
        /// 移除路径线
        /// </summary>
        public void RemovePath(string pathId)
        {
            if (pathLines.TryGetValue(pathId, out var lineRenderer))
            {
                if (lineRenderer != null && lineRenderer.gameObject != null)
                {
                    Destroy(lineRenderer.gameObject);
                }
                pathLines.Remove(pathId);
            }
        }

        /// <summary>
        /// 清除所有路径线
        /// </summary>
        public void ClearAllPaths()
        {
            foreach (var lineRenderer in pathLines.Values)
            {
                if (lineRenderer != null && lineRenderer.gameObject != null)
                {
                    Destroy(lineRenderer.gameObject);
                }
            }
            pathLines.Clear();
        }

        /// <summary>
        /// 设置路径线颜色
        /// </summary>
        public void SetPathColor(string pathId, Color color)
        {
            if (pathLines.TryGetValue(pathId, out var lineRenderer))
            {
                lineRenderer.material.SetColor("_Color", color);
            }
        }

        /// <summary>
        /// 设置路径线移动速度
        /// </summary>
        public void SetPathSpeed(string pathId, float speed)
        {
            if (pathLines.TryGetValue(pathId, out var lineRenderer))
            {
                lineRenderer.material.SetFloat("_Speed", speed);
            }
        }


        public override void Dispose()
        {
            ClearAllPaths();
        }
    }
}
