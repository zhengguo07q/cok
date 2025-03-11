using GameLogic.Country.View.Object;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameLogic.Country.View.AI;
using static GameLogic.Country.View.Animation.AnimationDeploy;

namespace GameLogic.Country.View.Formation
{
    /// <summary>
    /// 编队管理器，处理编队的整体行为和战术
    /// </summary>
    public class FormationManager
    {
        private FormationType formationType;                // 编队类型
        private readonly List<FormationUnit> units = new(); // 编队单位列表
        private FormationUnit leader;                       // 编队领导
        private bool isInCombat;                            // 是否在战斗中
        private FormationManager currentTarget;             // 目标
        private Vector3 position;                           // 编队位置
        private FormationTactics currentTactics;            // 策略

        // HTN任务系统
        private HTNPlanner formationPlanner;
        private HTNState formationState;

        public bool IsInCombat => isInCombat;
        public bool IsAllUnitsDead => units.All(u => u.IsDead);
        public int AliveUnitsCount => units.Count(u => !u.IsDead);
        public Vector3 Position => position;

        public FormationManager(Vector3 position, FormationType type)
        {
            this.position = position;
            this.formationType = type;
            this.currentTactics = FormationTactics.Normal;
            
            // 初始化编队HTN状态和规划器
            formationState = new HTNState
            {
                Position = position,
                IsMoving = false,
                IsActioning = false,
                IsComplete = false,
                MoveSpeed = 1.0f
            };
            
            formationPlanner = new HTNPlanner(formationState);
        }

        /// <summary>
        /// 添加任务到编队规划器
        /// </summary>
        public void AddTask(HTNTask task)
        {
            formationPlanner.AddTask(task);
        }

        /// <summary>
        /// 清除所有任务
        /// </summary>
        public void ClearTasks()
        {
            formationPlanner.ClearTasks();
        }

        /// <summary>
        /// 添加单位到编队中
        /// </summary>
        public void AddUnit(MovableObject unitObject, UnitRoleType roleType, int index)
        {
            var unit = new FormationUnit(unitObject, roleType, index, this);
            if (roleType == UnitRoleType.Leader)
            {
                leader = unit;
            }
            units.Add(unit);
        }

        /// <summary>
        /// 获取编队中所有单位的位置偏移
        /// </summary>
        /// <returns>相对于领袖的位置偏移列表</returns>
        public List<Vector3> GetFormationOffsets(Vector3 direction)
        {
            List<Vector3> offsets = new();

            // 领袖位置（原点）
            offsets.Add(Vector3.zero);
            // 转换为2D方向向量（Isometric平面上的XY）
            Vector2 dir2D = new(direction.x, direction.y);

            // 根据阵型和战术计算其他单位的位置
            for (int i = 1; i < units.Count; i++)
            {
                var unit = units[i];
                Vector2 offset = PositionHelper.GetUnitPositionInFormation(
                    formationType,
                    currentTactics,
                    unit.RoleType,
                    unit.UnitIndex,
                    dir2D
                );

                offsets.Add(new Vector3(offset.x, offset.y, 0));
            }

            return offsets;
        }

        /// <summary>
        /// 更新编队阵型
        /// </summary>
        public void UpdateFormation(Vector3 direction)
        {
            // 如果在战斗中，不更新阵型
            if (isInCombat) return;
            Vector2 dir2D = new Vector2(direction.x, direction.y);
            foreach (var unit in units)
            {
                Vector2 offset = PositionHelper.GetUnitPositionInFormation(
                    formationType,
                    currentTactics,
                    unit.RoleType,
                    unit.UnitIndex,
                    dir2D
                );
                unit.SetLocalPosition(new Vector3(offset.x, offset.y, 0));
                unit.UpdateWorldPosition(position);
            }
        }

        /// <summary>
        /// 移动编队到指定位置
        /// </summary>
        public void MoveTo(Vector3 targetPosition, float speed = 1.0f)
        {
            // 如果在战斗中，不能移动
            if (isInCombat) return;

            // 更新HTN状态
            formationState.TargetPosition = targetPosition;
            formationState.MoveSpeed = speed;
            formationState.IsMoving = true;

            // 领袖移动到目标位置
            if (leader != null && leader.MovableObject != null)
            {
                leader.MovableObject.MoveTo(targetPosition, speed);
            }

            // 其他单位根据阵型跟随领袖
            UpdateUnitMovements(speed);
        }


