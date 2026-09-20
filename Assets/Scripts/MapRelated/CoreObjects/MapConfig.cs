using UnityEngine;
using Unity;

[CreateAssetMenu(fileName = "MapGenerationConfig", menuName = "Configs/Map Generation")]
public class MapConfig : ScriptableObject
{
    [Header("Noise Settings")]
    public float noiseScale = 25f;
    public int noiseOctaves = 4;
    public float noiseThreshold = 0.4f;
    public float falloffStrength = 3f;
    public float islandSizeMultiplier = 1f;

    [Header("Resource Settings")]
    
    [Header("Tree Settings")]
    public int treeClusterCount = 5;
    public float treeClusterRadius = 5f;
    [Range(0f, 1f)]public float treeClusterDensity = 0.5f;

    
    [Header("Rock Settings")]
    public int rockClusterCount = 3;
    public float rockClusterRadius = 5f;
    [Range(0f, 1f)]public float rockClusterDensity = 0.5f;

        
    [Header("Gold Settings")]
    public int goldClusterCount = 1;
    public float goldClusterRadius = 2f;
    [Range(0f, 1f)]public float goldClusterDensity = 0.5f;

    
    [Header("Grass Settings")]
    [Range(0f, 1f)]public float grassDensity = 0.2f;
}