using GameLogic.Country.View.Object;
using UnityEngine;

namespace GameLogic.Country.View.AI
{
    /// <summary>
    /// 移动任务
    /// </summary>
    public class MoveTask : HTNTask
    {
        private Vector3 targetPosition;
        private MovableObject owner;
        private float moveSpeed; // 移动速度

        public MoveTask(MovableObject owner, Vector3 target, float moveSpeed=1.0f)
        {
            this.owner = owner;
            this.targetPosition = target;
            this.moveSpeed = moveSpeed;
        }

        public override bool CanExecute(HTNState state)
        {
            return !state.IsActioning;
        }

        public override void Execute(HTNState state)
        {
            state.IsMoving = true;
            state.TargetPosition = targetPosition;
            state.MoveSpeed = moveSpeed; // 设置移动速度
            owner.UpdatePosition();
        }

        public override bool IsComplete(HTNState state)
        {
            return Vector3.Distance(state.Position, targetPosition) < 0.1f;
        }

        public override void OnExit(HTNState state)
        {
            state.IsMoving = false;
            state.MoveSpeed = 0; // 设置移动速度为0
        }
    }
}