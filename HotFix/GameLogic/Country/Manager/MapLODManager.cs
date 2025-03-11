using UnityEngine;
using GameBase;

namespace GameLogic.Country.Manager
{
    /// <summary>
    /// 地图LOD级别
    /// </summary>
    public enum MapLODLevel
    {
        Highest = 0, // 镜头最远
        High,
        Medium,
        Low,
        Lowest, // 镜头最近
    }


    [System.Serializable]
    public class MapLODConfig
    {
        public Vector2Int baseMapSize = new(120000, 120000);  // 基础地图大小（像素）
        public int baseTileSize = 100;                        // 基础Tile大小（像素）
        public Vector2Int baseTileCount = new(1200, 1200);   // 基础Tile数量

        // LOD层级配置
        public LODLevel[] levels = new LODLevel[]
        {
            new() { textureSize = 2048,  chunksPerAxis = 1,   visibleRange = 2000f }, // Level 0
            new() { textureSize = 4096,  chunksPerAxis = 2,   visibleRange = 1500f }, // Level 1
            new() { textureSize = 8192,  chunksPerAxis = 4,   visibleRange = 1000f }, // Level 2
            new() { textureSize = 16384, chunksPerAxis = 8,   visibleRange = 500f },  // Level 3
            new() { textureSize = 32768, chunksPerAxis = 16,  visibleRange = 0f }     // Level 4
        };
    }

    [System.Serializable]
    public class LODLevel
    {
        public int textureSize;      // 每个区块的纹理大小
        public int chunksPerAxis;    // 每个轴上的区块数量
        public float visibleRange;   // 可见范围（相机高度阈值）
    }

    [GameObjectBinding(path: "[GameModule]/Root/MapLODManager")]
    public class MapLODManager : BehaviourSingletonGameObject<MapLODManager>
    {
        private SceneReferenceManager SceneRef => SceneReferenceManager.Instance;

        [Header("Map Level Settings")]
        [SerializeField]
        private LODLevelConfig[] lodLevels = new LODLevelConfig[]
        {
            new() {  // Level 0 - Highest (最远视图)
                mapSize = new Vector2Int(120000, 120000),  // 基础地图大小
                gridSize = new Vector2Int(1, 1),          // 1x1 网格
                visibleRange = 2000f,
                targetPPU = 20,
                tilePixelSize = 2048                      // 单个tile的像素大小
            },
            new() {  // Level 1 - High
                mapSize = new Vector2Int(120000, 120000),
                gridSize = new Vector2Int(2, 2),          // 2x2 网格
                visibleRange = 1500f,
                targetPPU = 40,
                tilePixelSize = 1024
            },
            new() {  // Level 2 - Medium
                mapSize = new Vector2Int(120000, 120000),
                gridSize = new Vector2Int(4, 4),          // 4x4 网格
                visibleRange = 1000f,
                targetPPU = 60,
                tilePixelSize = 512
            },
            new() {  // Level 3 - Low
                mapSize = new Vector2Int(120000, 120000),
                gridSize = new Vector2Int(8, 8),          // 8x8 网格
                visibleRange = 500f,
                targetPPU = 80,
                tilePixelSize = 256
            },
            new() {  // Level 4 - Lowest (最近视图)
                mapSize = new Vector2Int(120000, 120000),
                gridSize = new Vector2Int(16, 16),        // 16x16 网格
                visibleRange = 0f,
                targetPPU = 100,
                tilePixelSize = 128
            }
        };

        [Header("Camera Height Settings")]
        [SerializeField]
        private float[] levelHeights = new float[]
        {
            2000f,  // Level1 高度阈值
            1500f,  // Level2 高度阈值
            1000f,  // Level3 高度阈值
            500f,   // Level4 高度阈值
            0f      // Level5 最低高度
        };

        [Header("PPU Settings")]
        [SerializeField] private int minPPU = 20;
        [SerializeField] private int maxPPU = 100;
        [SerializeField] private bool enablePPULimits = true;

        private MapLODLevel currentLevel = MapLODLevel.Low;
        private bool isInitialize = false;

        public event System.Action<MapLODLevel> OnLevelChanged;
        public MapLODLevel CurrentLODLevel => currentLevel;

        public void Initialize()
        {
            InitializeLODLayers();
            isInitialize = true;
        }

        private void InitializeLODLayers()
        {
            
        }

        void Update()
        {
            if (isInitialize == false) 
            {
                return;
            }
            UpdateLODLevel();
        }

        public void UpdateLODLevel()
        {
            float cameraHeight = 2f * SceneRef.Camera.orthographicSize;
            MapLODLevel newLevel = MapLODLevel.Low;

            for (int i = 0; i < levelHeights.Length; i++)
            {
                if (cameraHeight >= levelHeights[i])
                {
                    newLevel = (MapLODLevel)i;
                    break;
                }
            }

            if (newLevel != currentLevel)
            {
                currentLevel = newLevel;
                UpdatePPU();
                OnLevelChanged?.Invoke(currentLevel);
            }
        }

        private void UpdatePPU()
        {
            int targetPPU = CalculateOptimalPPU();
            SceneRef.PixelPerfectCamera.assetsPPU = targetPPU;

            // 刷新PixelPerfectCamera以应用新的PPU
            SceneRef.PixelPerfectCamera.enabled = false;
            SceneRef.PixelPerfectCamera.enabled = true;
        }

        private int CalculateOptimalPPU()
        {
            float cameraHeight = 2f * SceneRef.Camera.orthographicSize;
            float cameraWidth = cameraHeight * SceneRef.Camera.aspect;
            Vector2Int currentMapSize = lodLevels[(int)currentLevel].mapSize;

            float heightPPU = currentMapSize.y / cameraHeight;
            float widthPPU = currentMapSize.x / cameraWidth;

            int optimalPPU = Mathf.RoundToInt(Mathf.Min(heightPPU, widthPPU));

            return enablePPULimits ?
                Mathf.Clamp(optimalPPU, minPPU, maxPPU) :
                optimalPPU;
        }

        public MapLODLevel GetCurrentLevel() => currentLevel;

        public Vector2Int GetCurrentMapSize() => lodLevels[(int)currentLevel].mapSize;

        public LODLevelConfig GetLevelConfig(MapLODLevel level)
        {
            return lodLevels[(int)level];
        }

        public void Dispose()
        {
            isInitialize = false;
        }
    }

    [System.Serializable]
    public class LODLevelConfig
    {
        public Vector2Int mapSize;      // 地图大小
        public Vector2Int gridSize;     // 网格大小（每个方向的tile数量）
        public float visibleRange;      // 可见范围（相机高度阈值）
        public int targetPPU;           // 目标PPU值
        public int tilePixelSize;        // 单个tile的像素大小
    }
}