        /// <summary>
        /// 更新单位移动
        /// </summary>
        private void UpdateUnitMovements(float speed)
        {
            if (leader == null || leader.MovableObject == null) return;

            // 获取阵型偏移
            List<Vector3> offsets = GetFormationOffsets(leader.MovableObject.transform.forward);

            // 更新每个单位的目标位置
            for (int i = 1; i < units.Count && i < offsets.Count; i++)
            {
                var unit = units[i];
                if (unit.MovableObject == null) continue;

                // 计算相对于领袖的目标位置
                Vector3 targetPos = leader.Position + offsets[i];

                // 移动到目标位置
                unit.MovableObject.MoveTo(targetPos, speed);
            }
        }


        public void StartFormationCombat(FormationManager target)
        {
            if (isInCombat || target == null || target.IsAllUnitsDead) return;

            currentTarget = target;
            isInCombat = true;
            AssignTargetsToUnits();
        }


        public void StopFormationCombat()
        {
            if (!isInCombat) return;

            isInCombat = false;
            currentTarget = null;

            // 重置所有单位的战斗状态
            foreach (var unit in units)
            {
                unit.StopCombat();
            }

            // 更新阵型 - 添加方向参数
            if (leader != null && leader.MovableObject != null)
            {
                UpdateFormation(leader.MovableObject.transform.forward);
            }
        }

        private void AssignTargetsToUnits()
        {
            if (currentTarget == null) return;

            var aliveUnits = GetAliveUnits();
            var enemyUnits = currentTarget.GetAliveUnits();

            switch (currentTactics)
            {
                case FormationTactics.Aggressive:
                    AssignAggressiveTargets(aliveUnits, enemyUnits);
                    break;
                case FormationTactics.Defensive:
                    AssignDefensiveTargets(aliveUnits, enemyUnits);
                    break;
                default:
                    AssignNormalTargets(aliveUnits, enemyUnits);
                    break;
            }
        }

        public void OnUnitDamaged(FormationUnit unit, float damage)
        {
            // 根据受伤情况调整战术
            if (unit == leader && unit.IsDead)
            {
                HandleLeaderDeath();
            }
            else if (AliveUnitsCount < units.Count * 0.3f)
            {
                HandleHighCasualties();
            }
        }

        public void OnUnitDeath(FormationUnit unit)
        {
            if (unit == leader)
            {
                HandleLeaderDeath();
            }
            else if (AliveUnitsCount < units.Count * 0.3f)
            {
                HandleHighCasualties();
            }
        }

        private void HandleLeaderDeath()
        {
            // 领袖阵亡，切换到防守战术
            currentTactics = FormationTactics.Defensive;
            // 获取存活单位的方向，如果有的话
            var aliveUnits = GetAliveUnits();
            if (aliveUnits.Count > 0 && aliveUnits[0].MovableObject != null)
            {
                UpdateFormation(aliveUnits[0].MovableObject.transform.forward);
            }
        }

        private void HandleHighCasualties()
        {
            // 伤亡过高，尝试撤退
            StopFormationCombat();
            // TODO: 实现撤退逻辑
        }

        public void Update()
        {
            // 更新编队位置（使用领袖位置）
            if (leader != null && leader.MovableObject != null)
            {
                position = leader.Position;
                formationState.Position = position;
            }

            // 更新所有单位
            foreach (var unit in units)
            {
                unit.Update();
            }

            if (isInCombat)
            {
                UpdateCombatState();
            }
            
            // 更新HTN规划器
            formationPlanner.Update();
        }

        public void Dispose()
        {
            foreach (var unit in units)
            {
                unit.Dispose();
            }
            units.Clear();
            leader = null;
            currentTarget = null;
        }

        private List<FormationUnit> GetAliveUnits()
        {
            return units.Where(u => !u.IsDead).ToList();
        }

