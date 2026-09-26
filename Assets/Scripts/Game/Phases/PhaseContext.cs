using System;
using System.Collections.Generic;

namespace FortuneIslands.Game.Phases
{
    // Everything a phase handler is allowed to use: the islands in play and a way to say "this phase is done".
    // Handlers get this instead of the whole GameManager, so they cannot start games, spawn islands
    // or change phases directly.
    public sealed class PhaseContext
    {
        private readonly Action _completePhase;

        public PhaseContext(IReadOnlyList<IslandController> islands, Action completePhase)
        {
            Islands = islands ?? throw new ArgumentNullException(nameof(islands));
            _completePhase = completePhase ?? throw new ArgumentNullException(nameof(completePhase));
        }

        public IReadOnlyList<IslandController> Islands { get; }

        // The phase ends on the next TurnController.Tick().
        public void CompletePhase() => _completePhase();
    }
}
