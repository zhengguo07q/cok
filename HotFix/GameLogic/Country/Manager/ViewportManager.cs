using GameBase;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Experimental.Rendering.Universal.PixelPerfectCamera;
using GameBase.Manager;
using System;

namespace GameLogic.Country.Manager
{
    [GameObjectBinding(path: "[GameModule]/Root/ViewportManager")]
    public class ViewportManager : BehaviourSingletonGameObject<ViewportManager>
    {
        private SceneReferenceManager SceneRef => SceneReferenceManager.Instance;
        private InputManager InputManager => InputManager.Instance;

        [Header("Camera Settings")]
        [SerializeField] private Vector2 minBounds = new(-204.8f, -102.4f); //计算方法为backgroundTs.Scale * image_pixs / ppu
        [SerializeField] private Vector2 maxBounds = new(204.8f, 102.4f);
        [SerializeField] private Vector2Int referenceResolution = new(640, 1136);
        [SerializeField] private int referencePixelsPerUnit = 100;

        [Header("Movement Settings")]
        [SerializeField] private float smoothTime = 0.15f;
        [SerializeField] private float zoomSpeed = 0.2f;
        [SerializeField] private float zoomSmoothness = 0.1f;
        [SerializeField] private Vector3 targetPosition;
        [SerializeField] private Vector3 velocity = Vector3.zero;

        [Header("Zoom Limits")]
        [SerializeField] private float minOrthographicSize = 5f;
        [SerializeField] private float maxOrthographicSize = 50f;
        [SerializeField] private bool enableZoomLimits = true;

        [Header("Movement Limits")]
        [SerializeField] private bool enableBoundaryLimits = true;
        [SerializeField] private float boundaryPadding = 0f;

        [Header("PPU Limits")]
        [SerializeField] private int minPPU = 10;
        [SerializeField] private int maxPPU = 70;
        [SerializeField] private bool enablePPULimits = true;


        public event Action OnCameraMoved;

        private bool isViewportInitialized = false;
        public bool IsViewportInitialized => isViewportInitialized;

        public event Action OnViewportInitialized;

        // 修改拖拽相关字段
        private Vector2 lastDragPosition;
        private bool isDragging = false;

        private float currentZoomVelocity;
        private float targetPPU;
        private float currentPPUFloat;
        private bool isZooming;

        /// <summary>
        /// 获取世界边界矩形
        /// </summary>
        public Rect WorldBounds => new Rect(
            minBounds.x,                     // x position
            minBounds.y,                     // y position
            maxBounds.x - minBounds.x,       // width
            maxBounds.y - minBounds.y        // height
        );

        public void Initialize()
        {
            SetupCamera();
            SetupInput();
        }

        private void SetupCamera()
        {
            SceneRef.Camera.clearFlags = CameraClearFlags.SolidColor;
            SceneRef.Camera.backgroundColor = Color.black;
            SceneRef.CameraData.renderType = CameraRenderType.Base;
            SceneRef.CameraData.renderPostProcessing = false;

            SceneRef.PixelPerfectCamera.assetsPPU = referencePixelsPerUnit;
            SceneRef.PixelPerfectCamera.refResolutionX = referenceResolution.x;
            SceneRef.PixelPerfectCamera.refResolutionY = referenceResolution.y;
            SceneRef.PixelPerfectCamera.cropFrame = CropFrame.None;
            SceneRef.PixelPerfectCamera.gridSnapping = GridSnapping.UpscaleRenderTexture;
        }

        public void SetupInput()
        {
            InputManager.OnZooming += HandleZoom;
            InputManager.OnDragBegin += HandleDragBegin;
            InputManager.OnDragUpdate += HandleDragUpdate;
            InputManager.OnDragEnd += HandleDragEnd;
            InputManager.OnClicked += HandleClick;
        }