        private void AssignAggressiveTargets(List<FormationUnit> aliveUnits, List<FormationUnit> enemyUnits)
        {
            if (enemyUnits.Count == 0) return;

            // 优先攻击敌方领袖
            var enemyLeader = enemyUnits.FirstOrDefault(u => u.RoleType == UnitRoleType.Leader);
            if (enemyLeader != null)
            {
                // 分配一半的单位攻击领袖
                int leaderAttackers = Mathf.Max(1, aliveUnits.Count / 2);
                for (int i = 0; i < leaderAttackers && i < aliveUnits.Count; i++)
                {
                    aliveUnits[i].StartCombat(enemyLeader);
                }

                // 剩余单位攻击其他敌人
                for (int i = leaderAttackers; i < aliveUnits.Count; i++)
                {
                    var target = enemyUnits[i % enemyUnits.Count];
                    aliveUnits[i].StartCombat(target);
                }
            }
            else
            {
                // 如果没有领袖，集中火力攻击
                foreach (var unit in aliveUnits)
                {
                    var target = enemyUnits[Random.Range(0, enemyUnits.Count)];
                    unit.StartCombat(target);
                }
            }
        }

        private void AssignDefensiveTargets(List<FormationUnit> aliveUnits, List<FormationUnit> enemyUnits)
        {
            if (enemyUnits.Count == 0) return;

            // 找出我方领袖
            var ourLeader = aliveUnits.FirstOrDefault(u => u.RoleType == UnitRoleType.Leader);
            if (ourLeader != null)
            {
                // 分配最近的敌人给领袖
                var nearestEnemy = FindNearestEnemy(ourLeader, enemyUnits);
                ourLeader.StartCombat(nearestEnemy);

                // 其他单位优先攻击接近领袖的敌人
                var remainingUnits = aliveUnits.Where(u => u.RoleType != UnitRoleType.Leader).ToList();
                foreach (var unit in remainingUnits)
                {
                    var target = FindNearestEnemyToLeader(ourLeader, enemyUnits);
                    unit.StartCombat(target);
                }
            }
            else
            {
                // 领袖已阵亡，采用保守战术
                foreach (var unit in aliveUnits)
                {
                    var target = FindNearestEnemy(unit, enemyUnits);
                    unit.StartCombat(target);
                }
            }
        }

        private void AssignNormalTargets(List<FormationUnit> aliveUnits, List<FormationUnit> enemyUnits)
        {
            if (enemyUnits.Count == 0) return;

            // 一对一分配目标
            for (int i = 0; i < aliveUnits.Count; i++)
            {
                var unit = aliveUnits[i];
                var target = enemyUnits[i % enemyUnits.Count];
                unit.StartCombat(target);
            }
        }

        private FormationUnit FindNearestEnemy(FormationUnit unit, List<FormationUnit> enemies)
        {
            return enemies.OrderBy(e =>
                Vector3.Distance(unit.Position, e.Position)
            ).First();
        }

        private FormationUnit FindNearestEnemyToLeader(FormationUnit leader, List<FormationUnit> enemies)
        {
            // 找出还没有被分配为目标的最近敌人
            return enemies.OrderBy(e =>
                Vector3.Distance(leader.Position, e.Position)
            ).First();
        }

        private void UpdateCombatState()
        {
            if (currentTarget == null || currentTarget.IsAllUnitsDead)
            {
                StopFormationCombat();
                return;
            }

            // 检查并更新战术
            UpdateFormationTactics();

            // 重新分配目标（如果需要）
            if (ShouldReassignTargets())
            {
                AssignTargetsToUnits();
            }
        }

        private void UpdateFormationTactics()
        {
            float healthPercentage = (float)AliveUnitsCount / units.Count;

            // 根据战斗情况调整战术
            if (healthPercentage < 0.3f)
            {
                currentTactics = FormationTactics.Retreat;
            }
            else if (healthPercentage < 0.5f)
            {
                currentTactics = FormationTactics.Defensive;
            }
            else if (healthPercentage > 0.7f)
            {
                currentTactics = FormationTactics.Aggressive;
            }
            else
            {
                currentTactics = FormationTactics.Normal;
            }

            // 更新阵型 - 添加方向参数
            if (leader != null && leader.MovableObject != null)
            {
                UpdateFormation(leader.MovableObject.transform.forward);
            }
        }

        private bool ShouldReassignTargets()
        {
            // 在以下情况重新分配目标：
            // 1. 战术发生变化
            // 2. 有单位死亡
            // 3. 定期检查（例如每5秒）
            return true; // 简化版本，实际实现需要更复杂的逻辑
        }

    }

    /// <summary>
    ///  战斗策略
    /// </summary>
    public enum FormationTactics
    {
        Normal,
        Aggressive,
        Defensive,
        Retreat
    }
}
