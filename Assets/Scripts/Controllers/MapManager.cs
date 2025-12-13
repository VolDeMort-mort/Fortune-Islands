using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    [Header("UI")]
    public Button BtnGenerate;
    public Button BtnSpawnUnit;    


    [Header("Configuration")]
    public Vector2Int mapSize;
    public Transform worldContainer;
    public BiomeConfig[] availableBiomes; 

    [Header("Noise Settings")]
    public float noiseScale = 25f;
    [Range(1, 10)] public int noiseOctaves = 4;
    [Range(0f, 1f)] public float noiseThreshold = 0.4f;
    public float falloffStrength = 3f;
    public float islandSizeMultiplier = 1f;

    [Header("Resource Settings")]
    public int treeClusterCount = 5;
    public float treeClusterRadius = 5f;
    [Range(0f, 1f)]public float treeClusterDensity = 0.5f;
    public int rockClusterCount = 3;
    public float rockClusterRadius = 5f;
    [Range(0f, 1f)]public float rockClusterDensity = 0.5f;

    public int goldClusterCount = 1;
    public float goldClusterRadius = 2f;
    [Range(0f, 1f)]public float goldClusterDensity = 0.5f;
    [Range(0f, 1f)]public float grassDensity = 0.2f;

    [Header("Unit Settings")]
    public GameObject villagerPrefab;

    public WorldMap map;
    private TileService tileService;
    private BiomeConfig currentBiome;

    void Initialize()
    {
        map = new WorldMap(mapSize);
        tileService = new TileService(map);
    }

    void Start()
    {
        GenerateWorld();
        SpawnVillager();
        if (BtnSpawnUnit != null) 
            BtnSpawnUnit.onClick.AddListener(SpawnVillager);
        if (BtnGenerate != null)
            BtnGenerate.onClick.AddListener(GenerateWorld);

    }

    public void GenerateWorld()
    {
        Initialize();

        if (availableBiomes.Length > 0)
        {
            currentBiome = availableBiomes[UnityEngine.Random.Range(0, availableBiomes.Length)];
            Debug.Log($"Selected Biome: {currentBiome.biomeName}");
        }
        else
        {
            Debug.LogError("No Biomes assigned in Inspector!");
            return;
        }

        GenerateTerrainData();
        
        ClearResources();
        GenerateResourceData();

        RenderMap();     
    }

    void GenerateTerrainData()
    {
        float seed = UnityEngine.Random.Range(0f, 10000f);
        Vector2 offset = new Vector2(seed, seed);

        for (int x = 0; x < map.mapSize.x; x++)
        {
            for (int y = 0; y < map.mapSize.y; y++)
            {
                // 1. Calculate Noise + Falloff
                float noiseVal = NoiseService.GetNoiseValue(x, y, offset, noiseScale, noiseOctaves);
                float falloff = NoiseService.GetFalloffValue(x, y, new Vector2(map.mapSize.x, map.mapSize.y), falloffStrength, islandSizeMultiplier);
                float finalValue = noiseVal - falloff;

                // 2. Determine Water/Ground
                if (finalValue > noiseThreshold)
                {
                    // Grid[x, y].Type = CellType.Ground;
                    map.GetCell(x, y).Type = CellType.Ground;
                }
                else
                {   
                    map.GetCell(x, y).Type = CellType.Water;
                    // Grid[x, y].Type = CellType.Water;
                }
            }
        }

        // 3. Post-Processing: Calculate Bitmask for Edges/Corners
        for (int x = 0; x < map.mapSize.x; x++)
        {
            for (int y = 0; y < map.mapSize.y; y++)
            {
                if (map.GetCell(x, y).Type == CellType.Ground)
                {
                    tileService.CalculateTileVariation(x, y);
                }
            }
        }
    }

    void GenerateResourceData()
    {
        List<Vector2Int> groundTiles = map.GetGroundTiles();
        if (groundTiles.Count == 0) return;

        List<IGenerationPattern> generationSteps = new List<IGenerationPattern>();

        generationSteps.Add(new ClusterPattern(MapResourceType.Tree,treeClusterCount, treeClusterRadius, treeClusterDensity));
        
        generationSteps.Add(new ClusterPattern(MapResourceType.Rock, rockClusterCount, rockClusterRadius, rockClusterDensity));
        
        generationSteps.Add(new ClusterPattern(MapResourceType.Gold, goldClusterCount, goldClusterRadius, goldClusterDensity));

        generationSteps.Add(new RandomPattern(MapResourceType.Grass, grassDensity, true));


        foreach (var pattern in generationSteps)
        {
            pattern.Generate(map, groundTiles, currentBiome);
        }
    }

    void RenderMap()
    {
        for (int x = 0; x < map.mapSize.x; x++)
        {
            for (int y = 0; y < map.mapSize.y; y++)
            {
                CellData cell = map.GetCell(x, y);
                Vector3 pos = new Vector3(0, 0, 0);

                // Layer 0: Surface
                GameObject surfacePrefab = null;
                Quaternion rotation = Quaternion.identity;

                if (cell.Type == CellType.Ground)
                {
                    pos = new Vector3(x, 0.15f, y);
                    rotation = tileService.GetRotationForCell(x, y, cell.Bitmask, cell.Variation);
                    switch (cell.Variation)
                    {
                        case TileVariation.Center:      surfacePrefab = currentBiome.groundCenter; break;
                        case TileVariation.InnerCorner: surfacePrefab = currentBiome.groundInnerCorner; break;
                        case TileVariation.OuterCorner: surfacePrefab = currentBiome.groundOuterCorner; break;
                        case TileVariation.Edge:        surfacePrefab = currentBiome.groundEdge; break;
                        case TileVariation.Tip:         surfacePrefab = currentBiome.groundTip; break;
                        case TileVariation.Isolated:    surfacePrefab = currentBiome.groundIsolated; break;
                        default:                        surfacePrefab = currentBiome.groundCenter; break;
                    }

                    // Layer 1: Resources 
                    if (cell.OccupyingObject != null)
                    {
                        Instantiate(cell.OccupyingObject, new Vector3(x, 1f, y), Quaternion.identity, worldContainer);
                        // cell.OccupyingObject = resourceObj; 
                    }
                }
                else if(cell.Type == CellType.Water)
                {
                    pos = new Vector3(x, 0, y);
                    surfacePrefab = currentBiome.deepWater;
                }

                Instantiate(surfacePrefab, pos, rotation, worldContainer);
            }
        }
    }

    // void ClearWorld()
    // {
    //     for (int i = worldContainer.childCount - 1; i >= 0; i--)
    //     {
    //         Destroy(worldContainer.GetChild(i).gameObject);
    //     }
    // }
    // void ClearGrid()
    // {
    //     for (int x = 0; x < map.mapSize.x; x++)
    //     {
    //         for (int y = 0; y < map.mapSize.y; y++)
    //         {
    //             map.SetCell(x, y, null);
    //         }
    //     }
    // }

    void ClearResources()
    {
        for (int x = 0; x < map.mapSize.x; x++)
        {
            for (int y = 0; y < map.mapSize.y; y++)
            {
                if (map.GetCell(x, y).OccupyingObject != null){
                    Destroy(map.GetCell(x, y).OccupyingObject);
                    map.GetCell(x, y).OccupyingObject = null;
                }
            }
        }
    }

    void SpawnVillager()
    {
        if (map == null)
        {
            Debug.LogError("Generate the map first!");
            return;
        }

        // Try 100 times to find a random empty spot
        for (int i = 0; i < 100; i++)
        {
            int rx = UnityEngine.Random.Range(0, map.mapSize.x);
            int ry = UnityEngine.Random.Range(0, map.mapSize.y);
            CellData cell = map.GetCell(rx, ry);

            // Check if valid spawn point (Ground + No Tree/Rock + No other Villager)
            if (cell.Type == CellType.Ground && cell.OccupyingObject == null)
            {
                Vector3 spawnPos = new Vector3(rx, 2f, ry);
                GameObject unitObj = Instantiate(villagerPrefab, spawnPos, Quaternion.identity, worldContainer);
                
                // Initialize 
                VillagerController controller = unitObj.GetComponent<VillagerController>();
                if (controller != null)
                {
                    controller.Initialize(this, new Vector2Int(rx, ry));
                }
                
                Debug.Log($"Spawned Villager at {rx}, {ry}");
                return;
            }
        }
        Debug.LogWarning("Could not find empty spot for Villager.");
    }
}