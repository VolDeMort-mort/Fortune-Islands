using UnityEngine;

public class BuildState : GameState
{
    public BuildState(GameManager game) : base(game) { }

    public override void Enter()
    {
        Debug.Log("--- STATE: BUILD ---");
        

        foreach (var island in game.AllIslands)
        {
            island.buildManager.SetActive(true);
        }
    }

    public override void Exit()
    {
        foreach (var island in game.AllIslands)
        {
            island.buildManager.SetActive(false);
        }
        
    }
}