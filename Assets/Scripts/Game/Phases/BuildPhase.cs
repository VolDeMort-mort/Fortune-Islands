using FortuneIslands.Core;

namespace FortuneIslands.Game.Phases
{
    public class BuildPhase : PhaseHandler
    {
        public BuildPhase(PhaseContext context) : base(context) { }

        public override void Enter()
        {
            Log.Info("--- PHASE: BUILD ---");

            foreach (var island in Context.Islands)
            {
                island.buildManager.SetActive(true);
            }
        }

        public override void Exit()
        {
            foreach (var island in Context.Islands)
            {
                island.buildManager.SetActive(false);
            }
        }
    }
}
