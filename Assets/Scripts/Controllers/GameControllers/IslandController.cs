using System.Data.Common;
using UnityEngine;

public class IslandController : MonoBehaviour
{
    [Header("Identity")]
    public int PlayerID; // 0 = P1, 1 = P2...
    public bool isLocalPlayer;

    [Header("Island managers")]
    public MapManager mapManager; 
    public BuildingManager buildManager;
    public ResourceManager resourceManager;

    public void Initialize(int id)
    {
        PlayerID = id;
        resourceManager.Initialize(this);
        buildManager.Initialize(this);
        mapManager.Initialize(this);
    }
}