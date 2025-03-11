using GameLogic.Country.View.AI;
using GameLogic.Country.View.Object;
using static GameLogic.Country.View.Animation.AnimationDefinition;
using UnityEngine;
using TEngine;
using GameLogic.Country.View.Component;

namespace GameLogic.Country.View.Formation
{
    /// <summary>
    /// 战斗属性
    /// </summary>
    public class CombatStats
    {
        public float MaxHealth { get; set; }
        public float CurrentHealth { get; set; }
        public float AttackDamage { get; set; }
        public float AttackRange { get; set; }
        public float AttackInterval { get; set; }
    }

    /// <summary>
    /// 战斗组件，处理所有战斗相关逻辑
    /// </summary>
    public class CompCombat : ComponentBase
    {
        public SceneObject Owner;
        public CombatStats Stats;

        public bool IsDead => Stats.CurrentHealth <= 0;
        public bool IsEngaged { get; private set; }
        public bool IsValid { get; private set; } = true;
        public CompCombat CurrentTarget { get; private set; }


        public override void Initialize()
        {
            var config = Owner.SceneObjectInfo.MapObjectEntity;
            Stats = new CombatStats
            {
                //MaxHealth = config.MaxHealth,
                //CurrentHealth = config.MaxHealth,
                //AttackDamage = config.AttackDamage,
                //AttackRange = config.AttackRange,
                //AttackInterval = config.AttackInterval
            };
        }

        /// <summary>
        /// 提供设置 IsValid 的方法
        /// </summary>
        public void SetValid(bool valid)
        {
            IsValid = valid;
            if (!valid)
            {
                StopCombat();
            }
        }

        public void StartCombat(CompCombat target)
        {
            if (target == null || target.IsDead) return;

            var OwnerMovable = Owner as MovableObject;
            CurrentTarget = target;
            IsEngaged = true;

            // 移动到攻击范围内
            float distance = Vector3.Distance(Owner.Position, target.Owner.Position);
            if (distance > Stats.AttackRange)
            {
                Vector3 attackPosition = target.Owner.Position +
                    (Owner.Position - target.Owner.Position).normalized * (Stats.AttackRange * 0.9f);
                if(OwnerMovable)
                {
                    OwnerMovable.MoveTo(attackPosition);
                }
            }
            if (OwnerMovable)
            {
                OwnerMovable.AddTask(new CombatTask(this, target, Stats.AttackInterval));
            }
            
        }

        public void StopCombat()
        {
            IsEngaged = false;
            CurrentTarget = null;
           // combatPlanner.ClearTasks();
        }

        public void TakeDamage(float damage, out bool isDead)
        {
            isDead = false;
            if (IsDead) return;

            Stats.CurrentHealth = Mathf.Max(0, Stats.CurrentHealth - damage);
            OnTakeDamage(damage);

            isDead = IsDead;
            if (isDead)
            {
                OnDeath();
            }
        }

        private void OnTakeDamage(float damage)
        {
            Owner.HolderRef.CompAnimation.PlayAction(AnimationType.Hit);
            ShowDamageNumber(damage);
        }

        private void OnDeath()
        {
            Owner.HolderRef.CompAnimation.PlayAction(AnimationType.Death);
        }

        private void ShowDamageNumber(float damage)
        {
            // 实现伤害数字显示
        //    var damageText = GameModule.UI.CreateDamageText(damage);
         //   damageText.SetPosition(owner.Position + Vector3.up * 2f);
        }


        public override void Dispose()
        {
            StopCombat();
            Owner = null;
        }
    }


}
