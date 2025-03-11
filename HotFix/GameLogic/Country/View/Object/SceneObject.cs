using GameLogic.Country.Manager;
using GameLogic.Country.Model;
using TEngine;
using UnityEngine;
using GameBase.Utility;
using GameBase.Layer;
using GameLogic.Country.View.Component;

namespace GameLogic.Country.View.Object
{
    /// <summary>
    /// 工厂对象
    /// </summary>
    public abstract class FactoryObject : MonoBehaviour, IVisibilityObject
    {

        public static SceneReferenceManager SceneRef;

        public static LayerIndexInfo LayerIndexInfo;
        public static bool IsStaticInitialize = false;
        /// <summary>
        /// 创建对象
        /// </summary>
        public static T CreateInstance<T>(Transform rootReference) where T : FactoryObject, new()
        {
            var instance = new GameObject(typeof(T).Name);
            instance.transform.parent = rootReference;

            if (IsStaticInitialize == false) 
            {
                SceneRef = SceneReferenceManager.Instance;
                LayerIndexInfo = SceneRef.Scene.SceneObjectLayer.LayerMetaInfo.LayerIndexInfo;
                IsStaticInitialize = true;
            }
            T componentIns = instance.AddComponent<T>();
            
            return componentIns;
        }

        public virtual Vector2 Position => throw new System.NotImplementedException();

        /// <summary>
        /// 可见性
        /// </summary>
        public void OnVisibilityChanged(bool isVisible)
        {
            this.gameObject.SetActive(isVisible);
        }
    }

    /// <summary>
    /// 场景对象
    /// </summary>
    public abstract class SceneObject : FactoryObject
    {
        public SceneObjectInfo SceneObjectInfo { get; set; } // 场景对象数据

        public readonly ComponentHolder HolderRef = new(); // 组件持有者

        public GameObject ObjectView { get; set; }    // 视图（底部显示）
        public GameObject IconView { get; set; }      // 图标视图（中部显示）

        protected virtual string ObjectPath { get => "default_resource"; }
        protected virtual string IconPath { get => "Icon_building_icon_1"; }


        [SerializeField] protected virtual Vector3 MountBottom => transform.position + new Vector3(0, 0, 0);
        [SerializeField] protected virtual Vector3 MountHead => transform.position + new Vector3(0, 0, 0);
        [SerializeField] protected virtual Vector3 MountEffect => transform.position + new Vector3(0, 0, 0);

        [SerializeField] protected bool IsDirty = false; // 标记需要更新

        [SerializeField] public bool IsTemporary { get; set; }

        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Initialize()
        {
            if (SceneObjectInfo == null)
            {
                Log.Error("SceneObjectInfo is null during initialization");
                return;
            }

            // 使用类型和ID命名对象
            gameObject.name = $"{SceneObjectInfo.MapObjectEntity.ObjectType}_{SceneObjectInfo.Id}";
            gameObject.SetActive(true);

            HolderRef.InitializeAll();

            // 应用层信息
            LayerUtility.SetCameraIndex(gameObject, LayerIndexInfo.CameraIndex);
            LayerUtility.SetLayerIndexInRender(gameObject, LayerIndexInfo.LayerIndex);
        }

        private void Update()
        {
            if (IsDirty) 
            {
                OnUpdate();
            }
            OnDynamicUpdate();
        }

        /// <summary>
        /// 当对象信息更新时调用
        /// </summary>
        protected virtual void OnUpdate()
        {

        }

        /// <summary>
        /// 动态更新 每帧都执行，子类可以重写此方法实现持续更新
        /// </summary>
        protected virtual void OnDynamicUpdate()
        {
            // 基类不做任何事情
        }

        /// <summary>
        /// 标记为脏
        /// </summary>
        public void MarkAsDirty()
        {
            IsDirty = true;
        }

        /// <summary>
        /// 显示详细视图
        /// </summary>
        public virtual void ShowObjectView()
        {
            if (ObjectView) ObjectView.SetActive(true);
            if (IconView) IconView.SetActive(false);
        }

        /// <summary>
        /// 显示图标视图
        /// </summary>
        public virtual void ShowIconView()
        {
            if (ObjectView) ObjectView.SetActive(false);
            if (IconView) IconView.SetActive(true);
        }

        /// <summary>
        /// 隐藏所有视图
        /// </summary>
        public virtual void HideView()
        {
            if (ObjectView) ObjectView.SetActive(false);
            if (IconView) IconView.SetActive(false);
        }

        /// <summary>
        /// 得到位置
        /// </summary>
        public override Vector2 Position => new (
            SceneObjectInfo.Position.x,
            SceneObjectInfo.Position.y
        );

        /// <summary>
        /// 更新位置
        /// </summary>
        public virtual void UpdatePosition()
        {
        }

        /// <summary>
        /// 初始化组件
        /// </summary>
        protected virtual void InitializeHolder()
        {
        }
        /// <summary>
        /// 销毁持有者
        /// </summary>
        protected virtual void Dispose()
        {
            HolderRef.DisposeAll();
            if (gameObject != null)
            {
                GameObject.Destroy(gameObject);
            }
        }


        private void OnDestroy()
        {
            Dispose();
        }

    }
}
