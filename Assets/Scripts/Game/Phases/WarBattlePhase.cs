using FortuneIslands.Core;

namespace FortuneIslands.Game.Phases
{
    // The marked attacks play out: boats sail, warriors fight. The phase ends when every battle is resolved.
    public class WarBattlePhase : PhaseHandler
    {
        public WarBattlePhase(PhaseContext context) : base(context) { }

        public override void Enter()
        {
            Log.Info("--- PHASE: WAR BATTLE ---");

            // Stage 4 resolves the planned attacks here and calls CompletePhase() when they are over.
            // There is nothing to resolve yet, so the phase ends right away.
            Context.CompletePhase();
        }
    }
}
