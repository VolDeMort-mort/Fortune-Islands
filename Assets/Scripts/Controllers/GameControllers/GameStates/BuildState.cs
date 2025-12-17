using UnityEngine;

public class BuildState : GameState
{
    public BuildState(GameManager game) : base(game) { }

    public override void Enter()
    {
        Debug.Log("--- STATE: BUILD ---");
        
        // Enable UI for building
        // game.uiController.SetBuildMenuActive(true); 

        // Tell all islands: "Allow Building"
        foreach (var island in game.AllIslands)
        {
            island.buildManager.SetActive(true);
        }
    }

    public override void Exit()
    {
        // Disable building on all islands
        foreach (var island in game.AllIslands)
        {
            island.buildManager.SetActive(false);
        }
        
        // game.uiController.SetBuildMenuActive(false);
    }
}