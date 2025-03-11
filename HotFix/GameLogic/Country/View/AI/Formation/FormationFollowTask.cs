using GameLogic.Country.View.Object;
using UnityEngine;
using System.Collections.Generic;
namespace GameLogic.Country.View.AI.Formation
{ 
    public class FormationFollowTask : HTNTask
    {
        private readonly MovableObject follower;
        private readonly MovableObject leader;
        private readonly Vector3 offset;
        private readonly float speed;
        private float updateTimer = 0;
        private readonly List<Vector3> soldierTargetPositions = new();

        public FormationFollowTask(MovableObject follower, MovableObject leader, Vector3 offset, float speed)
        {
            this.follower = follower;
            this.leader = leader;
            this.offset = offset;
            this.speed = speed;
        }

        public override bool CanExecute(HTNState state)
        {
            return !state.IsActioning;
        }

        public override void Execute(HTNState state)
        {
            state.IsMoving = true;
            state.MoveSpeed = speed;
            // 初始目标位置
            state.TargetPosition = leader.transform.position + offset;
        }

        public override bool IsComplete(HTNState state)
        {
            // 每帧更新目标位置
            updateTimer += Time.deltaTime;
            if (updateTimer >= 0.1f) // 每0.1秒更新一次
            {
                updateTimer = 0;
                state.TargetPosition = leader.transform.position + offset;
            }
            
            // 当领袖停止移动或达到目标时完成
            return leader.IsTaskQueueEmpty() || 
                   Vector3.Distance(leader.transform.position, leader.htnState.TargetPosition) < 0.1f;
        }

        public override void OnExit(HTNState state)
        {
            state.IsMoving = false;
        }

        public void SetupFormation(FormationType type, MovableObject leader, List<MovableObject> soldiers)
        {
            // ... existing code ...
            
            // 初始化目标位置列表
            soldierTargetPositions.Clear();
            for (int i = 0; i < soldiers.Count; i++)
            {
                soldierTargetPositions.Add(Vector3.zero);
            }
        }

        private void UpdateSoldierPositionsWithoutMoveTo()
        {
            if (leader == null || formation == null) return;
            
            Vector3 direction = CalculateMoveDirection();
            var offsets = formation.GetFormationOffsets(direction);
            
            // 确保列表大小匹配
            while (soldierTargetPositions.Count < soldiers.Count)
            {
                soldierTargetPositions.Add(Vector3.zero);
            }
            
            // 只更新目标位置，不调用MoveTo
            for (int i = 0; i < soldiers.Count && i < offsets.Count - 1; i++)
            {
                soldierTargetPositions[i] = leader.transform.position + offsets[i + 1];
                // 这里可以添加逻辑来决定何时真正调用MoveTo
            }
        }
    }
}