        public void HandleZoom(float zoomDelta, Vector2 zoomCenter)
        {
            if (!enableZoomLimits) 
                return;

            // 获取缩放前视口中心的世界坐标
            Vector2 viewportCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Vector3 viewportCenterWorld = GetWorldPosition(viewportCenter);

            // 初始化当前PPU的浮点值
            if (!isZooming)
            {
                currentPPUFloat = SceneRef.PixelPerfectCamera.assetsPPU;
                targetPPU = currentPPUFloat;
                isZooming = true;
            }

            // 直接使用缩放增量更新目标PPU
            if (Mathf.Abs(zoomDelta) > 0.01f)
            {
                // 根据滚轮方向确定缩放方向
                float scaleFactor = zoomDelta > 0 ? (1 + zoomSpeed) : (1 - zoomSpeed);
                targetPPU = Mathf.Clamp(targetPPU * scaleFactor, minPPU, maxPPU);
            }

            // 平滑过渡到目标PPU
            currentPPUFloat = Mathf.Lerp(
                currentPPUFloat,
                targetPPU,
                Time.deltaTime / zoomSmoothness
            );

            // 应用新的PPU值
            int newPPU = Mathf.RoundToInt(currentPPUFloat);
            if (newPPU != SceneRef.PixelPerfectCamera.assetsPPU)
            {
                SceneRef.PixelPerfectCamera.assetsPPU = newPPU;
                RefreshPixelPerfectCamera();

                // 计算缩放后视口中心的世界坐标
                Vector3 newViewportCenterWorld = GetWorldPosition(viewportCenter);
                
                // 计算并应用位置偏移，以保持视口中心点不变
                Vector3 offset = viewportCenterWorld - newViewportCenterWorld;
                Vector3 newPosition = SceneRef.Camera.transform.position + offset;
                
                // 确保新位置在边界内
                if (enableBoundaryLimits)
                {
                    newPosition = ClampPositionToBounds(newPosition);
                }

                // 更新相机位置
                SetCameraPositionImmediate(newPosition);
                
                OnCameraMoved?.Invoke();
            }
        }

        private void Update()
        {
            // 在Update中持续更新缩放
            if (isZooming && Mathf.Abs(currentPPUFloat - targetPPU) > 0.1f)
            {
                HandleZoom(0, Vector2.zero);
            }
            else if (isZooming)
            {
                isZooming = false;
            }
        }

        public void HandleDragBegin(Vector2 startPosition, Vector2 currentPosition, Vector2 delta)
        {
            if (InputManager.IsPointerOverUI)
                return;

            isDragging = true;
            lastDragPosition = currentPosition;
        }

        public void HandleDragUpdate(Vector2 startPosition, Vector2 currentPosition, Vector2 delta)
        {
            if (InputManager.IsPointerOverUI || !isDragging)
                return;

            // 计算屏幕空间的增量移动
            Vector2 dragDelta = currentPosition - lastDragPosition;
            
            // 将屏幕空间的增量转换为世界空间的增量
            Vector3 worldSpaceDelta = ScreenToWorldDelta(dragDelta);
            
            // 更新相机位置
            Vector3 newPosition = SceneRef.Camera.transform.position - worldSpaceDelta;
            SetCameraPositionImmediate(newPosition);
            
            // 更新上一次拖拽位置
            lastDragPosition = currentPosition;
            
            OnCameraMoved?.Invoke();
        }

        public void HandleDragEnd(Vector2 startPosition, Vector2 currentPosition, Vector2 delta)
        {
            if (InputManager.IsPointerOverUI)
                return;

            isDragging = false;
            targetPosition = SceneRef.Camera.transform.position;
            velocity = Vector3.zero;
        }

        private Vector3 ScreenToWorldDelta(Vector2 screenDelta)
        {
            // 计算屏幕空间到世界空间的转换比例
            float orthoSize = SceneRef.Camera.orthographicSize;
            float screenHeight = Screen.height;
            float worldToScreenRatio = (2f * orthoSize) / screenHeight;
            
            // 转换增量
            return new Vector3(
                screenDelta.x * worldToScreenRatio * SceneRef.Camera.aspect,
                screenDelta.y * worldToScreenRatio,
                0
            );
        }

        private void SetCameraPositionImmediate(Vector3 position)
        {
            position = ClampPositionToBounds(position);
            SceneRef.Camera.transform.position = position;
            targetPosition = position;
        }

        private void MoveToPosition(Vector3 position)
        {
            if (isDragging)
            {
                SetCameraPositionImmediate(position);
                return;
            }

            position = ClampPositionToBounds(position);
            targetPosition = position;
            
            SceneRef.Camera.transform.position = Vector3.SmoothDamp(
                SceneRef.Camera.transform.position,
                targetPosition,
                ref velocity,
                smoothTime
            );
        }

        private Vector3 GetWorldPosition(Vector2 screenPosition)
        {
            float zDistance = -SceneRef.Camera.transform.position.z;
            Vector3 screenPosWithZ = new Vector3(screenPosition.x, screenPosition.y, zDistance);
            Vector3 worldPosition = SceneRef.Camera.ScreenToWorldPoint(screenPosWithZ);
            worldPosition.z = 0; // 保持Z轴为0
            return worldPosition;
        }

