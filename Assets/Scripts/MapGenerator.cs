using System;
using System.Collections.Generic;
using Mono.Cecil;
using Unity.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour
{


    // UI properties
    public Button BtnGenerate;


    // Assets properties
    [Header("Asserts properties")]
    public GameObject grassGround;
    public GameObject sandGround;
    public GameObject winterGround;
    public GameObject deepWater;

    public GameObject[] treeResource;
    public GameObject[] mountainResource;
    public GameObject world;


    // // Perlin noise properties
    // public float noiseFreq;

    // // Voronoi noise properties
    // public int islandsNumber;
    // public int blocksPerIsland;



    [Header("Island Settings")]
    public Vector3 mapSize;
    public float noiseSeed;
    public float noiseScale;
    [Range(1, 20)] public float noiseOctaves;
    public float noiseThreshold;

    [Tooltip("Percentage of map that remains at full height before falloff starts (0-1)")]
    [Range(0f, 0.9f)] public float islandCoreSize = 0.4f;

    [Tooltip("How steep the falloff is - higher = steeper cliffs")]
    [Range(1f, 10f)] public float falloffStrength = 3.85f;

    [Tooltip("Overall island size multiplier")]
    [Range(0.1f, 2f)] public float islandSizeMultiplier = 1.25f;

    [Tooltip("Square of 1 island")]
    public int islandMinSquare = 400;
    public int islandMaxSquare = 600;


    [Header("Cluster Settings")]
    public int treeClusterCount = 5;
    public float treeClusterRadius = 15f;
    public float treeClusterDensity = 0.7f;

    public int mountainClusterCount = 3;
    public float mountainClusterRadius = 12f;
    public float mountainClusterDensity = 0.5f;

    private List<Vector2Int> groundPositions = new List<Vector2Int>();
    private List<ResourceCluster> resourceClusters = new List<ResourceCluster>();

    void Start()
    {
        BtnGenerate.onClick.AddListener(GenerateIsland);
    }

    void ClearMap()
    {
        foreach (Transform child in world.transform)
        {
            Destroy(child.gameObject);
        }
    }

    void GenerateIsland()
    {
        int ground_count = 0;
        int water_count = 0;
        int attempt_count = 0;

        while (ground_count < islandMinSquare || ground_count > islandMaxSquare)
        {
            ground_count = 0;
            water_count = 0;
            attempt_count++;
            
            ClearMap();
            groundPositions.Clear();

            noiseSeed = UnityEngine.Random.Range(1, 999999999);
            
            Vector2 org = new Vector2(Mathf.Sqrt(noiseSeed), Mathf.Sqrt(noiseSeed));

            for (int x = 0; x < mapSize.x; x++)
            {
                for (int z = 0; z < mapSize.z; z++)
                {
                    Vector3 pos = new Vector3(x, 1, z);
                    float groundValue = NoiseFunction(x, z, org);
                    bool isGround = groundValue > noiseThreshold;

                    if (isGround)
                    {
                        Instantiate(grassGround, pos, Quaternion.identity, world.transform);
                        groundPositions.Add(new Vector2Int(x, z));
                        ground_count++;
                    }
                    else
                    {
                        Instantiate(deepWater, pos, Quaternion.identity, world.transform);
                        water_count++;

                    }
                }
            }
            if (attempt_count > 100)
            {
                Debug.Log($"Limit generation attempts reached: {attempt_count}");
                return;
            }
        }
        Debug.Log($"---Island generation results---" +
                $"\nGeneration attempts: {attempt_count}" +
                $"\nGround square: {ground_count}" +
                $"\nWater square: {water_count}");

        GenerateResourceClusters();
        PlaceResources();
    }

    float NoiseFunction(int x, int z, Vector2 org)
    {
        float noiseSize = noiseScale;
        float opacity = 1;
        float threshold = 0;

        // Generate multi-octave noise
        for (int octave = 0; octave < noiseOctaves; octave++)
        {
            float xValue = x / (noiseScale * 100) + org.x;
            float zValue = z / (noiseScale * 100) + org.y;

            float y = noise.snoise(new float2(xValue, zValue));

            threshold += Mathf.InverseLerp(0, 1, y) / opacity;

            noiseSize /= 2f;
            opacity *= 2f;
        }

        // Apply island falloff gradient
        float falloff = ImprovedFallOffMap((float)x, (float)z);

        // Subtract falloff - higher falloff = more likely to be water
        return threshold - falloff;
    }

    float ImprovedFallOffMap(float x, float z)
    {
        // Normalize coordinates to -1 to 1 range (center = 0,0)
        float normX = (x / mapSize.x) * 2f - 1f;
        float normZ = (z / mapSize.z) * 2f - 1f;

        // Calculate distance from center (0 at center, 1 at corners)
        float distanceFromCenter = Mathf.Sqrt(normX * normX + normZ * normZ);

        // Apply island size multiplier
        distanceFromCenter /= islandSizeMultiplier;

        // Calculate falloff with smooth gradient
        float falloff = 0f;

        if (distanceFromCenter > islandCoreSize)
        {
            // Normalize distance for falloff calculation
            float normalizedDist = (distanceFromCenter - islandCoreSize) / (1f - islandCoreSize);

            // Apply power curve for smooth falloff
            falloff = Mathf.Pow(normalizedDist, falloffStrength);
        }

        // Scale falloff to ensure water at edges
        // Multiply by a value that guarantees water formation
        return falloff * 2f;
    }


    void GenerateResourceClusters()
    {
        resourceClusters.Clear();

        if (groundPositions.Count == 0) return;

        for (int i = 0; i < treeClusterCount; i++)
        {
            Vector2Int randomGround = groundPositions[UnityEngine.Random.Range(0, groundPositions.Count)];
            resourceClusters.Add(new ResourceCluster(
                new Vector2(randomGround.x, randomGround.y),
                treeClusterRadius,
                0,
                treeClusterDensity
            ));
        }

        for (int i = 0; i < mountainClusterCount; i++)
        {
            Vector2Int randomGround = groundPositions[UnityEngine.Random.Range(0, groundPositions.Count)];
            resourceClusters.Add(new ResourceCluster(
                new Vector2(randomGround.x, randomGround.y),
                mountainClusterRadius,
                1,
                mountainClusterDensity
            ));
        }
    }

    void PlaceResources()
    {
        HashSet<Vector2Int> groundSet = new HashSet<Vector2Int>(groundPositions);

        foreach (Vector2Int groundPos in groundPositions)
        {
            ResourceCluster strongestCluster = null;
            float strongestInfluence = 0f;

            foreach (ResourceCluster cluster in resourceClusters)
            {
                float influence = cluster.GetInfluence(groundPos.x, groundPos.y);
                if (influence > strongestInfluence)
                {
                    strongestInfluence = influence;
                    strongestCluster = cluster;
                }
            }

            bool nearGround = true;
            for (int i = -1; i < 2 && nearGround; i++)
            {
                for (int j = -1; j < 2 && nearGround; j++)
                {
                    if (!groundSet.Contains(new Vector2Int(groundPos.x + i, groundPos.y + j)))
                        nearGround = false;
                }
            }

            // bool nearGround = groundSet.Contains(new Vector2Int(groundPos.x + 1, groundPos.y + 1)) &&
            //         groundSet.Contains(new Vector2Int(groundPos.x + 1, groundPos.y - 1)) &&
            //         groundSet.Contains(new Vector2Int(groundPos.x - 1, groundPos.y + 1)) &&
            //         groundSet.Contains(new Vector2Int(groundPos.x - 1, groundPos.y - 1)) &&
            //         groundSet.Contains(new Vector2Int(groundPos.x - 1, groundPos.y)) &&
            //         groundSet.Contains(new Vector2Int(groundPos.x + 1, groundPos.y)) &&
            //         groundSet.Contains(new Vector2Int(groundPos.x, groundPos.y - 1)) &&
            //         groundSet.Contains(new Vector2Int(groundPos.x, groundPos.y - 1));

            if (strongestCluster != null && UnityEngine.Random.value < strongestInfluence && nearGround)
            {
                    Vector3 resourcePos = new Vector3(groundPos.x + (float)0.5, (float)1.75, groundPos.y + (float)0.5);

                    if (strongestCluster.resourceType == 0 && treeResource != null && treeResource.Length > 0)
                    {
                        int treeIndex = UnityEngine.Random.Range(0, treeResource.Length);
                        Instantiate(treeResource[treeIndex], resourcePos, Quaternion.identity, world.transform);
                    }
                    else if (strongestCluster.resourceType == 1 && mountainResource != null && mountainResource.Length > 0)
                    {
                        int mountainIndex = UnityEngine.Random.Range(0, mountainResource.Length);
                        Instantiate(mountainResource[mountainIndex], resourcePos, Quaternion.identity, world.transform);
                    }
            }
        }
    }
}
//     void MakeIslandsMap()
//     {
//         ClearMap();
//         blocksPerIsland = (int)(islandsNumber) * 2;


//         for (int x = 0; x < mapSize.x; x += blocksPerIsland)
//         {
//             for (int z = 0; z < mapSize.x; z += blocksPerIsland)
//             {
//                 Vector3 pos = new Vector3(x + UnityEngine.Random.Range(0, blocksPerIsland), mapSize.y,
//                 z + UnityEngine.Random.Range(0, blocksPerIsland));
//                 Instantiate(grassGround, pos, Quaternion.identity, world.transform);
//             }
//         }
//     }


//     void MakePerlinMap()
//     {
//         ClearMap();
//         noiseSeed = UnityEngine.Random.Range(1, 10000);


//         for (int x = 0; x < mapSize.x; x++)
//         {
//             for (int z = 0; z < mapSize.z; z++)
//             {
//                 Vector3 pos = new Vector3(x, mapSize.y, z);


//                 double groundValue = Mathf.PerlinNoise((pos.x + noiseSeed) / noiseFreq, (pos.z + noiseSeed) / noiseFreq);
//                 bool isGround = groundValue < noiseThreshold;
//                 if (!isGround) Instantiate(deepWater, pos, Quaternion.identity, world.transform);
//                 else { Instantiate(grassGround, pos, Quaternion.identity, world.transform); }
//             }

//         }
//     }
// }
