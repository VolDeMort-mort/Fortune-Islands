using UnityEngine;

public class IslandController : MonoBehaviour
{
    [Header("Identity")]
    public int PlayerID;
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
        unitManager.Initialize(this);


            unitManager.SpawnUnitRandomly();

    }
}