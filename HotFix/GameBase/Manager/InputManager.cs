using GameBase.Scene;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using UnityEngine.EventSystems;

namespace GameBase.Manager
{
    [GameObjectBinding(path: "[GameModule]/Root/InputManager")]
    public class InputManager : BehaviourSingletonGameObject<InputManager>
    {
        [Header("Input Settings")]
        [SerializeField] private float clickThreshold = 5f;        // 判定为点击的最大移动距离
        [SerializeField] private float dragThreshold = 5f;         // 判定为拖动的最小移动距离
        [SerializeField] private float minPinchDistance = 50f;     // 最小触发缩放的触摸距离
        [SerializeField] private float mouseWheelThreshold = 0.1f; // 鼠标滚轮缩放阈值

        [Header("Debug Information")]
        [SerializeField] private bool showDebugInfo = false;       // 是否显示调试信息

        [Header("Runtime Status")]
        [SerializeField] private bool isPressed;                   // 当前是否按下
        [SerializeField] private bool isDragging;                  // 当前是否正在拖动
        [SerializeField] private Vector2 pressPosition;            // 按下位置
        [SerializeField] private Vector2 currentPosition;          // 当前位置
        [SerializeField] private Vector2 deltaPosition;            // 位置变化
        [SerializeField] private float zoomDelta;                  // 缩放变化
        [SerializeField] private Vector2 zoomCenter;               // 缩放中心点
        [SerializeField] private bool isPointerOverUI;             // 是否在UI上

        // 属性包装器
        public bool IsPressed => isPressed;
        public bool IsDragging => isDragging;
        public Vector2 PressPosition => pressPosition;
        public Vector2 Position => currentPosition;
        public Vector2 DeltaPosition => deltaPosition;
        public bool IsPointerOverUI
        {
            get => isPointerOverUI;
            set => isPointerOverUI = value;
        }

        // 触摸相关
        private float previousTouchDistance;
        private bool isTouching;

        // 事件委托
        public delegate void InputEventHandler(Vector2 position);
        public event InputEventHandler OnPressed;
        public event InputEventHandler OnReleased;
        public event InputEventHandler OnClicked;

        // 添加缩放事件
        public delegate void ZoomEventHandler(float zoomDelta, Vector2 zoomCenter);
        public event ZoomEventHandler OnZooming;

        // 在事件委托区域添加
        public delegate void DragEventHandler(Vector2 startPosition, Vector2 currentPosition, Vector2 delta);
        public event DragEventHandler OnDragBegin;
        public event DragEventHandler OnDragUpdate;
        public event DragEventHandler OnDragEnd;

        public void Awake()
        {
            EnhancedTouchSupport.Enable();
        }

        public void Update()
        {
            ResetFrameData();

            if (Touchscreen.current != null && Touch.activeTouches.Count > 0)
            {
                HandleTouchInput();
            }
            else
            {
                HandleMouseInput();
            }
        }

        private void ResetFrameData()
        {
            deltaPosition = Vector2.zero;
            zoomDelta = 0f;
            zoomCenter = Vector2.zero;
            isPointerOverUI = false;

            if (showDebugInfo)
            {
                Debug.Log($"Frame Reset - Position: {currentPosition}, Delta: {deltaPosition}");
            }
        }

        private void HandleMouseInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            currentPosition = mouse.position.ReadValue();
            isPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

            if (isPointerOverUI && !isPressed) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                if (!isPointerOverUI)
                {
                    isPressed = true;
                    isDragging = false;
                    pressPosition = currentPosition;
                    OnPressed?.Invoke(currentPosition);
                }
            }
            else if (mouse.leftButton.wasReleasedThisFrame)
            {
                if (isPressed)
                {
                    bool wasDragging = isDragging;
                    isPressed = false;
                    isDragging = false;

                    if (wasDragging)
                    {
                        OnDragEnd?.Invoke(pressPosition, currentPosition, deltaPosition);
                    }
                    else if (Vector2.Distance(currentPosition, pressPosition) < clickThreshold)
                    {
                        OnClicked?.Invoke(currentPosition);
                    }

                    OnReleased?.Invoke(currentPosition);
                }
            }

            if (isPressed)
            {
                deltaPosition = mouse.delta.ReadValue();

                // 持续拖动事件
                if (isDragging)
                {
                    OnDragUpdate?.Invoke(pressPosition, currentPosition, deltaPosition);
                }
                // 拖动开始判断
                if (!isDragging && Vector2.Distance(currentPosition, pressPosition) > dragThreshold)
                {
                    isDragging = true;
                    OnDragBegin?.Invoke(pressPosition, currentPosition, deltaPosition);
                }

            }

            // 处理缩放
            if (!isPointerOverUI)
            {
                var scrollDelta = mouse.scroll.ReadValue().normalized.y;
                if (Mathf.Abs(scrollDelta) > mouseWheelThreshold)
                {
                    zoomDelta = -scrollDelta;
                    zoomCenter = currentPosition;
                    OnZooming?.Invoke(zoomDelta, zoomCenter);
                }
            }
        }

        private void HandleTouchInput()
        {
            var touches = Touch.activeTouches;

            isPointerOverUI = touches.Count > 0 &&
                EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject(touches[0].touchId);

            if (touches.Count == 1)
            {
                var touch = touches[0];
                currentPosition = touch.screenPosition;

                if (isPointerOverUI && !isPressed) return;

                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    if (!isPointerOverUI)
                    {
                        isPressed = true;
                        isDragging = false;
                        pressPosition = currentPosition;
                        OnPressed?.Invoke(currentPosition);
                    }
                }
                else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended)
                {
                    if (isPressed)
                    {
                        bool wasDragging = isDragging;
                        isPressed = false;
                        isDragging = false;

                        if (wasDragging)
                        {
                            OnDragEnd?.Invoke(pressPosition, currentPosition, deltaPosition);
                        }
                        else if (Vector2.Distance(currentPosition, pressPosition) < clickThreshold)
                        {
                            OnClicked?.Invoke(currentPosition);
                        }

                        OnReleased?.Invoke(currentPosition);
                    }
                }

                if (isPressed)
                {
                    deltaPosition = touch.delta;

                    if (isDragging)
                    {
                        OnDragUpdate?.Invoke(pressPosition, currentPosition, deltaPosition);
                    }

                    if (!isDragging && Vector2.Distance(currentPosition, pressPosition) > dragThreshold)
                    {
                        isDragging = true;
                        OnDragBegin?.Invoke(pressPosition, currentPosition, deltaPosition);
                    }

                }
            }
            // 双指触摸（缩放）
            else if (touches.Count == 2)
            {
                HandlePinchZoom(touches.ToArray());
            }
            else
            {
                isTouching = false;
            }
        }

        private void HandlePinchZoom(Touch[] touches)
        {
            var touch1 = touches[0];
            var touch2 = touches[1];
            float currentTouchDistance = Vector2.Distance(touch1.screenPosition, touch2.screenPosition);

            if (!isTouching)
            {
                previousTouchDistance = currentTouchDistance;
                isTouching = true;
            }
            else if (Mathf.Abs(currentTouchDistance - previousTouchDistance) > minPinchDistance)
            {
                zoomDelta = (currentTouchDistance - previousTouchDistance) / minPinchDistance;
                zoomCenter = (touch1.screenPosition + touch2.screenPosition) * 0.5f;
                OnZooming?.Invoke(zoomDelta, zoomCenter);
                previousTouchDistance = currentTouchDistance;
            }
        }

        public void Dispose()
        {
            EnhancedTouchSupport.Disable();
        }
    }
}