using UnityEngine;

[CreateAssetMenu(fileName = "New Biome", menuName = "Map/BiomeConfig")]
public class BiomeConfig : ScriptableObject
{
    public string biomeName;
    
    [Header("Layer 0: Surface")]
    public GameObject deepWater;
    
    [Space(10)]
    public GameObject groundCenter;      // Surrounded by ground (Mask 15)
    public GameObject groundInnerCorner; // Ground on 4 sides, but 1 diagonal is water
    public GameObject groundOuterCorner; // Ground on 2 sides (L-Shape)
    public GameObject groundEdge;        // Ground on 3 sides (Flat edge)
    public GameObject groundTip;         // Ground on only 1 side (The case you asked for)
    public GameObject groundIsolated;    // Ground on 0 sides (Single block island)

    [Header("Layer 1: Resources")]
    public GameObject[] trees;
    public GameObject[] rocks;
}