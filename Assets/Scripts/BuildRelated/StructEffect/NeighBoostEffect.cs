using UnityEngine;

public class NeighBoostEffect : MonoBehaviour, IBuildingFeature
{
    [Header("Boost Settings")]
    public string targetBuildingID; // e.g., "Farm" (Matches Structure Data)
    public int resourceBonus = 5;   // "+5 Food to neighbors"
    public float range = 1.5f;      // 1.5f covers immediate grid neighbors

    public void Initialize(IslandController island)
    {

        // Just holds data
    }
}