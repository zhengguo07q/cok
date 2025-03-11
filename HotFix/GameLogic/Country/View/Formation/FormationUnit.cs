using GameLogic.Country.View.Object;
using UnityEngine;
using static GameLogic.Country.View.Animation.AnimationDeploy;

namespace GameLogic.Country.View.Formation
{
    /// <summary>
    /// 编队单位组件
    /// </summary>
    /// <summary>
    /// 编队中的单位，管理单位在编队中的行为
    /// </summary>
    public class FormationUnit
    {
        private readonly MovableObject movableObject;
        private readonly UnitRoleType roleType;
        private readonly int unitIndex;
        private readonly FormationManager formation;
        private Vector3 localPosition;
        private FormationUnit currentTarget;

        public MovableObject MovableObject => movableObject;
        public UnitRoleType RoleType => roleType;
        public int UnitIndex => unitIndex;
        public bool IsDead => movableObject == null || movableObject.GetComponent<CompCombat>().IsDead;
        public Vector3 Position => movableObject != null ? movableObject.transform.position : Vector3.zero;

        public FormationUnit(MovableObject movableObject, UnitRoleType roleType, int index, FormationManager formation)
        {
            this.movableObject = movableObject;
            this.roleType = roleType;
            this.unitIndex = index;
            this.formation = formation;
            this.localPosition = Vector3.zero;
        }

        public void SetLocalPosition(Vector3 position)
        {
            localPosition = position;
        }

        public void UpdateWorldPosition(Vector3 formationPosition)
        {
            if (movableObject == null) return;

            // 只有在不战斗时才直接设置位置
            if (currentTarget == null)
            {
                Vector3 worldPosition = formationPosition + localPosition;
                movableObject.transform.position = worldPosition;
                movableObject.SceneObjectInfo.Position = worldPosition;
            }
        }

        //public void TakeDamage(float damage)
        //{
        //    combat.TakeDamage(damage, out bool isDead);
        //    if (isDead)
        //    {
        //        formation.OnUnitDeath(this);
        //    }
        //    else
        //    {
        //        formation.OnUnitDamaged(this, damage);
        //    }
        //}


        public void StartCombat(FormationUnit target)
        {
            if (movableObject == null || target == null || target.IsDead) return;

            currentTarget = target;

            // 移动到目标附近
            Vector3 combatPosition = target.Position + (Position - target.Position).normalized * 1.5f;
            movableObject.MoveTo(combatPosition);

            // 开始战斗动作
            movableObject.PerformAction(1.0f); // 假设1.0f是战斗动作的持续时间
        }

        public void StopCombat()
        {
            currentTarget = null;
        }


        public void Update()
        {
            if (movableObject == null) return;

            // 确保MovableObject被更新
            movableObject.MarkAsDirty();

            // 处理战斗逻辑
            if (currentTarget != null && !currentTarget.IsDead)
            {
                // 如果目标移动了，跟随目标
                float distance = Vector3.Distance(Position, currentTarget.Position);
                if (distance > 2.0f)
                {
                    Vector3 combatPosition = currentTarget.Position + (Position - currentTarget.Position).normalized * 1.5f;
                    movableObject.MoveTo(combatPosition);
                }
            }
        }

        public void Dispose()
        {
            // 不在这里销毁MovableObject，因为它可能被其他地方引用
            currentTarget = null;
        }
    }
}
