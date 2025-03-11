using System;
using System.Linq;
using System.Reflection;


namespace GameBase.Layer
{
    /// <summary>
    /// 构建类型
    /// </summary>
    public enum BuildType
    {
        Async,
        Sync,
    }
    /// <summary>
    /// 构建层属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class LayerBindingAttribute : Attribute
    {
        /// <summary>
        /// 资源位置
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// 层名字
        /// </summary>
        public LayerName LayerName { get; set; }

        /// <summary>
        /// 构建类型，同步还是异步， 默认是同步
        /// </summary>
        public BuildType BuildType { get; set; }


        public LayerBindingAttribute(LayerName layerName, string location =null, BuildType buildType = BuildType.Sync)
        {
            Location = location;
            LayerName = layerName;
            BuildType = buildType;
        }
    }

    /// <summary>
    /// 定义层枚举，用来做元数据配置
    /// </summary>
    public enum LayerName
    {
        BackgroundLayer,
        TileLayer,
        GridLayer,
        SceneObjectLayer,
        PathLayer,
        MainUILayer,
        SceneWindowLayer,
        SceneEffectLayer,
        UILayer,
        GuideLayer,
        EffectLayer,
        DiglogLayer,
        TooltipLayer,
        LoadingLayer,
        AlertLayer,
        ReconnectLayer
    }

    public class LayerIndexInfo
    {
        /// <summary>
        /// 层名，用来做元数据映射
        /// </summary>
        public LayerName LayerName;
        /// <summary>
        /// 层索引，用来做层内深度排序
        /// </summary>
        public int LayerIndex;
        /// <summary>
        /// 渲染图层，每个图层代表一个摄像机，0-4图层为系统保留图层，5-31为用户可以使用图层。 
        /// 目前5为UI层，6为场景层，31为屏幕层
        /// </summary>
        public int CameraIndex;

        public LayerIndexInfo(LayerName layerName, int layerIndex, int cameraIndex)
        {
            LayerName = layerName;
            LayerIndex = layerIndex;
            CameraIndex = cameraIndex;
        }

        public class LayerMetaInfo {
            /// <summary>
            /// 资源位置
            /// </summary>
            public string Location { get; set; }

            /// <summary>
            /// 构建类型，同步还是异步， 默认是同步
            /// </summary>
            public BuildType BuildType { get; set; }

            /// <summary>
            /// 层索引信息
            /// </summary>
            public LayerIndexInfo LayerIndexInfo { get; set; }


            /// <summary>
            /// 构建层元信息，需要获取枚举定义的属性值
            /// </summary>
            /// <param name="location"></param>
            /// <param name="layerName"></param>
            /// <param name="buildType"></param>
            public LayerMetaInfo(string location, LayerName layerName, BuildType buildType = BuildType.Sync)
            {
                Location = location;
                BuildType = buildType;
                Type layerDefinitionType = typeof(WindowLayerDefinition);
                FieldInfo fieldInfo = layerDefinitionType.GetField(layerName.ToString(), BindingFlags.Public | BindingFlags.Static);
                if (fieldInfo == null) { 
                    throw new NullReferenceException($"Type : {layerDefinitionType} can not find property: {layerName} !");
                }
                if (fieldInfo != null && fieldInfo.FieldType == typeof(LayerIndexInfo))
                {
                    var layerIndexInfo = fieldInfo.GetValue(null);
                    LayerIndexInfo = (LayerIndexInfo)layerIndexInfo;

                }
            }

            /// <summary>
            /// 获得绑定的场景资源
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public static LayerMetaInfo GetLayerBindingMetaInfo<T>()
            {
                LayerMetaInfo layerMetaInfo = null;
                Type layerBindingAttributeType = typeof(T);
                var attributes = layerBindingAttributeType.GetCustomAttributes(typeof(LayerBindingAttribute), false);

                if (attributes.Length == 0) { 
                    throw new Exception($"LayerBindingAttribute is not found , type: {layerBindingAttributeType}");
                }
                foreach (LayerBindingAttribute attr in attributes.Cast<LayerBindingAttribute>())
                {
                    layerMetaInfo = new LayerMetaInfo(attr.Location, attr.LayerName, attr.BuildType);
                }
                return layerMetaInfo;
            }
        }
    }

    /// <summary>
    /// 层索引的定义
    /// </summary>
    public static class WindowLayerDefinition
    {
        public static readonly LayerIndexInfo BackgroundLayer = new(LayerName.BackgroundLayer, 0, 6); //6为场景图层
        public static readonly LayerIndexInfo TileLayer = new(LayerName.TileLayer, 100, 6); //6为场景图层
        public static readonly LayerIndexInfo GridLayer = new (LayerName.GridLayer, 200, 6);
        public static readonly LayerIndexInfo SceneObjectLayer = new (LayerName.SceneObjectLayer, 300, 6);
        public static readonly LayerIndexInfo PathLayer = new(LayerName.PathLayer, 400, 6);
        public static readonly LayerIndexInfo MainUILayer = new (LayerName.MainUILayer, 500, 5);        // 5为射线图层
        public static readonly LayerIndexInfo SceneWindowLayer = new (LayerName.SceneWindowLayer, 600, 5);
        public static readonly LayerIndexInfo SceneEffectLayer = new (LayerName.SceneEffectLayer, 700, 5);
        public static readonly LayerIndexInfo UILayer = new (LayerName.UILayer, 2000, 5);
        public static readonly LayerIndexInfo GuideLayer = new (LayerName.GuideLayer, 4000, 5);
        public static readonly LayerIndexInfo EffectLayer = new (LayerName.EffectLayer, 5000, 5);
        public static readonly LayerIndexInfo DiglogLayer = new (LayerName.DiglogLayer, 6000, 16);
        public static readonly LayerIndexInfo TooltipLayer = new (LayerName.TooltipLayer, 8000, 16);
        public static readonly LayerIndexInfo LoadingLayer = new (LayerName.LoadingLayer, 9000, 5);
        public static readonly LayerIndexInfo AlertLayer = new (LayerName.AlertLayer, 10000, 16);
        public static readonly LayerIndexInfo ReconnectLayer = new (LayerName.ReconnectLayer, 11000, 16);
    }


}
