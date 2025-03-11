using GameBase.Layer;
using GameBase.Utility;
using GameLogic.Country.View.Animation;
using GameLogic.Country.View.Object;
using System;
using System.CodeDom;
using TEngine;
using UnityEngine;
using static GameLogic.Country.View.Animation.AnimationDefinition;

namespace GameLogic.Country.View.Component
{
    /// <summary>
    /// 动画组件
    /// </summary>
    public class CompAnimation : ComponentBase
    {
        public MovableObject SceneObject { get; set; }
        public AnimationManager AnimationManager { get; private set; }
        public Animator Animator { get; private set; }
        
        private GameObject _modelObject;
        private float _currentMoveSpeed;
        private Vector3 _lastPosition;

        // 新增事件系统
        public event Action<AnimationType> OnAnimationStart;
        public event Action<AnimationType> OnAnimationEnd;

        public override void Initialize()
        {
            if (SceneObject == null) return;
            
            LoadModel();
            InitializeAnimation();
        }

        /// <summary>
        /// 加载模型
        /// </summary>
        private void LoadModel()
        {
            // 从SceneObject获取模型路径
            var movable = SceneObject as MovableObject;
            if (movable == null) return;

            var prefab = GameModule.Resource.LoadAsset<GameObject>(movable.ModelPath);
            
            if (prefab != null)
            {
                _modelObject = GameObject.Instantiate(prefab, SceneObject.ObjectView.transform);
                _modelObject.name = movable.ModelPath;
                _modelObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                Animator = _modelObject.GetComponent<Animator>() ?? _modelObject.AddComponent<Animator>();

                LayerUtility.SetLayerIndexInRender(_modelObject, Object.SceneObject.LayerIndexInfo.LayerIndex);
            }
        }

        /// <summary>
        /// 初始化动画
        /// </summary>
        private void InitializeAnimation()
        {
            if (Animator == null) return;

            AnimationManager = new AnimationManager();
            AnimationManager.Initialize(Animator, new AnimationConfig
            {
                Id = SceneObject.SceneObjectInfo.MapObjectEntity.Model,
                HasMoveAnimation = true,
                HasAttackAnimation = true,
                HasIdleAnimation = true
            });
        }

        /// <summary>
        /// 更新动画状态
        /// </summary>
        public void UpdateAnimationState()
        {
            if (AnimationManager == null) return;

            // 计算移动速度
            Vector3 currentPos = SceneObject.transform.position;
            Vector3 movement = currentPos - _lastPosition;
            _currentMoveSpeed = movement.magnitude / Time.deltaTime;
            _lastPosition = currentPos;

            // 更新动画状态
            bool isMoving = _currentMoveSpeed > 0.1f;
            AnimationManager.SetMoving(isMoving);
            AnimationManager.SetMoveSpeed(_currentMoveSpeed);

            // 更新朝向
            if (isMoving)
            {
                UpdateRotation(movement);
            }
        }

        /// <summary>
        /// 更新动画旋转
        /// </summary>
        private void UpdateRotation(Vector3 movement)
        {
            if (movement != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movement);
                SceneObject.transform.rotation = Quaternion.Slerp(
                    SceneObject.transform.rotation,
                    targetRotation,
                    Time.deltaTime * 10f
                );
            }
        }

        /// <summary>
        /// 播放动画
        /// </summary>
        public void PlayAction(AnimationType type, float speedMultiplier = 1f)
        {
            if (AnimationManager == null) return;

            switch (type)
            {
                case AnimationType.Attack:
                    AnimationManager.PlayAnimation(type);
                    break;
            }

            OnAnimationStart?.Invoke(type);
        }

        public override void Dispose()
        {
            if (_modelObject != null)
            {
                GameObject.Destroy(_modelObject);
                _modelObject = null;
            }
            AnimationManager?.Dispose();
            base.Dispose();
        }
    }
}