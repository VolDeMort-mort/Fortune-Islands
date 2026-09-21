using UnityEngine;

using FortuneIslands.Core;

namespace FortuneIslands.Game.GameStates
{
    public class WarState : GameState
    {
        public WarState(GameManager game) : base(game) { }

        public override void Enter()
        {
            Log.Info("--- STATE: WAR ---");

            foreach (var island in game.AllIslands)
            {
                island.buildManager.SetActive(false);
                if (island.unitManager != null) island.unitManager.SetWarPhase(true);
            }
        }

        public override void Exit()
        {
            foreach (var island in game.AllIslands)
            {
                if (island.unitManager != null) island.unitManager.SetWarPhase(false);
            }
        }
    }
}