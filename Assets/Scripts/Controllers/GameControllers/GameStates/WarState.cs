using UnityEngine;

public class WarState : GameState
{
    public WarState(GameManager game) : base(game) { }

    public override void Enter()
    {
        Debug.Log("--- STATE: WAR ---");

        foreach (var island in game.AllIslands)
        {
            // Assuming you have a UnitManager
            // island.unitManager.ExecuteCombatTurn();
        }
    }
}