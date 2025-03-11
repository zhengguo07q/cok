using static GameLogic.Country.View.Animation.AnimationDefinition;
using UnityEngine;
using GameLogic.Country.View.Formation;

namespace GameLogic.Country.View.AI
{
    /// <summary>
    /// 战斗任务
    /// </summary>
    public class CombatTask : HTNTask
    {
        private CompCombat owner;
        private CompCombat target;
        private float attackInterval;  // 攻击间隔
        private float nextAttackTime;  // 下次攻击时间

        public CombatTask(CompCombat owner, CompCombat target, float attackInterval = 1f)
        {
            this.owner = owner;
            this.target = target;
            this.attackInterval = attackInterval;
        }

        public override bool CanExecute(HTNState state)
        {
            // 检查目标是否有效且在攻击范围内
            if (target == null || !target.IsValid)
            {
                return false;
            }

            float distance = Vector3.Distance(state.Position, target.Owner.Position);
            return !state.IsMoving && distance <= owner.Stats.AttackRange;
        }

        public override void Execute(HTNState state)
        {
            state.IsActioning = true;
            nextAttackTime = Time.time;

            // 播放战斗动画
         //   owner.Owner.PlayAction(AnimationType.Attack);
        }

        public override bool IsComplete(HTNState state)
        {
            // 如果目标无效或已死亡，结束战斗
            if (target == null || !target.IsValid || target.IsDead)
            {
                return true;
            }

            // 检查是否需要执行下一次攻击
            if (Time.time >= nextAttackTime)
            {
                // 执行伤害计算
                DealDamage();
                // 设置下次攻击时间
                nextAttackTime = Time.time + attackInterval;
                // 重新播放攻击动画
           //     owner.Owner.PlayAction(AnimationType.Attack);
            }

            // 继续战斗
            return false;
        }

        private void DealDamage()
        {
            if (target != null && target.IsValid)
            {
                target.TakeDamage(owner.Stats.AttackDamage, out _);
            }
        }

        public override void OnExit(HTNState state)
        {
            state.IsActioning = false;
        }
    }
}