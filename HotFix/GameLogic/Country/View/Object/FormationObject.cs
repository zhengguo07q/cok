using static GameLogic.Country.View.Animation.AnimationDeploy;
using System.Collections.Generic;
using UnityEngine;
using GameLogic.Country.View.Formation;
using GameLogic.Country.View.AI.Formation;

namespace GameLogic.Country.View.Object
{
    /// <summary>
    /// 编队场景对象，作为编队在场景中的表现
    /// </summary>
    public class FormationObject : MovableObject
    {
        private FormationManager formation;
        private MovableObject leader;  // 领袖单位
        private readonly List<MovableObject> soldiers = new();  // 士兵单位列表
        private Vector3 lastMoveDirection = Vector3.zero;
        private bool reachedTarget = false;

        /// <summary>
        /// 设置编队
        /// </summary>
        public void SetupFormation(FormationType type, MovableObject leader, List<MovableObject> soldiers)
        {
            this.leader = leader;
            this.soldiers.AddRange(soldiers);

            formation = new FormationManager(Position, type);

            // 添加领袖
            formation.AddUnit(leader, UnitRoleType.Leader, 0);

            // 添加士兵
            for (int i = 0; i < soldiers.Count; i++)
            {
                formation.AddUnit(soldiers[i], UnitRoleType.Soldier, i + 1);
            }

            formation.UpdateFormation(leader.transform.right);
        }

        /// <summary>
        /// 移动编队到目标位置
        /// </summary>
        public override void MoveTo(Vector3 target, float speed = 1.0f)
        {
            // 领袖移动
            AddTask(new FormationMoveTask(this, target, speed));

            // 计算初始阵型
            Vector3 direction = (target - transform.position).normalized;
            var offsets = formation.GetFormationOffsets(direction);

            // 一次性设置士兵移动命令
            for (int i = 0; i < soldiers.Count && i < offsets.Count - 1; i++)
            {
                // 使用FormationFollowTask而不是普通MoveTo
                soldiers[i].AddTask(new FormationFollowTask(soldiers[i], leader, offsets[i + 1], speed));
            }
        }

        /// <summary>
        /// 同步移动实现（供HTN任务调用）
        /// </summary>
        internal void MoveToSync(Vector3 target, float speed = 1.0f)
        {
            // 使用HTNState中的状态
            htnState.TargetPosition = target;
            htnState.MoveSpeed = speed;
            htnState.IsMoving = true;

            // 领袖移动到目标位置
            if (leader != null)
            {
                leader.MoveTo(target, speed);
            }

            // 士兵根据阵型跟随领袖移动
            UpdateSoldierPositions();
        }

        /// <summary>
        /// 更新士兵位置，使其保持阵型
        /// </summary>
        private void UpdateSoldierPositions()
        {
            if (leader == null || formation == null) return;
            // 获取移动方向
            Vector3 direction;
            if (htnState.IsMoving)
            {
                direction = (htnState.TargetPosition - leader.transform.position).normalized;
            }
            else
            {
                direction = leader.transform.right; // Isometric前进方向
            }
            // 获取阵型中每个位置相对于领袖的偏移
            var offsets = formation.GetFormationOffsets(direction);

            // 更新每个士兵的目标位置
            for (int i = 0; i < soldiers.Count && i < offsets.Count - 1; i++)
            {
                Vector3 soldierTarget = leader.transform.position + offsets[i + 1];
                soldiers[i].MoveTo(soldierTarget, htnState.MoveSpeed);
            }
        }

        /// <summary>
        /// 开始战斗
        /// </summary>
        public void StartCombat(FormationObject target)
        {
            // 使用HTN任务系统异步战斗
            AddTask(new FormationCombatTask(this, target));
        }

        /// <summary>
        /// 同步战斗实现（供HTN任务调用）
        /// </summary>
        public void StartCombatSync(FormationObject target)
        {
            formation?.StartFormationCombat(target.formation);
        }

        /// <summary>
        /// 停止战斗
        /// </summary>
        public void StopCombatSync()
        {
            formation?.StopFormationCombat();
        }

        /// <summary>
        /// 动态更新
        /// </summary>
        protected override void OnDynamicUpdate()
        {
            base.OnDynamicUpdate();

            if (htnState.IsMoving && leader != null)
            {
                // 获取移动方向
                Vector3 moveDirection = CalculateMoveDirection();

                // 只有当方向变化超过阈值时才更新阵型
                if (Vector3.Angle(lastMoveDirection, moveDirection) > 10f)
                {
                    formation.UpdateFormation(moveDirection);
                    lastMoveDirection = moveDirection;

                    // 只有在方向变化时才更新士兵位置
                    UpdateSoldierPositionsWithoutMoveTo();
                }

                // 检查是否到达目标
                float distance = Vector3.Distance(leader.transform.position, htnState.TargetPosition);
                if (distance < 0.1f && !reachedTarget)
                {
                    reachedTarget = true;
                    // 到达目标时更新最终位置
                    UpdateSoldierPositions();
                }
            }

            formation?.Update();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        protected override void Dispose()
        {
            // 清理领袖和士兵
            if (leader != null)
            {
                leader = null;
            }

            soldiers.Clear();
            formation?.Dispose();
            formation = null;

            base.Dispose();
        }

        private void OnDrawGizmos()
        {
            if (formation != null && leader != null)
            {
                // 绘制移动方向
                Gizmos.color = Color.blue;
                Vector3 dir = (htnState.TargetPosition - leader.transform.position).normalized;
                Gizmos.DrawLine(leader.transform.position, leader.transform.position + dir * 2);

                // 绘制编队预期位置
                Gizmos.color = Color.yellow;
                var offsets = formation.GetFormationOffsets(dir);
                foreach (var offset in offsets)
                {
                    Gizmos.DrawSphere(leader.transform.position + offset, 0.2f);
                }
            }
        }

        private Vector3 CalculateMoveDirection()
        {
            if (htnState.IsMoving)
            {
                return (htnState.TargetPosition - leader.transform.position).normalized;
            }
            else
            {
                return leader.transform.right; // Isometric前进方向
            }
        }

        private void UpdateSoldierPositionsWithoutMoveTo()
        {
            if (leader == null || formation == null) return;

            Vector3 direction = (htnState.TargetPosition - leader.transform.position).normalized;
            var offsets = formation.GetFormationOffsets(direction);

            // 只更新目标位置，不调用MoveTo
            for (int i = 0; i < soldiers.Count && i < offsets.Count - 1; i++)
            {
                Vector3 soldierTarget = leader.transform.position + offsets[i + 1];
                soldierTargetPositions[i] = soldierTarget;
            }
        }
    }
}

  