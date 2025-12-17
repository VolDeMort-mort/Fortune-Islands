using System.Data.Common;
using UnityEngine;

public class IslandController : MonoBehaviour
{
    [Header("Identity")]
    public int PlayerID; // 0 = P1, 1 = P2...
    public bool isLocalPlayer;
    public Transform worldContainer;

    [Header("Island managers")]
    public MapManager mapManager; 
    public BuildingManager buildManager;
    public ResourceManager resourceManager;
    public DiceManager diceManager;
    public UnitManager unitManager;

    public void Initialize(int id)
    {

        mapManager.worldContainer = worldContainer;

        PlayerID = id;
        resourceManager.Initialize(this);
        buildManager.Initialize(this);
        mapManager.Initialize(this);
        diceManager.Initialize(this);
        diceManager.Initialize(this);
        unitManager.Initialize(this);
    }

// Called by the State Machine (GameManager)
    public void OnPhaseChanged(GameState newPhase)
    {
        switch (newPhase)
        {
            case BuildState:
                Debug.Log($"Island {PlayerID}: Build Mode ON");
                buildManager.SetActive(true);
                break;

            case RollState:
                Debug.Log($"Island {PlayerID}: Rolling Dice...");
                buildManager.SetActive(false); // Stop building
                if (diceManager != null) diceManager.PerformRoll();
                break;

            case WarState:
                Debug.Log($"Island {PlayerID}: War Started!");
                buildManager.SetActive(false);
                if (unitManager != null) unitManager.ExecuteCombatTurn();
                break;
        }
    }
}