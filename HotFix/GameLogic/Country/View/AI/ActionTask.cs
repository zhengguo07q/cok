using GameLogic.Country.View.Object;
using UnityEngine;

namespace GameLogic.Country.View.AI
{
    /// <summary>
    /// 普通独立任务（比如采集等）
    /// </summary>
    public class ActionTask : HTNTask
    {
        private float actionTime;
        private float actionDuration;
        private MovableObject owner;

        public ActionTask(MovableObject owner, float duration)
        {
            this.owner = owner;
            this.actionDuration = duration;
        }

        public override bool CanExecute(HTNState state)
        {
            return !state.IsMoving;
        }

        public override void Execute(HTNState state)
        {
            state.IsActioning = true;
            actionTime = Time.time;
        }

        public override bool IsComplete(HTNState state)
        {
            return Time.time - actionTime >= actionDuration;
        }

        public override void OnExit(HTNState state)
        {
            state.IsActioning = false;
        }
    }
}
