using GameBase;
using UnityEngine;

namespace GameLogic.Country.Manager
{
    [GameObjectBinding(path: "[GameModule]/Root/CameraAnimationManager")]
    public class CameraAnimationManager : BehaviourSingletonGameObject<CameraAnimationManager>
    {
        private SceneReferenceManager SceneRef => SceneReferenceManager.Instance;
        private ViewportManager ViewportManager => ViewportManager.Instance;

        [Header("Animation Settings")]
        [SerializeField] private float focusAnimationDuration = 0.5f;
        private float animationTimer = 0f;
        private bool isAnimating = false;

        private AnimationState currentAnimation;
        private bool isInitialize;

        public bool IsAnimating => isAnimating;

        private struct AnimationState
        {
            public Vector3 startPosition;
            public Vector3 targetPosition;
            public int startPPU;
            public int targetPPU;
        }

        public void Initialize()
        {
            isInitialize = true;
        }

        /// <summary>
        /// 聚焦到指定位置，带平滑动画
        /// </summary>
        public void FocusOnPosition(Vector3 worldPosition, int targetPPU = 80)
        {
            if (isAnimating) return;

            isAnimating = true;
            animationTimer = 0f;

            currentAnimation = new AnimationState
            {
                startPosition = SceneRef.Camera.transform.position,
                targetPosition = ViewportManager.ClampPositionToBounds(worldPosition),
                startPPU = SceneRef.PixelPerfectCamera.assetsPPU,
                targetPPU = targetPPU
            };

            // 保持z轴不变
            currentAnimation.targetPosition.z = currentAnimation.startPosition.z;
        }

        public void Update()
        {
            if (isInitialize == false) 
            {
                return;
            }
            if (isAnimating)
            {
                UpdateAnimation();
            }
        }

        private void UpdateAnimation()
        {
            animationTimer += Time.deltaTime;
            float progress = animationTimer / focusAnimationDuration;

            if (progress >= 1f)
            {
                CompleteAnimation();
            }
            else
            {
                UpdateAnimationFrame(progress);
            }
        }

        private void CompleteAnimation()
        {
            isAnimating = false;
            ViewportManager.SetCameraPosition(currentAnimation.targetPosition);
            ViewportManager.SetPPU(currentAnimation.targetPPU);
        }

        private void UpdateAnimationFrame(float progress)
        {
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            // 计算当前帧的位置和PPU
            Vector3 newPosition = Vector3.Lerp(
                currentAnimation.startPosition,
                currentAnimation.targetPosition,
                smoothProgress
            );

            int newPPU = Mathf.RoundToInt(Mathf.Lerp(
                currentAnimation.startPPU,
                currentAnimation.targetPPU,
                smoothProgress
            ));

            // 通过ViewportManager更新相机状态
            ViewportManager.SetCameraPosition(newPosition);
            ViewportManager.SetPPU(newPPU);
        }

        public void Dispose()
        {
            isInitialize = false;
        }
    }
}