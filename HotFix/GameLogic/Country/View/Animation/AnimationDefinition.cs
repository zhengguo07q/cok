using UnityEngine;

namespace GameLogic.Country.View.Animation
{
    /// <summary>
    /// 动画定义
    /// </summary>
    public static class AnimationDefinition
    {
        /// <summary>
        /// 动画状态
        /// </summary>
        public enum AnimationType
        {
            None = 0,
            Idle = 1,       // 待机
            Move = 2,       // 移动
            Attack = 3,     // 攻击
            Hit = 4,        // 受击
            Death = 5,      // 死亡
        }

        /// <summary>
        /// Animator参数名称
        /// </summary>
        public static class Parameters
        {
            public static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");          // 移动速度
            public static readonly int IsAttacking = Animator.StringToHash("IsAttacking");      // 设置攻击
            public static readonly int ActionType = Animator.StringToHash("ActionType");        // 动作状态
            public static readonly int ActionTrigger = Animator.StringToHash("ActionTrigger");  // 立刻触发动作
            public static readonly int IsMoving = Animator.StringToHash("IsMoving");

            //public static readonly int FormationType = Animator.StringToHash("FormationType");
            //public static readonly int UnitRole = Animator.StringToHash("UnitRole");
            //public static readonly int IsInFormation = Animator.StringToHash("IsInFormation");
        }

        /// <summary>
        /// 动画配置
        /// </summary>
        public static class AnimConfig
        {
            public static readonly string ClipIdleKey = "Animations_{0}_idle";
            public static readonly string ClipMoveKey = "Animations_{0}_move";
            public static readonly string ClipAttackKey = "Animations_{0}_attack";
            public static readonly string ClipHitKey = "Animations_{0}_hit";
            public static readonly string ClipDeathKey = "Animations_{0}_death";
            

            public static readonly string ControllerKey = "Controllers_{0}";

            public static readonly float CrossFadeDuration = 0.25f;         // 默认动画过渡时间
            public static readonly float ActionAnimationSpeed = 1.0f;       // 默认动作动画播放速度
            public static readonly float FormationBlendDuration = 0.5f;     // 阵型混合时间
        }
    }
}