using FortuneIslands.Core;

namespace FortuneIslands.Game.Phases
{
    // Players mark where their warriors should sail. For now this enables selecting and moving units;
    // marking attack targets on other islands arrives with the combat rework (stage 4).
    public class WarPlanningPhase : PhaseHandler
    {
        public WarPlanningPhase(PhaseContext context) : base(context) { }

        public override void Enter()
        {
            Log.Info("--- PHASE: WAR PLANNING ---");

            foreach (var island in Context.Islands)
            {
                if (island.unitManager != null) island.unitManager.SetWarPhase(true);
            }
        }

        public override void Exit()
        {
            foreach (var island in Context.Islands)
            {
                if (island.unitManager != null) island.unitManager.SetWarPhase(false);
            }
        }
    }
}
