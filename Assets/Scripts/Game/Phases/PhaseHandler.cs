namespace FortuneIslands.Game.Phases
{
    // Applies one phase to the world: what switches on when the phase starts and off when it ends.
    // Which phase runs and when is decided by TurnController, never by a handler.
    public abstract class PhaseHandler
    {
        protected readonly PhaseContext Context;

        protected PhaseHandler(PhaseContext context)
        {
            Context = context;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
    }
}