        public void SetBounds(Vector2 min, Vector2 max)
        {
            minBounds = min;
            maxBounds = max;
        }



        public void SetCameraPosition(Vector3 position)
        {
            position = ClampPositionToBounds(position);
            SceneRef.Camera.transform.position = position;
            OnCameraMoved?.Invoke();
        }

        public void SetPPU(int ppu)
        {
            if (!enablePPULimits) 
                return;

            // 获取缩放前视口中心的世界坐标
            Vector2 viewportCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Vector3 viewportCenterWorld = GetWorldPosition(viewportCenter);

            // 应用新的PPU值
            ppu = Mathf.Clamp(ppu, minPPU, maxPPU);
            SceneRef.PixelPerfectCamera.assetsPPU = ppu;
            
            // 刷新像素完美相机
            RefreshPixelPerfectCamera();

            // 计算缩放后视口中心的世界坐标
            Vector3 newViewportCenterWorld = GetWorldPosition(viewportCenter);
            
            // 计算并应用位置偏移，以保持视口中心点不变
            Vector3 offset = viewportCenterWorld - newViewportCenterWorld;
            Vector3 newPosition = SceneRef.Camera.transform.position + offset;
            
            // 确保新位置在边界内
            if (enableBoundaryLimits)
            {
                newPosition = ClampPositionToBounds(newPosition);
            }

            // 更新相机位置
            SetCameraPositionImmediate(newPosition);
            
            OnCameraMoved?.Invoke();
        }

        private void RefreshPixelPerfectCamera()
        {
            if (SceneRef.PixelPerfectCamera.assetsPPU <= 0)
                return;
            
            SceneRef.PixelPerfectCamera.enabled = false;
            SceneRef.PixelPerfectCamera.enabled = true;
        }

        /// <summary>
        /// 初始化视口位置
        /// </summary>
        /// <param name="position">目标位置</param>
        /// <param name="orthographicSize">正交相机大小（可选）</param>
        public void InitializeViewport(Vector3 position, float? orthographicSize = null)
        {
            // 设置正交相机大小（如果提供）
            if (orthographicSize.HasValue)
            {
                float size = orthographicSize.Value;
                if (enableZoomLimits)
                {
                    size = Mathf.Clamp(size, minOrthographicSize, maxOrthographicSize);
                }
                SceneRef.Camera.orthographicSize = size;
            }

            // 设置位置
            SetCameraPosition(position);
            
            // 标记视口已初始化
            isViewportInitialized = true;
            
            // 触发初始化完成事件
            OnViewportInitialized?.Invoke();
        }

        public void Dispose()
        {
            if (InputManager != null)
            {
                InputManager.OnZooming -= HandleZoom;
                InputManager.OnDragBegin -= HandleDragBegin;
                InputManager.OnDragUpdate -= HandleDragUpdate;
                InputManager.OnDragEnd -= HandleDragEnd;
                InputManager.OnClicked -= HandleClick;
            }
        }

        /// <summary>
        /// 限制不允许超出地图边界
        /// 与设置的地图边框有关系
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Vector3 ClampPositionToBounds(Vector3 position)
        {
            if (!enableBoundaryLimits)
                return position;

            // 计算相机视口的一半大小
            float verticalSize = SceneRef.Camera.orthographicSize;
            float horizontalSize = verticalSize * SceneRef.Camera.aspect;

            // 添加边界填充
            float paddedMinX = minBounds.x + horizontalSize + boundaryPadding;
            float paddedMaxX = maxBounds.x - horizontalSize - boundaryPadding;
            float paddedMinY = minBounds.y + verticalSize + boundaryPadding;
            float paddedMaxY = maxBounds.y - verticalSize - boundaryPadding;

            // 限制相机位置
            position.x = Mathf.Clamp(position.x, paddedMinX, paddedMaxX);
            position.y = Mathf.Clamp(position.y, paddedMinY, paddedMaxY);
            position.z = SceneRef.Camera.transform.position.z; // 保持Z轴不变

            return position;
        }

        private void HandleClick(Vector2 screenPosition)
        {
            if (InputManager.IsPointerOverUI)
                return;

            // 将屏幕坐标转换为世界坐标
            Vector3 worldPosition = GetWorldPosition(screenPosition);
            
            // 通知TileLayer处理点击
            var tileLayer = SceneRef.Scene.TileLayer;
            if (tileLayer != null)
            {
                tileLayer.HandleClick(worldPosition);
            }
        }
    }
}
