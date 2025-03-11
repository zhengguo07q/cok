using UnityEngine;
using System.Collections.Generic;
using GameBase;

namespace GameLogic.Country.Manager
{
    [GameObjectBinding(path: "[GameModule]/Root/VisibilityManager")]
    public class VisibilityManager : BehaviourSingletonGameObject<VisibilityManager>
    {
        [Header("Visibility Settings")]
        [SerializeField] private float updateThreshold = 0.5f;
        [SerializeField] private float visibilityMargin = 2f;
        [SerializeField] private int maxItemsPerNode = 8;
        [SerializeField] private float minNodeSize = 10f;

        [Header("Layer Management")]
        [SerializeField] private bool showDebugInfo = false;
        private readonly Dictionary<string, LayerQuadTree> layerTrees = new();
        private Vector3 lastCameraPosition;
        private Rect lastVisibleBounds;
        private bool isInitialize;

        private class LayerQuadTree
        {
            public QuadTree<IVisibilityObject> Tree { get; set; }
            public HashSet<IVisibilityObject> VisibleObjects { get; set; }
            public int TotalObjectCount { get; set; }
            public string LayerName { get; private set; }

            public LayerQuadTree(string layerName, Rect bounds, int maxItemsPerNode, float minNodeSize)
            {
                LayerName = layerName;
                Tree = new QuadTree<IVisibilityObject>(bounds, maxItemsPerNode, minNodeSize);
                VisibleObjects = new HashSet<IVisibilityObject>();
                TotalObjectCount = 0;
            }
        }

        public void Initialize() 
        { 
        }

        public void ResetQuadTree(string layerName, Rect worldBounds)
        {
            if (!layerTrees.TryGetValue(layerName, out var layerTree))
            {
                layerTree = new LayerQuadTree(layerName, worldBounds, maxItemsPerNode, minNodeSize);
                layerTrees[layerName] = layerTree;
            }
            else
            {
                layerTree.Tree = new QuadTree<IVisibilityObject>(worldBounds, maxItemsPerNode, minNodeSize);
                layerTree.VisibleObjects.Clear();
                layerTree.TotalObjectCount = 0;
            }
        }

        public void Insert(string layerName, Vector2 position, IVisibilityObject visibilityObject)
        {
            if (!layerTrees.TryGetValue(layerName, out var layerTree)) return;

            layerTree.Tree.Insert(position, visibilityObject);
            layerTree.TotalObjectCount++;

            if (showDebugInfo)
            {
                Debug.Log($"[{layerName}] Inserted object at position {position}, Total: {layerTree.TotalObjectCount}");
            }
        }

        public void Remove(string layerName, IVisibilityObject visibilityObject)
        {
            if (!layerTrees.TryGetValue(layerName, out var layerTree)) return;

            layerTree.Tree.Remove(visibilityObject);
            layerTree.VisibleObjects.Remove(visibilityObject);
            layerTree.TotalObjectCount--;

            visibilityObject.OnVisibilityChanged(false);
        }

        public void UpdateItemPosition(string layerName, Vector2 newPosition, IVisibilityObject visibilityObject)
        {
            if (!layerTrees.TryGetValue(layerName, out var layerTree)) return;
            layerTree.Tree.Update(newPosition, visibilityObject);
        }

        public void UpdateVisibility(Camera camera)
        {
            if (camera == null) return;

            float moveDistance = Vector3.Distance(lastCameraPosition, camera.transform.position);
            if (moveDistance < updateThreshold) return;

            Rect visibleBounds = CalculateVisibleBounds(camera);
            if (visibleBounds == lastVisibleBounds) return;

            // 一次性计算可见区域，所有层共用
            foreach (var layerTree in layerTrees.Values)
            {
                UpdateLayerVisibility(layerTree, visibleBounds);
            }

            lastVisibleBounds = visibleBounds;
            lastCameraPosition = camera.transform.position;

            if (showDebugInfo)
            {
                foreach (var layerTree in layerTrees.Values)
                {
                    Debug.Log($"[{layerTree.LayerName}] Visible objects: {layerTree.VisibleObjects.Count}/{layerTree.TotalObjectCount}");
                }
            }
        }

        private void UpdateLayerVisibility(LayerQuadTree layerTree, Rect visibleBounds)
        {
            var newVisibleObjects = new HashSet<IVisibilityObject>(layerTree.Tree.QueryRange(visibleBounds));

            // 处理不再可见的对象
            foreach (var obj in layerTree.VisibleObjects)
            {
                if (!newVisibleObjects.Contains(obj))
                {
                    obj.OnVisibilityChanged(false);
                }
            }

            // 处理新变得可见的对象
            foreach (var obj in newVisibleObjects)
            {
                if (!layerTree.VisibleObjects.Contains(obj))
                {
                    obj.OnVisibilityChanged(true);
                }
            }

            layerTree.VisibleObjects = newVisibleObjects;
        }

        private Rect CalculateVisibleBounds(Camera camera)
        {
            Vector3[] corners = new Vector3[4];
            float z = camera.nearClipPlane;
            corners[0] = camera.ViewportToWorldPoint(new Vector3(0, 0, z));
            corners[1] = camera.ViewportToWorldPoint(new Vector3(1, 0, z));
            corners[2] = camera.ViewportToWorldPoint(new Vector3(0, 1, z));
            corners[3] = camera.ViewportToWorldPoint(new Vector3(1, 1, z));

            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            foreach (var corner in corners)
            {
                minX = Mathf.Min(minX, corner.x);
                minY = Mathf.Min(minY, corner.y);
                maxX = Mathf.Max(maxX, corner.x);
                maxY = Mathf.Max(maxY, corner.y);
            }

            return new Rect(
                minX - visibilityMargin,
                minY - visibilityMargin,
                maxX - minX + visibilityMargin * 2,
                maxY - minY + visibilityMargin * 2
            );
        }

        public void Dispose()
        {
            foreach (var layerTree in layerTrees.Values)
            {
                layerTree.VisibleObjects.Clear();
                layerTree.Tree = null;
            }
            layerTrees.Clear();
            isInitialize = false;
        }
    }

    public interface IVisibilityObject
    {
        Vector2 Position { get; }
        void OnVisibilityChanged(bool isVisible);
    }
} 