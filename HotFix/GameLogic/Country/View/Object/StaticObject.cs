using GameLogic.Country.View.Component;
using GameLogic.Country.View.Formation;
using TEngine;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GameLogic.Country.View.Object
{
    /// <summary>
    /// 场景中不可以移动的对象, 目前场景中的对象都是使用2D来进行渲染
    /// </summary>
    public class StaticObject : SceneObject
    {
        [Header("Static Info")]
        [SerializeField] protected SpriteRenderer obectSprite;

        [SerializeField] protected SpriteRenderer iconSprite;

        public override void Initialize()
        {
            CreateViewContainers();
            LoadResources();
            UpdatePosition();
            ShowObjectView();
            InitializeHolder();
            base.Initialize();
        }

        protected override void InitializeHolder()
        {
            var compNameDisplay = HolderRef.Add<CompNameDisplay>();
            compNameDisplay.SceneObject = this;

            var compCollider = HolderRef.Add<CompCollider>();
            compCollider.SceneObject = this;

            var compCombat = HolderRef.Add<CompCombat>();
            compCombat.Owner = this;
        }


        protected virtual void CreateViewContainers()
        {
            // 创建详细视图容器
            ObjectView = new GameObject("Object");
            ObjectView.transform.SetParent(transform, false);

            // 创建图标视图容器
            IconView = new GameObject("Icon");
            IconView.transform.SetParent(transform, false);
        }

        protected virtual void LoadResources()
        {
            LoadObjectResources();
            LoadIconResources();
        }


        protected virtual void LoadObjectResources()
        {
            // 创建详细视图精灵
            var objectGo = new GameObject("ObjectSprite");
            objectGo.transform.SetParent(ObjectView.transform, false);
            obectSprite = objectGo.AddComponent<SpriteRenderer>();

            // 加载详细视图的精灵
            GameModule.Resource.LoadAsset<Sprite>(ObjectPath, sprite =>
            {
                if (obectSprite != null)
                {
                    obectSprite.sprite = sprite;
                    obectSprite.sortingOrder = 1;
                    HolderRef.CompCollider.UpdateColliderSize(); // 更新碰撞体大小
                }
            });
        }

        protected virtual void LoadIconResources()
        {
            var iconGo = new GameObject("IconSprite");
            iconGo.transform.SetParent(IconView.transform, false);
            iconSprite = iconGo.AddComponent<SpriteRenderer>();

            // 加载图标精灵
            GameModule.Resource.LoadAsset<Sprite>(IconPath, sprite =>
            {
                if (iconSprite != null)
                {
                    iconSprite.sprite = sprite;
                    iconSprite.sortingOrder = 2;
                    if (obectSprite == null || obectSprite.sprite == null)
                    {
                        HolderRef.CompCollider.UpdateColliderSize(); // 更新碰撞体大小
                    }
                }
            });
        }

        /// <summary>
        /// 更新静态物体位置
        /// </summary>
        public override void UpdatePosition()
        {
            if (SceneObjectInfo != null)
            {
                var scene = SceneRef.Scene;
                // 获取当前场景的TileLayer
                if (scene == null || scene.TileLayer == null) return;

                // 将世界坐标转换为Tile坐标
                Vector3 worldPosition = new Vector3(
                    SceneObjectInfo.Position.x,
                    SceneObjectInfo.Position.y,
                    0
                );

                // 获取Tilemap组件
                Tilemap tilemap = scene.TileLayer.Tilemap;
                if (tilemap == null) return;

                // 更新Tile坐标
                var tilePosition = tilemap.WorldToCell(worldPosition);

                // 将对象放置在Tile的中心位置
                transform.position = tilemap.GetCellCenterWorld(tilePosition);
            }
        }

        protected override void Dispose()
        {
            base.Dispose();
        }

    }
}
