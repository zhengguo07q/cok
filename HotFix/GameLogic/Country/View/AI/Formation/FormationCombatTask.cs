using GameLogic.Country.View.Object;


namespace GameLogic.Country.View.AI.Formation
{
    /// <summary>
    /// 编队战斗任务
    /// </summary>
    public class FormationCombatTask : HTNTask
    {
        private readonly FormationObject formation;
        private readonly FormationObject target;
        private bool combatStarted = false;

        public FormationCombatTask(FormationObject formation, FormationObject target)
        {
            this.formation = formation;
            this.target = target;
        }

        public override bool CanExecute(HTNState state)
        {
            return !state.IsMoving;
        }

        public override void Execute(HTNState state)
        {
            state.IsActioning = true;
            formation.StartCombatSync(target);
            combatStarted = true;
        }

        public override bool IsComplete(HTNState state)
        {
            // 简单示例：检查目标是否已被击败
            // 实际实现可能需要更复杂的战斗结束条件
            return !combatStarted || target == null;
        }

        public override void OnExit(HTNState state)
        {
            state.IsActioning = false;
            formation.StopCombatSync();
        }
    }
}
