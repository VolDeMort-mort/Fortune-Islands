using UnityEngine;

public class RollState : GameState
{
    public RollState(GameManager game) : base(game) { }

    public override void Enter()
    {
        Debug.Log("--- STATE: ROLL ---");

        // Trigger the dice roll logic immediately
        foreach (var island in game.AllIslands)
        {
            if (island.diceManager != null)
            {
                island.diceManager.PerformRoll();
            }
        }
        
        // Auto-advance to next state after a short delay (animation)?
        // Or wait for user input. Let's wait for user input in this design.
    }
}