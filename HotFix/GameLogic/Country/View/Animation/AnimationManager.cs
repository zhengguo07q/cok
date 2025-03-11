using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using TEngine;
using UnityEngine;
using static GameLogic.Country.View.Animation.AnimationDefinition;

namespace GameLogic.Country.View.Animation
{
    /// <summary>
    /// 动画管理器，用于播放动画
    /// </summary>
    public class AnimationManager
    {
        private Animator animator;
        private Dictionary<AnimationType, AnimationClip> animationClips;
        private AnimationType currentAnimationType = AnimationType.None;
        private bool isTransitioning;
        private AnimationConfig animationConfig = new();

        public void Initialize(Animator animator, AnimationConfig animationConfig=null) 
        {
            this.animator = animator;
            if (animationConfig != null) 
            {
                this.animationConfig = animationConfig;
            }
            animationClips = new Dictionary<AnimationType, AnimationClip>();
          //  LoadAnimationClips();
        }


        /// <summary>
        /// 载入所有的动画剪辑
        /// </summary>
        private void LoadAnimationClips()
        {
            if (animationConfig.HasIdleAnimation)
            {
                LoadAnimationClip(AnimationType.Idle, string.Format(AnimConfig.ClipIdleKey, animationConfig.Id));
            }
            if (animationConfig.HasMoveAnimation)
            {
                LoadAnimationClip(AnimationType.Move, string.Format(AnimConfig.ClipMoveKey, animationConfig.Id)); 
            }
            if (animationConfig.HasAttackAnimation)
            {
                LoadAnimationClip(AnimationType.Attack, string.Format(AnimConfig.ClipAttackKey, animationConfig.Id));  
            }
            if (animationConfig.HasHitAnimation)
            {
                LoadAnimationClip(AnimationType.Hit, string.Format(AnimConfig.ClipHitKey, animationConfig.Id)); 
            }
            if (animationConfig.HasDeathAnimation)
            {
                LoadAnimationClip(AnimationType.Death, string.Format(AnimConfig.ClipDeathKey, animationConfig.Id)); 
            }

        }

        /// <summary>
        /// 加载动画剪辑
        /// </summary>
        /// <param name="type"></param>
        /// <param name="path"></param>
        private void LoadAnimationClip(AnimationType type, string path)
        {
            var clip = GameModule.Resource.LoadAsset<AnimationClip>(path);
            if (clip != null)
            {
                animationClips[type] = clip;

                // 设置动画片段到Animator
                var overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
                overrideController[$"Base_{type}"] = clip;
                animator.runtimeAnimatorController = overrideController;
                if (type == AnimationType.Idle && animationConfig.HasIdleAnimation)
                {
                    PlayAnimation(AnimationType.Idle, 0);
                }
            }
            else
            {
                Log.Error($"Failed to load animation clip: {path}");
            }
        }

        /// <summary>
        /// 播放动画
        /// </summary>
        /// <param name="type"></param>
        /// <param name="transitionDuration"></param>
        public void PlayAnimation(AnimationType type, float transitionDuration = -1)
        {
            if (transitionDuration < 0)
            {
                transitionDuration = AnimConfig.CrossFadeDuration;
            }

            if (currentAnimationType == type && !isTransitioning)
            {
                return;
            }

            if (animationClips.TryGetValue(type, out var clip))
            {
                // 使用CrossFade实现平滑过渡
                animator.CrossFade(clip.name, transitionDuration);
                currentAnimationType = type;
                isTransitioning = true;

                // 监听过渡完成
                WaitForTransitionAsync(transitionDuration).Forget();
            }
        }

        /// <summary>
        /// 等待一段时间动画完成
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        private async UniTask WaitForTransitionAsync(float duration)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(duration));
            isTransitioning = false;
        }

        /// <summary>
        /// 设置移动参数
        /// </summary>
        /// <param name="speed"></param>
        public void SetMoveSpeed(float speed)
        {
            animator.SetFloat(Parameters.MoveSpeed, speed);
        }

        /// <summary>
        /// 开启移动
        /// </summary>
        /// <param name="isMoving"></param>
        public void SetMoving(bool isMoving)
        {
            animator.SetBool(Parameters.IsMoving, isMoving);
        }

        /// <summary>
        /// 立刻触发动作
        /// </summary>
        /// <param name="actionType"></param>
        public void TriggerAction(AnimationType actionType)
        {
            animator.SetInteger(Parameters.ActionType, (int)actionType);
            animator.SetTrigger(Parameters.ActionTrigger);
        }

        public void Dispose() 
        {
            
        }
    }
}