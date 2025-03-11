using GameLogic.Country.View.Object;
using UnityEngine;


namespace GameLogic.Country.View.AI.Formation
{
    /// <summary>
    /// 编队移动任务
    /// </summary>
    public class FormationMoveTask : HTNTask
    {
        private readonly FormationObject formation;
        private readonly Vector3 targetPosition;
        private readonly float moveSpeed;

        public FormationMoveTask(FormationObject formation, Vector3 target, float speed = 1.0f)
        {
            this.formation = formation;
            this.targetPosition = target;
            this.moveSpeed = speed;
        }

        public override bool CanExecute(HTNState state)
        {
            return !state.IsActioning;
        }

        public override void Execute(HTNState state)
        {
            formation.MoveToSync(targetPosition, moveSpeed);
        }

        public override bool IsComplete(HTNState state)
        {
            return Vector3.Distance(state.Position, targetPosition) < 0.1f;
        }

        public override void OnExit(HTNState state)
        {
            state.IsMoving = false;
        }
    }
}
