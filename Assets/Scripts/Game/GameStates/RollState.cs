using UnityEngine;

public class RollState : GameState
{
    public RollState(GameManager game) : base(game) { }

    public override void Enter()
    {
        Debug.Log("--- STATE: ROLL ---");

        foreach (var island in game.AllIslands)
        {
            if (island.diceManager != null)
            {
                island.diceManager.PerformRoll();
            }
        }
        
    }
}