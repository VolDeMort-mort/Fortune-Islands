using FortuneIslands.Core;

namespace FortuneIslands.Game.Phases
{
    // Every building rolls its dice once. The phase then stays open for TurnRules.rollTimeLimit
    // so players can see the results, or until everyone is ready.
    public class RollPhase : PhaseHandler
    {
        public RollPhase(PhaseContext context) : base(context) { }

        public override void Enter()
        {
            Log.Info("--- PHASE: ROLL ---");

            foreach (var island in Context.Islands)
            {
                if (island.diceManager != null) island.diceManager.PerformRoll();
            }
        }
    }
}
