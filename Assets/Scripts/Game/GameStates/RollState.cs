using UnityEngine;

using FortuneIslands.Core;

namespace FortuneIslands.Game.GameStates
{
    public class RollState : GameState
    {
        public RollState(GameManager game) : base(game) { }

        public override void Enter()
        {
            Log.Info("--- STATE: ROLL ---");

            foreach (var island in game.AllIslands)
            {
                if (island.diceManager != null)
                {
                    island.diceManager.PerformRoll();
                }
            }

        }
    }
}