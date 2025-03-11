using GameLogic.Country.View.Object;
using UnityEngine;

namespace GameLogic.Country.View.Component
{
    public class CompCollider : ComponentBase
    {
        public SceneObject SceneObject { get; set; }
        private BoxCollider2D _collider;

        public override void Initialize()
        {
            if (SceneObject == null || SceneObject.gameObject == null)
            {
                Debug.LogError("ComponentCollider.Initialize: SceneObject is not set!");
                return;
            }

            CreateCollider();
            UpdateColliderSize();
        }

        private void CreateCollider()
        {
            // 添加并配置碰撞体
            _collider = SceneObject.gameObject.AddComponent<BoxCollider2D>();
            _collider.isTrigger = true;

            // 初始大小基于Tilemap
            var tilemap = SceneObject.SceneRef.Scene.TileLayer.Tilemap;
            if (tilemap != null)
            {
                Vector2 cellSize = tilemap.cellSize;
                _collider.size = new Vector2(cellSize.x * 0.8f, cellSize.y * 0.8f);
            }
        }

        public void UpdateColliderSize()
        {
            if (_collider == null) return;

            // 优先使用详细视图精灵
            var objectSprite = SceneObject.ObjectView.GetComponentInChildren<SpriteRenderer>();
            if (objectSprite != null && objectSprite.sprite != null)
            {
                UpdateSizeBySprite(objectSprite);
                return;
            }

            // 其次使用图标视图精灵
            var iconSprite = SceneObject.IconView.GetComponentInChildren<SpriteRenderer>();
            if (iconSprite != null && iconSprite.sprite != null)
            {
                UpdateSizeBySprite(iconSprite);
            }
        }

        private void UpdateSizeBySprite(SpriteRenderer renderer)
        {
            var bounds = renderer.sprite.bounds;
            _collider.size = new Vector2(bounds.size.x * 0.8f, bounds.size.y * 0.8f);
            _collider.offset = new Vector2(0, bounds.center.y);
        }

        public override void Dispose()
        {
            if (_collider != null)
            {
                GameObject.Destroy(_collider);
                _collider = null;
            }
            base.Dispose();
        }
    }
} 